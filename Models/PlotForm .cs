using System.ComponentModel.DataAnnotations;
using Land_plans_Report.Data;
using Land_plans_Report.Helpers;

namespace Land_plans_Report.Models
{
    public class PlotForm : IValidatableObject
    {
        [Required(ErrorMessage = "رقم الهوية مطلوب")]
        [RegularExpression("^[12][0-9]{9}$", ErrorMessage = "رقم الهوية 10 أرقام إنجليزية ويبدأ بـ 1 أو 2")]
        public string? BeneficiaryNationalId { get; set; }

        [Required(ErrorMessage = "اسم المستفيد مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى 200 حرف")]
        public string? BeneficiaryName { get; set; }

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [RegularExpression(@"^(05[0-9]{8}|\+9665[0-9]{8})$", ErrorMessage = "رقم الجوال بصيغة 05XXXXXXXX أو +9665XXXXXXXX")]
        public string? BeneficiaryPhone { get; set; }

        [Required(ErrorMessage = "كود المخطط مطلوب")]
        [StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string? PlanCode { get; set; }

        [Required(ErrorMessage = "اسم المخطط مطلوب")]
        [StringLength(200, ErrorMessage = "الحد الأقصى 200 حرف")]
        public string? PlanName { get; set; }

        [Required(ErrorMessage = "اختر المحافظة")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? Governorate { get; set; }

        [Required(ErrorMessage = "المنطقة مطلوبة")]
        [StringLength(150, ErrorMessage = "الحد الأقصى 150 حرفاً")]
        public string? Region { get; set; }

        [Required(ErrorMessage = "رقم القطعة مطلوب")]
        [StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string? PlotNumber { get; set; }

        [Required(ErrorMessage = "اختر الحالة الفنية للقطعة")]
        [AllowedValues(Options.Complete, Options.Incomplete, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? PlotCondition { get; set; }

        [Required(ErrorMessage = "اختر التوصية")]
        [AllowedValues(Options.Suitable, Options.NotSuitable, null, ErrorMessage = "قيمة غير صحيحة")]
        public string? Recommendation { get; set; }

        [StringLength(500, ErrorMessage = "الحد الأقصى 500 حرف")]
        public string? IncompleteNotes { get; set; }

        [StringLength(500, ErrorMessage = "الحد الأقصى 500 حرف")]
        public string? NotSuitableNotes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (!string.IsNullOrEmpty(Governorate) && !Locations.IsValid(Region, Governorate))
                yield return new ValidationResult("اختر المحافظة من القائمة", new[] { nameof(Governorate) });
            if (PlotCondition == Options.Incomplete && string.IsNullOrWhiteSpace(IncompleteNotes))
                yield return new ValidationResult("وضّح أوجه عدم الاكتمال", new[] { nameof(IncompleteNotes) });
            if (Recommendation == Options.NotSuitable && string.IsNullOrWhiteSpace(NotSuitableNotes))
                yield return new ValidationResult("وضّح أسباب عدم الصلاحية", new[] { nameof(NotSuitableNotes) });
        }

        public static PlotForm FromEntity(ReportPlotDetails e) => new()
        {
            BeneficiaryNationalId = e.BeneficiaryNationalId,
            BeneficiaryName = e.BeneficiaryName,
            BeneficiaryPhone = e.BeneficiaryPhone,
            PlanCode = e.PlanCode,
            PlanName = e.PlanName,
            Governorate = e.Governorate,
            Region = e.Region,
            PlotNumber = e.PlotNumber,
            PlotCondition = e.PlotCondition,
            Recommendation = e.Recommendation,
            IncompleteNotes = e.IncompleteNotes,
            NotSuitableNotes = e.NotSuitableNotes,
        };

        public void ApplyTo(ReportPlotDetails e)
        {
            e.BeneficiaryNationalId = BeneficiaryNationalId;
            e.BeneficiaryName = BeneficiaryName ?? "";
            e.BeneficiaryPhone = BeneficiaryPhone;
            e.PlanCode = PlanCode ?? "";
            e.PlanName = PlanName;
            e.Governorate = Governorate ?? "";
            e.Region = Region ?? "";
            e.PlotNumber = PlotNumber ?? "";
            e.PlotCondition = PlotCondition;
            e.Recommendation = Recommendation;
            e.IncompleteNotes = IncompleteNotes;
            e.NotSuitableNotes = NotSuitableNotes;
        }
    }
}