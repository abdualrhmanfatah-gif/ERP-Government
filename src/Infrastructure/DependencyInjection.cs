using System.Text;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Data.Interceptors;
using ERP_Government.Infrastructure.Identity;
using ERP_Government.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, ImmutableEntityConstraint>();
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.EnrichSqlServerDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // JWT Authentication
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured. Set Jwt:Key in configuration.");
        var key = Encoding.ASCII.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<ERP_Government.Domain.Security.Entities.User>,
            Microsoft.AspNetCore.Identity.PasswordHasher<ERP_Government.Domain.Security.Entities.User>>();

        // Approval Rules Engine
        builder.Services.AddScoped<ERP_Government.Application.Security.Common.IApprovalRuleEvaluationService,
            ERP_Government.Application.Security.Common.ApprovalRuleEvaluationService>();
        builder.Services.AddScoped<ERP_Government.Application.Security.Common.IApprovalService,
            ERP_Government.Application.Security.Common.ApprovalService>();

        // Workflow Engine
        builder.Services.AddScoped<ERP_Government.Application.Workflow.IWorkflowEngine,
            ERP_Government.Infrastructure.Services.WorkflowEngine>();

        // Outbox pattern
        builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));
        builder.Services.AddHostedService<OutboxProcessorService>();

        // Notification Service
        builder.Services.AddScoped<ERP_Government.Application.Common.Interfaces.INotificationService,
            ERP_Government.Infrastructure.Services.NotificationService>();

        // Report Audit Service
        builder.Services.AddScoped<ERP_Government.Application.Accounting.Reports.Common.ReportAuditService>();

        // Report Exporters
        builder.Services.AddScoped<ERP_Government.Infrastructure.Services.PdfReportExporter>();
        builder.Services.AddScoped<ERP_Government.Infrastructure.Services.ExcelReportExporter>();

        // Document Services
        builder.Services.AddScoped<ERP_Government.Application.Parties.Common.IDocumentStatusLogger,
            ERP_Government.Application.Security.Common.DocumentStatusLogger>();
        builder.Services.AddScoped<ERP_Government.Application.Security.Common.IAttachmentGateService,
            ERP_Government.Application.Security.Common.AttachmentGateService>();
        builder.Services.AddScoped<ERP_Government.Application.Documents.Common.IFileStorageService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var basePath = config["FileStorage:BasePath"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");
            return new ERP_Government.Infrastructure.Services.LocalFileStorageService(basePath);
        });

        // Background Job Infrastructure
        builder.Services.Configure<BackgroundJobOptions>(builder.Configuration.GetSection("BackgroundJobs"));
        builder.Services.AddBackgroundJobEngine();
        builder.Services.AddBackgroundJob<RecurringEntryProcessor>();
        builder.Services.AddBackgroundJob<BankStatementImportJob>();
        builder.Services.AddBackgroundJob<NotificationDispatchJob>();
    }
}
