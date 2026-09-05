using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptVoucherDepositSlip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingEvents_Moves_MoveId",
                table: "AccountingEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Appropriations_BudgetItems_BudgetItemId",
                table: "Appropriations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appropriations_Budgets_BudgetId",
                table: "Appropriations");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetClassifications_BudgetClassifications_ParentId",
                table: "BudgetClassifications");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Accounts_AccountId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_BudgetClassifications_BudgetClassificationId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_BudgetItems_ParentId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Budgets_BudgetId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_CostCenters_CostCenterId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Funds_FundId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_BudgetTypes_BudgetTypeId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_FiscalYears_FiscalYearId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Funds_FundId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Encumbrances_Appropriations_AppropriationId",
                table: "Encumbrances");

            migrationBuilder.DropForeignKey(
                name: "FK_Encumbrances_Encumbrances_ReversalOfId",
                table: "Encumbrances");

            migrationBuilder.DropForeignKey(
                name: "FK_Encumbrances_PurchaseOrders_PurchaseOrderId",
                table: "Encumbrances");

            migrationBuilder.DropForeignKey(
                name: "FK_Encumbrances_Suppliers_VendorId",
                table: "Encumbrances");

            migrationBuilder.DropForeignKey(
                name: "FK_Funds_Accounts_DefaultRevenueDebitAccountId",
                table: "Funds");

            migrationBuilder.DropForeignKey(
                name: "FK_Funds_FiscalYears_FiscalYearId",
                table: "Funds");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringEntries_Moves_GeneratedMoveId",
                table: "RecurringEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringEntryExecutionLogs_Moves_GeneratedMoveId",
                table: "RecurringEntryExecutionLogs");

            migrationBuilder.DropTable(
                name: "MoveLines");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Moves");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_MoveId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_AccountingEvents_MoveId",
                table: "AccountingEvents");

            migrationBuilder.DropIndex(
                name: "IX_AccountingEvents_SourceTable_SourceId_EventType",
                table: "AccountingEvents");

            migrationBuilder.DropColumn(
                name: "AmountNet",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "BaseAmountNet",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "CancelledById",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "IsFullyPaid",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "MoveId",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalDeductionAmount",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalNetAmount",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalPaidAmount",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalRemainingAmount",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "VoidedAt",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "AllocatedAmount",
                table: "PaymentOrderLines");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "PaymentOrderLines");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "PaymentOrderLines");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "PaymentOrderLines");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "PaymentOrderDeductions");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "YearEndClosingEntries",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "RevenueReceipts",
                newName: "JournalEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_RevenueReceipts_MoveId",
                table: "RevenueReceipts",
                newName: "IX_RevenueReceipts_JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "GeneratedMoveId",
                table: "RecurringEntryExecutionLogs",
                newName: "GeneratedJournalEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringEntryExecutionLogs_GeneratedMoveId",
                table: "RecurringEntryExecutionLogs",
                newName: "IX_RecurringEntryExecutionLogs_GeneratedJournalEntryId");

            migrationBuilder.RenameColumn(
                name: "GeneratedMoveId",
                table: "RecurringEntries",
                newName: "GeneratedJournalEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringEntries_GeneratedMoveId",
                table: "RecurringEntries",
                newName: "IX_RecurringEntries_GeneratedJournalEntryId");

            migrationBuilder.RenameColumn(
                name: "VoidedById",
                table: "PaymentOrders",
                newName: "VendorPartyId");

            migrationBuilder.RenameColumn(
                name: "RejectedById",
                table: "PaymentOrders",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "ClosingMoveId",
                table: "FiscalYears",
                newName: "ClosingJournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "DepreciationSchedules",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveLineId",
                table: "BankStatementLines",
                newName: "JournalEntryLineId");

            migrationBuilder.RenameIndex(
                name: "IX_BankStatementLines_MoveLineId",
                table: "BankStatementLines",
                newName: "IX_BankStatementLines_JournalEntryLineId");

            migrationBuilder.RenameColumn(
                name: "MoveLineId",
                table: "BankReconciliationLines",
                newName: "JournalEntryLineId");

            migrationBuilder.RenameIndex(
                name: "IX_BankReconciliationLines_MoveLineId",
                table: "BankReconciliationLines",
                newName: "IX_BankReconciliationLines_JournalEntryLineId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "AssetRevaluations",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "AssetMovements",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "AssetImpairments",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "AssetDisposals",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "SourceTable",
                table: "AccountingEvents",
                newName: "SourceDocumentType");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "AccountingEvents",
                newName: "SourceDocumentId");

            migrationBuilder.RenameColumn(
                name: "MoveId",
                table: "AccountingEvents",
                newName: "JournalEntryId");

            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                table: "RFQSuppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "RevenueReceipts",
                type: "int",
                nullable: false,
                defaultValue: 6,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                table: "Quotations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierPartyId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "PaymentOrders",
                type: "int",
                nullable: false,
                defaultValue: 6,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<string>(
                name: "LegalAuthority",
                table: "Funds",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FundNumber",
                table: "Funds",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Funds",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Encumbrances",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VendorPartyId",
                table: "Encumbrances",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Budgets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "BudgetItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentTypeCode",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Attachments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "ApprovalHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStep",
                table: "ApprovalHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetBudgetItemId",
                table: "Appropriations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AccountingEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "EventCategory",
                table: "AccountingEvents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BudgetItemMonthlyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetItemId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItemMonthlyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetItemMonthlyPlans_BudgetItems_BudgetItemId",
                        column: x => x.BudgetItemId,
                        principalTable: "BudgetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlipNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SlipDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FormType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositSlips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAttachmentRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttachmentTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAttachmentRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentStatusLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ref = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EntryType = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    EntryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: true),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceEventId = table.Column<int>(type: "int", nullable: true),
                    ReversalOfId = table.Column<int>(type: "int", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostedById = table.Column<int>(type: "int", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledById = table.Column<int>(type: "int", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_AccountingEvents_SourceEventId",
                        column: x => x.SourceEventId,
                        principalTable: "AccountingEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_FiscalPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "FiscalPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_FiscalYears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_JournalEntries_ReversalOfId",
                        column: x => x.ReversalOfId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Users_CancelledById",
                        column: x => x.CancelledById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Users_PostedById",
                        column: x => x.PostedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartyType = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    FundId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    BudgetItemId = table.Column<int>(type: "int", nullable: true),
                    EncumbranceId = table.Column<int>(type: "int", nullable: true),
                    PaymentOrderId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.CheckConstraint("CK_JournalEntryLines_DebitCreditXOR", "([Debit] > 0) != ([Credit] > 0)");
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VoucherDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepositSlipId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedById = table.Column<int>(type: "int", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReviewedById = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptVouchers_DepositSlips_DepositSlipId",
                        column: x => x.DepositSlipId,
                        principalTable: "DepositSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptVouchers_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptVoucherId = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CheckNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CheckDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BouncedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReplacementVoucherId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checks_ReceiptVouchers_ReceiptVoucherId",
                        column: x => x.ReceiptVoucherId,
                        principalTable: "ReceiptVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checks_ReceiptVouchers_ReplacementVoucherId",
                        column: x => x.ReplacementVoucherId,
                        principalTable: "ReceiptVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptVoucherLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptVoucherId = table.Column<int>(type: "int", nullable: false),
                    RevenueAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptVoucherLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptVoucherLines_ReceiptVouchers_ReceiptVoucherId",
                        column: x => x.ReceiptVoucherId,
                        principalTable: "ReceiptVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierPartyId",
                table: "PurchaseOrders",
                column: "SupplierPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_JournalEntryId",
                table: "PaymentOrders",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Appropriations_TargetBudgetItemId",
                table: "Appropriations",
                column: "TargetBudgetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvents_EventType_SourceDocumentType_SourceDocumentId_Status",
                table: "AccountingEvents",
                columns: new[] { "EventType", "SourceDocumentType", "SourceDocumentId", "Status" },
                unique: true,
                filter: "[Status] = 'Posted'");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvents_JournalEntryId",
                table: "AccountingEvents",
                column: "JournalEntryId",
                unique: true,
                filter: "[JournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItemMonthlyPlans_BudgetItemId",
                table: "BudgetItemMonthlyPlans",
                column: "BudgetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItemMonthlyPlans_BudgetItemId_Month",
                table: "BudgetItemMonthlyPlans",
                columns: new[] { "BudgetItemId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Checks_ReceiptVoucherId",
                table: "Checks",
                column: "ReceiptVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_ReplacementVoucherId",
                table: "Checks",
                column: "ReplacementVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_Checks_Status",
                table: "Checks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DepositSlips_FormType",
                table: "DepositSlips",
                column: "FormType");

            migrationBuilder.CreateIndex(
                name: "IX_DepositSlips_SlipDate",
                table: "DepositSlips",
                column: "SlipDate");

            migrationBuilder.CreateIndex(
                name: "IX_DepositSlips_SlipNumber",
                table: "DepositSlips",
                column: "SlipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositSlips_Status",
                table: "DepositSlips",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAttachmentRequirements_DocumentType",
                table: "DocumentAttachmentRequirements",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAttachmentRequirements_DocumentType_AttachmentTypeCode",
                table: "DocumentAttachmentRequirements",
                columns: new[] { "DocumentType", "AttachmentTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentStatusLogs_ChangedAt",
                table: "DocumentStatusLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentStatusLogs_EntityName_DocumentId",
                table: "DocumentStatusLogs",
                columns: new[] { "EntityName", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CancelledById",
                table: "JournalEntries",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryStatus_DocumentDate",
                table: "JournalEntries",
                columns: new[] { "EntryStatus", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_FiscalYearId",
                table: "JournalEntries",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalId",
                table: "JournalEntries",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PeriodId",
                table: "JournalEntries",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PostedById",
                table: "JournalEntries",
                column: "PostedById");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversalOfId",
                table: "JournalEntries",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SourceEventId",
                table: "JournalEntries",
                column: "SourceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId_JournalEntryId",
                table: "JournalEntryLines",
                columns: new[] { "AccountId", "JournalEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_BudgetItemId",
                table: "JournalEntryLines",
                column: "BudgetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_CostCenterId",
                table: "JournalEntryLines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_CurrencyId",
                table: "JournalEntryLines",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_EncumbranceId",
                table: "JournalEntryLines",
                column: "EncumbranceId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_FundId",
                table: "JournalEntryLines",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_PaymentOrderId",
                table: "JournalEntryLines",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_ProjectId",
                table: "JournalEntryLines",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_NameAr",
                table: "Parties",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_PartyCode",
                table: "Parties",
                column: "PartyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parties_PartyType_IsActive",
                table: "Parties",
                columns: new[] { "PartyType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Parties_TaxNumber",
                table: "Parties",
                column: "TaxNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVoucherLines_ReceiptVoucherId",
                table: "ReceiptVoucherLines",
                column: "ReceiptVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVoucherLines_RevenueAccountId",
                table: "ReceiptVoucherLines",
                column: "RevenueAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_DepositSlipId",
                table: "ReceiptVouchers",
                column: "DepositSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_PartyId",
                table: "ReceiptVouchers",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_Status",
                table: "ReceiptVouchers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_VoucherDate",
                table: "ReceiptVouchers",
                column: "VoucherDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptVouchers_VoucherNumber",
                table: "ReceiptVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingEvents_JournalEntries_JournalEntryId",
                table: "AccountingEvents",
                column: "JournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appropriations_BudgetItems_BudgetItemId",
                table: "Appropriations",
                column: "BudgetItemId",
                principalTable: "BudgetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appropriations_Budgets_BudgetId",
                table: "Appropriations",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Budgets_BudgetId",
                table: "BudgetItems",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_BudgetTypes_BudgetTypeId",
                table: "Budgets",
                column: "BudgetTypeId",
                principalTable: "BudgetTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_FiscalYears_FiscalYearId",
                table: "Budgets",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Funds_FundId",
                table: "Budgets",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Encumbrances_Appropriations_AppropriationId",
                table: "Encumbrances",
                column: "AppropriationId",
                principalTable: "Appropriations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringEntries_JournalEntries_GeneratedJournalEntryId",
                table: "RecurringEntries",
                column: "GeneratedJournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringEntryExecutionLogs_JournalEntries_GeneratedJournalEntryId",
                table: "RecurringEntryExecutionLogs",
                column: "GeneratedJournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingEvents_JournalEntries_JournalEntryId",
                table: "AccountingEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Appropriations_BudgetItems_BudgetItemId",
                table: "Appropriations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appropriations_Budgets_BudgetId",
                table: "Appropriations");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Budgets_BudgetId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_BudgetTypes_BudgetTypeId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_FiscalYears_FiscalYearId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Funds_FundId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Encumbrances_Appropriations_AppropriationId",
                table: "Encumbrances");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringEntries_JournalEntries_GeneratedJournalEntryId",
                table: "RecurringEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringEntryExecutionLogs_JournalEntries_GeneratedJournalEntryId",
                table: "RecurringEntryExecutionLogs");

            migrationBuilder.DropTable(
                name: "BudgetItemMonthlyPlans");

            migrationBuilder.DropTable(
                name: "Checks");

            migrationBuilder.DropTable(
                name: "DocumentAttachmentRequirements");

            migrationBuilder.DropTable(
                name: "DocumentStatusLogs");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "ReceiptVoucherLines");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "ReceiptVouchers");

            migrationBuilder.DropTable(
                name: "DepositSlips");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_SupplierPartyId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_JournalEntryId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_Appropriations_TargetBudgetItemId",
                table: "Appropriations");

            migrationBuilder.DropIndex(
                name: "IX_AccountingEvents_EventType_SourceDocumentType_SourceDocumentId_Status",
                table: "AccountingEvents");

            migrationBuilder.DropIndex(
                name: "IX_AccountingEvents_JournalEntryId",
                table: "AccountingEvents");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "RFQSuppliers");

            migrationBuilder.DropColumn(
                name: "PartyId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "SupplierPartyId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "VendorPartyId",
                table: "Encumbrances");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "AttachmentTypeCode",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "ApprovalStep",
                table: "ApprovalHistory");

            migrationBuilder.DropColumn(
                name: "TargetBudgetItemId",
                table: "Appropriations");

            migrationBuilder.DropColumn(
                name: "EventCategory",
                table: "AccountingEvents");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "YearEndClosingEntries",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "RevenueReceipts",
                newName: "MoveId");

            migrationBuilder.RenameIndex(
                name: "IX_RevenueReceipts_JournalEntryId",
                table: "RevenueReceipts",
                newName: "IX_RevenueReceipts_MoveId");

            migrationBuilder.RenameColumn(
                name: "GeneratedJournalEntryId",
                table: "RecurringEntryExecutionLogs",
                newName: "GeneratedMoveId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringEntryExecutionLogs_GeneratedJournalEntryId",
                table: "RecurringEntryExecutionLogs",
                newName: "IX_RecurringEntryExecutionLogs_GeneratedMoveId");

            migrationBuilder.RenameColumn(
                name: "GeneratedJournalEntryId",
                table: "RecurringEntries",
                newName: "GeneratedMoveId");

            migrationBuilder.RenameIndex(
                name: "IX_RecurringEntries_GeneratedJournalEntryId",
                table: "RecurringEntries",
                newName: "IX_RecurringEntries_GeneratedMoveId");

            migrationBuilder.RenameColumn(
                name: "VendorPartyId",
                table: "PaymentOrders",
                newName: "VoidedById");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "PaymentOrders",
                newName: "RejectedById");

            migrationBuilder.RenameColumn(
                name: "ClosingJournalEntryId",
                table: "FiscalYears",
                newName: "ClosingMoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "DepreciationSchedules",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryLineId",
                table: "BankStatementLines",
                newName: "MoveLineId");

            migrationBuilder.RenameIndex(
                name: "IX_BankStatementLines_JournalEntryLineId",
                table: "BankStatementLines",
                newName: "IX_BankStatementLines_MoveLineId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryLineId",
                table: "BankReconciliationLines",
                newName: "MoveLineId");

            migrationBuilder.RenameIndex(
                name: "IX_BankReconciliationLines_JournalEntryLineId",
                table: "BankReconciliationLines",
                newName: "IX_BankReconciliationLines_MoveLineId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "AssetRevaluations",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "AssetMovements",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "AssetImpairments",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "AssetDisposals",
                newName: "MoveId");

            migrationBuilder.RenameColumn(
                name: "SourceDocumentType",
                table: "AccountingEvents",
                newName: "SourceTable");

            migrationBuilder.RenameColumn(
                name: "SourceDocumentId",
                table: "AccountingEvents",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "AccountingEvents",
                newName: "MoveId");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "RevenueReceipts",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 6);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "PaymentOrders",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountNet",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "PaymentOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "PaymentOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmountNet",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "PaymentOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                table: "PaymentOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CancelledById",
                table: "PaymentOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullyPaid",
                table: "PaymentOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MoveId",
                table: "PaymentOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RejectedAt",
                table: "PaymentOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "PaymentOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductionAmount",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNetAmount",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaidAmount",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRemainingAmount",
                table: "PaymentOrders",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "PaymentOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VoidedAt",
                table: "PaymentOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AllocatedAmount",
                table: "PaymentOrderLines",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "PaymentOrderLines",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "PaymentOrderLines",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "PaymentOrderLines",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "PaymentOrderDeductions",
                type: "decimal(23,2)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LegalAuthority",
                table: "Funds",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FundNumber",
                table: "Funds",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Funds",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Encumbrances",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Budgets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AccountingEvents",
                type: "int",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "Moves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CancelledById = table.Column<int>(type: "int", nullable: true),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: true),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    PostedById = table.Column<int>(type: "int", nullable: true),
                    ReversalOfMoveId = table.Column<int>(type: "int", nullable: true),
                    SourceEventId = table.Column<int>(type: "int", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EntryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntryType = table.Column<int>(type: "int", maxLength: 20, nullable: true),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Ref = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moves_AccountingEvents_SourceEventId",
                        column: x => x.SourceEventId,
                        principalTable: "AccountingEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_FiscalPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "FiscalPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_FiscalYears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_Moves_ReversalOfMoveId",
                        column: x => x.ReversalOfMoveId,
                        principalTable: "Moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_Users_CancelledById",
                        column: x => x.CancelledById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Moves_Users_PostedById",
                        column: x => x.PostedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommercialRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IndustryClassification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupplierNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoveLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    MoveId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoveLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoveLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoveLines_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoveLines_Moves_MoveId",
                        column: x => x.MoveId,
                        principalTable: "Moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_MoveId",
                table: "PaymentOrders",
                column: "MoveId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvents_MoveId",
                table: "AccountingEvents",
                column: "MoveId",
                unique: true,
                filter: "[MoveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingEvents_SourceTable_SourceId_EventType",
                table: "AccountingEvents",
                columns: new[] { "SourceTable", "SourceId", "EventType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MoveLines_AccountId",
                table: "MoveLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveLines_AccountId_MoveId",
                table: "MoveLines",
                columns: new[] { "AccountId", "MoveId" });

            migrationBuilder.CreateIndex(
                name: "IX_MoveLines_CostCenterId",
                table: "MoveLines",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveLines_CurrencyId",
                table: "MoveLines",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MoveLines_MoveId",
                table: "MoveLines",
                column: "MoveId");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_CancelledById",
                table: "Moves",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_EntryNumber",
                table: "Moves",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Moves_EntryStatus_DocumentDate",
                table: "Moves",
                columns: new[] { "EntryStatus", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Moves_FiscalYearId",
                table: "Moves",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_JournalId",
                table: "Moves",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_PeriodId",
                table: "Moves",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_PostedById",
                table: "Moves",
                column: "PostedById");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_ReversalOfMoveId",
                table: "Moves",
                column: "ReversalOfMoveId");

            migrationBuilder.CreateIndex(
                name: "IX_Moves_SourceEventId",
                table: "Moves",
                column: "SourceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsActive",
                table: "Suppliers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Status",
                table: "Suppliers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingEvents_Moves_MoveId",
                table: "AccountingEvents",
                column: "MoveId",
                principalTable: "Moves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appropriations_BudgetItems_BudgetItemId",
                table: "Appropriations",
                column: "BudgetItemId",
                principalTable: "BudgetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appropriations_Budgets_BudgetId",
                table: "Appropriations",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetClassifications_BudgetClassifications_ParentId",
                table: "BudgetClassifications",
                column: "ParentId",
                principalTable: "BudgetClassifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Accounts_AccountId",
                table: "BudgetItems",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_BudgetClassifications_BudgetClassificationId",
                table: "BudgetItems",
                column: "BudgetClassificationId",
                principalTable: "BudgetClassifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_BudgetItems_ParentId",
                table: "BudgetItems",
                column: "ParentId",
                principalTable: "BudgetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Budgets_BudgetId",
                table: "BudgetItems",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_CostCenters_CostCenterId",
                table: "BudgetItems",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Funds_FundId",
                table: "BudgetItems",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_BudgetTypes_BudgetTypeId",
                table: "Budgets",
                column: "BudgetTypeId",
                principalTable: "BudgetTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_FiscalYears_FiscalYearId",
                table: "Budgets",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Funds_FundId",
                table: "Budgets",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encumbrances_Appropriations_AppropriationId",
                table: "Encumbrances",
                column: "AppropriationId",
                principalTable: "Appropriations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encumbrances_Encumbrances_ReversalOfId",
                table: "Encumbrances",
                column: "ReversalOfId",
                principalTable: "Encumbrances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encumbrances_PurchaseOrders_PurchaseOrderId",
                table: "Encumbrances",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Encumbrances_Suppliers_VendorId",
                table: "Encumbrances",
                column: "VendorId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Funds_Accounts_DefaultRevenueDebitAccountId",
                table: "Funds",
                column: "DefaultRevenueDebitAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Funds_FiscalYears_FiscalYearId",
                table: "Funds",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringEntries_Moves_GeneratedMoveId",
                table: "RecurringEntries",
                column: "GeneratedMoveId",
                principalTable: "Moves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringEntryExecutionLogs_Moves_GeneratedMoveId",
                table: "RecurringEntryExecutionLogs",
                column: "GeneratedMoveId",
                principalTable: "Moves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
