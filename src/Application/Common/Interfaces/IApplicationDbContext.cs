using ERP_Government.Domain.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Banking.Entities;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ERP_Government.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Module 1: Financial Settings
    DbSet<Currency> Currencies { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<FiscalYear> FiscalYears { get; }
    DbSet<FiscalPeriod> FiscalPeriods { get; }
    DbSet<DocumentSequence> DocumentSequences { get; }
    DbSet<YearEndClosingEntry> YearEndClosingEntries { get; }

    // Module 2: Security & Users
    DbSet<User> Users { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<SecurityRole> SecurityRoles { get; }
    DbSet<SecurityPermission> SecurityPermissions { get; }
    DbSet<UserPermission> UserPermissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<SecurityAuditLog> SecurityAuditLogs { get; }
    DbSet<ApprovalRule> ApprovalRules { get; }
    DbSet<ApprovalHistory> ApprovalHistory { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<DocumentStatusLog> DocumentStatusLogs { get; }
    DbSet<DocumentAttachmentRequirement> DocumentAttachmentRequirements { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditTrail> AuditTrails { get; }

    // Module 13: Workflow
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowHistory> WorkflowHistory { get; }

    // Module 3: Organizational Structure
    DbSet<OrganizationalUnit> OrganizationalUnits { get; }
    DbSet<CostCenter> CostCenters { get; }
    DbSet<Project> Projects { get; }
    DbSet<Employee> Employees { get; }

    // Module 4: General Ledger
    DbSet<AccountGroup> AccountGroups { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Journal> Journals { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }
    DbSet<PostingRule> PostingRules { get; }
    DbSet<PostingRuleLine> PostingRuleLines { get; }
    DbSet<JournalEntryTemplate> JournalEntryTemplates { get; }
    DbSet<JournalEntryTemplateLine> JournalEntryTemplateLines { get; }
    DbSet<RecurringEntry> RecurringEntries { get; }

    // Module 5: Budgeting & Funds
    DbSet<BudgetClassification> BudgetClassifications { get; }
    DbSet<Fund> Funds { get; }
    DbSet<BudgetType> BudgetTypes { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<BudgetItem> BudgetItems { get; }
    DbSet<BudgetItemAllocation> BudgetItemAllocations { get; }
    DbSet<Encumbrance> Encumbrances { get; }
    DbSet<EncumbranceLine> EncumbranceLines { get; }
    DbSet<BudgetTransaction> BudgetTransactions { get; }
    DbSet<BudgetItemMonthlyPlan> BudgetItemMonthlyPlans { get; }
    DbSet<YearClosingRun> YearClosingRuns { get; }
    DbSet<FinalAccount> FinalAccounts { get; }
    DbSet<FinalAccountLine> FinalAccountLines { get; }

    // Module 6: Procurement
    DbSet<PurchaseRequest> PurchaseRequests { get; }
    DbSet<PurchaseRequestDetail> PurchaseRequestDetails { get; }
    DbSet<Quotation> Quotations { get; }
    DbSet<QuotationDetail> QuotationDetails { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; }
    DbSet<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNote> GoodsReceiptNotes { get; }
    DbSet<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNoteDetail> GoodsReceiptNoteDetails { get; }
    DbSet<SupplierInvoice> SupplierInvoices { get; }
    DbSet<SupplierInvoiceDetail> SupplierInvoiceDetails { get; }

    // Module 7: Parties
    DbSet<Party> Parties { get; }

    // Module 8: Payments
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<PaymentOrder> PaymentOrders { get; }
    // PaymentOrderLine removed per ADR-001 D-4
    DbSet<PaymentOrderDeduction> PaymentOrderDeductions { get; }
    DbSet<DisbursementRequest> DisbursementRequests { get; }
    DbSet<Payment> Payments { get; }

    // Module 9: Committees
    DbSet<Committee> Committees { get; }
    DbSet<CommitteeMember> CommitteeMembers { get; }
    DbSet<CommitteeAssignment> CommitteeAssignments { get; }

    // Module 10: Revenue
    DbSet<RevenueReceipt> RevenueReceipts { get; }
    DbSet<RevenueReceiptLine> RevenueReceiptLines { get; }
    DbSet<ReceiptVoucher> ReceiptVouchers { get; }
    DbSet<ReceiptVoucherLine> ReceiptVoucherLines { get; }
    DbSet<Check> Checks { get; }
    DbSet<DepositSlip> DepositSlips { get; }

    // Module 11: Assets
    DbSet<AssetGroup> AssetGroups { get; }
    DbSet<Asset> Assets { get; }
    DbSet<AssetMovement> AssetMovements { get; }
    DbSet<DepreciationSchedule> DepreciationSchedules { get; }
    DbSet<AssetRevaluation> AssetRevaluations { get; }
    DbSet<AssetImpairment> AssetImpairments { get; }
    DbSet<AssetDisposal> AssetDisposals { get; }
    DbSet<AssetPhysicalCount> AssetPhysicalCounts { get; }
    DbSet<AssetPhysicalCountDetail> AssetPhysicalCountDetails { get; }

    // Module 12: Inventory
    DbSet<Location> Locations { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<ItemCategory> ItemCategories { get; }
    DbSet<ERP_Government.Domain.Inventory.Entities.Unit> Units { get; }
    DbSet<Item> Items { get; }
    DbSet<ItemUnit> ItemUnits { get; }
    DbSet<StockTransaction> StockTransactions { get; }
    DbSet<StockTake> StockTakes { get; }
    DbSet<StockTakeDetail> StockTakeDetails { get; }

    // Module 14: Banking
    DbSet<BankStatement> BankStatements { get; }
    DbSet<BankStatementLine> BankStatementLines { get; }
    DbSet<BankReconciliation> BankReconciliations { get; }
    DbSet<BankReconciliationLine> BankReconciliationLines { get; }

    // Infrastructure
    DbSet<OutboxMessage> OutboxMessages { get; }

    // Background Jobs
    DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobDefinition> BackgroundJobDefinitions { get; }
    DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobInstance> BackgroundJobInstances { get; }
    DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobExecutionLog> BackgroundJobExecutionLogs { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
