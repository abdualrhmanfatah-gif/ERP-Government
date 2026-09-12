using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using Microsoft.AspNetCore.Identity;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// User seed data — 3 example users for testing.
/// Password for all: Test123!
/// </summary>
public static class UserSeedData
{
    public static List<(User User, string RoleCode)> GetUsers()
    {
        var hasher = new PasswordHasher<User>();
        var users = new List<(User User, string RoleCode)>
        {
            (new User
            {
                Login = "administrator@localhost",
                IsActive = true,
                AccountType = AccountType.Internal,
                MustChangePassword = false,
            }, "ADMIN"),

            (new User
            {
                Login = "payment.manager@localhost",
                IsActive = true,
                AccountType = AccountType.Internal,
                MustChangePassword = false,
            }, "PAY_MGR"),

            (new User
            {
                Login = "viewer@localhost",
                IsActive = true,
                AccountType = AccountType.Internal,
                MustChangePassword = false,
            }, "VIEWER"),

            (new User
            {
                Login = "accountant@localhost",
                IsActive = true,
                AccountType = AccountType.Internal,
                MustChangePassword = false,
            }, "ACCT"),

            (new User
            {
                Login = "budget.officer@localhost",
                IsActive = true,
                AccountType = AccountType.Internal,
                MustChangePassword = false,
            }, "BUD_OFF"),
        };

        foreach (var (user, _) in users)
        {
            user.PasswordHash = hasher.HashPassword(user, "Test123!");
        }

        return users;
    }
}
