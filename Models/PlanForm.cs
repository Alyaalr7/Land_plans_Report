using System.ComponentModel.DataAnnotations;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;

namespace Land_plans_Report.Models
{
    // فورم تقرير المخطط: كل الفالديشن هنا (الإلزامي + الشكل)
    // المسودة تتجاهل أخطاء الحقول الفاضية في الكنترولر، والإرسال يشيك على كل شي
    public class PlanForm : IValidatableObject
    {
        [Required(ErrorMessage = "كود المخطط مطلوب")]
        [StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string? PlanCode { get; set; }

        [Required(ErrorMessage = "اسم المخطط مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى 200 حرف")]
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "المساحة مطلوبة")]
        [Range(typeof(decimal), "0.01", "9999999999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المساحة يجب أن تكون أكبر من صفر")]
        public decimal? Area { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "اختر المحافظة")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? Governorate { get; set; }

        [Required(ErrorMessage = "المركز أو القرية مطلوب")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? CenterOrVillage { get; set; }

        [Required(ErrorMessage = "عدد القطع مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد القطع يجب أن يكون 1 أو أكثر")]
        public int? PlotCount { get; set; }

        [Required(ErrorMessage = "البعد عن المركز مطلوب")]
        [Range(typeof(decimal), "0", "9999.99", ParseLimitsInInvariantCulture = true,
               ErrorMessage = "المسافة يجب أن تكون بين 0 و 9999.99 كم")]
        public decimal? DistanceFromCenter { get; set; }

        [Required(ErrorMessage = "اختر حالة الإسفلت")]
        [AllowedValues(Options.NotExists, Options.Partial, Options.Exists, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? AsphaltStatus { get; set; }

        [Required(ErrorMessage = "اختر حالة شبكة المياه والصرف")]
        [AllowedValues(Options.Yes, Options.No, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? WaterDrainage { get; set; }

        [Required(ErrorMessage = "اختر حالة الكهرباء")]
        [AllowedValues(Options.NotExists, Options.Exists, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? ElectricityStatus { get; set; }

        // حالة العمران في المخطط: نسبة من 0 إلى 100 (حسب الـ BRD)
        [Required(ErrorMessage = "نسبة العمران مطلوبة")]
        [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
               ErrorMessage = "النسبة يجب أن تكون رقماً من 0 إلى 100")]
        public string? UrbanCondition { get; set; }

        // قاعدة شكل تعتمد على حقلين: المحافظة لازم تكون تابعة للمنطقة
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (!string.IsNullOrEmpty(Governorate) && !Locations.IsValid(Region, Governorate))
                yield return new ValidationResult("اختر المحافظة من القائمة", new[] { nameof(Governorate) });
        }

        // ── من الداتا بيز للفورم (صفحة التعديل) ──
        public static PlanForm FromEntity(ReportPlanDetails e) => new()
        {
            PlanCode = e.PlanCode,
            PlanName = e.PlanName,
            Area = e.Area,
            Region = e.Region,
            Governorate = e.Governorate,
            CenterOrVillage = e.CenterOrVillage,
            PlotCount = e.PlotCount,
            DistanceFromCenter = e.DistanceFromCenter,
            AsphaltStatus = e.AsphaltStatus,
            WaterDrainage = e.WaterDrainage,
            ElectricityStatus = e.ElectricityStatus,
            UrbanCondition = e.UrbanCondition,
        };

        // ── من الفورم للداتا بيز (الحفظ) ──
        public void ApplyTo(ReportPlanDetails e)
        {
            e.PlanCode = PlanCode;
            e.PlanName = PlanName;
            e.Area = Area;
            e.Region = Region;
            e.Governorate = Governorate;
            e.CenterOrVillage = CenterOrVillage;
            e.PlotCount = PlotCount;
            e.DistanceFromCenter = DistanceFromCenter;
            e.AsphaltStatus = AsphaltStatus;
            e.WaterDrainage = WaterDrainage;
            e.ElectricityStatus = ElectricityStatus;
            e.UrbanCondition = UrbanCondition;
        }
    }
}