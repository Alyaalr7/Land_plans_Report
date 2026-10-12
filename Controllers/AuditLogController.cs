using Land_plans_Report.Data;
using Land_plans_Report.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Land_plans_Report.Controllers
{
    // سجل التدقيق — لمدير النظام فقط
    // القاعدة: عمليات المسودات لا تظهر لغير صاحبها، حتى للمدير
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _db;
        public AuditLogController(AppDbContext db) => _db = db;

        private const int MaxRows = 500;

        // GET: /AuditLog?userId=5&period=7
        public async Task<IActionResult> Index(int? userId, string period = "30")
        {
            var me = HttpContext.Session.GetInt32("UserId");
            if (me == null) return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            // مسودات غيري: عملياتها مخفية بالكامل
            var hiddenDraftReportIds = await _db.Reports
                .Where(r => r.Status == "draft" && r.CreatedByUserId != me.Value)
                .Select(r => r.ReportId)
                .ToListAsync();

            var visible = _db.AuditLog
                .Where(a => a.ActionType != "Withdraw")
                .Where(a => a.ReportId == null || !hiddenDraftReportIds.Contains(a.ReportId.Value));

            // المؤشرات (على كل السجل، بدون الفلاتر)
            var today = DateTime.Today;
            var week = today.AddDays(-6);

            var vm = new AuditLogViewModel
            {
                TodayCount = await visible.CountAsync(a => a.ActionDate >= today),
                SubmittedWeek = await visible.CountAsync(a => a.ActionDate >= week && (a.ActionType == "Submit" || a.ActionType == "Resubmit")),
                DecisionsWeek = await visible.CountAsync(a => a.ActionDate >= week && (a.ActionType == "Approve" || a.ActionType == "Return")),
                FailedLoginsWeek = await visible.CountAsync(a => a.ActionDate >= week && a.ActionType == "LoginFailed"),
                UserId = userId,
                Period = period
            };

            // الفلاتر
            var query = visible;
            if (userId != null) query = query.Where(a => a.UserId == userId);

            DateTime? from = period switch
            {
                "today" => today,
                "7" => week,
                "30" => today.AddDays(-29),
                _ => null
            };
            if (from != null) query = query.Where(a => a.ActionDate >= from);

            vm.Rows = await query
                .OrderByDescending(a => a.ActionDate)
                .Take(MaxRows)
                .Select(a => new UserActivityRow
                {

                    ActionType = a.ActionType,
                    Details = a.ActionDetails,
                    Date = a.ActionDate,
                    UserId = a.UserId,
                    ReportCreator = _db.Reports.Where(r => r.ReportId == a.ReportId).Select(r => r.CreatedByUser!.FullName).FirstOrDefault(),
                    RoleName = _db.Users.Where(u => u.UserId == a.UserId).Select(u => u.Role!.RoleName).FirstOrDefault(),
                    UserName = _db.Users.Where(u => u.UserId == a.UserId).Select(u => u.FullName).FirstOrDefault(),
                    ReportId = a.ReportId,
                    ReportNumber = _db.Reports.Where(r => r.ReportId == a.ReportId).Select(r => r.ReportNumber).FirstOrDefault(),
                    ReportType = _db.Reports.Where(r => r.ReportId == a.ReportId).Select(r => r.ReportType).FirstOrDefault()
                })
                .ToListAsync();
            vm.Truncated = vm.Rows.Count == MaxRows;

            var users = await _db.Users.OrderBy(u => u.FullName).Select(u => new { u.UserId, u.FullName }).ToListAsync();
            vm.Users = users.Select(u => (u.UserId, u.FullName)).ToList();
            vm.UserName = users.FirstOrDefault(u => u.UserId == userId)?.FullName;

            return View(vm);
        }
    }
}
