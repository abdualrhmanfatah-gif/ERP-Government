using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialControlEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GeneratedById = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IssuedById = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalAccounts_FiscalYears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YearClosingRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYearId = table.Column<int>(type: "int", nullable: false),
                    RunAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RunById = table.Column<int>(type: "int", nullable: false),
                    RunType = table.Column<int>(type: "int", nullable: false),
                    LapsedAppropriationTotal = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    LapsedEncumbranceTotal = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReversedById = table.Column<int>(type: "int", nullable: true),
                    ReversedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearClosingRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearClosingRuns_FiscalYears_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "FiscalYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinalAccountLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalAccountId = table.Column<int>(type: "int", nullable: false),
                    Dimension = table.Column<int>(type: "int", nullable: false),
                    DimensionId = table.Column<int>(type: "int", nullable: false),
                    DimensionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DimensionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BudgetedAmount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    Variance = table.Column<decimal>(type: "decimal(23,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalAccountLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalAccountLines_FinalAccounts_FinalAccountId",
                        column: x => x.FinalAccountId,
                        principalTable: "FinalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalAccountLines_FinalAccountId",
                table: "FinalAccountLines",
                column: "FinalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalAccountLines_FinalAccountId_Dimension_DimensionId",
                table: "FinalAccountLines",
                columns: new[] { "FinalAccountId", "Dimension", "DimensionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalAccounts_FiscalYearId",
                table: "FinalAccounts",
                column: "FiscalYearId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearClosingRuns_FiscalYearId",
                table: "YearClosingRuns",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_YearClosingRuns_FiscalYearId_Status",
                table: "YearClosingRuns",
                columns: new[] { "FiscalYearId", "Status" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalAccountLines");

            migrationBuilder.DropTable(
                name: "YearClosingRuns");

            migrationBuilder.DropTable(
                name: "FinalAccounts");
        }
    }
}
