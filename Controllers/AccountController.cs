using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Land_plans_Report.Data;
namespace Land_plans_Report.Controllers
{
    public class AccountController : Controller

    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        public IActionResult Login() => View();


        // صفحة "غير مصرح" (تظهر بدل الخطأ لما يُرفض المستخدم)
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }



        // ⚠️ تسجيل الدخول هنا حقيقي (يقرأ من جدول Users فعلياً)، لكن مقارنة كلمة
        // المرور لا تزال مبسّطة مؤقتاً (نص صريح TEMP_PLAIN_FOR_DEV_ONLY:xxx كما
        // بالبيانات التجريبية بالسكربت). عند التجهيز للإنتاج الفعلي، تُستبدل
        // سطر المقارنة بـ BCrypt.Verify(password, user.PasswordHash)،
        // ولاحقاً يُستبدل هذا الإجراء بالكامل بتكامل نفاذ.





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                return View();
            }

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            bool passwordOk = user != null &&
                user.PasswordHash == $"TEMP_PLAIN_FOR_DEV_ONLY:{password}";

            if (user == null || !passwordOk)
            {
                // ⚠️ تسجيل محاولات الدخول الفاشلة (بدون كلمة المرور أبداً) — مهم لاكتشاف محاولات اختراق متكررة
                // ملاحظة: يُسجَّل فقط لو اسم المستخدم موجود فعلاً (قيد قاعدة البيانات الحالي يتطلب UserId صحيح)؛
                // محاولات بأسماء مستخدمين غير موجودة أصلاً لا تُسجَّل حالياً — يُنصح بجعل عمود UserId قابلاً
                // لقيمة NULL بجدول AuditLog مستقبلاً لتغطية هذي الحالة أيضاً
                if (user != null)
                    await LogAudit(user.UserId, null, "LoginFailed", "محاولة دخول فاشلة (كلمة مرور غير صحيحة)");

                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }
            HttpContext.Session.Clear();
            // تخزين هوية المستخدم بالجلسة (Session) — يُستبدل لاحقاً بنفاذ
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("RoleCode", user.Role!.RoleCode);
            HttpContext.Session.SetString("FullName", user.FullName);


            // تحويل RoleCode إلى الفئة العامة اللي تستخدمها الواجهة (Admin/Technical/Approver)
            string uiRole = user.Role!.RoleCode switch
            {
                "Admin" => "Admin",
                "Technical" => "Technical",
                "PlotApprover" or "PlanApprover" => "Approver",
                _ => "Technical"
            };

            await LogAudit(user.UserId, null, "Login", $"تسجيل دخول: {user.FullName}");

            return RedirectToAction("Index", "Dashboard", new { role = uiRole });
        }








        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }




        // ممكن تنحذف ليه حطينا report_id ممكن تسجيل الدخول ما يكون مربوط ب id معين


        private async Task LogAudit(int userId, int? reportId, string actionType, string? details)
        {
            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = userId,
                ReportId = reportId,
                ActionType = actionType,
                ActionDetails = details,
                ActionDate = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
    }
}
