using Land_plans_Report.Data;
using Signature = Land_plans_Report.Data.Signature;

namespace Land_plans_Report.Models
{
    // صف عملية واحدة (ملف المستخدم + سجل التدقيق)
    public class UserActivityRow
    {
        public string ActionType { get; set; } = "";
        public string? Details { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? ReportId { get; set; }
        public string? ReportCreator { get; set; }
        public string? ReportNumber { get; set; }
        public string? ReportType { get; set; }
        public string? RoleName { get; set; }
    }

    // جدول العمليات المشترك (_ActivityTable)
    public class ActivityTableModel
    {
        public List<UserActivityRow> Rows { get; set; } = new();
        public bool ShowUser { get; set; }
    }

    // ملف المستخدم: بياناته + ملخص نشاطه
    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;

        public DateTime? LastLogin { get; set; }
        public int LoginCount { get; set; }
        public int FailedLogins30Days { get; set; }

        public Dictionary<string, int> ReportCounts { get; set; } = new();   // الفني
        public int ApprovedCount { get; set; }                                 // المعتمِد
        public int ReturnedCount { get; set; }

        public bool NeedsSignature { get; set; }
        public Signature? Signature { get; set; }

        public List<UserActivityRow> Activity { get; set; } = new();
    }

    // لوحة سجل التدقيق
    public class AuditLogViewModel
    {
        public int TodayCount { get; set; }
        public int SubmittedWeek { get; set; }
        public int DecisionsWeek { get; set; }
        public int FailedLoginsWeek { get; set; }

        public List<UserActivityRow> Rows { get; set; } = new();
        public bool Truncated { get; set; }

        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Period { get; set; } = "30";
        public List<(int Id, string Name)> Users { get; set; } = new();
    }
}
