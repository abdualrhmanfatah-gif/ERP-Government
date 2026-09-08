using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovalDelegations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalDelegationDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DelegatorUserId = table.Column<int>(type: "int", nullable: false),
                    DelegateUserId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CanReDelegate = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDelegationDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO ApprovalDelegationDeletionReport
                    (DelegatorUserId, DelegateUserId, EntityType, StartDate, EndDate, Status, CanReDelegate, Reason, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    DelegatorUserId, DelegateUserId, EntityType, StartDate, EndDate, Status, CanReDelegate, Reason, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM ApprovalDelegations;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'ApprovalDelegations', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table ApprovalDelegations; rows preserved in ApprovalDelegationDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "ApprovalDelegations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalDelegations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DelegateUserId = table.Column<int>(type: "int", nullable: false),
                    DelegatorUserId = table.Column<int>(type: "int", nullable: false),
                    CanReDelegate = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDelegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalDelegations_Users_DelegateUserId",
                        column: x => x.DelegateUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprovalDelegations_Users_DelegatorUserId",
                        column: x => x.DelegatorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegations_DelegateUserId",
                table: "ApprovalDelegations",
                column: "DelegateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegations_DelegatorUserId",
                table: "ApprovalDelegations",
                column: "DelegatorUserId");

            migrationBuilder.Sql("""
                INSERT INTO ApprovalDelegations
                    (DelegatorUserId, DelegateUserId, EntityType, StartDate, EndDate, Status, CanReDelegate, Reason, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    DelegatorUserId, DelegateUserId, EntityType, StartDate, EndDate, Status, CanReDelegate, Reason, Created, CreatedBy, LastModified, LastModifiedBy
                FROM ApprovalDelegationDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "ApprovalDelegationDeletionReport");
        }
    }
}
