using System.Security.Claims;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Security.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly PasswordHasher<User> _passwordHasher;

    public IdentityService(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<string?> GetUserNameAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Login;
    }

    public async Task<bool> IsInRoleAsync(int userId, string role)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Role?.Code == role;
    }

    public async Task<bool> AuthorizeAsync(int userId, string policyName)
    {
        // Check JWT permissions first (from current request claims)
        var jwtPermissions = _user.Permissions;
        if (jwtPermissions != null && jwtPermissions.Contains(policyName))
            return true;

        // Fallback to DB check
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return false;

        var hasPermission = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == user.RoleId && rp.Permission.Code == policyName);

        if (hasPermission)
            return true;

        hasPermission = await _context.UserPermissions
            .AnyAsync(up => up.UserId == userId && up.Permission.Code == policyName);

        return hasPermission;
    }

    public async Task<(Result Result, int UserId)> CreateUserAsync(string userName, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Login == userName))
            return (Result.Failure(["User already exists."]), 0);

        var user = new User
        {
            Login = userName,
            IsActive = true,
            AccountType = Domain.Security.Enums.AccountType.Internal,
            RoleId = 1
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (Result.Success(), user.Id);
    }

    public async Task<Result> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
            return Result.Success();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        return Result.Success();
    }
}
