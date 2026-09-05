using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Security.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ERP_Government.Web.Endpoint;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login");
        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    [EndpointSummary("Log in")]
    [EndpointDescription("Authenticates a user and returns a JWT token.")]
    public static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IApplicationDbContext db,
        [FromServices] IConfiguration config)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Login == request.Email);

        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTimeOffset.UtcNow)
            return Results.Json(new { error = "Account is locked." }, statusCode: 423);

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, request.Password);

        if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
                user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
            await db.SaveChangesAsync(CancellationToken.None);
            return Results.Unauthorized();
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(CancellationToken.None);

        var permissions = await db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role?.Code ?? string.Empty)
        };

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var jwtKey = config["Jwt:Key"] ?? "ERP_Government_DefaultKey_2026!ChangeInProduction";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = tokenString, userId = user.Id, role = user.Role?.Code });
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user.")]
    public static async Task<IResult> Logout(HttpContext http)
    {
        // JWT is stateless — client discards token
        await Task.CompletedTask;
        return Results.Ok();
    }

    public record LoginRequest(string Email, string Password);
}
