using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoDMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoDMatrixDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionAId = table.Column<int>(type: "int", nullable: false),
                    PermissionBId = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    ActionOnViolation = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoDMatrixDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO SoDMatrixDeletionReport
                    (PermissionAId, PermissionBId, RiskLevel, ActionOnViolation, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    PermissionAId, PermissionBId, RiskLevel, ActionOnViolation, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM SoDMatrix;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'SoDMatrix', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table SoDMatrix; rows preserved in SoDMatrixDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "SoDMatrix");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoDMatrix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionAId = table.Column<int>(type: "int", nullable: false),
                    PermissionBId = table.Column<int>(type: "int", nullable: false),
                    ActionOnViolation = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskLevel = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoDMatrix", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoDMatrix_SecurityPermissions_PermissionAId",
                        column: x => x.PermissionAId,
                        principalTable: "SecurityPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoDMatrix_SecurityPermissions_PermissionBId",
                        column: x => x.PermissionBId,
                        principalTable: "SecurityPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SoDMatrix_PermissionAId_PermissionBId",
                table: "SoDMatrix",
                columns: new[] { "PermissionAId", "PermissionBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoDMatrix_PermissionBId",
                table: "SoDMatrix",
                column: "PermissionBId");

            migrationBuilder.Sql("""
                INSERT INTO SoDMatrix
                    (PermissionAId, PermissionBId, RiskLevel, ActionOnViolation, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    PermissionAId, PermissionBId, RiskLevel, ActionOnViolation, Description, IsActive, Created, CreatedBy, LastModified, LastModifiedBy
                FROM SoDMatrixDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "SoDMatrixDeletionReport");
        }
    }
}
