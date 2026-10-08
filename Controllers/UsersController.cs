using Land_plans_Report.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Land_plans_Report.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        // GET: /Users  — قائمة حقيقية من جدول Users (بدل البيانات الثابتة السابقة)
        public async Task<IActionResult> Index()
        {
            var roleCode = HttpContext.Session.GetString("RoleCode");
            if (roleCode == null) return RedirectToAction("Login", "Account");
            if (roleCode != "Admin") return RedirectToAction("AccessDenied", "Account"); // بس مدير النظام يدير المستخدمين

            ViewBag.UserRole = "Admin";

            var users = await _db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // GET: /Users/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string fullName, string username, string password, int roleId)
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");

            if (await _db.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "اسم المستخدم مستخدم من قبل، اختاري اسماً آخر.");
                ViewBag.UserRole = "Admin";
                ViewBag.Roles = await _db.Roles.ToListAsync();
                return View();
            }

            _db.Users.Add(new User
            {
                FullName = fullName,
                Username = username,
                // ⚠️ نفس أسلوب التطوير المؤقت المستخدم بالحسابات التجريبية (نص صريح بادئته TEMP_PLAIN_FOR_DEV_ONLY)
                // يُستبدل بتشفير BCrypt حقيقي قبل أي استخدام فعلي خارج بيئة الاختبار
                PasswordHash = $"TEMP_PLAIN_FOR_DEV_ONLY:{password}",
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            var adminId = HttpContext.Session.GetInt32("UserId");
            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = adminId!.Value,
                ActionType = "UserAdded",
                ActionDetails = $"إضافة مستخدم جديد: {fullName} ({username})",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: /Users/Edit/5 — تعديل دور مستخدم موجود أو تفعيله
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";
            ViewBag.Roles = await _db.Roles.ToListAsync();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int roleId, bool isActive)
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return Forbid();

            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.RoleId = roleId;
            user.IsActive = isActive; // تعطيل بدل حذف — يحافظ على تاريخ عملياته بسجل التدقيق

            var adminId = HttpContext.Session.GetInt32("UserId");
            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = adminId!.Value,
                ActionType = "UserUpdated",
                ActionDetails = $"تعديل بيانات المستخدم: {user.FullName}",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
