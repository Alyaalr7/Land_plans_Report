namespace Land_plans_Report.Helpers
{
    // أنواع جدول Signatures + مين يحتاج توقيع
    public static class SignatureKinds
    {
        public const string Signature = "signature";
        public const string Stamp = "stamp";

        // الأدوار اللي توقّع على التقارير: الفني (معد التقرير) والمعتمِدين
        public static readonly string[] SigningRoles = { "Technical", "PlotApprover", "PlanApprover" };

        public static bool RoleSigns(string? roleCode) =>
            roleCode != null && SigningRoles.Contains(roleCode);
    }
}
