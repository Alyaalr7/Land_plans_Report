using Microsoft.AspNetCore.Mvc;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;
using Microsoft.EntityFrameworkCore;
namespace Land_plans_Report.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly AppDbContext _db;
        public ApprovalController(AppDbContext db) => _db = db;

        // GET: /Approval  — قائمة التقارير بانتظار اعتماد المستخدم الحالي
        // كانت فجوة: هذي الشاشة لم تكن موجودة أصلاً، فقط شاشة مراجعة تقرير واحد
        public async Task<IActionResult> Index()
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");
            if (roleCode != "PlanApprover" && roleCode != "PlotApprover")
                return RedirectToAction("AccessDenied", "Account");


            ViewBag.UserRole = "Approver";

            var query = _db.Reports.Include(r => r.CreatedByUser).Where(r => r.Status == "pending");

            // ⚠️ إعادة هيكلة: أنواع التقارير المسموحة تُقرأ من ReportTypeApprovalRoles
            // (بيانات) بدل شرط if/else مكتوب بالكود — مدير النظام يقدر يضيف/يغيّر
            // التوجيه من الواجهة مستقبلاً بدون أي تعديل برمجي
            if (roleCode == "PlanApprover")
                query = query.Where(r => r.ReportType == "plan");   // معتمد المخططات يشوف المخططات
            else
                query = query.Where(r => r.ReportType != "plan");   // معتمد القطع يشوف الباقي

            var pending = await query.OrderBy(r => r.CreatedAt).ToListAsync();
            return View(pending);
        }





        // GET: /Approval/Review/5
        public async Task<IActionResult> Review(int id)
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _db.Reports
                .Include(r => r.PlotDetails)
                .Include(r => r.PlanDetails)
                .Include(r => r.RawLandDetails)
                .Include(r => r.UnplannedLandDetails)
                .Include(r => r.Photos)
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            // نفس قاعدة الصلاحية: يُمنع دور من مراجعة نوع لا يخصه
            if (!CanApprove(report.ReportType))
                return RedirectToAction("AccessDenied", "Account");
            // ⚠️ إصلاح جوهري: تقرير خلص قراره (معتمد أو معاد) ما يفتح كشاشة "اتخاذ قرار" مرة ثانية
            // يمنع التعديل المتكرر على قرار سابق — يُحوَّل تلقائياً لشاشة العرض للقراءة فقط
            if (report.Status != "pending")
                return RedirectToAction("Details", "Reports", new { id = report.ReportId });

            ViewBag.UserRole = "Approver";

            // معاينة توقيعي والختم قبل الاعتماد
            ViewBag.MySignatureId = await ActiveSignatureIdAsync(userId.Value);
            ViewBag.StampId = await ActiveStampIdAsync();
            return View(report);
        }




        // POST: /Approval/Submit
        // كانت فجوة: لم يكن هناك أي INSERT في ReportApprovals ولا تحديث لحالة التقرير
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int reportId, string action, string? approverNotes)
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _db.Reports.FirstOrDefaultAsync(r => r.ReportId == reportId);
            if (report == null) return NotFound();

            // ⚠️ تحقق الصلاحية الحرج (كان مفقوداً بالكامل): يمنع اعتماد نوع لا يخص الدور
            if (!CanApprove(report.ReportType))
                return RedirectToAction("AccessDenied", "Account");
            // منع اتخاذ قرار ثانٍ على تقرير خلص قراره أصلاً
            if (report.Status != "pending")
                return RedirectToAction("Details", "Reports", new { id = report.ReportId });

            var decision = action == "approve" ? "approve" : "return";

            // ⚠️ إصلاح: الملاحظات إلزامية عند "إعادة للتعديل" فقط، اختيارية عند "موافقة"
            if (decision == "return" && string.IsNullOrWhiteSpace(approverNotes))
            {
                TempData["ApprovalError"] = "توضيح سبب الإعادة إلزامي عند اختيار (إعادة للتعديل).";
                return RedirectToAction("Review", new { id = reportId });
            }

            // الاعتماد يحتاج توقيع المعتمد وختم الجهة (الإعادة ما تحتاجهم)
            int? signatureId = null, stampId = null;
            if (decision == "approve")
            {
                signatureId = await ActiveSignatureIdAsync(userId.Value);
                stampId = await ActiveStampIdAsync();
                if (signatureId == null || stampId == null)
                {
                    TempData["ApprovalError"] = signatureId == null
                        ? "لا يمكن الاعتماد بدون توقيع مفعّل. تواصل مع مدير النظام."
                        : "لا يمكن الاعتماد بدون ختم مفعّل للجهة. تواصل مع مدير النظام.";
                    return RedirectToAction("Review", new { id = reportId });
                }
            }

            _db.ReportApprovals.Add(new ReportApproval
            {
                ReportId = report.ReportId,
                ApproverUserId = userId.Value,
                Decision = decision,
                ApproverNotes = approverNotes,
                DecisionDate = DateTime.Now,
                SignatureId = signatureId,   // نسخة ثابتة وقت القرار
                StampId = stampId
            });

            report.Status = decision == "approve" ? "approved" : "returned";
            report.UpdatedAt = DateTime.Now;

            _db.AuditLog.Add(new AuditLogEntry
            {
                ReportId = report.ReportId,
                UserId = userId.Value,
                ActionType = decision == "approve" ? "Approve" : "Return",
                ActionDetails = approverNotes,
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = decision == "approve"
                ? $"تم اعتماد التقرير {report.ReportNumber}"
                : $"تمت إعادة التقرير {report.ReportNumber} للتعديل";
            return RedirectToAction("Index");
        }

        private Task<int?> ActiveSignatureIdAsync(int userId) => _db.Signatures
            .Where(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId == userId)
            .Select(s => (int?)s.SignatureId)
            .FirstOrDefaultAsync();

        private Task<int?> ActiveStampIdAsync() => _db.Signatures
            .Where(s => s.Kind == SignatureKinds.Stamp && s.IsActive)
            .Select(s => (int?)s.SignatureId)
            .FirstOrDefaultAsync();

        private (int? userId, string? roleCode) CurrentUser() => (
            HttpContext.Session.GetInt32("UserId"),
            HttpContext.Session.GetString("RoleCode")
        );



        // مين يعتمد وش: معتمد المخططات = المخططات، معتمد القطع = الباقي
        private bool CanApprove(string reportType)
        {
            var roleCode = HttpContext.Session.GetString("RoleCode");
            return (roleCode == "PlanApprover" && reportType == "plan")
                || (roleCode == "PlotApprover" && reportType != "plan");
        }

        // ⚠️ إعادة هيكلة: بدل شرط switch مكتوب بالكود، نقرأ القاعدة من جدول
        // ReportTypeApprovalRoles — مدير النظام يعدّلها من الواجهة مستقبلاً
        // بدون أي حاجة لتعديل الكود أو إعادة نشر التطبيق

    }
}
