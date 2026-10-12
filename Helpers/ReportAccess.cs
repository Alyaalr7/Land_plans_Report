using Land_plans_Report.Data;

namespace Land_plans_Report.Helpers
{
    // مين يشوف التقرير (نفس قاعدة صفحة التفاصيل والطباعة)
    public static class ReportAccess
    {
        public static bool CanView(Report report, int userId, string? roleCode)
        {
            if (report.Status == "draft") return report.CreatedByUserId == userId; // المسودة لصاحبها فقط

            return roleCode == "Admin"
                || (roleCode == "Technical" && report.CreatedByUserId == userId)
                || (roleCode == "PlotApprover" && report.ReportType != "plan")
                || (roleCode == "PlanApprover" && report.ReportType == "plan");
        }
    }
}
