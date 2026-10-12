using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;
using Land_plans_Report.Models;


namespace Land_plans_Report.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        private const int LateDays = 5;   // التقرير يعتبر متأخر بعد هذي المدة بانتظار الاعتماد

        private static readonly string[] MonthNames =
            { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

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

                ViewBag.Admin = await BuildAdminAsync();
            }
            else if (roleCode == "PlotApprover" || roleCode == "PlanApprover")
            {
                var pendingQuery = _db.Reports.Where(r => r.Status == "pending");
                pendingQuery = roleCode == "PlotApprover"
                    ? pendingQuery.Where(r => r.ReportType != "plan")
                    : pendingQuery.Where(r => r.ReportType == "plan");
                ViewBag.PendingList = await pendingQuery
                    .OrderBy(r => r.SubmittedAt).Take(5).ToListAsync();

                ViewBag.Approver = await BuildApproverAsync(userId.Value, roleCode);
            }
            else if (roleCode == "Technical")
            {
                ViewBag.DraftsList = await reportsQuery.Where(r => r.Status == "draft")
                    .OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync();
            }

            return View(recent);
        }

        // ── بيانات العرض فقط (قراءة من الداتابيس، بدون أي تعديل) ──

        private static DateTime MonthStart(DateTime d) => new DateTime(d.Year, d.Month, 1);

        // آخر 6 أشهر: عدد التواريخ في كل شهر لسلسلتين
        private static List<MonthPoint> BuildMonths(List<DateTime> first, List<DateTime> second)
        {
            var start = MonthStart(DateTime.Today).AddMonths(-5);
            return Enumerable.Range(0, 6).Select(i =>
            {
                var m = start.AddMonths(i);
                return new MonthPoint
                {
                    Label = MonthNames[m.Month - 1],
                    First = first.Count(d => MonthStart(d) == m),
                    Second = second.Count(d => MonthStart(d) == m)
                };
            }).ToList();
        }

        private static double? AverageDays(IEnumerable<(DateTime From, DateTime To)> spans)
        {
            var valid = spans.Where(s => s.To >= s.From).Select(s => (s.To - s.From).TotalDays).ToList();
            return valid.Count == 0 ? null : Math.Round(valid.Average(), 1);
        }

        private async Task<AdminDashboardData> BuildAdminAsync()
        {
            var now = DateTime.Now;
            var monthStart = MonthStart(DateTime.Today);
            var chartStart = monthStart.AddMonths(-5);
            var lateBefore = now.AddDays(-LateDays);

            // مدير النظام ما عنده مسودات، فالمحسوب هو المرسل فقط
            var sent = _db.Reports.Where(r => r.Status != "draft");
            var lateQuery = sent.Where(r => r.Status == "pending" && r.SubmittedAt <= lateBefore);

            var d = new AdminDashboardData
            {
                Sent = await sent.CountAsync(),
                SentThisMonth = await sent.CountAsync(r => r.SubmittedAt >= monthStart),
                Pending = await sent.CountAsync(r => r.Status == "pending"),
                Approved = await sent.CountAsync(r => r.Status == "approved"),
                Returned = await sent.CountAsync(r => r.Status == "returned"),
                LateCount = await lateQuery.CountAsync(),
                HasActiveStamp = await _db.Signatures.AnyAsync(s => s.Kind == SignatureKinds.Stamp && s.IsActive)
            };

            // متوسط مدة الاعتماد (من الإرسال إلى الاعتماد)
            var spans = await _db.ReportApprovals
                .Where(a => a.Decision == "approve" && a.Report!.SubmittedAt != null)
                .Select(a => new { From = a.Report!.SubmittedAt!.Value, To = a.DecisionDate })
                .ToListAsync();
            d.AvgApprovalDays = AverageDays(spans.Select(x => (x.From, x.To)));

            // حركة التقارير: مرسلة / معتمدة
            var sentDates = await sent.Where(r => r.SubmittedAt >= chartStart).Select(r => r.SubmittedAt!.Value).ToListAsync();
            var approvedDates = await _db.ReportApprovals
                .Where(a => a.Decision == "approve" && a.DecisionDate >= chartStart)
                .Select(a => a.DecisionDate).ToListAsync();
            d.Months = BuildMonths(sentDates, approvedDates);

            // حسب النوع
            var byType = await sent.GroupBy(r => r.ReportType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();
            d.ByType = byType.OrderByDescending(x => x.Count).Select(x => (x.Type, x.Count)).ToList();

            // جاهزية التوقيع
            var signers = await _db.Users
                .Where(u => u.IsActive && SignatureKinds.SigningRoles.Contains(u.Role!.RoleCode))
                .Select(u => new MissingSignature { UserId = u.UserId, FullName = u.FullName, RoleName = u.Role!.RoleName })
                .ToListAsync();
            var withSignature = await _db.Signatures
                .Where(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId != null)
                .Select(s => s.UserId!.Value)
                .ToListAsync();
            d.SigningUsers = signers.Count;
            d.MissingSignatures = signers.Where(u => !withSignature.Contains(u.UserId)).OrderBy(u => u.FullName).ToList();

            // المتأخرة
            var late = await lateQuery
                .OrderBy(r => r.SubmittedAt).Take(5)
                .Select(r => new { r.ReportId, r.ReportNumber, r.ReportType, r.SubmittedAt })
                .ToListAsync();
            d.Late = late.Select(r => new WaitingReport
            {
                ReportId = r.ReportId,
                ReportNumber = r.ReportNumber,
                ReportType = r.ReportType,
                Days = (int)(now - r.SubmittedAt!.Value).TotalDays
            }).ToList();

            d.ApproverRoleNames = await _db.Roles
                .Where(r => r.RoleCode == "PlanApprover" || r.RoleCode == "PlotApprover")
                .ToDictionaryAsync(r => r.RoleCode, r => r.RoleName);

            return d;
        }

        private async Task<ApproverDashboardData> BuildApproverAsync(int me, string roleCode)
        {
            var now = DateTime.Now;
            var monthStart = MonthStart(DateTime.Today);
            var chartStart = monthStart.AddMonths(-5);

            var pending = _db.Reports.Where(r => r.Status == "pending");
            pending = roleCode == "PlotApprover"
                ? pending.Where(r => r.ReportType != "plan")
                : pending.Where(r => r.ReportType == "plan");

            var mine = _db.ReportApprovals.Where(a => a.ApproverUserId == me);

            var d = new ApproverDashboardData
            {
                Pending = await pending.CountAsync(),
                ApprovedThisMonth = await mine.CountAsync(a => a.Decision == "approve" && a.DecisionDate >= monthStart),
                ReturnedThisMonth = await mine.CountAsync(a => a.Decision == "return" && a.DecisionDate >= monthStart),
                HasSignature = await _db.Signatures.AnyAsync(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId == me),
                HasActiveStamp = await _db.Signatures.AnyAsync(s => s.Kind == SignatureKinds.Stamp && s.IsActive)
            };

            // متوسط مدة قراري (من الإرسال إلى القرار)
            var spans = await mine
                .Where(a => a.Report!.SubmittedAt != null)
                .Select(a => new { From = a.Report!.SubmittedAt!.Value, To = a.DecisionDate })
                .ToListAsync();
            d.AvgDecisionDays = AverageDays(spans.Select(x => (x.From, x.To)));

            // بانتظاري: الأقدم أول
            var waiting = await pending
                .OrderBy(r => r.SubmittedAt).Take(6)
                .Select(r => new { r.ReportId, r.ReportNumber, r.ReportType, r.SubmittedAt, Preparer = r.CreatedByUser!.FullName })
                .ToListAsync();
            d.Waiting = waiting.Select(r => new WaitingReport
            {
                ReportId = r.ReportId,
                ReportNumber = r.ReportNumber,
                ReportType = r.ReportType,
                Preparer = r.Preparer,
                Days = r.SubmittedAt == null ? 0 : (int)(now - r.SubmittedAt.Value).TotalDays
            }).ToList();

            // قراراتي: اعتماد / إعادة
            var decisions = await mine
                .Where(a => a.DecisionDate >= chartStart)
                .Select(a => new { a.Decision, a.DecisionDate })
                .ToListAsync();
            d.Months = BuildMonths(
                decisions.Where(x => x.Decision == "approve").Select(x => x.DecisionDate).ToList(),
                decisions.Where(x => x.Decision == "return").Select(x => x.DecisionDate).ToList());

            return d;
        }
    }
}
