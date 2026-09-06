using System.Reflection;
using ERP_Government.Application.Accounting.Common.Interfaces;
using ERP_Government.Application.Accounting.Common.Services;
using ERP_Government.Application.Accounting.EventHandlers;

using ERP_Government.Application.Common.Behaviours;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Security;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg => 
            cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            
        });

        // Authorization services
        builder.Services.AddScoped<IPermissionService, PermissionService>();

        builder.Services.AddScoped<IDocumentSequenceService, DocumentSequenceService>();
        builder.Services.AddScoped<IExchangeRateResolver, ExchangeRateResolver>();
        builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
        builder.Services.AddScoped<IBudgetAvailabilityService, BudgetAvailabilityService>();
        builder.Services.AddScoped<PostingRuleMatcher>();
        builder.Services.AddScoped<JournalEntryGenerator>();
        builder.Services.AddScoped<AccountingEventAuditor>();
        builder.Services.AddScoped<RetryPolicy>();
    }
}
