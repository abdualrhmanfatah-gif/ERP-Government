using ERP_Government.Domain.Common;

namespace ERP_Government.Domain.Security.Entities;

public class UserSession : BaseEntity
{
    public int UserId { get; set; }
    public string SessionTokenHash { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? DeviceFingerprint { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public DateTimeOffset AbsoluteExpiryAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? LogoutReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public User User { get; set; } = null!;
}
