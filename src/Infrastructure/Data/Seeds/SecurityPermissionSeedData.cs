using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// SecurityPermission seed data — 62 permissions for Egyptian Pension Fund ERP.
/// </summary>
public static class SecurityPermissionSeedData
{
    public static List<SecurityPermission> GetPermissions()
    {
        var perms = new List<SecurityPermission>();
        

        perms.AddRange(CreateModulePermissions("ACCOUNTING", "المحاسبة"));
        perms.AddRange(CreateModulePermissions("BUDGET", "الموازنة"));
        perms.AddRange(CreateModulePermissions("PROCUREMENT", "المشتريات"));
        perms.AddRange(CreateModulePermissions("PAYMENTS", "المدفوعات"));
        perms.AddRange(CreateModulePermissionsNoPost("HR", "الموارد البشرية"));
        perms.AddRange(CreateModulePermissions("ASSETS", "الأصول"));
        perms.AddRange(CreateModulePermissions("INVENTORY", "المخزون"));
        perms.AddRange(CreateModulePermissions("REVENUE", "الإيرادات"));
        perms.AddRange(CreateModulePermissionsNoPost("COMMITTEES", "اللجان"));
        perms.AddRange(CreateModulePermissionsNoApproveNoPost("SECURITY", "الأمان"));
        perms.Add(new() { Module="REPORTS", Action="READ", Code="REPORTS_READ", Name="قراءة التقارير", PermissionLevel=PermissionLevel.Read, IsSensitive=false, DataScope=DataScope.All });
        perms.Add(new() { Module="REPORTS", Action="EXPORT", Code="REPORTS_EXPORT", Name="تصدير التقارير", PermissionLevel=PermissionLevel.Read, IsSensitive=false, DataScope=DataScope.All });
        perms.AddRange(CreateModulePermissionsNoApproveNoPost("USERS", "المستخدمين"));

        return perms;
    }

    private static List<SecurityPermission> CreateModulePermissions(string module, string nameAr)
    {
        return
        [
            new() { Module=module, Action="READ", Code=$"{module}_READ", Name=$"قراءة {nameAr}", PermissionLevel=PermissionLevel.Read, IsSensitive=false, DataScope=DataScope.All },
            new() { Module=module, Action="CREATE", Code=$"{module}_CREATE", Name=$"إنشاء {nameAr}", PermissionLevel=PermissionLevel.Create, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="WRITE", Code=$"{module}_WRITE", Name=$"تعديل {nameAr}", PermissionLevel=PermissionLevel.Write, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="DELETE", Code=$"{module}_DELETE", Name=$"حذف {nameAr}", PermissionLevel=PermissionLevel.Delete, IsSensitive=true, DataScope=DataScope.Department },
            new() { Module=module, Action="APPROVE", Code=$"{module}_APPROVE", Name=$"اعتماد {nameAr}", PermissionLevel=PermissionLevel.Approve, IsSensitive=true, DataScope=DataScope.Department },
            new() { Module=module, Action="POST", Code=$"{module}_POST", Name=$"ترحيل {nameAr}", PermissionLevel=PermissionLevel.Post, IsSensitive=true, DataScope=DataScope.Department },
        ];
    }

    private static List<SecurityPermission> CreateModulePermissionsNoPost(string module, string nameAr)
    {
        return
        [
            new() { Module=module, Action="READ", Code=$"{module}_READ", Name=$"قراءة {nameAr}", PermissionLevel=PermissionLevel.Read, IsSensitive=false, DataScope=DataScope.All },
            new() { Module=module, Action="CREATE", Code=$"{module}_CREATE", Name=$"إنشاء {nameAr}", PermissionLevel=PermissionLevel.Create, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="WRITE", Code=$"{module}_WRITE", Name=$"تعديل {nameAr}", PermissionLevel=PermissionLevel.Write, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="DELETE", Code=$"{module}_DELETE", Name=$"حذف {nameAr}", PermissionLevel=PermissionLevel.Delete, IsSensitive=true, DataScope=DataScope.Department },
            new() { Module=module, Action="APPROVE", Code=$"{module}_APPROVE", Name=$"اعتماد {nameAr}", PermissionLevel=PermissionLevel.Approve, IsSensitive=true, DataScope=DataScope.Department },
        ];
    }

    private static List<SecurityPermission> CreateModulePermissionsNoApproveNoPost(string module, string nameAr)
    {
        return
        [
            new() { Module=module, Action="READ", Code=$"{module}_READ", Name=$"قراءة {nameAr}", PermissionLevel=PermissionLevel.Read, IsSensitive=false, DataScope=DataScope.All },
            new() { Module=module, Action="CREATE", Code=$"{module}_CREATE", Name=$"إنشاء {nameAr}", PermissionLevel=PermissionLevel.Create, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="WRITE", Code=$"{module}_WRITE", Name=$"تعديل {nameAr}", PermissionLevel=PermissionLevel.Write, IsSensitive=false, DataScope=DataScope.Department },
            new() { Module=module, Action="DELETE", Code=$"{module}_DELETE", Name=$"حذف {nameAr}", PermissionLevel=PermissionLevel.Delete, IsSensitive=true, DataScope=DataScope.Department },
        ];
    }
}
