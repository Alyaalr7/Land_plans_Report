namespace Land_plans_Report.Helpers
{
    // ═══ قيم خيارات النموذج — تُحفظ بالعربي كما هي بالداتا بيز ═══
    public static class Options
    {
        // نعم / لا
        public const string Yes = "نعم";
        public const string No = "لا";

        // يوجد / لا يوجد / جزئي (الإسفلت والكهرباء)
        public const string Exists = "يوجد";
        public const string NotExists = "لا يوجد";
        public const string Partial = "جزئي";

        // الحالة الفنية للقطعة
        public const string Complete = "مكتمل";
        public const string Incomplete = "غير مكتمل";

        // التوصية
        public const string Suitable = "صالحة للبناء";
        public const string NotSuitable = "غير صالحة";
    }

    // ═══ قيم تُحفظ بالإنجليزي وتُعرض بالعربي ═══
    public static class Labels
    {
        public static string ReportType(string? code) => code switch
        {
            "plan" => "تقرير مخطط",
            "plot" => "تقرير قطعة أرض",
            "raw" => "تقرير أرض خام",
            "unplanned" => "تقرير أرض بلا مخطط",
            _ => code ?? ""
        };

        public static string Status(string? code) => code switch
        {
            "draft" => "مسودة",
            "pending" => "بانتظار الاعتماد",
            "returned" => "معاد للتعديل",
            "approved" => "معتمد",
            _ => code ?? ""
        };
    }

    // ═══ المناطق والمحافظات — المحافظات تتعبى حسب المنطقة المختارة ═══
    public static class Locations
    {
        public static readonly Dictionary<string, string[]> Governorates = new()
        {
            ["منطقة المدينة المنورة"] = new[]
            {
                "المدينة المنورة", "ينبع", "العلا", "المهد", "الحناكية",
                "بدر", "خيبر", "العيص", "وادي الفرع"
            }
        };

        public static IEnumerable<string> Regions => Governorates.Keys;

        // هل المحافظة فعلاً تابعة للمنطقة؟
        public static bool IsValid(string? region, string? governorate) =>
            region != null && governorate != null
            && Governorates.TryGetValue(region, out var list)
            && list.Contains(governorate);
    }
}