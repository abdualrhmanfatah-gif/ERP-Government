using ERP_Government.Domain.Assets.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// AssetGroup seed data: main groups with practical sub-groups.
/// </summary>
public static class AssetGroupSeedData
{
    public static List<AssetGroup> GetAssetGroups() =>
    [
        new() { Code="AG-001", Name="مباني وإنشاءات", Description="المجموعة الرئيسية للمباني والمنشآت", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=50, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-002", Name="أثاث ومعدات", Description="المجموعة الرئيسية للأثاث والمعدات العامة", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=10, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-003", Name="مركبات", Description="المجموعة الرئيسية للمركبات ووسائل النقل", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-004", Name="أجهزة وتقنية معلومات", Description="المجموعة الرئيسية للأجهزة والأنظمة التقنية", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-005", Name="أصول غير ملموسة", Description="المجموعة الرئيسية للبرمجيات والتراخيص", DepreciationMethod="StraightLine", DefaultUsefulLifeYears=3, IsDepreciable=true, AssetCategory="Intangible" },

        new() { Code="AG-001-01", Name="مباني إدارية", ParentAssetGroupId=1, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=50, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-001-02", Name="مباني خدمية", ParentAssetGroupId=1, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=40, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-001-03", Name="منشآت ومرافق", ParentAssetGroupId=1, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=25, IsDepreciable=true, AssetCategory="Tangible" },

        new() { Code="AG-002-01", Name="أثاث مكتبي", ParentAssetGroupId=2, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=10, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-002-02", Name="معدات مكتبية", ParentAssetGroupId=2, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=7, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-002-03", Name="معدات تشغيلية", ParentAssetGroupId=2, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=10, IsDepreciable=true, AssetCategory="Tangible" },

        new() { Code="AG-003-01", Name="سيارات ركوب", ParentAssetGroupId=3, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-003-02", Name="حافلات ومركبات نقل", ParentAssetGroupId=3, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=7, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-003-03", Name="دراجات ومركبات خفيفة", ParentAssetGroupId=3, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=4, IsDepreciable=true, AssetCategory="Tangible" },

        new() { Code="AG-004-01", Name="أجهزة حاسب", ParentAssetGroupId=4, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-004-02", Name="خوادم ومعدات شبكات", ParentAssetGroupId=4, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=5, IsDepreciable=true, AssetCategory="Tangible" },
        new() { Code="AG-004-03", Name="طابعات وماسحات", ParentAssetGroupId=4, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=4, IsDepreciable=true, AssetCategory="Tangible" },

        new() { Code="AG-005-01", Name="برمجيات", ParentAssetGroupId=5, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=3, IsDepreciable=true, AssetCategory="Intangible" },
        new() { Code="AG-005-02", Name="تراخيص رقمية", ParentAssetGroupId=5, DepreciationMethod="StraightLine", DefaultUsefulLifeYears=3, IsDepreciable=true, AssetCategory="Intangible" },
    ];

    /// <summary>
    /// Account code mappings per group code: (AssetAccountCode, AccumulatedDepreciationAccountCode,
    /// DepreciationExpenseAccountCode, DisposalAccountCode). Resolved to IDs by the initialiser
    /// after accounts are seeded.
    /// </summary>
    public static IReadOnlyDictionary<string, AssetGroupAccountMapping> AccountMappings =>
        new Dictionary<string, AssetGroupAccountMapping>
        {
            ["AG-001-01"] = new AssetGroupAccountMapping("1121", "2311", "35111", "2812"),
            ["AG-001-02"] = new AssetGroupAccountMapping("1122", "2311", "35111", "2812"),
            ["AG-001-03"] = new AssetGroupAccountMapping("1122", "2311", "35111", "2812"),
            ["AG-002-01"] = new AssetGroupAccountMapping("1151", "2314", "35114", "2812"),
            ["AG-002-02"] = new AssetGroupAccountMapping("1159", "2312", "35112", "2812"),
            ["AG-002-03"] = new AssetGroupAccountMapping("1131", "2312", "35112", "2812"),
            ["AG-003-01"] = new AssetGroupAccountMapping("1141", "2313", "35113", "2812"),
            ["AG-003-02"] = new AssetGroupAccountMapping("1141", "2313", "35113", "2812"),
            ["AG-003-03"] = new AssetGroupAccountMapping("1141", "2313", "35113", "2812"),
            ["AG-004-01"] = new AssetGroupAccountMapping("1158", "2312", "35112", "2812"),
            ["AG-004-02"] = new AssetGroupAccountMapping("1158", "2312", "35112", "2812"),
            ["AG-004-03"] = new AssetGroupAccountMapping("1159", "2312", "35112", "2812"),
            ["AG-005-01"] = new AssetGroupAccountMapping("1161", "2316", "35115", "2812"),
            ["AG-005-02"] = new AssetGroupAccountMapping("1162", "2316", "35115", "2812"),
        };

    /// <summary>
    /// Account codes for one asset group's posting accounts.
    /// </summary>
    public sealed record AssetGroupAccountMapping(
        string AssetAccountCode,
        string AccumulatedDepreciationAccountCode,
        string DepreciationExpenseAccountCode,
        string DisposalAccountCode);
}
