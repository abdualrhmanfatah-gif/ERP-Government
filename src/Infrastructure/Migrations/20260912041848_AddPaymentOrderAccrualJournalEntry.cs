using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOrderAccrualJournalEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccrualJournalEntryId",
                table: "PaymentOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_AccrualJournalEntryId",
                table: "PaymentOrders",
                column: "AccrualJournalEntryId",
                unique: true,
                filter: "[AccrualJournalEntryId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_JournalEntries_AccrualJournalEntryId",
                table: "PaymentOrders",
                column: "AccrualJournalEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_JournalEntries_AccrualJournalEntryId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_AccrualJournalEntryId",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "AccrualJournalEntryId",
                table: "PaymentOrders");
        }
    }
}
