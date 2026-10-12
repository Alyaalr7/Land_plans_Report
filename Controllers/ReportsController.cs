using Land_plans_Report.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;              // 🆕 عشان Options و Labels
using Microsoft.AspNetCore.Mvc.ModelBinding;  // 🆕 نحتاجه في الجزء 4 (تجاهل أخطاء المسودة)
namespace Land_plans_Report.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ReportsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }


        // GET: /Reports/Details/5 — شاشة عرض للقراءة فقط، منفصلة تماماً عن شاشة الاعتماد
        // تُستخدم من: منسوب الفني (تقاريره هو)، المعتمِدين (نوعهم)، مدير النظام (الكل)
        public async Task<IActionResult> Details(int id)
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");
            // get the report from db and store in report varebal
            var report = await _db.Reports
                .Include(r => r.PlanDetails)
                .Include(r => r.PlotDetails)
                .Include(r => r.RawLandDetails)
                .Include(r => r.UnplannedLandDetails)
                .Include(r => r.Photos)
                .Include(r => r.Approvals)
                .FirstOrDefaultAsync(r => r.ReportId == id);
            // if there was no report store return error 404 not found 
            if (report == null) return NotFound();

            // ⚠️ المسودة ملك خاص لصاحبها فقط، حتى مدير النظام ما يفتحها مباشرة برابط
            if (report.Status == "draft" && report.CreatedByUserId != userId)
                return RedirectToAction("AccessDenied", "Account");
            bool allowed = roleCode == "Admin"
                || (roleCode == "Technical" && report.CreatedByUserId == userId)
                || (roleCode == "PlotApprover" && report.ReportType != "plan")
                || (roleCode == "PlanApprover" && report.ReportType == "plan");

            if (!allowed) return RedirectToAction("AccessDenied", "Account");

            ViewBag.UserRole = UiRole(roleCode!);
            return View(report);
        }


        //------------------------------------------------------------------------------------------



        // GET: /Reports/Print/5 — نفس منطق صلاحية العرض، بصفحة طباعة مستقلة
        public async Task<IActionResult> Print(int id)
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _db.Reports
                .Include(r => r.PlanDetails)
                .Include(r => r.PlotDetails)
                .Include(r => r.RawLandDetails)
                .Include(r => r.UnplannedLandDetails)
                .Include(r => r.Photos)
                .Include(r => r.CreatedByUser)
                .Include(r => r.Approvals).ThenInclude(a => a.Approver).ThenInclude(u => u!.Role).FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null) return NotFound();

            // ⚠️ المسودة ملك خاص لصاحبها فقط، حتى مدير النظام ما يفتحها مباشرة برابط
            if (report.Status == "draft" && report.CreatedByUserId != userId)
                return RedirectToAction("AccessDenied", "Account");
            bool allowed = roleCode == "Admin"
                || (roleCode == "Technical" && report.CreatedByUserId == userId)
                || (roleCode == "PlotApprover" && report.ReportType != "plan")
                || (roleCode == "PlanApprover" && report.ReportType == "plan");

            if (!allowed) return RedirectToAction("Login", "Account");
            // للتقارير اللي ما انحفظ فيها التوقيع وقت العملية: نعرض التوقيع الفعّال الحالي
            ViewBag.PreparerSignatureId = await _db.Signatures
                .Where(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId == report.CreatedByUserId)
                .Select(s => (int?)s.SignatureId).FirstOrDefaultAsync();

            var lastApproval = report.Approvals.Where(a => a.Decision == "approve").OrderByDescending(a => a.DecisionDate).FirstOrDefault();
            if (lastApproval != null)
            {
                ViewBag.ApproverSignatureId = await _db.Signatures
                    .Where(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId == lastApproval.ApproverUserId)
                    .Select(s => (int?)s.SignatureId).FirstOrDefaultAsync();
                ViewBag.StampId = await _db.Signatures
                    .Where(s => s.Kind == SignatureKinds.Stamp && s.IsActive)
                    .Select(s => (int?)s.SignatureId).FirstOrDefaultAsync();
            }

            return View(report);
        }


        //-------------------------------------------------------------------------------------------------

        // GET: /Reports  — قائمة حقيقية، مفلترة حسب دور المستخدم
        public async Task<IActionResult> Index()
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.UserRole = UiRole(roleCode!);
            ViewBag.ActivePage = "reports";

            var query = _db.Reports.AsQueryable();

            // ⚠️ إصلاح جوهري: المسودة ملك خاص لمن أنشأها فقط، ما تظهر لأي شخص ثاني
            // بأي قائمة عامة بالنظام (حتى مدير النظام والمعتمِدين) — بغض النظر عن دورهم
            query = query.Where(r => r.Status != "draft" || r.CreatedByUserId == userId.Value);

            // منسوب الشؤون الفنية يشوف تقاريره هو فقط
            if (roleCode == "Technical")
                query = query.Where(r => r.CreatedByUserId == userId.Value);

            // مساعد الوكيل يشوف بس تقارير الأراضي (قطعة/خام/بلا مخطط)
            if (roleCode == "PlotApprover")
                query = query.Where(r => r.ReportType != "plan");

            // المدير العام يشوف بس تقارير المخططات
            if (roleCode == "PlanApprover")
                query = query.Where(r => r.ReportType == "plan");

            var reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(reports);
        }


        //--------------------------------------------------------------------------------------------------------------------

        // GET: /Reports/Create?type=plot
        // GET: /Reports/Create?type=plot
        public IActionResult Create(string type = "plan")
        {
            var (userId, _) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");
            if (!ValidTypes.Contains(type)) type = "plan";

            ViewBag.UserRole = "Technical";
            ViewBag.ActivePage = "create";

            return View(new ReportFormViewModel
            {
                ReportType = type,
                Plan = type == "plan" ? new PlanForm() : null,
                Plot = type == "plot" ? new PlotForm() : null,
                RawLand = type == "raw" ? new RawLandForm() : null,
                UnplannedLand = type == "unplanned" ? new UnplannedLandForm() : null
            });
        }
        //------------------------------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportFormViewModel vm, string submitAction)
        {
            var (userId, _) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");
            if (!ValidTypes.Contains(vm.ReportType)) return BadRequest();

            ViewBag.UserRole = "Technical";
            ViewBag.ActivePage = "create";
            bool isSubmit = submitAction == "submit";

            // ── الفالديشن ──
            // الصور إلزامية عند الإرسال فقط (حسب الـ BRD)
            if (isSubmit && (vm.Photos == null || !vm.Photos.Any(f => f.Length > 0)))
                ModelState.AddModelError(nameof(vm.Photos), "أرفق صورة واحدة على الأقل");

            // المسودة: نتجاهل أخطاء الحقول الفاضية، ونبقي أخطاء القيم الغلط
            if (!isSubmit) IgnoreEmptyFieldErrors();

            if (!ModelState.IsValid) return View(vm);

            // ── قاعدة بزنس: لا تقرير مكرر لنفس القطعة (القطعة لسا على الطريقة القديمة) ──
            if (vm.ReportType == "plot")
            {
                var planCode = vm.Plot?.PlanCode;
                var plotNumber = vm.Plot?.PlotNumber;

                bool duplicateExists = await _db.ReportPlotDetails
                    .AnyAsync(d => d.PlanCode == planCode && d.PlotNumber == plotNumber &&
                                  (d.Report!.Status == "pending" || d.Report.Status == "approved"));

                if (duplicateExists)
                {
                    ModelState.AddModelError("", $"يوجد تقرير سابق نشط لنفس القطعة (المخطط {planCode}، القطعة {plotNumber}).");
                    return View(vm);
                }
            }

            // ── الحفظ ──
            using var transaction = await _db.Database.BeginTransactionAsync();

            var report = new Report
            {
                ReportNumber = await GenerateReportNumberAsync(),
                ReportType = vm.ReportType,
                CreatedByUserId = userId.Value,
                Status = isSubmit ? "pending" : "draft",
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                GpsAccuracyM = vm.GpsAccuracyM,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now,
                SubmittedAt = isSubmit ? DateTime.Now : null
            };

            _db.Reports.Add(report);
            await _db.SaveChangesAsync(); // نحتاج ReportId قبل إضافة التفاصيل

            switch (vm.ReportType)
            {
                case "plan":
                    var planDetails = new ReportPlanDetails { ReportId = report.ReportId };
                    (vm.Plan ?? new PlanForm()).ApplyTo(planDetails);
                    _db.ReportPlanDetails.Add(planDetails);
                    break;
                case "plot":
                    var plotDetails = new ReportPlotDetails { ReportId = report.ReportId };
                    (vm.Plot ?? new PlotForm()).ApplyTo(plotDetails);
                    _db.ReportPlotDetails.Add(plotDetails);
                    break;
                case "raw":
                    var rawDetails = new ReportRawLandDetails { ReportId = report.ReportId };
                    (vm.RawLand ?? new RawLandForm()).ApplyTo(rawDetails);
                    _db.ReportRawLandDetails.Add(rawDetails);
                    break;
                case "unplanned":
                    var unplannedDetails = new ReportUnplannedLandDetails { ReportId = report.ReportId };
                    (vm.UnplannedLand ?? new UnplannedLandForm()).ApplyTo(unplannedDetails);
                    _db.ReportUnplannedLandDetails.Add(unplannedDetails);
                    break;

                    // ↓↓↓ الثلاثة الباقية: انسخي كما هي من كودك القديم (case "plot" و "raw" و "unplanned")
                    //     مع حذف سطر TransferNumber من plot فقط
            }

            // ── الصور ──
            if (vm.Photos != null && vm.Photos.Any(f => f.Length > 0))
            {

                var (validPhotos, photoError) = ValidateAndPreparePhotos(vm.Photos);
                if (photoError != null)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(nameof(vm.Photos), photoError);
                    return View(vm);
                }

                foreach (var file in validPhotos)
                {
                    // التخزين خارج wwwroot باسم عشوائي (التفاصيل في SecureFileStorage)
                    var storedPath = await SecureFileStorage.SaveAsync(_env, file, $"reports/{report.ReportNumber}");

                    _db.ReportPhotos.Add(new ReportPhoto
                    {
                        ReportId = report.ReportId,
                        FilePath = storedPath,
                        FileSizeKB = (int)(file.Length / 1024),
                        UploadedAt = DateTime.Now
                    });
                }
            }

            // ── سجل التدقيق ──
            _db.AuditLog.Add(new AuditLogEntry
            {
                ReportId = report.ReportId,
                UserId = userId.Value,
                ActionType = isSubmit ? "Submit" : "Create",
                ActionDetails = $"إنشاء تقرير {report.ReportNumber} ({vm.ReportType})",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = isSubmit ? "تم إرسال التقرير للاعتماد" : "تم حفظ التقرير كمسودة";

            return RedirectToAction("Index");
        }


        // ═══════════════════ دوال مساعدة (أضيفيها مع الباقي) ═══════════════════

        private static readonly HashSet<string> ValidTypes = new() { "plan", "plot", "raw", "unplanned" };

        // المسودة: أي خطأ على حقل فاضي = Required فقط (قيود الشكل ما تعطي خطأ على الفاضي)
        // فنمسحه، وتبقى أخطاء القيم الغلط زي ما هي
        private void IgnoreEmptyFieldErrors()
        {
            foreach (var entry in ModelState.Values)
            {
                if (entry.Errors.Count > 0 && string.IsNullOrWhiteSpace(entry.AttemptedValue))
                {
                    entry.Errors.Clear();
                    entry.ValidationState = ModelValidationState.Valid; // لازم، وإلا IsValid يظل false
                }
            }
        }




        //----------------------------------------------------------------------------------------------------------------


        // GET: /Reports/Edit/5 — يعيد فتح تقرير "مسودة" أو "معاد للتعديل" فقط، لصاحبه حصراً




        // GET: /Reports/Edit/5 — يعيد فتح تقرير "مسودة" أو "معاد للتعديل" فقط، لصاحبه حصراً
        public async Task<IActionResult> Edit(int id)
        {
            var (userId, _) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");
            var report = await LoadReportForEditAsync(id);
            if (report == null) return NotFound();
            if (report.CreatedByUserId != userId) return RedirectToAction("AccessDenied", "Account");
            if (!IsEditable(report)) return RedirectToAction("Details", new { id });

            ViewBag.UserRole = "Technical";

            var vm = new ReportFormViewModel
            {
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                GpsAccuracyM = report.GpsAccuracyM,
                Notes = report.Notes,
                Plan = report.PlanDetails != null ? PlanForm.FromEntity(report.PlanDetails) : null,
                Plot = report.PlotDetails != null ? PlotForm.FromEntity(report.PlotDetails) : null,
                RawLand = report.RawLandDetails != null ? RawLandForm.FromEntity(report.RawLandDetails) : null,
                UnplannedLand = report.UnplannedLandDetails != null ? UnplannedLandForm.FromEntity(report.UnplannedLandDetails) : null
            };
            FillFromReport(vm, report);

            return View(vm);
        }

        //-------------------------------------------------------------------------------------------------
        // POST: /Reports/Edit/5 — تحديث فعلي (UPDATE) لنفس الصف، وليس إنشاء صف جديد
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, ReportFormViewModel vm, string submitAction)
        {
            var (userId, _) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await LoadReportForEditAsync(id);
            if (report == null) return NotFound();
            if (report.CreatedByUserId != userId) return RedirectToAction("AccessDenied", "Account");
            if (!IsEditable(report)) return RedirectToAction("Details", new { id });

            ViewBag.UserRole = "Technical";
            FillFromReport(vm, report);
            bool isSubmit = submitAction == "submit";

            // ── الفالديشن (نفس Create) ──
            if (!isSubmit) IgnoreEmptyFieldErrors();

            if (isSubmit && report.Photos.Count == 0 && (vm.Photos == null || !vm.Photos.Any(f => f.Length > 0)))
                ModelState.AddModelError(nameof(vm.Photos), "أرفق صورة واحدة على الأقل");

            var validPhotos = new List<IFormFile>();
            if (vm.Photos != null && vm.Photos.Any(f => f.Length > 0))
            {
                var (valid, photoError) = ValidateAndPreparePhotos(vm.Photos);
                if (photoError != null) ModelState.AddModelError(nameof(vm.Photos), photoError);
                validPhotos = valid;
            }

            if (!ModelState.IsValid) return View(vm);

            // ── نسخ القيم ──
            var previousStatus = report.Status;

            report.Latitude = vm.Latitude;
            report.Longitude = vm.Longitude;
            report.GpsAccuracyM = vm.GpsAccuracyM;
            report.Notes = vm.Notes;
            report.Status = isSubmit ? "pending" : report.Status;   // الحفظ يخلي الحالة زي ما هي (مسودة أو معاد للتعديل)
            report.UpdatedAt = DateTime.Now;
            if (isSubmit) report.SubmittedAt = DateTime.Now;

            switch (report.ReportType)
            {
                case "plan":
                    report.PlanDetails ??= new ReportPlanDetails { ReportId = report.ReportId };
                    (vm.Plan ?? new PlanForm()).ApplyTo(report.PlanDetails);
                    break;
                case "plot":
                    report.PlotDetails ??= new ReportPlotDetails { ReportId = report.ReportId };
                    (vm.Plot ?? new PlotForm()).ApplyTo(report.PlotDetails);
                    break;
                case "raw":
                    report.RawLandDetails ??= new ReportRawLandDetails { ReportId = report.ReportId };
                    (vm.RawLand ?? new RawLandForm()).ApplyTo(report.RawLandDetails);
                    break;
                case "unplanned":
                    report.UnplannedLandDetails ??= new ReportUnplannedLandDetails { ReportId = report.ReportId };
                    (vm.UnplannedLand ?? new UnplannedLandForm()).ApplyTo(report.UnplannedLandDetails);
                    break;

                    // ↓↓↓ الصقي هنا cases "plot" و "raw" و "unplanned" من Edit القديم حقك كما هي
                    //     (بدون سطر TransferNumber)
            }

            // ── الصور (نفس Create) ──
            if (validPhotos.Count > 0)
            {
                foreach (var file in validPhotos)
                {
                    var storedPath = await SecureFileStorage.SaveAsync(_env, file, $"reports/{report.ReportNumber}");

                    _db.ReportPhotos.Add(new ReportPhoto
                    {
                        ReportId = report.ReportId,
                        FilePath = storedPath,
                        FileSizeKB = (int)(file.Length / 1024),
                        UploadedAt = DateTime.Now
                    });
                }
            }

            _db.AuditLog.Add(new AuditLogEntry
            {
                ReportId = report.ReportId,
                UserId = userId.Value,
                ActionType = isSubmit ? "Resubmit" : "Update",
                ActionDetails = $"تعديل التقرير {report.ReportNumber} (كان: {previousStatus})",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = isSubmit ? "تم إرسال التقرير للاعتماد" : "تم حفظ التعديلات";
            return RedirectToAction("Details", new { id });
        }







        //--------------------------------------------------------------------------------------------------
        // ============ دوال مساعدة ============

        // POST: /Reports/Withdraw/5 — سحب تقرير "بانتظار الاعتماد" ليرجع مسودة، لصاحبه فقط
        // يحل سيناريو "أرسلت غلط، أبيها أرجعها قبل ما يشوفها المعتمِد"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var (userId, roleCode) = CurrentUser();
            if (userId == null) return RedirectToAction("Login", "Account");

            var report = await _db.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
            if (report == null) return NotFound();

            if (roleCode != "Technical" || report.CreatedByUserId != userId)
                return RedirectToAction("AccessDenied", "Account");
            if (report.CreatedByUserId != userId) return RedirectToAction("Login", "Account");
            if (report.Status != "pending")
                return RedirectToAction("Details", new { id });

            report.Status = "draft";
            report.UpdatedAt = DateTime.Now;

            _db.AuditLog.Add(new AuditLogEntry
            {
                ReportId = report.ReportId,
                UserId = userId.Value,
                ActionType = "Withdraw",
                ActionDetails = $"سحب التقرير {report.ReportNumber} قبل الاعتماد للتعديل",
                ActionDate = DateTime.Now
            });
            TempData["Success"] = "تم سحب التقرير وإرجاعه مسودة";
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }
        //-------------------------------------------------------------------------

        private (int? userId, string? roleCode) CurrentUser()
        {
            return (
                HttpContext.Session.GetInt32("UserId"),
                HttpContext.Session.GetString("RoleCode")
            );
        }
        //----------------------------------------------------------------
        private IActionResult Denied() => RedirectToAction("AccessDenied", "Account");
        //------------------------------------------------------------------------
        private static string UiRole(string roleCode) => roleCode switch
        {
            "Admin" => "Admin",
            "Technical" => "Technical",
            "PlotApprover" or "PlanApprover" => "Approver",
            _ => "Technical"
        };
        //----------------------------------------------------------------------

        private static string FieldPrefix(string reportType) => reportType switch
        {
            "plan" => "Plan",
            "plot" => "Plot",
            "raw" => "Raw",
            "unplanned" => "Unplanned",
            _ => ""
        };
        //---------------------------------------------------------------------------------------------------
        // فحص الصور: العدد هنا، والنوع الحقيقي والحجم في SecureFileStorage.Validate
        private const int MaxPhotoCount = 3;
        private (List<IFormFile> valid, string? error) ValidateAndPreparePhotos(List<IFormFile> photos)
        {
            var valid = new List<IFormFile>();
            if (photos.Count(f => f.Length > 0) > MaxPhotoCount)
                return (valid, $"الحد الأقصى {MaxPhotoCount} ملفات.");
            foreach (var file in photos.Where(f => f.Length > 0))
            {
                var error = SecureFileStorage.Validate(file);
                if (error != null) return (valid, error);
                valid.Add(file);
            }
            return (valid, null);
        }

        //-------------------------------------------------------------------------------
        private static decimal? ParseDecimal(string? value) =>
            decimal.TryParse(value, out var d) ? d : null;
        //---------------------------------------------------------------
        // توليد رقم تقرير تلقائي فريد — كانت فجوة: لم يكن هناك أي منطق لهذا سابقاً
        private async Task<string> GenerateReportNumberAsync()
        {
            var year = DateTime.Now.Year;
            var countThisYear = await _db.Reports.CountAsync(r => r.CreatedAt.Year == year);
            var nextNumber = countThisYear + 1;
            return $"TR-{year}-{nextNumber:D6}";
        }
        private Task<Report?> LoadReportForEditAsync(int id) => _db.Reports
    .Include(r => r.PlanDetails).Include(r => r.PlotDetails)
    .Include(r => r.RawLandDetails).Include(r => r.UnplannedLandDetails)
    .Include(r => r.Photos).Include(r => r.Approvals)
    .FirstOrDefaultAsync(r => r.ReportId == id);

        private static bool IsEditable(Report report) =>
            report.Status == "draft" || report.Status == "returned";

        private static void FillFromReport(ReportFormViewModel vm, Report report)
        {
            vm.ReportId = report.ReportId;
            vm.ReportNumber = report.ReportNumber;
            vm.ReportType = report.ReportType;
            vm.Status = report.Status;
            vm.ExistingPhotoCount = report.Photos.Count;
            vm.LastReturnNote = report.Approvals
                .Where(a => a.Decision == "return")
                .OrderByDescending(a => a.DecisionDate)
                .FirstOrDefault()?.ApproverNotes;

        }
    }
}
