using ERP_Government.Domain.Constants;
using ERP_Government.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP_Government.Application.FunctionalTests.Infrastructure;

public static class TestApp
{
    private static int? _userId;
    private static List<string>? _roles;

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static int? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

    public static async Task<int?> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", []);
    }

    public static async Task<int?> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", [Roles.Administrator]);
    }

    public static async Task<int?> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed role if needed
        if (roles.Length > 0)
        {
            foreach (var role in roles)
            {
                if (!context.SecurityRoles.Any(r => r.Code == role))
                {
                    context.SecurityRoles.Add(new Domain.Security.Entities.SecurityRole
                    {
                        Code = role,
                        Name = role,
                        IsActive = true
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        var roleEntity = roles.Length > 0
            ? context.SecurityRoles.First(r => r.Code == roles[0])
            : context.SecurityRoles.First(r => r.Code == "USER");

        var hasher = new PasswordHasher<Domain.Security.Entities.User>();
        var user = new Domain.Security.Entities.User
        {
            Login = userName,
            IsActive = true,
            AccountType = Domain.Security.Enums.AccountType.Internal,
            RoleId = roleEntity.Id
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _userId = user.Id;
        _roles = [.. roles];
        return _userId;
    }

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}
