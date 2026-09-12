using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRFQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_RFQSuppliers_RFQSupplierId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_RequestForQuotations_RFQId",
                table: "Quotations");

            migrationBuilder.DropTable(
                name: "RFQSuppliers");

            migrationBuilder.DropTable(
                name: "RequestForQuotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_RFQId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_RFQSupplierId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_CommitteeAssignments_RfqId",
                table: "CommitteeAssignments");

            migrationBuilder.DropColumn(
                name: "RFQId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RFQSupplierId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "RfqId",
                table: "CommitteeAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RFQId",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RFQSupplierId",
                table: "Quotations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RfqId",
                table: "CommitteeAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestForQuotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseRequestId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DeadlineDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RFQDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RFQNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestForQuotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestForQuotations_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RFQSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RFQId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvitationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupplierPartyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFQSuppliers_RequestForQuotations_RFQId",
                        column: x => x.RFQId,
                        principalTable: "RequestForQuotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RFQId",
                table: "Quotations",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RFQSupplierId",
                table: "Quotations",
                column: "RFQSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAssignments_RfqId",
                table: "CommitteeAssignments",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotations_PurchaseRequestId",
                table: "RequestForQuotations",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotations_RFQNumber",
                table: "RequestForQuotations",
                column: "RFQNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotations_Status",
                table: "RequestForQuotations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RFQSuppliers_RFQId",
                table: "RFQSuppliers",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_RFQSuppliers_SupplierPartyId",
                table: "RFQSuppliers",
                column: "SupplierPartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_RFQSuppliers_RFQSupplierId",
                table: "Quotations",
                column: "RFQSupplierId",
                principalTable: "RFQSuppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_RequestForQuotations_RFQId",
                table: "Quotations",
                column: "RFQId",
                principalTable: "RequestForQuotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
