using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// AssetGroupAttribute seed data — 68 bindings.
/// Each entry maps a group code to an attribute definition code.
/// </summary>
public static class AssetGroupAttributeSeedData
{
    /// <summary>
    /// Bindings as (GroupCode, DefinitionCode, IsRequired, SortOrder).
    /// Resolved to IDs by the initialiser after groups and definitions are seeded.
    /// </summary>
    public static IReadOnlyList<(string GroupCode, string DefinitionCode, bool IsRequired, int SortOrder)> Bindings =>
    [
        ("AG-001-01", "DEED_NO",            true,  1),
        ("AG-001-01", "AREA_SQM",           true,  2),
        ("AG-001-01", "FLOORS_COUNT",       false, 3),
        ("AG-001-01", "CONSTRUCTION_DATE",  false, 4),
        ("AG-001-01", "HAS_ELEVATOR",       false, 5),
        ("AG-001-01", "OPERATIONAL",        false, 6),

        ("AG-001-02", "DEED_NO",            true,  1),
        ("AG-001-02", "AREA_SQM",           true,  2),
        ("AG-001-02", "FLOORS_COUNT",       false, 3),
        ("AG-001-02", "CONSTRUCTION_DATE",  false, 4),

        ("AG-002-01", "MATERIAL",           true,  1),
        ("AG-002-01", "COLOR",              false, 2),
        ("AG-002-01", "STYLE_MODEL",        false, 3),
        ("AG-002-01", "LENGTH_CM",          false, 4),
        ("AG-002-01", "WIDTH_CM",           false, 5),
        ("AG-002-01", "HEIGHT_CM",          false, 6),
        ("AG-002-01", "WEIGHT_KG",          false, 7),

        ("AG-002-02", "MATERIAL",           false, 1),
        ("AG-002-02", "POWER_KW",           false, 2),
        ("AG-002-02", "WEIGHT_KG",          false, 3),
        ("AG-002-02", "COLOR",              false, 4),

        ("AG-002-03", "POWER_KW",           true,  1),
        ("AG-002-03", "WEIGHT_KG",          false, 2),
        ("AG-002-03", "MATERIAL",           false, 3),

        ("AG-003-01", "PLATE_NO",           true,  1),
        ("AG-003-01", "CHASSIS_NO",         true,  2),
        ("AG-003-01", "MODEL_YEAR",         true,  3),
        ("AG-003-01", "MANUFACTURER",       false, 4),
        ("AG-003-01", "FUEL_TYPE",          false, 5),
        ("AG-003-01", "ENGINE_CAPACITY_L",  false, 6),
        ("AG-003-01", "SEATS_COUNT",        false, 7),
        ("AG-003-01", "INSPECTION_END",     false, 8),
        ("AG-003-01", "INSURANCE_END",      false, 9),

        ("AG-003-02", "PLATE_NO",           true,  1),
        ("AG-003-02", "CHASSIS_NO",         true,  2),
        ("AG-003-02", "MODEL_YEAR",         true,  3),
        ("AG-003-02", "PAYLOAD_KG",         false, 4),
        ("AG-003-02", "FUEL_TYPE",          false, 5),
        ("AG-003-02", "SEATS_COUNT",        false, 6),
        ("AG-003-02", "INSPECTION_END",     false, 7),

        ("AG-003-03", "FUEL_TYPE",          false, 1),
        ("AG-003-03", "WEIGHT_KG",          false, 2),
        ("AG-003-03", "SEATS_COUNT",        false, 3),

        ("AG-004-01", "SERIAL_NO",          true,  1),
        ("AG-004-01", "WARRANTY_END",       true,  2),
        ("AG-004-01", "MANUFACTURER",       false, 3),
        ("AG-004-01", "MODEL_YEAR",         false, 4),
        ("AG-004-01", "CONDITION",          false, 5),
        ("AG-004-01", "PROCESSOR",          false, 6),
        ("AG-004-01", "DEVICE_TYPE",        false, 7),
        ("AG-004-01", "MODEL",              false, 8),
        ("AG-004-01", "RAM_GB",             false, 9),
        ("AG-004-01", "RAM_TYPE",           false, 10),
        ("AG-004-01", "STORAGE_GB",         false, 11),
        ("AG-004-01", "STORAGE_TYPE",       false, 12),
        ("AG-004-01", "GRAPHICS_CARD",      false, 13),
        ("AG-004-01", "OPERATING_SYSTEM",   false, 14),
        ("AG-004-01", "SCREEN_SIZE_INCH",   false, 15),
        ("AG-004-01", "SCREEN_RESOLUTION",  false, 16),
        ("AG-004-01", "ARABIC_KEYBOARD",    false, 17),
        ("AG-004-01", "OPERATIONAL",        false, 18),

        ("AG-004-02", "SERIAL_NO",          true,  1),
        ("AG-004-02", "WARRANTY_END",       true,  2),
        ("AG-004-02", "NETWORK_PORTS",      false, 3),
        ("AG-004-02", "CONNECTIVITY",       false, 4),

        ("AG-004-03", "SERIAL_NO",          true,  1),
        ("AG-004-03", "WARRANTY_END",       true,  2),
        ("AG-004-03", "PRINT_SPEED_PPM",    false, 3),
        ("AG-004-03", "COLOR_SUPPORT",      false, 4),
        ("AG-004-03", "CONNECTIVITY",       false, 5),

        ("AG-005-01", "VERSION",            true,  1),
        ("AG-005-01", "LICENSE_TYPE",       true,  2),
        ("AG-005-01", "LICENSE_USERS",      false, 3),
        ("AG-005-01", "LICENSE_START",      false, 4),
        ("AG-005-01", "LICENSE_END",        false, 5),
        ("AG-005-01", "AUTO_RENEW",         false, 6),

        ("AG-005-02", "VERSION",            true,  1),
        ("AG-005-02", "LICENSE_TYPE",       true,  2),
        ("AG-005-02", "LICENSE_USERS",      false, 3),
        ("AG-005-02", "LICENSE_END",        false, 4),
        ("AG-005-02", "AUTO_RENEW",         false, 5),
    ];
}
