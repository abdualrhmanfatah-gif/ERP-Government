namespace ERP_Government.Application.Common.Security;

/// <summary>
/// Centralized permission codes for authorization policies.
/// Format: {Module}.{Action}
/// </summary>
public static class PermissionCodes
{
    // ─── FinancialSettings ────────────────────────────────────────────
    public const string CurrenciesView = "Currencies.View";
    public const string CurrenciesCreate = "Currencies.Create";
    public const string CurrenciesUpdate = "Currencies.Update";
    public const string CurrenciesActivate = "Currencies.Activate";
    public const string CurrenciesDeactivate = "Currencies.Deactivate";

    public const string ExchangeRatesView = "ExchangeRates.View";
    public const string ExchangeRatesCreate = "ExchangeRates.Create";
    public const string ExchangeRatesUpdate = "ExchangeRates.Update";
    public const string ExchangeRatesActivate = "ExchangeRates.Activate";
    public const string ExchangeRatesDeactivate = "ExchangeRates.Deactivate";

    public const string FiscalYearsView = "FiscalYears.View";
    public const string FiscalYearsCreate = "FiscalYears.Create";
    public const string FiscalYearsUpdate = "FiscalYears.Update";
    public const string FiscalYearsOpen = "FiscalYears.Open";
    public const string FiscalYearsClose = "FiscalYears.Close";

    public const string FiscalPeriodsView = "FiscalPeriods.View";
    public const string FiscalPeriodsCreate = "FiscalPeriods.Create";
    public const string FiscalPeriodsUpdate = "FiscalPeriods.Update";
    public const string FiscalPeriodsLock = "FiscalPeriods.Lock";
    public const string FiscalPeriodsUnlock = "FiscalPeriods.Unlock";

    public const string ClosingEntriesView = "ClosingEntries.View";
    public const string ClosingEntriesGenerate = "ClosingEntries.Generate";
    public const string ClosingEntriesApprove = "ClosingEntries.Approve";
    public const string ClosingEntriesReverse = "ClosingEntries.Reverse";

    public const string DocumentSequencesView = "DocumentSequences.View";
    public const string DocumentSequencesCreate = "DocumentSequences.Create";
    public const string DocumentSequencesUpdate = "DocumentSequences.Update";
    public const string DocumentSequencesDeactivate = "DocumentSequences.Deactivate";

    // ─── Accounting ───────────────────────────────────────────────────
    public const string ChartOfAccountsRead = "Accounting.ChartOfAccounts.Read";
    public const string ChartOfAccountsCreate = "Accounting.ChartOfAccounts.Create";
    public const string ChartOfAccountsEdit = "Accounting.ChartOfAccounts.Edit";

    public const string JournalsRead = "Accounting.Journals.Read";
    public const string JournalsCreate = "Accounting.Journals.Create";
    public const string JournalsEdit = "Accounting.Journals.Edit";

    public const string JournalEntriesRead = "Accounting.JournalEntries.Read";
    public const string JournalEntriesCreate = "Accounting.JournalEntries.Create";
    public const string JournalEntriesSubmit = "Accounting.JournalEntries.Submit";
    public const string JournalEntriesApprove = "Accounting.JournalEntries.Approve";
    public const string JournalEntriesPost = "Accounting.JournalEntries.Post";
    public const string JournalEntriesReverse = "Accounting.JournalEntries.Reverse";
    public const string JournalEntriesUpdateLines = "Accounting.JournalEntries.UpdateLines";
    public const string JournalEntriesCancel = "Accounting.JournalEntries.Cancel";

    public const string PostingRulesRead = "Accounting.PostingRules.Read";
    public const string PostingRulesCreate = "Accounting.PostingRules.Create";
    public const string PostingRulesEdit = "Accounting.PostingRules.Edit";
    public const string PostingRulesDelete = "Accounting.PostingRules.Delete";

    public const string TemplatesRead = "Accounting.Templates.Read";
    public const string TemplatesCreate = "Accounting.Templates.Create";
    public const string TemplatesUpdate = "Accounting.Templates.Update";

    public const string RecurringEntriesRead = "Accounting.RecurringEntries.Read";
    public const string RecurringEntriesCreate = "Accounting.RecurringEntries.Create";
    public const string RecurringEntriesPause = "Accounting.RecurringEntries.Pause";
    public const string RecurringEntriesResume = "Accounting.RecurringEntries.Resume";
    public const string RecurringEntriesCancel = "Accounting.RecurringEntries.Cancel";

    public const string AccountingEventsRead = "Accounting.AccountingEvents.Read";

    public const string BalancesRebuild = "Accounting.Balances.Rebuild";
    public const string BalancesFinalize = "Accounting.Balances.Finalize";
    public const string BalancesUnfinalize = "Accounting.Balances.Unfinalize";
    public const string BalancesRead = "Accounting.Balances.Read";

    // ─── Budgeting ────────────────────────────────────────────────────
    public const string FundsView = "Funds.View";
    public const string FundsCreate = "Funds.Create";
    public const string FundsUpdate = "Funds.Update";
    public const string FundsActivate = "Funds.Activate";
    public const string FundsDeactivate = "Funds.Deactivate";
    public const string FundsToggleActive = "Funds.ToggleActive";

    public const string BudgetsView = "Budgets.View";
    public const string BudgetsCreate = "Budgets.Create";
    public const string BudgetsUpdate = "Budgets.Update";
    public const string BudgetsSubmit = "Budgets.Submit";
    public const string BudgetsApprove = "Budgets.Approve";
    public const string BudgetsActivate = "Budgets.Activate";
    public const string BudgetsSuspend = "Budgets.Suspend";
    public const string BudgetsClose = "Budgets.Close";
    public const string BudgetsCancel = "Budgets.Cancel";

    public const string BudgetItemsView = "BudgetItems.View";
    public const string BudgetItemsCreate = "BudgetItems.Create";
    public const string BudgetItemsUpdate = "BudgetItems.Update";
    public const string BudgetItemsDelete = "BudgetItems.Delete";
    public const string BudgetItemsMove = "BudgetItems.Move";

    public const string BudgetClassificationsView = "BudgetClassifications.View";
    public const string BudgetClassificationsCreate = "BudgetClassifications.Create";
    public const string BudgetClassificationsUpdate = "BudgetClassifications.Update";
    public const string BudgetClassificationsToggleActive = "BudgetClassifications.ToggleActive";

    public const string BudgetTypesView = "BudgetTypes.View";
    public const string BudgetTypesCreate = "BudgetTypes.Create";
    public const string BudgetTypesUpdate = "BudgetTypes.Update";
    public const string BudgetTypesToggleActive = "BudgetTypes.ToggleActive";

    public const string AppropriationsView = "Appropriations.View";
    public const string AppropriationsCreate = "Appropriations.Create";
    public const string AppropriationsUpdate = "Appropriations.Update";
    public const string AppropriationsDelete = "Appropriations.Delete";
    public const string AppropriationsSubmit = "Appropriations.Submit";
    public const string AppropriationsApprove = "Appropriations.Approve";
    public const string AppropriationsActivate = "Appropriations.Activate";
    public const string AppropriationsSuspend = "Appropriations.Suspend";
    public const string AppropriationsClose = "Appropriations.Close";
    public const string AppropriationsCancel = "Appropriations.Cancel";
    public const string AppropriationsReverse = "Appropriations.Reverse";

    public const string EncumbrancesView = "Encumbrances.View";
    public const string EncumbrancesCreate = "Encumbrances.Create";
    public const string EncumbrancesSubmit = "Encumbrances.Submit";
    public const string EncumbrancesApprove = "Encumbrances.Approve";
    public const string EncumbrancesActivate = "Encumbrances.Activate";
    public const string EncumbrancesRelease = "Encumbrances.Release";
    public const string EncumbrancesSuspend = "Encumbrances.Suspend";
    public const string EncumbrancesClose = "Encumbrances.Close";
    public const string EncumbrancesCancel = "Encumbrances.Cancel";
    public const string EncumbrancesReverse = "Encumbrances.Reverse";
    public const string EncumbrancesUpdate = "Encumbrances.Update";
    public const string EncumbrancesDelete = "Encumbrances.Delete";

    // ─── Parties ──────────────────────────────────────────────────
    public const string PartiesView = "Parties.View";
    public const string PartiesCreate = "Parties.Create";
    public const string PartiesUpdate = "Parties.Update";

    public const string PurchaseRequestsView = "PurchaseRequests.View";
    public const string PurchaseRequestsCreate = "PurchaseRequests.Create";
    public const string PurchaseRequestsSubmit = "PurchaseRequests.Submit";
    public const string PurchaseRequestsApprove = "PurchaseRequests.Approve";
    public const string PurchaseRequestsReject = "PurchaseRequests.Reject";

    public const string RFQView = "RFQ.View";
    public const string RFQCreate = "RFQ.Create";
    public const string RFQPublish = "RFQ.Publish";
    public const string RFQComplete = "RFQ.Complete";
    public const string RFQCancel = "RFQ.Cancel";

    public const string QuotationsView = "Quotations.View";
    public const string QuotationsCreate = "Quotations.Create";

    public const string PurchaseOrdersView = "PurchaseOrders.View";
    public const string PurchaseOrdersCreate = "PurchaseOrders.Create";
    public const string PurchaseOrdersSubmit = "PurchaseOrders.Submit";
    public const string PurchaseOrdersApprove = "PurchaseOrders.Approve";
    public const string PurchaseOrdersCancel = "PurchaseOrders.Cancel";

    // ─── Payments ─────────────────────────────────────────────────────
    public const string BankAccountsView = "BankAccounts.View";
    public const string BankAccountsCreate = "BankAccounts.Create";
    public const string BankAccountsUpdate = "BankAccounts.Update";
    public const string BankAccountsActivate = "BankAccounts.Activate";
    public const string BankAccountsDeactivate = "BankAccounts.Deactivate";

    public const string PaymentOrdersView = "PaymentOrders.View";
    public const string PaymentOrdersCreate = "PaymentOrders.Create";
    public const string PaymentOrdersUpdate = "PaymentOrders.Update";
    public const string PaymentOrdersSubmit = "PaymentOrders.Submit";
    public const string PaymentOrdersApprove = "PaymentOrders.Approve";
    public const string PaymentOrdersReject = "PaymentOrders.Reject";
    public const string PaymentOrdersCancel = "PaymentOrders.Cancel";
    public const string PaymentOrdersSendToTreasury = "PaymentOrders.SendToTreasury";
    public const string PaymentOrdersVoid = "PaymentOrders.Void";

    // ─── Disbursements ────────────────────────────────────────────────
    public const string DisbursementRequestsView = "DisbursementRequests.View";
    public const string DisbursementRequestsCreate = "DisbursementRequests.Create";
    public const string DisbursementRequestsSubmit = "DisbursementRequests.Submit";
    public const string DisbursementRequestsApprove = "DisbursementRequests.Approve";
    public const string DisbursementRequestsReject = "DisbursementRequests.Reject";
    public const string DisbursementRequestsCancel = "DisbursementRequests.Cancel";

    // ─── Payments ─────────────────────────────────────────────────────
    public const string PaymentsView = "Payments.View";
    public const string PaymentsCreate = "Payments.Create";

    // ─── Committees ───────────────────────────────────────────────────
    public const string CommitteesView = "Committees.View";
    public const string CommitteesCreate = "Committees.Create";
    public const string CommitteesUpdate = "Committees.Update";
    public const string CommitteesActivate = "Committees.Activate";
    public const string CommitteesDeactivate = "Committees.Deactivate";
    public const string CommitteesDissolve = "Committees.Dissolve";

    public const string CommitteeMembersView = "CommitteeMembers.View";
    public const string CommitteeMembersAdd = "CommitteeMembers.Add";
    public const string CommitteeMembersRemove = "CommitteeMembers.Remove";

    public const string CommitteeAssignmentsView = "CommitteeAssignments.View";
    public const string CommitteeAssignmentsCreate = "CommitteeAssignments.Create";
    public const string CommitteeAssignmentsComplete = "CommitteeAssignments.Complete";

    // ─── Revenue ──────────────────────────────────────────────────────
    public const string RevenueReceiptsView = "RevenueReceipts.View";
    public const string RevenueReceiptsCreate = "RevenueReceipts.Create";
    public const string RevenueReceiptsApprove = "RevenueReceipts.Approve";
    public const string RevenueReceiptsPost = "RevenueReceipts.Post";
    public const string RevenueReceiptsCancel = "RevenueReceipts.Cancel";

    // ─── ReceiptVouchers ─────────────────────────────────────────────
    public const string ReceiptVouchersView = "ReceiptVouchers.View";
    public const string ReceiptVouchersCreate = "ReceiptVouchers.Create";
    public const string ReceiptVouchersSubmit = "ReceiptVouchers.Submit";
    public const string ReceiptVouchersApprove = "ReceiptVouchers.Approve";
    public const string ReceiptVouchersCancel = "ReceiptVouchers.Cancel";

    // ─── DepositSlips ────────────────────────────────────────────────
    public const string DepositSlipsView = "DepositSlips.View";
    public const string DepositSlipsCreate = "DepositSlips.Create";
    public const string DepositSlipsUpdate = "DepositSlips.Update";
    public const string DepositSlipsApprove = "DepositSlips.Approve";

    // ─── Checks ──────────────────────────────────────────────────────
    public const string ChecksView = "Checks.View";
    public const string ChecksClear = "Checks.Clear";
    public const string ChecksBounce = "Checks.Bounce";
    public const string ChecksReplace = "Checks.Replace";

    // ─── Assets ───────────────────────────────────────────────────────
    public const string AssetsView = "Assets.View";
    public const string AssetsCreate = "Assets.Create";
    public const string AssetsUpdate = "Assets.Update";

    public const string AssetGroupsView = "AssetGroups.View";
    public const string AssetGroupsCreate = "AssetGroups.Create";
    public const string AssetGroupsUpdate = "AssetGroups.Update";

    public const string AssetMovementsView = "AssetMovements.View";
    public const string AssetMovementsCreate = "AssetMovements.Create";
    public const string AssetMovementsApprove = "AssetMovements.Approve";

    public const string AssetDisposalsView = "AssetDisposals.View";
    public const string AssetDisposalsCreate = "AssetDisposals.Create";
    public const string AssetDisposalsApprove = "AssetDisposals.Approve";
    public const string AssetDisposalsPost = "AssetDisposals.Post";

    public const string AssetRevaluationsView = "AssetRevaluations.View";
    public const string AssetRevaluationsCreate = "AssetRevaluations.Create";
    public const string AssetRevaluationsApprove = "AssetRevaluations.Approve";
    public const string AssetRevaluationsPost = "AssetRevaluations.Post";

    public const string AssetImpairmentsView = "AssetImpairments.View";
    public const string AssetImpairmentsCreate = "AssetImpairments.Create";
    public const string AssetImpairmentsApprove = "AssetImpairments.Approve";
    public const string AssetImpairmentsPost = "AssetImpairments.Post";

    public const string DepreciationView = "Depreciation.View";
    public const string DepreciationPost = "Depreciation.Post";
    public const string DepreciationReverse = "Depreciation.Reverse";

    // ─── Inventory ────────────────────────────────────────────────────
    public const string ItemsView = "Items.View";
    public const string ItemsCreate = "Items.Create";
    public const string ItemsUpdate = "Items.Update";

    public const string ItemCategoriesView = "ItemCategories.View";
    public const string ItemCategoriesCreate = "ItemCategories.Create";
    public const string ItemCategoriesUpdate = "ItemCategories.Update";

    public const string UnitsView = "Units.View";
    public const string UnitsCreate = "Units.Create";
    public const string UnitsUpdate = "Units.Update";

    public const string WarehousesView = "Warehouses.View";
    public const string WarehousesCreate = "Warehouses.Create";
    public const string WarehousesUpdate = "Warehouses.Update";

    public const string LocationsView = "Locations.View";
    public const string LocationsCreate = "Locations.Create";
    public const string LocationsUpdate = "Locations.Update";

    public const string GoodsReceiptNotesView = "GoodsReceiptNotes.View";
    public const string GoodsReceiptNotesCreate = "GoodsReceiptNotes.Create";
    public const string GoodsReceiptNotesApprove = "GoodsReceiptNotes.Approve";

    public const string StockTakesView = "StockTakes.View";
    public const string StockTakesCreate = "StockTakes.Create";
    public const string StockTakesStart = "StockTakes.Start";
    public const string StockTakesComplete = "StockTakes.Complete";
    public const string StockTakesApprove = "StockTakes.Approve";

    public const string StockTransactionsView = "StockTransactions.View";

    // ─── Banking ──────────────────────────────────────────────────────
    public const string BankStatementsView = "BankStatements.View";
    public const string BankStatementsCreate = "BankStatements.Create";
    public const string BankStatementsImport = "BankStatements.Import";

    public const string BankReconciliationView = "BankReconciliation.View";
    public const string BankReconciliationCreate = "BankReconciliation.Create";
    public const string BankReconciliationApprove = "BankReconciliation.Approve";

    // ─── Approval Rules ───────────────────────────────────────────────
    public const string ApprovalRulesView = "ApprovalRules.View";
    public const string ApprovalRulesManage = "ApprovalRules.Manage";

    // ─── Workflow ─────────────────────────────────────────────────────
    public const string WorkflowDefinitionsView = "WorkflowDefinitions.View";
    public const string WorkflowDefinitionsManage = "WorkflowDefinitions.Manage";
    public const string WorkflowInstancesView = "WorkflowInstances.View";
    public const string WorkflowInstancesExecute = "WorkflowInstances.Execute";
    public const string WorkflowHistoryView = "WorkflowHistory.View";

    // ─── Security ─────────────────────────────────────────────────────
    public const string RolesView = "Roles.View";
    public const string RolesCreate = "Roles.Create";
    public const string RolesUpdate = "Roles.Update";
    public const string RolesDelete = "Roles.Delete";

    public const string PermissionsView = "Permissions.View";

    // ─── Users ───────────────────────────────────────────────────────
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDeactivate = "Users.Deactivate";
    public const string UsersManageSessions = "Users.ManageSessions";
    public const string UsersResetLogin = "Users.ResetLogin";
    public const string UsersManageRoles = "Users.ManageRoles";

    // ─── System ───────────────────────────────────────────────────────
    public const string SystemAdmin = "System.Admin";

    // ─── Organization ─────────────────────────────────────────────────
    public const string OrgUnitsView = "OrgUnits.View";
    public const string OrgUnitsCreate = "OrgUnits.Create";
    public const string OrgUnitsUpdate = "OrgUnits.Update";
    public const string OrgUnitsDelete = "OrgUnits.Delete";

    public const string EmployeesView = "Employees.View";
    public const string EmployeesCreate = "Employees.Create";
    public const string EmployeesUpdate = "Employees.Update";
    public const string EmployeesDelete = "Employees.Delete";

    public const string CostCentersView = "CostCenters.View";
    public const string CostCentersCreate = "CostCenters.Create";
    public const string CostCentersUpdate = "CostCenters.Update";
    public const string CostCentersDelete = "CostCenters.Delete";

    public const string ProjectsView = "Projects.View";
    public const string ProjectsCreate = "Projects.Create";
    public const string ProjectsUpdate = "Projects.Update";
    public const string ProjectsDelete = "Projects.Delete";

    // ─── Accounting Reports ────────────────────────────────────────────
    public const string ViewBalanceSheet = "Accounting.Reports.BalanceSheet";
    public const string ViewIncomeStatement = "Accounting.Reports.IncomeStatement";
    public const string ViewGeneralLedger = "Accounting.Reports.GeneralLedger";
    public const string ViewCashFlow = "Accounting.Reports.CashFlow";
    public const string ViewTrialBalance = "Accounting.Reports.TrialBalance";
    public const string ExportReports = "Accounting.Reports.Export";
    public const string PrintReports = "Accounting.Reports.Print";

    // ─── BackgroundJobs ──────────────────────────────────────────────
    public const string BackgroundJobsView = "BackgroundJobs.View";
    public const string BackgroundJobsManage = "BackgroundJobs.Manage";

    // ─── Notifications ──────────────────────────────────────────────
    public const string NotificationsView = "Notifications.View";
    public const string NotificationsMarkRead = "Notifications.MarkRead";
    public const string NotificationsDelete = "Notifications.Delete";
    public const string NotificationsCreate = "Notifications.Create";

    // ─── FinancialControl ────────────────────────────────────────────
    public const string FinancialControlLapseYear = "FinancialControl.LapseYear";
    public const string FinancialControlApproveFinalAccount = "FinancialControl.ApproveFinalAccount";

    // ─── Reporting ──────────────────────────────────────────────────
    public const string ReportingViewBudgetExecution = "Reporting.ViewBudgetExecution";
    public const string ReportingViewRevenueCollections = "Reporting.ViewRevenueCollections";
    public const string ReportingViewDisbursementRegister = "Reporting.ViewDisbursementRegister";
    public const string ReportingViewAvailabilitySnapshot = "Reporting.ViewAvailabilitySnapshot";
    public const string ReportingViewTrialBalanceReport = "Reporting.ViewTrialBalanceReport";
    public const string ReportingExportReports = "Reporting.ExportReports";
}
