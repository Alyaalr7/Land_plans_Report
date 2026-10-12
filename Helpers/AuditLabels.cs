using System.Globalization;

namespace Land_plans_Report.Helpers
{
    // سجل التدقيق: أسماء العمليات وتصنيفها وشكلها (عرض فقط)
    public static class AuditLabels
    {
        public static string Action(string? code) => code switch
        {
            "Login" => "تسجيل دخول",
            "LoginFailed" => "محاولة دخول فاشلة",
            "Create" => "إنشاء تقرير",
            "Submit" => "إرسال تقرير",
            "Update" => "تعديل تقرير",
            "Resubmit" => "إعادة إرسال تقرير",
            "Approve" => "اعتماد تقرير",
            "Return" => "إعادة تقرير للتعديل",
            "UserAdded" => "إضافة مستخدم",
            "UserUpdated" => "تعديل مستخدم",
            "UserActivated" => "تفعيل مستخدم",
            "UserDeactivated" => "تعطيل مستخدم",
            "SignatureUploaded" => "رفع توقيع",
            "SignatureDeactivated" => "تعطيل توقيع",
            "StampUploaded" => "رفع ختم",
            "StampDeactivated" => "تعطيل ختم",
            "StampUpdated" => "تعديل ختم",
            _ => code ?? ""
        };

        public static string ReportType(string? type) => type switch
        {
            "plan" => "مخطط",
            "plot" => "قطعة أرض",
            "raw" => "أرض خام",
            "unplanned" => "أرض بلا مخطط",
            _ => ""
        };

        // التفاصيل: جملة توضح العملية، تُبنى وقت العرض
        public static string? Details(string? actionType, string? details, string? reportType, string? reportCreator)
        {
            var d = details?.Trim();
            var t = ReportType(reportType);
            var tr = t == "" ? "تقرير" : "تقرير " + t;
            var by = string.IsNullOrEmpty(reportCreator) ? "" : " مقدَّم من " + reportCreator;

            switch (actionType)
            {
                case "Login": return "دخول ناجح إلى النظام";
                case "LoginFailed": return Inner(d) ?? "محاولة دخول غير ناجحة";
                case "Create": return $"حفظ {tr} كمسودة";
                case "Submit": return $"إرسال {tr} جديد للاعتماد";
                case "Update": return $"حفظ تعديلات {tr}";
                case "Resubmit": return $"إعادة إرسال {tr} بعد التعديل";
                case "Approve": return $"اعتماد {tr}{by}" + (string.IsNullOrEmpty(d) ? "" : " · " + d);
                case "Return": return string.IsNullOrEmpty(d) ? $"إعادة {tr}{by} للتعديل" : "السبب: " + d;
                case "UserAdded": return "إضافة المستخدم " + Name(d);
                case "UserUpdated": return "تعديل بيانات المستخدم " + Name(d);
                case "UserActivated": return "تفعيل حساب " + Name(d);
                case "UserDeactivated": return "تعطيل حساب " + Name(d);
                case "SignatureUploaded": return "رفع توقيع " + Name(d);
                case "SignatureDeactivated": return "تعطيل توقيع " + Name(d);
                default: return string.IsNullOrEmpty(d) ? null : d;
            }
        }

        // "تعديل بيانات المستخدم: فلان (username)" ← فلان
        private static string Name(string? d)
        {
            if (string.IsNullOrEmpty(d)) return "";
            int i = d.IndexOf(": ");
            var n = i >= 0 ? d.Substring(i + 2) : d;
            int p = n.IndexOf(" (");
            return p > 0 ? n.Substring(0, p) : n;
        }

        // "محاولة دخول فاشلة (كلمة مرور غير صحيحة)" ← كلمة مرور غير صحيحة
        private static string? Inner(string? d)
        {
            if (string.IsNullOrEmpty(d)) return null;
            int a = d.IndexOf('('), b = d.LastIndexOf(')');
            return a >= 0 && b > a ? d.Substring(a + 1, b - a - 1) : d;
        }

        // التصنيفات بالترتيب اللي تظهر فيه العلامات
        public static readonly (string Key, string Label)[] Categories =
        {
            ("login", "الدخول"),
            ("reports", "التقارير"),
            ("decisions", "الاعتماد"),
            ("admin", "الإدارة")
        };

        public static string Category(string? code) => code switch
        {
            "Login" or "LoginFailed" => "login",
            "Create" or "Submit" or "Update" or "Resubmit" => "reports",
            "Approve" or "Return" => "decisions",
            _ => "admin"
        };

        // لون الأيقونة (نفس ألوان المؤشرات)
        public static string Tone(string? code) => code switch
        {
            "LoginFailed" => "is-danger",
            "Approve" => "is-success",
            "Return" => "is-warning",
            "Create" or "Submit" or "Update" or "Resubmit" => "is-info",
            _ => "is-neutral"
        };

        public static string Icon(string? code) => code switch
        {
            "Login" or "LoginFailed" => "i-logout",
            "Create" or "Update" => "i-draft",
            "Submit" or "Resubmit" => "i-file",
            "Approve" => "i-check",
            "Return" => "i-return",
            "SignatureUploaded" or "SignatureDeactivated" or "StampUploaded" or "StampDeactivated" or "StampUpdated" => "i-stamp",
            _ => "i-users"
        };
    }

    // صلاحيات كل دور (للعرض فقط في صفحة المستخدمين)
    public static class RolePermissions
    {
        public static string[] For(string? roleCode) => roleCode switch
        {
            "Admin" => new[] { "إدارة المستخدمين", "إدارة التواقيع والأختام", "سجل التدقيق", "عرض التقارير المرسلة" },
            "Technical" => new[] { "إنشاء التقارير", "إرسال التقارير للاعتماد", "تعديل التقارير المعادة" },
            "PlotApprover" => new[] { "اعتماد تقارير القطع والأراضي", "إعادة التقارير للتعديل" },
            "PlanApprover" => new[] { "اعتماد تقارير المخططات", "إعادة التقارير للتعديل" },
            _ => Array.Empty<string>()
        };
    }

    // عرض التاريخ والوقت بالعربي: اليوم 4:39 ص · أمس 11:20 م · 2026/09/29 · 4:39 م
    // (ص/م تنكتب يدوياً — ثقافة ar-SA في .NET تحوّل التاريخ للهجري)
    public static class DateDisplay
    {
        public static string Time(DateTime d)
        {
            int h = d.Hour % 12;
            if (h == 0) h = 12;
            return $"{h}:{d.Minute:00} {(d.Hour < 12 ? "ص" : "م")}";
        }

        public static string Date(DateTime d) => d.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

        public static string When(DateTime d)
        {
            if (d.Date == DateTime.Today) return "اليوم " + Time(d);
            if (d.Date == DateTime.Today.AddDays(-1)) return "أمس " + Time(d);
            return Date(d) + " · " + Time(d);
        }
    }
}
