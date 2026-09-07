using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCashFlowMappingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashFlowMappingRuleDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountGroupId = table.Column<int>(type: "int", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowMappingRuleDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO CashFlowMappingRuleDeletionReport
                    (AccountGroupId, Section, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    AccountGroupId, Section, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM CashFlowMappingRules;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'CashFlowMappingRules', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table CashFlowMappingRules; rows preserved in CashFlowMappingRuleDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "CashFlowMappingRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashFlowMappingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountGroupId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Section = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowMappingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashFlowMappingRules_AccountGroups_AccountGroupId",
                        column: x => x.AccountGroupId,
                        principalTable: "AccountGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowMappingRules_AccountGroupId",
                table: "CashFlowMappingRules",
                column: "AccountGroupId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.Sql("""
                INSERT INTO CashFlowMappingRules
                    (AccountGroupId, Section, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    AccountGroupId, Section, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy
                FROM CashFlowMappingRuleDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "CashFlowMappingRuleDeletionReport");
        }
    }
}
