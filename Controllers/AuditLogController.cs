using Land_plans_Report.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Land_plans_Report.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _db;
        public AuditLogController(AppDbContext db) => _db = db;

        // GET: /AuditLog — سجل مخصص لكل دور، يعرض بس ما يخصه ويهمه
        //
        // القاعدة الأهم: أي عملية مرتبطة بتقرير لسه "مسودة" لا تظهر لأي شخص
        // غير من أنشأ التقرير نفسه — حتى مدير النظام، بلا استثناء
        //
        // "الأشياء المهمة" لكل دور (مو Dump خام لكل شي):
        //   - منسوب فني: عمليات لها علاقة بتقاريره هو فقط (إرسال، رد، اعتماد نهائي)
        //   - معتمِد: قراراته هو فقط (موافقة/إعادة) — بدون ضجيج تسجيل دخول أو إدارة مستخدمين
        //   - مدير النظام: كل شي إداري ونظامي، ما عدا تفاصيل المسودات الخاصة بغيره
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roleCode = HttpContext.Session.GetString("RoleCode");
            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.UserRole = roleCode == "Admin" ? "Admin" : (roleCode == "Technical" ? "Technical" : "Approver");

            // معرّفات التقارير التي ما زالت "مسودة" لغير المستخدم الحالي — تُستبعد أحداثها بالكامل
            var hiddenDraftReportIds = await _db.Reports
                .Where(r => r.Status == "draft" && r.CreatedByUserId != userId.Value)
                .Select(r => r.ReportId)
                .ToListAsync();

            var query = _db.AuditLog
                .Where(a => a.ReportId == null || !hiddenDraftReportIds.Contains(a.ReportId.Value));

            if (roleCode == "Admin")
            {
                // مدير النظام: كل شي إداري ونظامي (ما عدا المسودات الخاصة بغيره، مُستبعدة أعلاه)
                // بدون أحداث "Login" الروتينية كي لا يغرق السجل بضجيج غير مفيد إدارياً
                query = query.Where(a => a.ActionType != "Login");
            }
            else if (roleCode == "Technical")
            {
                // منسوب فني: بس الأحداث المرتبطة بتقاريره هو (إرسال، رد عليه، اعتماد نهائي)
                var myReportIds = await _db.Reports
                    .Where(r => r.CreatedByUserId == userId.Value)
                    .Select(r => r.ReportId)
                    .ToListAsync();
                query = query.Where(a => a.ReportId != null && myReportIds.Contains(a.ReportId.Value)
                    && (a.ActionType == "Submit" || a.ActionType == "Approve" || a.ActionType == "Return"));
            }
            else // PlotApprover / PlanApprover
            {
                // معتمِد: قراراته هو فقط، بدون أي عمليات إدارية أو عمليات مستخدمين آخرين
                query = query.Where(a => a.UserId == userId.Value
                    && (a.ActionType == "Approve" || a.ActionType == "Return"));
            }

            var entries = await query
                .OrderByDescending(a => a.ActionDate)
                .Take(200)
                .ToListAsync();

            var userIds = entries.Select(e => e.UserId).Distinct().ToList();
            var userNames = await _db.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName);

            var reportIds = entries.Where(e => e.ReportId.HasValue).Select(e => e.ReportId!.Value).Distinct().ToList();
            var reportNumbers = await _db.Reports
                .Where(r => reportIds.Contains(r.ReportId))
                .ToDictionaryAsync(r => r.ReportId, r => r.ReportNumber);

            ViewBag.UserNames = userNames;
            ViewBag.ReportNumbers = reportNumbers;
            return View(entries);
        }
    }
}
