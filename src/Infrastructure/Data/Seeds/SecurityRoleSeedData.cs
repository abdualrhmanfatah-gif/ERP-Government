using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// SecurityRole seed data — 23 roles for Egyptian Pension Fund ERP.
/// </summary>
public static class SecurityRoleSeedData
{
    public static List<SecurityRole> GetRoles() =>
    [
        // ═══ System Roles ═══
        new()
        {
            Code="ADMIN", Name="مدير النظام",
            RoleLevel=RoleLevel.System, IsSystem=true, RequiresMfa=true,
            Description="صلاحيات كاملة على النظام"
        },
        new()
        {
            Code="SYS_MGR", Name="مسؤول النظام",
            RoleLevel=RoleLevel.System, IsSystem=true, RequiresMfa=false,
            Description="إدارة المستخدمين والأمان"
        },

        // ═══ Financial Roles ═══
        new()
        {
            Code="FIN_MGR", Name="مدير المالية",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="محاسبة + موازنة + مدفوعات"
        },
        new()
        {
            Code="ACCT_SR", Name="محاسب أول",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="مراجعة + اعتماد + ترحيل محاسبي"
        },
        new()
        {
            Code="ACCT", Name="محاسب",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="إنشاء + تعديل القيود المحاسبية"
        },
        new()
        {
            Code="AUDITOR", Name="مراجع",
            RoleLevel=RoleLevel.Global, IsSystem=false, RequiresMfa=false,
            Description="قراءة + تقارير فقط"
        },

        // ═══ Budget Roles ═══
        new()
        {
            Code="BUD_MGR", Name="مدير الموازنة",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="موازنة شاملة كاملة"
        },
        new()
        {
            Code="BUD_OFF", Name="موظف موازنة",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="إدخال بيانات الموازنة"
        },

        // ═══ Procurement Roles ═══
        new()
        {
            Code="PROC_MGR", Name="مدير المشتريات",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="مشتريات كاملة"
        },
        new()
        {
            Code="PROC_OFF", Name="موظف مشتريات",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="طلبات + عروض أسعار"
        },
        new()
        {
            Code="VEND_MGR", Name="مدير الموردين",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="إدارة بيانات الموردين"
        },

        // ═══ Payment Roles ═══
        new()
        {
            Code="PAY_MGR", Name="مدير المدفوعات",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=true,
            Description="اعتماد أوامر الدفع"
        },
        new()
        {
            Code="PAY_OFF", Name="موظف مدفوعات",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="إنشاء أوامر الدفع"
        },

        // ═══ HR Roles ═══
        new()
        {
            Code="HR_MGR", Name="مدير الموارد البشرية",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="إدارة الموظفين كاملة"
        },
        new()
        {
            Code="HR_OFF", Name="موظف موارد بشرية",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="بيانات الموظفين"
        },

        // ═══ Asset Roles ═══
        new()
        {
            Code="ASST_MGR", Name="مدير الأصول",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="أصول + هلاك كامل"
        },
        new()
        {
            Code="ASST_OFF", Name="موظف أصول",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="ت track الأصول"
        },

        // ═══ Inventory Roles ═══
        new()
        {
            Code="INV_MGR", Name="مدير المخزون",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="مخزون كامل"
        },
        new()
        {
            Code="INV_OFF", Name="موظف مخزون",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="معاملات مخزون"
        },

        // ═══ Revenue Roles ═══
        new()
        {
            Code="REV_MGR", Name="مدير الإيرادات",
            RoleLevel=RoleLevel.Organization, IsSystem=false, RequiresMfa=false,
            Description="إيرادات كاملة"
        },
        new()
        {
            Code="REV_OFF", Name="موظف إيرادات",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="سندات إيراد"
        },

        // ═══ Committee Roles ═══
        new()
        {
            Code="CMIT_MEM", Name="عضو لجنة",
            RoleLevel=RoleLevel.Entity, IsSystem=false, RequiresMfa=false,
            Description="اعتماد لجان"
        },

        // ═══ Read-Only Roles ═══
        new()
        {
            Code="VIEWER", Name="مستعرض",
            RoleLevel=RoleLevel.Global, IsSystem=false, RequiresMfa=false,
            Description="قراءة + تقارير فقط"
        },
    ];
}
