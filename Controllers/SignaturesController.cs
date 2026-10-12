using Land_plans_Report.Data;
using Land_plans_Report.Helpers;
using Land_plans_Report.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Signature = Land_plans_Report.Data.Signature;
namespace Land_plans_Report.Controllers
{
    // التواقيع والأختام (مدير النظام فقط)
    //   - ختم واحد فعّال للجهة، وتوقيع واحد فعّال لكل مستخدم يوقّع
    //   - الرفع الجديد يعطّل القديم ولا يحذفه (التقارير المعتمدة تشير للقديم)
    //   - الصور تنفحص وتنحفظ عن طريق SecureFileStorage (خارج wwwroot)
    public class SignaturesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SignaturesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private bool IsAdmin => HttpContext.Session.GetString("RoleCode") == "Admin";

        // GET: /Signatures — لوحة: حالة الختم + تواقيع المستخدمين
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            var vm = new SignaturesIndexViewModel
            {
                ActiveStamp = await _db.Signatures
                    .FirstOrDefaultAsync(s => s.Kind == SignatureKinds.Stamp && s.IsActive),
                StampHistoryCount = await _db.Signatures
                    .CountAsync(s => s.Kind == SignatureKinds.Stamp && !s.IsActive),
                Users = await _db.Users
                .Include(u => u.Role)
                    .Where(u => u.IsActive && SignatureKinds.SigningRoles.Contains(u.Role!.RoleCode))
                    .OrderBy(u => u.FullName)
                    .ToListAsync(),
                ActiveSignatures = await _db.Signatures
                    .Where(s => s.Kind == SignatureKinds.Signature && s.IsActive)
                    .ToDictionaryAsync(s => s.UserId!.Value)
            };

            return View(vm);
        }

        // POST: /Signatures/UploadStamp — ختم جديد يحل محل الحالي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadStamp(IFormFile? file)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            var error = file == null ? "اختر صورة الختم." : SecureFileStorage.Validate(file, imagesOnly: true);
            if (error != null)
            {
                TempData["SignatureError"] = error;
                return RedirectToAction("Index");
            }

            var path = await SecureFileStorage.SaveAsync(_env, file!, "stamps");
            await ReplaceActiveAsync(SignatureKinds.Stamp, null, path, "StampUploaded", "رفع ختم جديد للجهة");

            TempData["Success"] = "تم رفع الختم وتفعيله";
            return RedirectToAction("Index");
        }

        // POST: /Signatures/UploadSignature — توقيع جديد لمستخدم يحل محل الحالي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSignature(int userId, IFormFile? file, string? returnTo)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            string? error = !SignatureKinds.RoleSigns(user.Role?.RoleCode) ? "دور هذا المستخدم لا يحتاج توقيع."
                          : file == null ? "اختر صورة التوقيع."
                          : SecureFileStorage.Validate(file, imagesOnly: true);

            if (error == null)
            {
                var path = await SecureFileStorage.SaveAsync(_env, file!, $"signatures/user-{userId}");
                await ReplaceActiveAsync(SignatureKinds.Signature, userId, path, "SignatureUploaded", $"رفع توقيع: {user.FullName}");
                TempData["Success"] = $"تم رفع توقيع {user.FullName}";
            }
            else
            {
                TempData[returnTo == "user" ? "UserError" : "SignatureError"] = error;
            }

            return returnTo == "user"
                ? RedirectToAction("Edit", "Users", new { id = userId })
                : RedirectToAction("Index");
        }

        // POST: /Signatures/Deactivate/5 — تعطيل (بدون حذف)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, string? returnTo)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            var item = await _db.Signatures.Include(s => s.User).FirstOrDefaultAsync(s => s.SignatureId == id);
            if (item == null) return NotFound();

            if (item.IsActive)
            {
                item.IsActive = false;
                bool isStamp = item.Kind == SignatureKinds.Stamp;
                _db.AuditLog.Add(new AuditLogEntry
                {
                    UserId = HttpContext.Session.GetInt32("UserId")!.Value,
                    ActionType = isStamp ? "StampDeactivated" : "SignatureDeactivated",
                    ActionDetails = isStamp ? "تعطيل ختم الجهة" : $"تعطيل توقيع: {item.User?.FullName}",
                    ActionDate = DateTime.Now
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = isStamp ? "تم تعطيل الختم" : "تم تعطيل التوقيع";
            }

            return returnTo == "user" && item.UserId != null
                ? RedirectToAction("Edit", "Users", new { id = item.UserId })
                : RedirectToAction("Index");
        }

        // يعطّل الفعّال الحالي (إن وجد) ثم يضيف الجديد — في transaction واحدة
        // الحفظ على مرحلتين لأن الداتابيز يمنع وجود فعّالين اثنين بنفس اللحظة
        private async Task ReplaceActiveAsync(string kind, int? userId, string path, string actionType, string details)
        {
            var adminId = HttpContext.Session.GetInt32("UserId")!.Value;

            using var transaction = await _db.Database.BeginTransactionAsync();

            var current = await _db.Signatures
                .Where(s => s.Kind == kind && s.UserId == userId && s.IsActive)
                .ToListAsync();
            current.ForEach(s => s.IsActive = false);
            await _db.SaveChangesAsync();

            _db.Signatures.Add(new Signature
            {
                Kind = kind,
                UserId = userId,
                FilePath = path,
                IsActive = true,
                UploadedAt = DateTime.Now,
                UploadedByUserId = adminId
            });

            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = adminId,
                ActionType = actionType,
                ActionDetails = details,
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
