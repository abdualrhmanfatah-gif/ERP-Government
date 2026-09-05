using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Security.Common.DTOs;

// T-S001 — SecurityRoleDto
public class SecurityRoleDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string RoleLevel { get; init; } = string.Empty;
    public bool IsMutuallyExclusive { get; init; }
    public int? ExclusiveWithRoleId { get; init; }
    public bool RequiresMfa { get; init; }
    public int? MaxSessionDuration { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SecurityRole, SecurityRoleDto>()
                .ForMember(d => d.RoleLevel, opt => opt.MapFrom(s => s.RoleLevel.ToString()));
        }
    }
}

// T-S002 — SecurityPermissionDto
public class SecurityPermissionDto
{
    public int Id { get; init; }
    public string Module { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string PermissionLevel { get; init; } = string.Empty;
    public bool IsSensitive { get; init; }
    public string DataScope { get; init; } = string.Empty;
    public bool IsActive { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SecurityPermission, SecurityPermissionDto>()
                .ForMember(d => d.PermissionLevel, opt => opt.MapFrom(s => s.PermissionLevel.ToString()))
                .ForMember(d => d.DataScope, opt => opt.MapFrom(s => s.DataScope.ToString()));
        }
    }
}

// T-S003 — UserDto (list view)
public class UserDto
{
    public int Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public bool MfaEnabled { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public int FailedLoginAttempts { get; init; }
    public bool IsLocked { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.AccountType, opt => opt.MapFrom(s => s.AccountType.ToString()))
                .ForMember(d => d.IsLocked, opt => opt.MapFrom(s => s.LockedUntil.HasValue && s.LockedUntil > DateTimeOffset.UtcNow));
        }
    }
}

// T-S004 — UserDetailDto (detail view with roles and session count)
public class UserDetailDto
{
    public int Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public bool MfaEnabled { get; init; }
    public string? MfaMethod { get; init; }
    public bool MustChangePassword { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public int FailedLoginAttempts { get; init; }
    public bool IsLocked { get; init; }
    public DateTimeOffset? LockedUntil { get; init; }
    public DateTimeOffset? PasswordChangedAt { get; init; }
    public int ActiveSessionCount { get; init; }
    public UserRoleDto? Role { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserDetailDto>()
                .ForMember(d => d.AccountType, opt => opt.MapFrom(s => s.AccountType.ToString()))
                .ForMember(d => d.IsLocked, opt => opt.MapFrom(s => s.LockedUntil.HasValue && s.LockedUntil > DateTimeOffset.UtcNow));
        }
    }
}

// T-S005 — UserRoleDto (role assignment summary)
public class UserRoleDto
{
    public int RoleId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

// FEATURE-008 — RolePermissionDto
public class RolePermissionDto
{
    public int RoleId { get; init; }
    public int PermissionId { get; init; }
    public string PermissionCode { get; init; } = string.Empty;
    public string PermissionName { get; init; } = string.Empty;
}

// FEATURE-008 — UserPermissionDto
public class UserPermissionDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int PermissionId { get; init; }
    public bool IsGranted { get; init; }
    public DateTimeOffset? EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public string? Reason { get; init; }
    public int? ApprovedById { get; init; }
    public string PermissionCode { get; init; } = string.Empty;
    public string PermissionName { get; init; } = string.Empty;
}

// T022 — EffectivePermissionDto
public class EffectivePermissionDto
{
    public int PermissionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty; // "role" | "override"
    public bool IsGranted { get; init; }
    public string? Reason { get; init; }
}

// T-S006 — UserSessionDto
public class UserSessionDto
{
    public int Id { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string? UserAgent { get; init; }
    public string? DeviceFingerprint { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastActivityAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    public string? LogoutReason { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<UserSession, UserSessionDto>();
        }
    }
}
