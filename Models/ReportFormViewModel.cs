using Land_plans_Report.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Land_plans_Report.Models
{
    public class ReportFormViewModel
    {
        [BindNever] public int ReportId { get; set; }
        [BindNever] public string? ReportNumber { get; set; }
        [BindNever] public string? Status { get; set; }
        [BindNever] public int ExistingPhotoCount { get; set; }
        [BindNever] public string? LastReturnNote { get; set; }

        public string ReportType { get; set; } = "plan";

        [Required(ErrorMessage = "حدد الموقع على الخريطة")]
        public decimal? Latitude { get; set; }
        [Required(ErrorMessage = "حدد الموقع على الخريطة")]
        public decimal? Longitude { get; set; }
        public int? GpsAccuracyM { get; set; }

        [StringLength(4000, ErrorMessage = "الحد الأقصى 4000 حرف")]
        public string? Notes { get; set; }

        public PlanForm? Plan { get; set; }
        public PlotForm? Plot { get; set; }
        public RawLandForm? RawLand { get; set; }
        public UnplannedLandForm? UnplannedLand { get; set; }

        public List<IFormFile>? Photos { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Latitude == null || Longitude == null)
            {
                yield return new ValidationResult(
                    "حدد الموقع على الخريطة",
                    new[]
                    {
                nameof(Latitude),
                nameof(Longitude)
                    });
            }
        }
    }
}
    