using System.ComponentModel.DataAnnotations;

namespace Land_plans_Report.Data
{
    // ─────────────────────────────────────────────────────────────
    // القاعدة في هذا الملف:
    //   عمود NOT NULL في الداتا بيز  →  string X { get; set; } = "";
    //   عمود NULL في الداتا بيز      →  string? X { get; set; }
    // ولا فالديشن هنا أبداً — الفالديشن مكانها الـ ViewModel
    // ─────────────────────────────────────────────────────────────

    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleCode { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string? StampText { get; set; }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? NationalId { get; set; }   // رقم الهوية — يصير المعرّف الأساسي مع نفاذ
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public Role? Role { get; set; }
    }

    public class Report
    {
        // ── يكتبها الخادم ──
        [Key]
        public int ReportId { get; set; }
        public string ReportNumber { get; set; } = "";
        public string ReportType { get; set; } = "";
        public int CreatedByUserId { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? PreparerSignatureId { get; set; }   // توقيع معد التقرير وقت الإرسال (يبقى ثابت حتى لو تغيّر توقيعه بعدين)

        // ── يعبيها الفني ──
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? GpsAccuracyM { get; set; }
        public string? Notes { get; set; }

        // ── العلاقات ──
        public User? CreatedByUser { get; set; }
        public ReportPlanDetails? PlanDetails { get; set; }
        public ReportPlotDetails? PlotDetails { get; set; }
        public ReportRawLandDetails? RawLandDetails { get; set; }
        public ReportUnplannedLandDetails? UnplannedLandDetails { get; set; }
        public List<ReportPhoto> Photos { get; set; } = new();
        public List<ReportApproval> Approvals { get; set; } = new();
    }

    public class ReportPlanDetails
    {
        [Key]
        public int ReportId { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public decimal? Area { get; set; }
        public string? Region { get; set; }
        public string? Governorate { get; set; }
        public string? CenterOrVillage { get; set; }
        public int? PlotCount { get; set; }
        public decimal? DistanceFromCenter { get; set; }
        public string? AsphaltStatus { get; set; }
        public string? WaterDrainage { get; set; }
        public string? ElectricityStatus { get; set; }
        public string? UrbanCondition { get; set; }

        public Report? Report { get; set; }
    }

    public class ReportPlotDetails
    {
        [Key]
        public int ReportId { get; set; }
        public string? BeneficiaryNationalId { get; set; }
        public string? BeneficiaryName { get; set; }
        public string? BeneficiaryPhone { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public string? Governorate { get; set; }
        public string? Region { get; set; }
        public string? PlotNumber { get; set; }
        public string? PlotCondition { get; set; }
        public string? Recommendation { get; set; }
        public string? IncompleteNotes { get; set; }
        public string? NotSuitableNotes { get; set; }

        public Report? Report { get; set; }
    }

    public class ReportRawLandDetails
    {
        [Key]
        public int ReportId { get; set; }
        public bool? HasApprovedPlan { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public decimal? Area { get; set; }
        public string? Region { get; set; }
        public string? Governorate { get; set; }
        public string? CenterOrVillage { get; set; }
        public int? PlotCount { get; set; }
        public decimal? DistanceFromCenter { get; set; }
        public string? AsphaltStatus { get; set; }
        public string? ElectricityStatus { get; set; }
        public string? UrbanCondition { get; set; }

        public Report? Report { get; set; }
    }

    public class ReportUnplannedLandDetails
    {
        [Key]
        public int ReportId { get; set; }
        public string? PlanName { get; set; }
        public string? PlanCode { get; set; }
        public decimal? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Region { get; set; }
        public string? CenterOrVillage { get; set; }
        public int? PlotCount { get; set; }
        public bool? InsideDevelopmentBoundary { get; set; }
        public decimal? DistanceFromCenter { get; set; }
        public string? AsphaltStatus { get; set; }
        public string? ElectricityStatus { get; set; }
        public string? UrbanCondition { get; set; }

        public Report? Report { get; set; }
    }

    public class ReportPhoto
    {
        [Key]
        public int PhotoId { get; set; }
        public int ReportId { get; set; }
        public string FilePath { get; set; } = "";
        public int? FileSizeKB { get; set; }
        public DateTime UploadedAt { get; set; }

        public Report? Report { get; set; }
    }

    public class ReportApproval
    {
        [Key]
        public int ApprovalId { get; set; }
        public int ReportId { get; set; }
        public int ApproverUserId { get; set; }
        public string Decision { get; set; } = ""; // approve | return
        public string? ApproverNotes { get; set; }
        public DateTime DecisionDate { get; set; }
        public int? SignatureId { get; set; }   // توقيع المعتمِد وقت الاعتماد
        public int? StampId { get; set; }       // ختم الجهة وقت الاعتماد

        public Report? Report { get; set; }
        public User? Approver { get; set; }
    }

    // التواقيع والأختام في جدول واحد:
    //   Kind = signature → توقيع شخص (UserId إلزامي)
    //   Kind = stamp     → ختم الجهة (UserId فاضي)
    // لا حذف — تعطيل فقط، لأن التقارير المعتمدة تشير للصف
    public class Signature
    {
        [Key]
        public int SignatureId { get; set; }
        public string Kind { get; set; } = "";
        public int? UserId { get; set; }
        public string FilePath { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime UploadedAt { get; set; }
        public int UploadedByUserId { get; set; }

        public User? User { get; set; }
    }

    public class AuditLogEntry
    {
        [Key]
        public int AuditId { get; set; }
        public int? ReportId { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = "";
        public string? ActionDetails { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
