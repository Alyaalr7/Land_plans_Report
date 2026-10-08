using Microsoft.AspNetCore.Mvc.Rendering;

namespace Land_plans_Report.Helpers
{
    public static class FormHelpers
    {
        // يرجّع القيمة اللي كتبها المستخدم — عشان النموذج ما يفضى لما يطلع خطأ
        public static string FormValue(this IHtmlHelper html, string name)
        {
            var request = html.ViewContext.HttpContext.Request;
            return request.HasFormContentType ? request.Form[name].ToString() : "";
        }

        // هل هذا الخيار كان مختار؟ (للـ radio والـ select)
        public static bool IsChecked(this IHtmlHelper html, string name, string value) =>
            html.FormValue(name) == value;

        // يلوّن الحقل أحمر لو فيه خطأ
        public static string ErrorClass(this IHtmlHelper html, string name) =>
            html.ViewData.ModelState[name]?.Errors.Count > 0 ? "has-error" : "";
    }
}