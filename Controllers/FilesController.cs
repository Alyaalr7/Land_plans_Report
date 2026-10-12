using Land_plans_Report.Data;
using Land_plans_Report.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Land_plans_Report.Controllers
{
    // عرض الملفات المحمية (المخزنة خارج wwwroot) — بعد التحقق من الدخول والصلاحية فقط
    public class FilesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public FilesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Files/Photo/15 — صورة أو PDF مرفق بتقرير
        public async Task<IActionResult> Photo(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roleCode = HttpContext.Session.GetString("RoleCode");
            if (userId == null) return Unauthorized();

            var photo = await _db.ReportPhotos
                .Include(p => p.Report)
                .FirstOrDefaultAsync(p => p.PhotoId == id);

            if (photo?.Report == null) return NotFound();
            if (!ReportAccess.CanView(photo.Report, userId.Value, roleCode)) return StatusCode(403);

            return Serve(photo.FilePath);
        }

        // GET: /Files/Signature/7 — صورة توقيع أو ختم
        public async Task<IActionResult> Signature(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roleCode = HttpContext.Session.GetString("RoleCode");
            if (userId == null) return Unauthorized();

            var item = await _db.Signatures.FindAsync(id);
            if (item == null) return NotFound();
            if (!await CanViewSignatureAsync(item, userId.Value, roleCode)) return StatusCode(403);

            return Serve(item.FilePath);
        }

        // مين يشوف التوقيع أو الختم:
        //   مدير النظام، وصاحب التوقيع
        //   ختم الجهة: أي مستخدم داخل النظام (يظهر في كل تقرير معتمد)
        //   توقيع شخص: من يقدر يشوف تقرير أعدّه أو اعتمده هذا الشخص (الطباعة)
        private async Task<bool> CanViewSignatureAsync(Land_plans_Report.Data.Signature item, int userId, string? roleCode)
        {
            if (roleCode == "Admin") return true;
            if (item.UserId == userId) return true;
            if (item.Kind == SignatureKinds.Stamp) return true;

            var reports = await _db.Reports
                .Where(r => r.CreatedByUserId == item.UserId
                         || r.Approvals.Any(a => a.ApproverUserId == item.UserId))
                .ToListAsync();
            return reports.Any(r => ReportAccess.CanView(r, userId, roleCode));
        }

        // يرسل الملف بنوعه الصحيح، ويمنع المتصفح من تخمين نوعه
        private IActionResult Serve(string? relativePath)
        {
            var full = SecureFileStorage.FullPath(_env, relativePath);
            if (full == null || !System.IO.File.Exists(full)) return NotFound();

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "private, max-age=3600";
            return PhysicalFile(full, SecureFileStorage.ContentType(full));
        }
    }
}
