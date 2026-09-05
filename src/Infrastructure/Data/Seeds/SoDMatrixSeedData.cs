using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// SoDMatrix seed data — 8 separation of duties rules.
/// </summary>
public static class SoDMatrixSeedData
{
    public static List<SoDMatrix> GetSoDMatrix() =>
    [
        new() { PermissionAId=11, PermissionBId=16, RiskLevel=RiskLevel.High, ActionOnViolation=ActionOnViolation.Block, Description="إنشاء مشتريات + اعتماد مشتريات" },
        new() { PermissionAId=17, PermissionBId=22, RiskLevel=RiskLevel.High, ActionOnViolation=ActionOnViolation.Block, Description="إنشاء مدفوعات + اعتماد مدفوعات" },
        new() { PermissionAId=5, PermissionBId=10, RiskLevel=RiskLevel.High, ActionOnViolation=ActionOnViolation.Block, Description="إنشاء موازنة + اعتماد موازنة" },
        new() { PermissionAId=1, PermissionBId=6, RiskLevel=RiskLevel.Medium, ActionOnViolation=ActionOnViolation.Warn, Description="إنشاء قيود + ترحيل قيود" },
        new() { PermissionAId=23, PermissionBId=28, RiskLevel=RiskLevel.Medium, ActionOnViolation=ActionOnViolation.Warn, Description="إنشاء موظفين + حذف موظفين" },
        new() { PermissionAId=29, PermissionBId=34, RiskLevel=RiskLevel.Medium, ActionOnViolation=ActionOnViolation.Warn, Description="إنشاء أصول + اعتماد أصول" },
        new() { PermissionAId=35, PermissionBId=40, RiskLevel=RiskLevel.Medium, ActionOnViolation=ActionOnViolation.Warn, Description="إنشاء إيرادات + ترحيل إيرادات" },
        new() { PermissionAId=53, PermissionBId=56, RiskLevel=RiskLevel.High, ActionOnViolation=ActionOnViolation.Block, Description="إنشاء مستخدمين + حذف مستخدمين" },
    ];
}
