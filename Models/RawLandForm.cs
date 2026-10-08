using System.ComponentModel.DataAnnotations;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;

namespace Land_plans_Report.Models
{
    public class RawLandForm : IValidatableObject
    {
        [Required(ErrorMessage = "حدد هل يوجد مخطط معتمد")]
        public bool? HasApprovedPlan { get; set; }

        [StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string? PlanCode { get; set; }

        [StringLength(200, ErrorMessage = "الحد الأقصى 200 حرف")]
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المساحة يجب أن تكون أكبر من صفر")]
        public decimal? Area { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "اختر المحافظة")]
        public string? Governorate { get; set; }

        [Required(ErrorMessage = "المركز أو القرية مطلوب")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? CenterOrVillage { get; set; }

        [Required(ErrorMessage = "عدد القطع مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد القطع يجب أن يكون 1 أو أكثر")]
        public int? PlotCount { get; set; }

        [Required(ErrorMessage = "البعد عن المركز مطلوب")]
        [Range(typeof(decimal), "0", "9999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المسافة يجب أن تكون بين 0 و 299.99 كم")]
        public decimal? DistanceFromCenter { get; set; }

        [Required(ErrorMessage = "اختر حالة الإسفلت")]
        [AllowedValues(Options.NotExists, Options.Partial, Options.Exists, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? AsphaltStatus { get; set; }

        [Required(ErrorMessage = "اختر حالة الكهرباء")]
        [AllowedValues(Options.NotExists, Options.Exists, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? ElectricityStatus { get; set; }

        [Required(ErrorMessage = "حالة العمران مطلوبة")]
        [StringLength(300, ErrorMessage = "الحد الأقصى 300 حرف")]
        public string? UrbanCondition { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (!string.IsNullOrEmpty(Governorate) && !Locations.IsValid(Region, Governorate))
                yield return new ValidationResult("اختر المحافظة من القائمة", new[] { nameof(Governorate) });
            if (HasApprovedPlan == true && string.IsNullOrWhiteSpace(PlanCode))
                yield return new ValidationResult("كود المخطط مطلوب", new[] { nameof(PlanCode) });
            if (HasApprovedPlan == true && string.IsNullOrWhiteSpace(PlanName))
                yield return new ValidationResult("اسم المخطط مطلوب", new[] { nameof(PlanName) });
        }

        public static RawLandForm FromEntity(ReportRawLandDetails e) => new()
        {
            HasApprovedPlan = e.HasApprovedPlan,
            PlanCode = e.PlanCode,
            PlanName = e.PlanName,
            Area = e.Area,
            Region = e.Region,
            Governorate = e.Governorate,
            CenterOrVillage = e.CenterOrVillage,
            PlotCount = e.PlotCount,
            DistanceFromCenter = e.DistanceFromCenter,
            AsphaltStatus = e.AsphaltStatus,
            ElectricityStatus = e.ElectricityStatus,
            UrbanCondition = e.UrbanCondition,
        };

        public void ApplyTo(ReportRawLandDetails e)
        {
            e.HasApprovedPlan = HasApprovedPlan; e.PlanCode = PlanCode; e.PlanName = PlanName;
            e.Area = Area; e.Region = Region; e.Governorate = Governorate;
            e.CenterOrVillage = CenterOrVillage; e.PlotCount = PlotCount;
            e.DistanceFromCenter = DistanceFromCenter; e.AsphaltStatus = AsphaltStatus;
            e.ElectricityStatus = ElectricityStatus; e.UrbanCondition = UrbanCondition;
        }
    }
}