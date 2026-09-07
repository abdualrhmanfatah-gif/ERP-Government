using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLineBudgetDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryTemplateLines_OrganizationalUnits_OrganizationUnitId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryTemplateLines_Projects_ProjectId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryTemplateLines_FundId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryTemplateLines_OrganizationUnitId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryTemplateLines_ProjectId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_BudgetItemId",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_EncumbranceId",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_FundId",
                table: "JournalEntryLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_ProjectId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "FundId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "JournalEntryTemplateLines");

            migrationBuilder.DropColumn(
                name: "BudgetItemId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "EncumbranceId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "FundId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "JournalEntryLines");

            migrationBuilder.RenameColumn(
                name: "DebitAmount",
                table: "JournalEntryTemplateLines",
                newName: "Debit");

            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "JournalEntryTemplateLines",
                newName: "Credit");

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "JournalEntryTemplateLines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "JournalEntryTemplateLines",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "JournalEntryTemplateLines");

            migrationBuilder.RenameColumn(
                name: "Debit",
                table: "JournalEntryTemplateLines",
                newName: "DebitAmount");

            migrationBuilder.RenameColumn(
                name: "Credit",
                table: "JournalEntryTemplateLines",
                newName: "CreditAmount");

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "JournalEntryTemplateLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FundId",
                table: "JournalEntryTemplateLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "JournalEntryTemplateLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationUnitId",
                table: "JournalEntryTemplateLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "JournalEntryTemplateLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BudgetItemId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EncumbranceId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FundId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "JournalEntryLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryTemplateLines_FundId",
                table: "JournalEntryTemplateLines",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryTemplateLines_OrganizationUnitId",
                table: "JournalEntryTemplateLines",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryTemplateLines_ProjectId",
                table: "JournalEntryTemplateLines",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_BudgetItemId",
                table: "JournalEntryLines",
                column: "BudgetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_EncumbranceId",
                table: "JournalEntryLines",
                column: "EncumbranceId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_FundId",
                table: "JournalEntryLines",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_ProjectId",
                table: "JournalEntryLines",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryTemplateLines_OrganizationalUnits_OrganizationUnitId",
                table: "JournalEntryTemplateLines",
                column: "OrganizationUnitId",
                principalTable: "OrganizationalUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryTemplateLines_Projects_ProjectId",
                table: "JournalEntryTemplateLines",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
