namespace Land_plans_Report.Helpers
{
    // ─────────────────────────────────────────────────────────────
    // تخزين الملفات المرفوعة بأمان:
    //   ١. المكان: App_Data/Files (خارج wwwroot) — المتصفح ما يوصل له مباشرة،
    //      والعرض يكون عن طريق FilesController بعد التحقق من الصلاحية
    //   ٢. الفحص: نوع الملف الحقيقي من أول بايتاته (مو من الامتداد اللي ينزوّر)
    //   ٣. الاسم: عشوائي (GUID) + الامتداد فقط، بدون أي جزء من اسم الملف الأصلي
    // ─────────────────────────────────────────────────────────────
    public static class SecureFileStorage
    {
        public const long MaxBytes = 10 * 1024 * 1024; // 10 ميجابايت

        // الأنواع المسموحة: الامتداد + نوع المحتوى + التوقيع (أول بايتات الملف)
        private static readonly (string Ext, string ContentType, byte[] Magic)[] Types =
        {
            (".jpg", "image/jpeg",      new byte[] { 0xFF, 0xD8, 0xFF }),
            (".png", "image/png",       new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            (".pdf", "application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }) // %PDF-
        };

        public static string Root(IWebHostEnvironment env) =>
            Path.Combine(env.ContentRootPath, "App_Data", "Files");

        // فحص ملف واحد — يرجع رسالة الخطأ، أو null إذا الملف سليم
        public static string? Validate(IFormFile file, bool imagesOnly = false)
        {
            if (file.Length == 0)
                return $"الملف \"{file.FileName}\" فارغ.";
            if (file.Length > MaxBytes)
                return $"الملف \"{file.FileName}\" حجمه أكبر من 10 ميجابايت.";

            var type = Detect(file);
            if (type == null || (imagesOnly && type.Value.Ext == ".pdf"))
                return imagesOnly
                    ? $"الملف \"{file.FileName}\" ليس صورة صالحة (PNG أو JPG فقط)."
                    : $"الملف \"{file.FileName}\" ليس صورة أو PDF صالح.";

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".jpeg") ext = ".jpg";
            if (ext != type.Value.Ext)
                return $"امتداد الملف \"{file.FileName}\" لا يطابق محتواه الفعلي.";

            return null;
        }

        // حفظ ملف (بعد فحصه) — يرجع المسار النسبي اللي ينحفظ في الداتابيز
        // مثال: reports/TR-2026-000001/3f2a...e1.jpg
        public static async Task<string> SaveAsync(IWebHostEnvironment env, IFormFile file, string folder)
        {
            var type = Detect(file) ?? throw new InvalidOperationException("نوع ملف غير مسموح");
            var relative = $"{folder.Trim('/')}/{Guid.NewGuid():N}{type.Ext}";
            var full = FullPath(env, relative) ?? throw new InvalidOperationException("مسار غير صالح");

            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            using (var stream = new FileStream(full, FileMode.CreateNew))
                await file.CopyToAsync(stream);

            return relative;
        }

        // المسار الكامل على الجهاز — null لو المسار يحاول يطلع برا مجلد الملفات
        public static string? FullPath(IWebHostEnvironment env, string? relative)
        {
            if (string.IsNullOrWhiteSpace(relative)) return null;
            var root = Path.GetFullPath(Root(env));
            var full = Path.GetFullPath(Path.Combine(root, relative.TrimStart('/', '\\')));
            return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full : null;
        }

        public static void Delete(IWebHostEnvironment env, string? relative)
        {
            var full = FullPath(env, relative);
            if (full != null && File.Exists(full)) File.Delete(full);
        }

        public static string ContentType(string path) =>
            Types.FirstOrDefault(t => t.Ext == Path.GetExtension(path).ToLowerInvariant()).ContentType
            ?? "application/octet-stream";

        public static bool IsPdf(string? path) =>
            path != null && path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        // قراءة أول بايتات الملف ومقارنتها بالتوقيعات المسموحة
        private static (string Ext, string ContentType, byte[] Magic)? Detect(IFormFile file)
        {
            var header = new byte[8];
            int read;
            using (var s = file.OpenReadStream())
                read = s.Read(header, 0, header.Length);

            foreach (var t in Types)
                if (read >= t.Magic.Length && header.AsSpan(0, t.Magic.Length).SequenceEqual(t.Magic))
                    return t;
            return null;
        }
    }
}
