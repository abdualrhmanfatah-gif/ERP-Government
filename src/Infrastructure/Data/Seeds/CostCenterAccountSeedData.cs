using ERP_Government.Domain.Organization.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// CostCenterAccount seed data — 6 mappings.
/// </summary>
public static class CostCenterAccountSeedData
{
    public static List<CostCenterAccount> GetMappings() =>
    [
        new() { CostCenterId=1, AccountId=1 },  // CC-FIN → إيرادات العمليات التجارية
        new() { CostCenterId=1, AccountId=3 },  // CC-FIN → إيرادات قطاع الخدمات
        new() { CostCenterId=2, AccountId=79 }, // CC-PROC → مشتريات محلية
        new() { CostCenterId=3, AccountId=30 }, // CC-HR → مرتبات الموظفين الدائمين
        new() { CostCenterId=4, AccountId=44 }, // CC-WH → المواد البترولية
        new() { CostCenterId=5, AccountId=56 }, // CC-ADMIN → صيانة المباني والطرق
    ];
}
