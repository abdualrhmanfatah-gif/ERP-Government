using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// AssetAttributeValue seed data — 20 values.
/// Each entry maps (AssetCode, DefinitionCode) to a typed value.
/// </summary>
public static class AssetAttributeValueSeedData
{
    /// <summary>
    /// Values as (AssetCode, DefinitionCode, TextValue, IntegerValue, DateValue, BooleanValue).
    /// Resolved to IDs by the initialiser after assets and definitions are seeded.
    /// </summary>
    public static IReadOnlyList<AssetValueBlueprint> Blueprints =>
    [
        // Vehicles
        new("AST-000003", "PLATE_NO",       TextValue: "أ ب ج 1234"),
        new("AST-000003", "MODEL_YEAR",     IntegerValue: 2024),
        new("AST-000003", "MANUFACTURER",   TextValue: "تويوتا"),

        new("AST-000004", "PLATE_NO",       TextValue: "د هـ و 5678"),
        new("AST-000004", "MODEL_YEAR",     IntegerValue: 2021),
        new("AST-000004", "MANUFACTURER",   TextValue: "هيونداي"),

        new("AST-000012", "PLATE_NO",       TextValue: "ز ح ط 9012"),
        new("AST-000012", "MODEL_YEAR",     IntegerValue: 2022),
        new("AST-000012", "MANUFACTURER",   TextValue: "نيسان"),

        new("AST-000014", "PLATE_NO",       TextValue: "ي ك ل 3456"),
        new("AST-000014", "MODEL_YEAR",     IntegerValue: 2023),
        new("AST-000014", "MANUFACTURER",   TextValue: "هيونداي"),

        // IT Equipment
        new("AST-000005", "SERIAL_NO",      TextValue: "DL7XK94"),
        new("AST-000005", "WARRANTY_END",   DateValue: new DateOnly(2028,5,1)),

        new("AST-000006", "SERIAL_NO",      TextValue: "OptiPlex-7090-001..010"),
        new("AST-000006", "WARRANTY_END",   DateValue: new DateOnly(2028,4,1)),

        new("AST-000013", "SERIAL_NO",      TextValue: "PF-3K4XMB"),
        new("AST-000013", "WARRANTY_END",   DateValue: new DateOnly(2027,2,1)),

        // Buildings
        new("AST-000001", "OPERATIONAL",    BooleanValue: true),
        new("AST-000002", "OPERATIONAL",    BooleanValue: true),

        // Furniture
        new("AST-000008", "CONDITION",      TextValue: "جيد"),
        new("AST-000009", "CONDITION",      TextValue: "ممتاز"),
    ];

    public record AssetValueBlueprint(
        string AssetCode,
        string DefinitionCode,
        string? TextValue = null,
        int? IntegerValue = null,
        DateOnly? DateValue = null,
        bool? BooleanValue = null);
}
