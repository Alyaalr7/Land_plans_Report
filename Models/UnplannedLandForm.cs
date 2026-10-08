using System.ComponentModel.DataAnnotations;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;

namespace Land_plans_Report.Models
{
    public class UnplannedLandForm : IValidatableObject
    {
        [Required(ErrorMessage = "اسم المخطط مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى 200 حرف")]
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "كود المخطط مطلوب")]
        [StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string? PlanCode { get; set; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(typeof(decimal), "0.01", "9999999999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المساحة يجب أن تكون أكبر من صفر")]
        public decimal? Area { get; set; }

        [Required(ErrorMessage = "اختر المحافظة")]
        public string? Governorate { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "المركز أو القرية مطلوب")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? CenterOrVillage { get; set; }

        [Required(ErrorMessage = "عدد القطع مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد القطع يجب أن يكون 1 أو أكثر")]
        public int? PlotCount { get; set; }

        [Required(ErrorMessage = "حدد هل الموقع داخل حد التنمية")]
        public bool? InsideDevelopmentBoundary { get; set; }

        [Required(ErrorMessage = "البعد عن المركز مطلوب")]
        [Range(typeof(decimal), "0", "9999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المسافة يجب أن تكون بين 0 و 9999.99 كم")]
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
        }

        public static UnplannedLandForm FromEntity(ReportUnplannedLandDetails e) => new()
        {
            PlanName = e.PlanName,
            PlanCode = e.PlanCode,
            Area = e.Area,
            Governorate = e.Governorate,
            Region = e.Region,
            CenterOrVillage = e.CenterOrVillage,
            PlotCount = e.PlotCount,
            InsideDevelopmentBoundary = e.InsideDevelopmentBoundary,
            DistanceFromCenter = e.DistanceFromCenter,
            AsphaltStatus = e.AsphaltStatus,
            ElectricityStatus = e.ElectricityStatus,
            UrbanCondition = e.UrbanCondition,
        };

        public void ApplyTo(ReportUnplannedLandDetails e)
        {
            e.PlanName = PlanName; e.PlanCode = PlanCode; e.Area = Area;
            e.Governorate = Governorate; e.Region = Region; e.CenterOrVillage = CenterOrVillage;
            e.PlotCount = PlotCount; e.InsideDevelopmentBoundary = InsideDevelopmentBoundary;
            e.DistanceFromCenter = DistanceFromCenter; e.AsphaltStatus = AsphaltStatus;
            e.ElectricityStatus = ElectricityStatus; e.UrbanCondition = UrbanCondition;
        }
    }
}