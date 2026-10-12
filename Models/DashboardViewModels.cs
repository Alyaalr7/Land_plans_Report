namespace Land_plans_Report.Models
{
    // نقطة شهرية في الرسم (عمودين: الأول والثاني)
    public class MonthPoint
    {
        public string Label { get; set; } = "";
        public int First { get; set; }
        public int Second { get; set; }
    }

    // تقرير معلّق مع عدد أيام الانتظار
    public class WaitingReport
    {
        public int ReportId { get; set; }
        public string ReportNumber { get; set; } = "";
        public string ReportType { get; set; } = "";
        public string? Preparer { get; set; }
        public int Days { get; set; }
    }

    // مستخدم يحتاج توقيع وما عنده
    public class MissingSignature
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? RoleName { get; set; }
    }

    // لوحة مدير النظام
    public class AdminDashboardData
    {
        public int Sent { get; set; }
        public int SentThisMonth { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Returned { get; set; }
        public int LateCount { get; set; }
        public double? AvgApprovalDays { get; set; }

        public List<MonthPoint> Months { get; set; } = new();          // مرسلة / معتمدة
        public List<(string Type, int Count)> ByType { get; set; } = new();

        public bool HasActiveStamp { get; set; }
        public int SigningUsers { get; set; }
        public List<MissingSignature> MissingSignatures { get; set; } = new();

        public List<WaitingReport> Late { get; set; } = new();
        public Dictionary<string, string> ApproverRoleNames { get; set; } = new();   // plan / other ← اسم الدور المسؤول
    }

    // لوحة المعتمِد
    public class ApproverDashboardData
    {
        public int Pending { get; set; }
        public int ApprovedThisMonth { get; set; }
        public int ReturnedThisMonth { get; set; }
        public double? AvgDecisionDays { get; set; }

        public bool HasSignature { get; set; }
        public bool HasActiveStamp { get; set; }

        public List<WaitingReport> Waiting { get; set; } = new();
        public List<MonthPoint> Months { get; set; } = new();          // اعتماد / إعادة
    }
}
