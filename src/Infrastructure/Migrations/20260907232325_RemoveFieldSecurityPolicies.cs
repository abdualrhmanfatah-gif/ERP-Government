using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldSecurityPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldSecurityPolicyDeletionReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    MaskingFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldSecurityPolicyDeletionReport", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO FieldSecurityPolicyDeletionReport
                    (EntityName, FieldName, RoleId, AccessLevel, MaskingFormat, Priority, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, DeletedAt)
                SELECT
                    EntityName, FieldName, RoleId, AccessLevel, MaskingFormat, Priority, IsActive, Created, CreatedBy, LastModified, LastModifiedBy, SYSDATETIMEOFFSET()
                FROM FieldSecurityPolicies;
                """);

            migrationBuilder.Sql("""
                INSERT INTO SecurityAuditLogs
                    (EventCategory, Action, UserId, EntityName, EntityId, IpAddress, DeviceInfo, SessionId, Success, FailureReason, OldValues, NewValues, Timestamp)
                VALUES
                    ('Schema', 'DeleteTable', 1, 'FieldSecurityPolicies', NULL, NULL, NULL, NULL, 1, NULL, NULL, N'Dropped table FieldSecurityPolicies; rows preserved in FieldSecurityPolicyDeletionReport.', SYSDATETIMEOFFSET());
                """);

            migrationBuilder.DropTable(
                name: "FieldSecurityPolicies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldSecurityPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaskingFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldSecurityPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldSecurityPolicies_SecurityRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SecurityRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldSecurityPolicies_EntityName_FieldName_RoleId",
                table: "FieldSecurityPolicies",
                columns: new[] { "EntityName", "FieldName", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FieldSecurityPolicies_RoleId",
                table: "FieldSecurityPolicies",
                column: "RoleId");

            migrationBuilder.Sql("""
                INSERT INTO FieldSecurityPolicies
                    (EntityName, FieldName, RoleId, AccessLevel, MaskingFormat, Priority, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
                SELECT
                    EntityName, FieldName, RoleId, AccessLevel, MaskingFormat, Priority, IsActive, Created, CreatedBy, LastModified, LastModifiedBy
                FROM FieldSecurityPolicyDeletionReport;
                """);

            migrationBuilder.DropTable(
                name: "FieldSecurityPolicyDeletionReport");
        }
    }
}
