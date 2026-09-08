using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecurringEntryExecutionLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurringEntryExecutionLogDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurringEntryId = table.Column<int>(type: "int", nullable: false),
                    ExecutionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GeneratedJournalEntryId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriggeredBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringEntryExecutionLogDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO RecurringEntryExecutionLogDeletionReport
                    (RecurringEntryId, ExecutionDate, GeneratedJournalEntryId, Status, StartedAt, CompletedAt, ErrorMessage, TriggeredBy, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    RecurringEntryId, ExecutionDate, GeneratedJournalEntryId,
                    CASE Status WHEN 'Created' THEN 0 WHEN 'Success' THEN 1 ELSE 2 END,
                    StartedAt, CompletedAt, ErrorMessage, TriggeredBy, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM RecurringEntryExecutionLogs;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'RecurringEntryExecutionLogs', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table RecurringEntryExecutionLogs; rows preserved in RecurringEntryExecutionLogDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "RecurringEntryExecutionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecurringEntryExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedJournalEntryId = table.Column<int>(type: "int", nullable: true),
                    RecurringEntryId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggeredBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringEntryExecutionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringEntryExecutionLogs_JournalEntries_GeneratedJournalEntryId",
                        column: x => x.GeneratedJournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringEntryExecutionLogs_RecurringEntries_RecurringEntryId",
                        column: x => x.RecurringEntryId,
                        principalTable: "RecurringEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringEntryExecutionLogs_GeneratedJournalEntryId",
                table: "RecurringEntryExecutionLogs",
                column: "GeneratedJournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringEntryExecutionLogs_RecurringEntryId",
                table: "RecurringEntryExecutionLogs",
                column: "RecurringEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringEntryExecutionLogs_RecurringEntryId_ExecutionDate",
                table: "RecurringEntryExecutionLogs",
                columns: new[] { "RecurringEntryId", "ExecutionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringEntryExecutionLogs_Status",
                table: "RecurringEntryExecutionLogs",
                column: "Status");

            migrationBuilder.Sql("""
                INSERT INTO RecurringEntryExecutionLogs
                    (RecurringEntryId, ExecutionDate, GeneratedJournalEntryId, Status, StartedAt, CompletedAt, ErrorMessage, TriggeredBy, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    RecurringEntryId, ExecutionDate, GeneratedJournalEntryId,
                    CASE Status WHEN 0 THEN 'Created' WHEN 1 THEN 'Success' ELSE 'Failed' END,
                    StartedAt, CompletedAt, ErrorMessage, TriggeredBy, Created, CreatedBy, LastModified, LastModifiedBy
                FROM RecurringEntryExecutionLogDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "RecurringEntryExecutionLogDeletionReport");
        }
    }
}
