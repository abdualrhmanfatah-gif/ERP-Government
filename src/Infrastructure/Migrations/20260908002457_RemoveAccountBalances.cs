using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountBalanceDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    FiscalPeriodId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    OpeningDebit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    OpeningCredit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    ClosingDebit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    ClosingCredit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalanceDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO AccountBalanceDeletionReport
                    (AccountId, FiscalYearId, FiscalPeriodId, CurrencyId, OpeningDebit, OpeningCredit, Debit, Credit, ClosingDebit, ClosingCredit, IsFinalized, FinalizedAt, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    AccountId, FiscalYearId, FiscalPeriodId, CurrencyId, OpeningDebit, OpeningCredit, Debit, Credit, ClosingDebit, ClosingCredit, IsFinalized, FinalizedAt, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM AccountBalances;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'AccountBalances', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table AccountBalances; rows preserved in AccountBalanceDeletionReport. Balances now computed live from JournalEntryLines.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "AccountBalances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    FiscalPeriodId = table.Column<int>(type: "int", nullable: false),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    ClosingCredit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    ClosingDebit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningCredit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    OpeningDebit = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalances_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountBalances_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountBalances_FiscalPeriods_FiscalPeriodId",
                        column: x => x.FiscalPeriodId,
                        principalTable: "FiscalPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountBalances_FiscalYears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_AccountId",
                table: "AccountBalances",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_AccountId_FiscalYearId_FiscalPeriodId_CurrencyId",
                table: "AccountBalances",
                columns: new[] { "AccountId", "FiscalYearId", "FiscalPeriodId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_CurrencyId",
                table: "AccountBalances",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_FiscalPeriodId",
                table: "AccountBalances",
                column: "FiscalPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_FiscalYearId",
                table: "AccountBalances",
                column: "FiscalYearId");

            migrationBuilder.Sql("""
                INSERT INTO AccountBalances
                    (AccountId, FiscalYearId, FiscalPeriodId, CurrencyId, OpeningDebit, OpeningCredit, Debit, Credit, ClosingDebit, ClosingCredit, IsFinalized, FinalizedAt, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    AccountId, FiscalYearId, FiscalPeriodId, CurrencyId, OpeningDebit, OpeningCredit, Debit, Credit, ClosingDebit, ClosingCredit, IsFinalized, FinalizedAt, IsActive, Created, CreatedBy, LastModified, LastModifiedBy
                FROM AccountBalanceDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "AccountBalanceDeletionReport");
        }
    }
}
