using System.Globalization;

namespace Land_plans_Report.Helpers
{
    // عرض فقط: تحويل القيم المخزنة لنص عربي (صفحات التفاصيل والمراجعة والطباعة)
    public static class ReportLabels
    {
        public static string Title(string? type) => type switch
        {
            "plan" => "تقرير الحالة الفنية للمخطط",
            "plot" => "تقرير الحالة الفنية لقطعة الأرض",
            "raw" => "تقرير الحالة الفنية لأرض خام",
            "unplanned" => "تقرير الحالة الفنية لأرض لا يوجد فيها مخطط",
            _ => "تقرير الحالة الفنية"
        };

        // قيم الاختيارات (Options) بالعربي، وأي قيمة ثانية تظهر كما هي
        public static string Option(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            if (value == Options.Exists) return "يوجد";
            if (value == Options.NotExists) return "لا يوجد";
            if (value == Options.Partial) return "جزئي";
            if (value == Options.Yes) return "نعم";
            if (value == Options.No) return "لا";
            if (value == Options.Complete) return "مكتمل البناء";
            if (value == Options.Incomplete) return "غير مكتمل البناء";
            if (value == Options.Suitable) return "صالحة للبناء";
            if (value == Options.NotSuitable) return "غير صالحة للبناء";
            return value;
        }

        public static string YesNo(bool? value) => value == null ? "" : value.Value ? "نعم" : "لا";

        // رقم بدون أصفار زائدة: 200000.20 ← 200,000.2
        public static string Number(decimal? value) =>
            value == null ? "" : value.Value.ToString("#,0.##", CultureInfo.InvariantCulture);

        // التاريخ الهجري (أم القرى) للمستندات الرسمية: 1447/04/24 هـ
        public static string Hijri(DateTime? date)
        {
            if (date == null) return "";
            var cal = new UmAlQuraCalendar();
            var d = date.Value;
            return $"{cal.GetYear(d)}/{cal.GetMonth(d):00}/{cal.GetDayOfMonth(d):00} هـ";
        }
    }
}
