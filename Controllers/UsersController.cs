using System.Text.RegularExpressions;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;
using Land_plans_Report.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Land_plans_Report.Controllers
{
    // إدارة المستخدمين (مدير النظام فقط)
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        private bool IsAdmin => HttpContext.Session.GetString("RoleCode") == "Admin";
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // GET: /Users — لوحة المستخدمين
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("RoleCode") == null) return RedirectToAction("Login", "Account");
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            var users = await _db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // عرض فقط: الأدوار (للمؤشر وقسم الأدوار) + آخر دخول لكل مستخدم
            ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleId).ToListAsync();
            ViewBag.LastLogins = await _db.AuditLog
                .Where(a => a.ActionType == "Login")
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Last = g.Max(a => a.ActionDate) })
                .ToDictionaryAsync(x => x.UserId, x => x.Last);

            return View(users);
        }

        // GET: /Users/Details/5 — ملف المستخدم
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

            var since = DateTime.Now.AddDays(-30);
            var logins = _db.AuditLog.Where(a => a.UserId == id && a.ActionType == "Login");

            var vm = new UserProfileViewModel
            {
                User = user,
                LastLogin = await logins.MaxAsync(a => (DateTime?)a.ActionDate),
                LoginCount = await logins.CountAsync(),
                FailedLogins30Days = await _db.AuditLog.CountAsync(a => a.UserId == id && a.ActionType == "LoginFailed" && a.ActionDate >= since),
                NeedsSignature = SignatureKinds.RoleSigns(user.Role?.RoleCode),
                Signature = await _db.Signatures.FirstOrDefaultAsync(s => s.Kind == SignatureKinds.Signature && s.IsActive && s.UserId == id),
                Activity = await _db.AuditLog
                    .Where(a => a.UserId == id && a.ActionType != "Withdraw")
                    .OrderByDescending(a => a.ActionDate)
                    .Take(30)
                    .Select(a => new UserActivityRow
                    {
                        ActionType = a.ActionType,
                        UserId = a.UserId,
                        Details = a.ActionDetails,
                        Date = a.ActionDate,
                        ReportId = a.ReportId,
                        ReportNumber = _db.Reports.Where(r => r.ReportId == a.ReportId).Select(r => r.ReportNumber).FirstOrDefault()
                    })
                    .ToListAsync()
            };

            if (user.Role?.RoleCode == "Technical")
            {
                vm.ReportCounts = await _db.Reports
                    .Where(r => r.CreatedByUserId == id)
                    .GroupBy(r => r.Status)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Key, x => x.Count);
            }
            else if (user.Role?.RoleCode is "PlotApprover" or "PlanApprover")
            {
                vm.ApprovedCount = await _db.ReportApprovals.CountAsync(a => a.ApproverUserId == id && a.Decision == "approve");
                vm.ReturnedCount = await _db.ReportApprovals.CountAsync(a => a.ApproverUserId == id && a.Decision == "return");
            }

            return View(vm);
        }

        // GET: /Users/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string fullName, string username, string password, int roleId,
                                                string? nationalId, string? email, string? phone)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            nationalId = LatinDigits(nationalId);
            phone = LatinDigits(phone);
            ModelState.Clear(); // رسائل الربط الافتراضية بالإنجليزي — نتحقق بأنفسنا تحت
            await ValidateUserAsync(null, fullName, nationalId, email, phone, roleId);
            if (string.IsNullOrWhiteSpace(username)) ModelState.AddModelError("", "اسم الدخول مطلوب.");
            else if (await _db.Users.AnyAsync(u => u.Username == username)) ModelState.AddModelError("", "اسم الدخول مستخدم من قبل.");
            if (string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("", "كلمة المرور مطلوبة.");

            if (!ModelState.IsValid)
            {
                ViewBag.UserRole = "Admin";
                ViewBag.Roles = await _db.Roles.ToListAsync();
                return View();
            }

            _db.Users.Add(new User
            {
                FullName = fullName.Trim(),
                Username = username.Trim(),
                // ⚠️ نفس أسلوب التطوير المؤقت المستخدم بالحسابات التجريبية (نص صريح بادئته TEMP_PLAIN_FOR_DEV_ONLY)
                // يُستبدل بتشفير BCrypt حقيقي قبل أي استخدام فعلي خارج بيئة الاختبار
                PasswordHash = $"TEMP_PLAIN_FOR_DEV_ONLY:{password}",
                RoleId = roleId,
                NationalId = nationalId,
                Email = Clean(email),
                Phone = Clean(phone),
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = CurrentUserId!.Value,
                ActionType = "UserAdded",
                ActionDetails = $"إضافة مستخدم جديد: {fullName} ({username})",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "تمت إضافة المستخدم";
            return RedirectToAction("Index");
        }

        // GET: /Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";
            ViewBag.Roles = await _db.Roles.ToListAsync();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string fullName, int roleId, bool isActive,
                                              string? nationalId, string? email, string? phone)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            nationalId = LatinDigits(nationalId);
            phone = LatinDigits(phone);
            ModelState.Clear();
            await ValidateUserAsync(id, fullName, nationalId, email, phone, roleId);

            // المدير ما يقدر يعطّل حسابه أو يغيّر دوره (عشان ما يقفل النظام على نفسه)
            if (id == CurrentUserId && (!isActive || roleId != user.RoleId))
                ModelState.AddModelError("", "لا يمكنك تعطيل حسابك أو تغيير دورك.");

            if (!ModelState.IsValid)
            {
                // نرجّع اللي كتبه المدير في الفورم (بدون حفظ)
                user.FullName = fullName;
                user.NationalId = nationalId;
                user.Email = email;
                user.Phone = phone;
                ViewBag.UserRole = "Admin";
                ViewBag.Roles = await _db.Roles.ToListAsync();
                return View(user);
            }

            user.FullName = fullName.Trim();
            user.RoleId = roleId;
            user.IsActive = isActive; // تعطيل بدل حذف — يحافظ على تاريخ عملياته بسجل التدقيق
            user.NationalId = nationalId;
            user.Email = Clean(email);
            user.Phone = Clean(phone);

            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = CurrentUserId!.Value,
                ActionType = "UserUpdated",
                ActionDetails = $"تعديل بيانات المستخدم: {user.FullName}",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "تم حفظ التعديلات";
            return RedirectToAction("Details", new { id });
        }

        // POST: /Users/ToggleActive/5 — تعطيل أو تفعيل سريع من القائمة أو الملف
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, string? returnTo)
        {
            if (!IsAdmin) return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (id == CurrentUserId)
            {
                TempData["UserError"] = "لا يمكنك تعطيل حسابك.";
            }
            else
            {
                user.IsActive = !user.IsActive;
                _db.AuditLog.Add(new AuditLogEntry
                {
                    UserId = CurrentUserId!.Value,
                    ActionType = user.IsActive ? "UserActivated" : "UserDeactivated",
                    ActionDetails = $"{(user.IsActive ? "تفعيل" : "تعطيل")} المستخدم: {user.FullName}",
                    ActionDate = DateTime.Now
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = user.IsActive ? "تم تفعيل الحساب" : "تم تعطيل الحساب";
            }

            return returnTo == "details" ? RedirectToAction("Details", new { id }) : RedirectToAction("Index");
        }

        // ═════════════════════ دوال مساعدة ═════════════════════

        // فحص الحقول المشتركة بين الإضافة والتعديل
        private async Task ValidateUserAsync(int? userId, string? fullName, string? nationalId, string? email, string? phone, int roleId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                ModelState.AddModelError("", "الاسم الكامل مطلوب.");

            if (string.IsNullOrWhiteSpace(nationalId))
                ModelState.AddModelError("", "رقم الهوية مطلوب.");
            else if (!Regex.IsMatch(nationalId, "^[12][0-9]{9}$"))
                ModelState.AddModelError("", "رقم الهوية 10 أرقام ويبدأ بـ 1 أو 2.");
            else if (await _db.Users.AnyAsync(u => u.NationalId == nationalId && u.UserId != userId))
                ModelState.AddModelError("", "رقم الهوية مسجّل لمستخدم آخر.");

            if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("", "صيغة البريد الإلكتروني غير صحيحة.");

            if (!string.IsNullOrWhiteSpace(phone) && !Regex.IsMatch(phone, "^05[0-9]{8}$"))
                ModelState.AddModelError("", "رقم الجوال بصيغة 05XXXXXXXX.");

            if (!await _db.Roles.AnyAsync(r => r.RoleId == roleId))
                ModelState.AddModelError("", "اختر الدور.");
        }

        // تحويل الأرقام العربية (١٢٣) لإنجليزية وحذف المسافات
        private static string? LatinDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var chars = value.Trim().Select(c =>
                c >= '٠' && c <= '٩' ? (char)('0' + (c - '٠')) :
                c >= '۰' && c <= '۹' ? (char)('0' + (c - '۰')) : c);
            return new string(chars.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
