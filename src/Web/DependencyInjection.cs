using System.Text.Json.Serialization;
using Azure.Identity;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUser, CurrentUser>();
        builder.Services.AddScoped<IRequestContext, RequestContext>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
        });

        builder.Services.AddCors();

        builder.Services.ConfigureHttpJsonOptions(o =>
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Authorization policies — each policy maps to a permission code.
        // Policies use RequireAssertion (open) until real RBAC is wired per DEP-020.
        builder.Services.AddAuthorization(options =>
        {
            // ─── FinancialSettings ────────────────────────────────────
            options.AddPolicy(PermissionCodes.CurrenciesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CurrenciesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CurrenciesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CurrenciesActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CurrenciesDeactivate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.ExchangeRatesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ExchangeRatesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ExchangeRatesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ExchangeRatesActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ExchangeRatesDeactivate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.FiscalYearsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalYearsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalYearsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalYearsOpen, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalYearsClose, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.FiscalPeriodsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalPeriodsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalPeriodsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalPeriodsLock, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FiscalPeriodsUnlock, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.DocumentSequencesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DocumentSequencesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DocumentSequencesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DocumentSequencesDeactivate, p => p.RequireAssertion(_ => true));

            // ─── Accounting ───────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ChartOfAccountsRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ChartOfAccountsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ChartOfAccountsEdit, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.JournalsRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalsEdit, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.JournalEntriesRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesPost, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesReverse, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesUpdateLines, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.JournalEntriesCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.PostingRulesRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PostingRulesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PostingRulesEdit, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.TemplatesRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.TemplatesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.TemplatesUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.RecurringEntriesRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RecurringEntriesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RecurringEntriesPause, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RecurringEntriesResume, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RecurringEntriesCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AccountingEventsRead, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BalancesRebuild, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BalancesFinalize, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BalancesUnfinalize, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BalancesRead, p => p.RequireAssertion(_ => true));

            // ─── Accounting Reports ────────────────────────────────────
            options.AddPolicy(PermissionCodes.ViewBalanceSheet, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ViewIncomeStatement, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ViewGeneralLedger, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ViewCashFlow, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ViewTrialBalance, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ExportReports, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PrintReports, p => p.RequireAssertion(_ => true));

            // ─── Budgeting ────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.FundsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FundsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FundsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FundsToggleActive, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BudgetsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsSuspend, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsClose, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetsCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BudgetItemsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetItemsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetItemsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetItemsDelete, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BudgetClassificationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetClassificationsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetClassificationsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BudgetTypesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetTypesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BudgetTypesUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AppropriationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsDelete, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsSuspend, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsClose, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AppropriationsCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.EncumbrancesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesRelease, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesSuspend, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesClose, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesCancel, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EncumbrancesReverse, p => p.RequireAssertion(_ => true));

            // ─── Parties ──────────────────────────────────────────
            options.AddPolicy(PermissionCodes.PartiesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PartiesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PartiesUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.PurchaseRequestsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseRequestsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseRequestsSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseRequestsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseRequestsReject, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.RFQView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RFQCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RFQPublish, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RFQComplete, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RFQCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.QuotationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.QuotationsCreate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.PurchaseOrdersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseOrdersCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseOrdersSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseOrdersApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PurchaseOrdersCancel, p => p.RequireAssertion(_ => true));

            // ─── Payments ─────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.BankAccountsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankAccountsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankAccountsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankAccountsActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankAccountsDeactivate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.PaymentOrdersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersReject, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersCancel, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersSendToTreasury, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentOrdersVoid, p => p.RequireAssertion(_ => true));

            // ─── Disbursements ────────────────────────────────────────
            options.AddPolicy(PermissionCodes.DisbursementRequestsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DisbursementRequestsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DisbursementRequestsSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DisbursementRequestsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DisbursementRequestsReject, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DisbursementRequestsCancel, p => p.RequireAssertion(_ => true));

            // ─── Payments ─────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.PaymentsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.PaymentsCreate, p => p.RequireAssertion(_ => true));

            // ─── Committees ───────────────────────────────────────────
            options.AddPolicy(PermissionCodes.CommitteesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteesActivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteesDeactivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteesDissolve, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.CommitteeMembersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteeMembersAdd, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteeMembersRemove, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.CommitteeAssignmentsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteeAssignmentsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CommitteeAssignmentsComplete, p => p.RequireAssertion(_ => true));

            // ─── Revenue ──────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.RevenueReceiptsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RevenueReceiptsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RevenueReceiptsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RevenueReceiptsPost, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RevenueReceiptsCancel, p => p.RequireAssertion(_ => true));

            // ─── Assets ───────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.AssetsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AssetGroupsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetGroupsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetGroupsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AssetMovementsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetMovementsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetMovementsApprove, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AssetDisposalsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetDisposalsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetDisposalsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetDisposalsPost, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AssetRevaluationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetRevaluationsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetRevaluationsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetRevaluationsPost, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.AssetImpairmentsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetImpairmentsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetImpairmentsApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.AssetImpairmentsPost, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.DepreciationView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DepreciationPost, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DepreciationReverse, p => p.RequireAssertion(_ => true));

            // ─── Inventory ────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ItemsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ItemsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ItemsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.ItemCategoriesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ItemCategoriesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ItemCategoriesUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.UnitsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UnitsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UnitsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.WarehousesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.WarehousesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.WarehousesUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.LocationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.LocationsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.LocationsUpdate, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.GoodsReceiptNotesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.GoodsReceiptNotesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.GoodsReceiptNotesApprove, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.StockTakesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.StockTakesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.StockTakesStart, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.StockTakesComplete, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.StockTakesApprove, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.StockTransactionsView, p => p.RequireAssertion(_ => true));

            // ─── Banking ──────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.BankStatementsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankStatementsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankStatementsImport, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.BankReconciliationView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankReconciliationCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BankReconciliationApprove, p => p.RequireAssertion(_ => true));

            // ─── Security ─────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.RolesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RolesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RolesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.RolesDelete, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.PermissionsView, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.UsersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersDeactivate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersManageSessions, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersResetLogin, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.UsersManageRoles, p => p.RequireAssertion(_ => true));

            // ─── Organization ─────────────────────────────────────────
            options.AddPolicy(PermissionCodes.OrgUnitsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.OrgUnitsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.OrgUnitsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.OrgUnitsDelete, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.EmployeesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EmployeesCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EmployeesUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.EmployeesDelete, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.CostCentersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CostCentersCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CostCentersUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.CostCentersDelete, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.ProjectsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ProjectsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ProjectsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ProjectsDelete, p => p.RequireAssertion(_ => true));

            // ─── BackgroundJobs ──────────────────────────────────────────
            options.AddPolicy(PermissionCodes.BackgroundJobsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.BackgroundJobsManage, p => p.RequireAssertion(_ => true));

            // ─── Notifications ──────────────────────────────────────────
            options.AddPolicy(PermissionCodes.NotificationsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.NotificationsMarkRead, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.NotificationsDelete, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.NotificationsCreate, p => p.RequireAssertion(_ => true));

            // ─── FinancialControl ──────────────────────────────────────
            options.AddPolicy(PermissionCodes.FinancialControlLapseYear, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.FinancialControlApproveFinalAccount, p => p.RequireAssertion(_ => true));

            // ─── ClosingEntries ──────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ClosingEntriesView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ClosingEntriesGenerate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ClosingEntriesApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ClosingEntriesReverse, p => p.RequireAssertion(_ => true));

            // ─── Revenue ─────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ReceiptVouchersView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReceiptVouchersCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReceiptVouchersSubmit, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReceiptVouchersApprove, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReceiptVouchersCancel, p => p.RequireAssertion(_ => true));

            options.AddPolicy(PermissionCodes.DepositSlipsView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DepositSlipsCreate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DepositSlipsUpdate, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.DepositSlipsApprove, p => p.RequireAssertion(_ => true));

            // ─── Checks ────────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ChecksView, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ChecksClear, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ChecksBounce, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ChecksReplace, p => p.RequireAssertion(_ => true));

            // ─── Reporting ────────────────────────────────────────────
            options.AddPolicy(PermissionCodes.ReportingViewBudgetExecution, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReportingViewRevenueCollections, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReportingViewDisbursementRegister, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReportingViewAvailabilitySnapshot, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReportingViewTrialBalanceReport, p => p.RequireAssertion(_ => true));
            options.AddPolicy(PermissionCodes.ReportingExportReports, p => p.RequireAssertion(_ => true));
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
