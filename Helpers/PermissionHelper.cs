
using Microsoft.AspNetCore.Http;
using System.Linq;
namespace Land_plans_Report.Helpers
{
    public static class PermissionHelper
    {
        public static bool Has(this HttpContext context, string permissionCode)
        {
            var stored = context.Session.GetString("Permissions");
            if (string.IsNullOrEmpty(stored)) return false;
            return stored.Split(',').Contains(permissionCode);
        }

    }
}
