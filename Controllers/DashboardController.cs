using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Land_plans_Report.Data;


namespace Land_plans_Report.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roleCode = HttpContext.Session.GetString("RoleCode");

            if (userId == null || roleCode == null) return RedirectToAction("Login", "Account");

            ViewBag.UserRole = roleCode switch
            {
                "Admin" => "Admin",
                "Technical" => "Technical",
                _ => "Approver"
            };
            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            var reportsQuery = _db.Reports.AsQueryable();

            // ⚠️ نفس القاعدة: المسودة تظهر فقط لصاحبها، حتى بلوحة تحكم مدير النظام
            reportsQuery = reportsQuery.Where(r => r.Status != "draft" || r.CreatedByUserId == userId.Value);

            if (roleCode == "Technical")
                reportsQuery = reportsQuery.Where(r => r.CreatedByUserId == userId.Value);
            else if (roleCode == "PlotApprover")
                reportsQuery = reportsQuery.Where(r => r.ReportType != "plan");
            else if (roleCode == "PlanApprover")
                reportsQuery = reportsQuery.Where(r => r.ReportType == "plan");

            ViewBag.CountApproved = await reportsQuery.CountAsync(r => r.Status == "approved");
            ViewBag.CountDraft = await reportsQuery.CountAsync(r => r.Status == "draft");
            ViewBag.CountReturned = await reportsQuery.CountAsync(r => r.Status == "returned");
            ViewBag.CountPending = await reportsQuery.CountAsync(r => r.Status == "pending");

            var recent = await reportsQuery
                .OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync();

            // بيانات إضافية خاصة بكل دور (لوحة مختلفة فعلياً، مو بس أرقام مختلفة)
            if (roleCode == "Admin")
            {
                ViewBag.TotalUsers = await _db.Users.CountAsync(u => u.IsActive);
                ViewBag.RecentAudit = await _db.AuditLog
                    .OrderByDescending(a => a.ActionDate).Take(5).ToListAsync();
                var auditUserIds = ViewBag.RecentAudit is List<AuditLogEntry> ral ? ral.Select(a => a.UserId).Distinct().ToList() : new List<int>();
                ViewBag.AuditUserNames = await _db.Users.Where(u => auditUserIds.Contains(u.UserId))
                    .ToDictionaryAsync(u => u.UserId, u => u.FullName);
            }
            else if (roleCode == "PlotApprover" || roleCode == "PlanApprover")
            {
                var pendingQuery = _db.Reports.Where(r => r.Status == "pending");
                pendingQuery = roleCode == "PlotApprover"
                    ? pendingQuery.Where(r => r.ReportType != "plan")
                    : pendingQuery.Where(r => r.ReportType == "plan");
                ViewBag.PendingList = await pendingQuery
                    .OrderBy(r => r.SubmittedAt).Take(5).ToListAsync();
            }
            else if (roleCode == "Technical")
            {
                ViewBag.DraftsList = await reportsQuery.Where(r => r.Status == "draft")
                    .OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync();
            }

            return View(recent);
        }
    }
}
