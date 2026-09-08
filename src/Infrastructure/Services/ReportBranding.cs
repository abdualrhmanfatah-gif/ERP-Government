namespace ERP_Government.Infrastructure.Services;

public static class ReportBranding
{
    public static string GovernmentLine { get; private set; } = "الجمهورية اليمنية";
    public static string OrganizationName { get; private set; } = string.Empty;
    public static string? DepartmentName { get; private set; }
    public static string? LogoPath { get; private set; }

    public static void Configure(
        string governmentLine,
        string organizationName,
        string? departmentName,
        string? logoPath)
    {
        GovernmentLine = governmentLine;
        OrganizationName = organizationName;
        DepartmentName = departmentName;
        LogoPath = logoPath;
    }
}
