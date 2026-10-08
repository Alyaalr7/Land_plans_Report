namespace Land_plans_Report.Models
{

    public class LocationPickerViewModel
    {
        public string FieldPrefix { get; set; } = "";
        public double? Latitude { get; set; }       // null = الفني ما حدد موقع
        public double? Longitude { get; set; }
        public int? GpsAccuracyMeters { get; set; }
    }

    public class FileUploadViewModel
    {
        public string FieldName { get; set; } = "Files";
        public string Title { get; set; } = "رفع الصور";
        public bool Required { get; set; } = true;
        public List<string>? SampleFileNames { get; set; }
    }

    public class ActionSummaryViewModel
    {
        public Dictionary<string, string>? Rows { get; set; }
        public string Note { get; set; } = "راجع البيانات والصور قبل حفظ التقرير أو إرساله.";
    }

    public class KpiCardViewModel
    {
        public string Value { get; set; } = "0";
        public string Label { get; set; } = "";
        public string IconClass { get; set; } = "green";
        public string IconText { get; set; } = "";
    }

    public class ReportListItemViewModel
    {
        public int Index { get; set; }
        public string ReportNumber { get; set; } = "";
        public string ReportType { get; set; } = "";
        public string CreatedDate { get; set; } = "";
        public string Status { get; set; } = "pending"; // approved | draft | pending | returned
    }

    public class ActivityItemViewModel
    {
        public string Title { get; set; } = "";
        public string Meta { get; set; } = "";
    }
}
