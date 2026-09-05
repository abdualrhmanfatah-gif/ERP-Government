using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using ERP_Government.Domain.Security.Entities;
using ERP_Government.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Middleware;

public class RecordSessionOnLoginMiddleware(RequestDelegate next, ILogger<RecordSessionOnLoginMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        string? email = null;
        var isLogin = context.Request.Path.Value?.Equals("/api/Users/login", StringComparison.OrdinalIgnoreCase) == true
            && HttpMethods.IsPost(context.Request.Method);

        if (isLogin)
        {
            context.Request.EnableBuffering();
            try
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("email", out var e))
                        email = e.GetString();
                    else if (doc.RootElement.TryGetProperty("Email", out var e2))
                        email = e2.GetString();
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to parse login body for session recording");
            }
        }

        await next(context);

        if (!isLogin || context.Response.StatusCode != 200 || string.IsNullOrWhiteSpace(email))
            return;

        try
        {
            var domainUser = await db.Users.FirstOrDefaultAsync(u => u.Login == email);
            if (domainUser is null)
                return;

            var ip = context.Connection.RemoteIpAddress?.ToString()
                ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? "0.0.0.0";
            var ua = context.Request.Headers.UserAgent.FirstOrDefault();

            var now = DateTimeOffset.UtcNow;

            var recentExists = await db.UserSessions.AnyAsync(s =>
                s.UserId == domainUser.Id &&
                s.IpAddress == ip &&
                s.CreatedAt > now.AddMinutes(-5));

            if (recentExists)
                return;

            var session = new UserSession
            {
                UserId = domainUser.Id,
                IpAddress = ip,
                UserAgent = ua,
                DeviceFingerprint = context.Request.Headers["X-Device-Fingerprint"].FirstOrDefault(),
                SessionTokenHash = Hash(Guid.NewGuid().ToString()),
                RefreshTokenHash = Hash(Guid.NewGuid().ToString()),
                CreatedAt = now,
                LastActivityAt = now,
                ExpiresAt = now.AddHours(8),
                RefreshTokenExpiresAt = now.AddDays(7),
                AbsoluteExpiryAt = now.AddDays(7),
                IsRevoked = false
            };

            db.UserSessions.Add(session);

            domainUser.LastLoginAt = now;

            await db.SaveChangesAsync();

            logger.LogInformation("Session recorded for user {UserId} from {Ip}", domainUser.Id, ip);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record session for {Email}", email);
        }
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
