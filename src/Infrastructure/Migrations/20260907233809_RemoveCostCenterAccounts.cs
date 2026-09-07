using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCostCenterAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCenterAccountDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenterAccountDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO CostCenterAccountDeletionReport
                    (CostCenterId, AccountId, DeletedAt)
                SELECT
                    CostCenterId, AccountId, SYSDATETIMEOFFSET()
                FROM CostCenterAccounts;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'CostCenterAccounts', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table CostCenterAccounts; rows preserved in CostCenterAccountDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "CostCenterAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCenterAccounts",
                columns: table => new
                {
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenterAccounts", x => new { x.CostCenterId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_CostCenterAccounts_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO CostCenterAccounts
                    (CostCenterId, AccountId)
                SELECT
                    CostCenterId, AccountId
                FROM CostCenterAccountDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "CostCenterAccountDeletionReport");
        }
    }
}
