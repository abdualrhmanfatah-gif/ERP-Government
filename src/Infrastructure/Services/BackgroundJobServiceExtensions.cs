using ERP_Government.Domain.BackgroundJobs.Common;
using ERP_Government.Domain.BackgroundJobs.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Extension methods for registering background job types in DI.
/// </summary>
public static class BackgroundJobServiceExtensions
{
    /// <summary>
    /// Register a background job type in DI. The job will be discovered by BackgroundJobEngine on startup.
    /// </summary>
    public static IServiceCollection AddBackgroundJob<TJob>(this IServiceCollection services)
        where TJob : class, IBackgroundJob
    {
        services.AddTransient<IBackgroundJob, TJob>();
        services.AddTransient(typeof(TJob));

        return services;
    }

    /// <summary>
    /// Register the BackgroundJobEngine and BackgroundJobStore in DI.
    /// </summary>
    public static IServiceCollection AddBackgroundJobEngine(this IServiceCollection services)
    {
        services.AddScoped<BackgroundJobStore>();
        services.AddHostedService<BackgroundJobEngine>();

        return services;
    }
}
