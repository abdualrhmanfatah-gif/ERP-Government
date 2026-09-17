using ERP_Government.Domain.Assets.Constants;
using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// Asset seed data — 15 assets across all statuses.
/// FKs resolved by code: Currency YER, Fund F001/F002, Employee EMP-001..005,
/// Location LOC-*, CostCenter CC-*, Account codes from chart.
/// </summary>
public static class AssetSeedData
{
    /// <summary>
    /// Assets with FK codes resolved to IDs by the initialiser.
    /// </summary>
    public static IReadOnlyList<AssetBlueprint> Blueprints =>
    [
        // ── Buildings ──────────────────────────────────────────────
        new("AST-000001", "مببنى الإدارة الرئيسي — صنعاء",
            "AG-001-01", "Active",
            480_000_000m, 50, "EMP-001", "LOC-RECV-1", "CC-ADMIN", "F002", "YER",
            new DateOnly(2020,1,1), new DateOnly(2020,6,1), new DateOnly(2024,1,1),
            "AST-TAG-0001", "BAR-000001"),

        new("AST-000002", "مببنى أرشيف المتقاعدين — عدن",
            "AG-001-02", "Active",
            210_000_000m, 40, "EMP-002", "LOC-STORE-2", "CC-ADMIN", "F002", "YER",
            new DateOnly(2021,3,15), new DateOnly(2021,9,1), new DateOnly(2024,1,1),
            "AST-TAG-0002", "BAR-000002"),

        // ── Vehicles ───────────────────────────────────────────────
        new("AST-000003", "سيارة تويوتا هايلكس — إدارة عامة",
            "AG-003-01", "Active",
            18_500_000m, 5, "EMP-001", "LOC-SHIP-1", "CC-ADMIN", "F001", "YER",
            new DateOnly(2024,1,15), new DateOnly(2024,3,1), new DateOnly(2024,3,1),
            "AST-TAG-0003", "BAR-000003",
            SerialNumber: "JTDKN3DU5A0012345"),

        new("AST-000004", "حافلة هيونداي H1 — نقل موظفين",
            "AG-003-02", "Active",
            32_000_000m, 7, "EMP-004", "LOC-SHIP-1", "CC-HR", "F001", "YER",
            new DateOnly(2023,6,1), new DateOnly(2023,9,1), new DateOnly(2023,9,1),
            "AST-TAG-0004", "BAR-000004",
            SerialNumber: "KMJCN3UDBA0067890"),

        new("AST-000012", "سيارة إدارية — عدن",
            "AG-003-01", "Disposed",
            15_800_000m, 5, "EMP-003", "LOC-SHIP-2", "CC-PROC", "F001", "YER",
            new DateOnly(2023,7,1), new DateOnly(2023,9,1), new DateOnly(2023,9,1),
            "AST-TAG-0012", "BAR-000012",
            SerialNumber: "4T1BK1FK5CU123456"),

        new("AST-000014", "حافلة نقل موظفين — عدن",
            "AG-003-02", "Active",
            45_000_000m, 7, "EMP-005", "LOC-SHIP-2", "CC-WH", "F001", "YER",
            new DateOnly(2024,6,1), new DateOnly(2024,9,1), new DateOnly(2024,9,1),
            "AST-TAG-0014", "BAR-000014",
            SerialNumber: "KMHJN3UDBA0098765"),

        // ── Furniture & Equipment ──────────────────────────────────
        new("AST-000008", "أثاث الإدارة العامة",
            "AG-002-01", "Active",
            3_600_000m, 10, "EMP-001", "LOC-STORE-1", "CC-ADMIN", "F001", "YER",
            new DateOnly(2025,1,1), new DateOnly(2025,2,1), new DateOnly(2025,2,1),
            "AST-TAG-0008", "BAR-000008"),

        new("AST-000009", "أجهزة تكييف — قاعة الاجتماعات",
            "AG-002-03", "Active",
            1_950_000m, 10, "EMP-005", "LOC-STORE-1", "CC-ADMIN", "F001", "YER",
            new DateOnly(2025,2,1), new DateOnly(2025,3,1), new DateOnly(2025,3,1),
            "AST-TAG-0009", "BAR-000009"),

        // ── IT Equipment ───────────────────────────────────────────
        new("AST-000005", "خادم مركزي Dell PowerEdge",
            "AG-004-02", "Active",
            12_400_000m, 5, "EMP-002", "LOC-STORE-1", "CC-FIN", "F001", "YER",
            new DateOnly(2025,4,1), new DateOnly(2025,5,1), new DateOnly(2025,5,1),
            "AST-TAG-0005", "BAR-000005",
            SerialNumber: "DL7XK94"),

        new("AST-000006", "حاسوب مكتبي — محاسبة (10 وحدات)",
            "AG-004-01", "Active",
            2_750_000m, 5, "EMP-002", "LOC-STORE-1", "CC-FIN", "F001", "YER",
            new DateOnly(2025,3,1), new DateOnly(2025,4,1), new DateOnly(2025,4,1),
            "AST-TAG-0006", "BAR-000006",
            SerialNumber: "OptiPlex-7090-001..010"),

        new("AST-000007", "طابعة ليزر شبكية",
            "AG-004-03", "Draft",
            480_000m, 4, null, "LOC-STORE-1", "CC-PROC", "F001", "YER",
            new DateOnly(2026,8,1), null, null,
            "AST-TAG-0007", "BAR-000007",
            SerialNumber: "HP-LaserJet-M404dn"),

        new("AST-000013", "حاسوب محمول Lenovo ThinkPad",
            "AG-004-01", "Active",
            1_800_000m, 3, "EMP-004", "LOC-STORE-2", "CC-HR", "F001", "YER",
            new DateOnly(2024,1,1), new DateOnly(2024,2,1), new DateOnly(2024,2,1),
            "AST-TAG-0013", "BAR-000013",
            SerialNumber: "PF-3K4XMB"),

        // ── Intangible ─────────────────────────────────────────────
        new("AST-000010", "نظام إدارة الموارد ERP — ترخيص",
            "AG-005-01", "Active",
            6_500_000m, 3, "EMP-002", null, "CC-FIN", "F001", "YER",
            new DateOnly(2025,1,1), new DateOnly(2025,1,15), new DateOnly(2025,1,15),
            "AST-TAG-0010", "BAR-000010"),

        new("AST-000011", "رخصة قواعد بيانات SQL Server",
            "AG-005-02", "Active",
            2_100_000m, 3, "EMP-002", null, "CC-FIN", "F001", "YER",
            new DateOnly(2025,1,1), new DateOnly(2025,1,15), new DateOnly(2025,1,15),
            "AST-TAG-0011", "BAR-000011"),

        new("AST-000015", "برنامج الرواتب — ترخيص سنوي",
            "AG-005-01", "Draft",
            850_000m, 3, null, null, "CC-HR", "F001", "YER",
            new DateOnly(2026,9,1), null, null,
            "AST-TAG-0015", "BAR-000015"),
    ];

    public record AssetBlueprint(
        string Code,
        string Name,
        string GroupCode,
        string Status,
        decimal OriginalValue,
        int UsefulLifeYears,
        string? EmployeeNumber,
        string? LocationCode,
        string CostCenterCode,
        string FundNumber,
        string CurrencyCode,
        DateOnly PurchaseDate,
        DateOnly? ActivationDate,
        DateOnly? DepreciationStartDate,
        string AssetTag,
        string Barcode,
        string? SerialNumber = null,
        string? Description = null,
        string? Notes = null,
        string AcquisitionType = AssetAcquisitionTypes.Purchase);
}
