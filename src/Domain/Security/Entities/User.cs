using ERP_Government.Domain.Common;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Domain.Security.Entities;

public class User : BaseAuditableEntity
{
    public string Login { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? PasswordHash { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTimeOffset? PasswordResetTokenExpiry { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaMethod { get; set; }
    public bool MustChangePassword { get; set; }
    public AccountType AccountType { get; set; }
    public int? DepartmentId { get; set; }
    public int RoleId { get; set; }
    public SecurityRole Role { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
}
