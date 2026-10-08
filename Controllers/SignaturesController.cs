using Land_plans_Report.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Land_plans_Report.Controllers
{
    public class SignaturesController : Controller
    {
        private readonly AppDbContext _db;
        public SignaturesController(AppDbContext db) => _db = db;

        // GET: /Signatures — الختمين الثابتين (مساعد الوكيل / المدير العام)، من عمود Roles.StampText
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            var approverRoles = await _db.Roles
                .Where(r => r.RoleCode == "PlotApprover" || r.RoleCode == "PlanApprover")
                .ToListAsync();

            return View(approverRoles);
        }

        // GET: /Signatures/Edit/2
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");
            ViewBag.UserRole = "Admin";

            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        // POST: /Signatures/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int roleId, string stampText)
        {
            if (HttpContext.Session.GetString("RoleCode") != "Admin") return RedirectToAction("AccessDenied", "Account");

            var role = await _db.Roles.FindAsync(roleId);
            if (role == null) return NotFound();

            role.StampText = stampText;

            var adminId = HttpContext.Session.GetInt32("UserId");
            _db.AuditLog.Add(new AuditLogEntry
            {
                UserId = adminId!.Value,
                ActionType = "StampUpdated",
                ActionDetails = $"تحديث نص ختم الدور: {role.RoleName}",
                ActionDate = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
