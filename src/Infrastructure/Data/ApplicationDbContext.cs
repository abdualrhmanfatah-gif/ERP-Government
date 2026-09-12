using System.Reflection;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Banking.Entities;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Committees.Entities;
using ERP_Government.Domain.FinancialSettings.Entities;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Procurement.Entities;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Module 1: Financial Settings
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<YearEndClosingEntry> YearEndClosingEntries => Set<YearEndClosingEntry>();

    // Module 2: Security & Users
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<SecurityRole> SecurityRoles => Set<SecurityRole>();
    public DbSet<SecurityPermission> SecurityPermissions => Set<SecurityPermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();
    public DbSet<ApprovalRule> ApprovalRules => Set<ApprovalRule>();
    public DbSet<ApprovalHistory> ApprovalHistory => Set<ApprovalHistory>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<DocumentStatusLog> DocumentStatusLogs => Set<DocumentStatusLog>();
    public DbSet<DocumentAttachmentRequirement> DocumentAttachmentRequirements => Set<DocumentAttachmentRequirement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    // Module 13: Workflow
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowHistory> WorkflowHistory => Set<WorkflowHistory>();

    // Module 3: Organizational Structure
    public DbSet<OrganizationalUnit> OrganizationalUnits => Set<OrganizationalUnit>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Employee> Employees => Set<Employee>();

    // Module 4: General Ledger
    public DbSet<AccountGroup> AccountGroups => Set<AccountGroup>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Journal> Journals => Set<Journal>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<PostingRule> PostingRules => Set<PostingRule>();
    public DbSet<PostingRuleLine> PostingRuleLines => Set<PostingRuleLine>();
    public DbSet<JournalEntryTemplate> JournalEntryTemplates => Set<JournalEntryTemplate>();
    public DbSet<JournalEntryTemplateLine> JournalEntryTemplateLines => Set<JournalEntryTemplateLine>();
    public DbSet<RecurringEntry> RecurringEntries => Set<RecurringEntry>();

    // Module 5: Budgeting & Funds
    public DbSet<BudgetClassification> BudgetClassifications => Set<BudgetClassification>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<BudgetType> BudgetTypes => Set<BudgetType>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<BudgetItemAllocation> BudgetItemAllocations => Set<BudgetItemAllocation>();
    public DbSet<Encumbrance> Encumbrances => Set<Encumbrance>();
    public DbSet<EncumbranceLine> EncumbranceLines => Set<EncumbranceLine>();
    public DbSet<BudgetTransaction> BudgetTransactions => Set<BudgetTransaction>();
    public DbSet<BudgetItemMonthlyPlan> BudgetItemMonthlyPlans => Set<BudgetItemMonthlyPlan>();
    public DbSet<YearClosingRun> YearClosingRuns => Set<YearClosingRun>();
    public DbSet<FinalAccount> FinalAccounts => Set<FinalAccount>();
    public DbSet<FinalAccountLine> FinalAccountLines => Set<FinalAccountLine>();

    // Module 6: Procurement (Target)
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestDetail> PurchaseRequestDetails => Set<PurchaseRequestDetail>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationDetail> QuotationDetails => Set<QuotationDetail>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNote> GoodsReceiptNotes => Set<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNote>();
    public DbSet<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNoteDetail> GoodsReceiptNoteDetails => Set<ERP_Government.Domain.Procurement.Entities.GoodsReceiptNoteDetail>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<SupplierInvoiceDetail> SupplierInvoiceDetails => Set<SupplierInvoiceDetail>();

    // Module 7: Parties
    public DbSet<ERP_Government.Domain.Parties.Entities.Party> Parties => Set<ERP_Government.Domain.Parties.Entities.Party>();

    // Module 8: Payments
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    // PaymentOrderLine removed per ADR-001 D-4
    public DbSet<PaymentOrderDeduction> PaymentOrderDeductions => Set<PaymentOrderDeduction>();
    public DbSet<DisbursementRequest> DisbursementRequests => Set<DisbursementRequest>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Module 8: Committees
    public DbSet<Committee> Committees => Set<Committee>();
    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<CommitteeAssignment> CommitteeAssignments => Set<CommitteeAssignment>();

    // Module 9: Revenue
    public DbSet<RevenueReceipt> RevenueReceipts => Set<RevenueReceipt>();
    public DbSet<RevenueReceiptLine> RevenueReceiptLines => Set<RevenueReceiptLine>();
    public DbSet<ReceiptVoucher> ReceiptVouchers => Set<ReceiptVoucher>();
    public DbSet<ReceiptVoucherLine> ReceiptVoucherLines => Set<ReceiptVoucherLine>();
    public DbSet<Check> Checks => Set<Check>();
    public DbSet<DepositSlip> DepositSlips => Set<DepositSlip>();

    // Module 10: Assets (Target)
    public DbSet<AssetGroup> AssetGroups => Set<AssetGroup>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();
    public DbSet<DepreciationSchedule> DepreciationSchedules => Set<DepreciationSchedule>();
    public DbSet<AssetRevaluation> AssetRevaluations => Set<AssetRevaluation>();
    public DbSet<AssetImpairment> AssetImpairments => Set<AssetImpairment>();
    public DbSet<AssetDisposal> AssetDisposals => Set<AssetDisposal>();
    public DbSet<AssetPhysicalCount> AssetPhysicalCounts => Set<AssetPhysicalCount>();
    public DbSet<AssetPhysicalCountDetail> AssetPhysicalCountDetails => Set<AssetPhysicalCountDetail>();

    // Module 11: Inventory (Target)
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<ERP_Government.Domain.Inventory.Entities.Unit> Units => Set<ERP_Government.Domain.Inventory.Entities.Unit>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemUnit> ItemUnits => Set<ItemUnit>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<StockTake> StockTakes => Set<StockTake>();
    public DbSet<StockTakeDetail> StockTakeDetails => Set<StockTakeDetail>();

    // Module 12: Banking
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<BankReconciliationLine> BankReconciliationLines => Set<BankReconciliationLine>();

    // Infrastructure
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // Background Jobs
    public DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobDefinition> BackgroundJobDefinitions => Set<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobDefinition>();
    public DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobInstance> BackgroundJobInstances => Set<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobInstance>();
    public DbSet<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobExecutionLog> BackgroundJobExecutionLogs => Set<ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobExecutionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
