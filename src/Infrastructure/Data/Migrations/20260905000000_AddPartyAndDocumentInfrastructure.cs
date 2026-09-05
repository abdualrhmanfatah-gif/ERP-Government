using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Government.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddPartyAndDocumentInfrastructure : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ============================================================
        // STEP 1: Create new tables
        // ============================================================

        migrationBuilder.CreateTable(
            name: "Parties",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PartyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                PartyType = table.Column<int>(type: "int", nullable: false),
                NameAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                NameEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Parties", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DocumentStatusLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DocumentId = table.Column<int>(type: "int", nullable: false),
                FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ChangedById = table.Column<int>(type: "int", nullable: false),
                ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentStatusLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_DocumentStatusLogs_Users_ChangedById",
                    column: x => x.ChangedById,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DocumentAttachmentRequirements",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AttachmentTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                TitleAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentAttachmentRequirements", x => x.Id);
            });

        // ============================================================
        // STEP 2: Alter existing tables
        // ============================================================

        migrationBuilder.AddColumn<int>(
            name: "ApprovalStep",
            table: "ApprovalHistory",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<int>(
            name: "Action",
            table: "ApprovalHistory",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<string>(
            name: "DocumentType",
            table: "Attachments",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "IsRequired",
            table: "Attachments",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "AttachmentTypeCode",
            table: "Attachments",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        // ============================================================
        // STEP 3: Add FK columns to referencing entities
        // ============================================================

        migrationBuilder.AddColumn<int>(
            name: "VendorPartyId",
            table: "PaymentOrders",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "VendorPartyId",
            table: "Encumbrances",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SupplierPartyId",
            table: "PurchaseOrders",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PartyId",
            table: "Quotations",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PartyId",
            table: "RFQSuppliers",
            type: "int",
            nullable: true);

        // ============================================================
        // STEP 4: Create indexes
        // ============================================================

        migrationBuilder.CreateIndex(
            name: "IX_Parties_PartyCode",
            table: "Parties",
            column: "PartyCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Parties_PartyType_IsActive",
            table: "Parties",
            columns: new[] { "PartyType", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Parties_TaxNumber",
            table: "Parties",
            column: "TaxNumber");

        migrationBuilder.CreateIndex(
            name: "IX_Parties_NameAr",
            table: "Parties",
            column: "NameAr");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentStatusLogs_EntityName_DocumentId",
            table: "DocumentStatusLogs",
            columns: new[] { "EntityName", "DocumentId" });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentStatusLogs_ChangedAt",
            table: "DocumentStatusLogs",
            column: "ChangedAt");

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAttachmentRequirements_DocumentType_AttachmentTypeCode",
            table: "DocumentAttachmentRequirements",
            columns: new[] { "DocumentType", "AttachmentTypeCode" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DocumentAttachmentRequirements_DocumentType",
            table: "DocumentAttachmentRequirements",
            column: "DocumentType");

        // ============================================================
        // STEP 5: Data migration — Supplier → Party
        // ============================================================

        migrationBuilder.Sql(@"
            -- Create Party records from Suppliers with first-wins dedup
            -- PartyType=0 (Supplier), generate PTY codes
            WITH RankedSuppliers AS (
                SELECT
                    s.Id,
                    s.SupplierName,
                    s.SupplierNameEn,
                    s.CommercialRegistrationNumber AS TaxNumber,
                    s.ContactPerson,
                    s.ContactEmail AS Email,
                    s.ContactPhone AS Phone,
                    s.Address,
                    s.City,
                    s.Notes,
                    s.IndustryClassification,
                    s.IsActive,
                    s.Created,
                    s.CreatedBy,
                    s.LastModified,
                    s.LastModifiedBy,
                    ROW_NUMBER() OVER (
                        PARTITION BY
                            CASE WHEN s.CommercialRegistrationNumber IS NOT NULL AND s.CommercialRegistrationNumber != ''
                                THEN s.CommercialRegistrationNumber
                                ELSE UPPER(LTRIM(RTRIM(s.SupplierName)))
                            END
                        ORDER BY s.Created ASC
                    ) AS rn
                FROM Suppliers s
            )
            INSERT INTO Parties (PartyCode, PartyType, NameAr, NameEn, TaxNumber, Phone, Email, Address, Notes, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
            SELECT
                CONCAT('PTY-', FORMAT(ROW_NUMBER() OVER (ORDER BY rs.Id), '00000')),
                0, -- PartyType.Supplier
                rs.SupplierName,
                rs.SupplierNameEn,
                rs.TaxNumber,
                rs.Phone,
                rs.Email,
                CASE
                    WHEN rs.Address IS NOT NULL AND rs.City IS NOT NULL THEN CONCAT(rs.Address, ', ', rs.City)
                    WHEN rs.Address IS NOT NULL THEN rs.Address
                    WHEN rs.City IS NOT NULL THEN rs.City
                    ELSE NULL
                END,
                CASE
                    WHEN rs.ContactPerson IS NOT NULL AND rs.Notes IS NOT NULL THEN CONCAT('-contact: ', rs.ContactPerson, '; ', rs.Notes)
                    WHEN rs.ContactPerson IS NOT NULL THEN CONCAT('contact: ', rs.ContactPerson)
                    WHEN rs.IndustryClassification IS NOT NULL AND rs.Notes IS NOT NULL THEN CONCAT('industry: ', rs.IndustryClassification, '; ', rs.Notes)
                    WHEN rs.IndustryClassification IS NOT NULL THEN CONCAT('industry: ', rs.IndustryClassification)
                    ELSE rs.Notes
                END,
                rs.IsActive,
                rs.Created,
                rs.CreatedBy,
                rs.LastModified,
                rs.LastModifiedBy
            FROM RankedSuppliers rs
            WHERE rs.rn = 1;
        ");

        // ============================================================
        // STEP 6: Data migration — copy FK values
        // ============================================================

        migrationBuilder.Sql(@"
            -- Map PaymentOrder.VendorId → VendorPartyId
            UPDATE po
            SET po.VendorPartyId = p.Id
            FROM PaymentOrders po
            INNER JOIN Parties p ON p.PartyType = 0 -- Supplier
            INNER JOIN Suppliers s ON s.Id = po.VendorId;
        ");

        migrationBuilder.Sql(@"
            -- Map Encumbrance.VendorId → VendorPartyId
            UPDATE e
            SET e.VendorPartyId = p.Id
            FROM Encumbrances e
            INNER JOIN Parties p ON p.PartyType = 0
            INNER JOIN Suppliers s ON s.Id = e.VendorId
            WHERE e.VendorId IS NOT NULL;
        ");

        migrationBuilder.Sql(@"
            -- Map PurchaseOrder.SupplierId → SupplierPartyId
            UPDATE po
            SET po.SupplierPartyId = p.Id
            FROM PurchaseOrders po
            INNER JOIN Parties p ON p.PartyType = 0
            INNER JOIN Suppliers s ON s.Id = po.SupplierId;
        ");

        migrationBuilder.Sql(@"
            -- Map Quotation.SupplierId → PartyId
            UPDATE q
            SET q.PartyId = p.Id
            FROM Quotations q
            INNER JOIN Parties p ON p.PartyType = 0
            INNER JOIN Suppliers s ON s.Id = q.SupplierId;
        ");

        migrationBuilder.Sql(@"
            -- Map RFQSupplier.SupplierId → PartyId
            UPDATE r
            SET r.PartyId = p.Id
            FROM RFQSuppliers r
            INNER JOIN Parties p ON p.PartyType = 0
            INNER JOIN Suppliers s ON s.Id = r.SupplierId;
        ");

        // ============================================================
        // STEP 7: Data migration — ApprovalHistory backfill
        // ============================================================

        migrationBuilder.Sql(@"
            -- Backfill Action from Decision string
            UPDATE ah
            SET ah.Action = CASE
                -- 'Approved' → Approve (1)
                WHEN ah.Decision = 'Approved' THEN 1
                -- 'Draft -> Submitted' or '* -> PendingApproval' → Submit (0)
                WHEN ah.Decision LIKE '%-> Submitted' THEN 0
                WHEN ah.Decision LIKE '%-> PendingApproval' THEN 0
                -- '* -> Approved' → Approve (1)
                WHEN ah.Decision LIKE '%-> Approved' THEN 1
                -- '* -> Cancelled' → Cancel (4)
                WHEN ah.Decision LIKE '%-> Cancelled' THEN 4
                -- Default: Approve (1)
                ELSE 1
            END,
            -- Preserve original Decision in Reason when Reason is empty
            ah.Reason = CASE
                WHEN (ah.Reason IS NULL OR ah.Reason = '') AND ah.Decision IS NOT NULL
                    THEN ah.Decision
                ELSE ah.Reason
            END
            FROM ApprovalHistory ah
            WHERE ah.Action = 0 AND ah.ApprovalStep = 1; -- Only backfill untouched rows
        ");

        // ============================================================
        // STEP 8: Data migration — Attachment DocumentType backfill
        // ============================================================

        migrationBuilder.Sql(@"
            -- Backfill DocumentType from EntityName (singularize common patterns)
            UPDATE a
            SET a.DocumentType = CASE
                WHEN a.EntityName = 'budgets' THEN 'Budget'
                WHEN a.EntityName = 'appropriations' THEN 'Appropriation'
                WHEN a.EntityName = 'encumbrances' THEN 'Encumbrance'
                WHEN a.EntityName = 'paymentorders' THEN 'PaymentOrder'
                WHEN a.EntityName = 'purchaseorders' THEN 'PurchaseOrder'
                WHEN a.EntityName = 'quotations' THEN 'Quotation'
                WHEN a.EntityName = 'revenuereceipts' THEN 'RevenueReceipt'
                ELSE a.EntityName
            END,
            a.AttachmentTypeCode = 'GENERAL'
            FROM Attachments a
            WHERE a.DocumentType = '' OR a.DocumentType IS NULL;
        ");

        // ============================================================
        // STEP 9: Drop Suppliers table
        // ============================================================

        migrationBuilder.DropTable(
            name: "Suppliers");

        // ============================================================
        // STEP 10: Seed DocumentAttachmentRequirements
        // ============================================================

        migrationBuilder.Sql(@"
            INSERT INTO DocumentAttachmentRequirements (DocumentType, AttachmentTypeCode, TitleAr, IsMandatory, IsActive, Created, CreatedBy, LastModified, LastModifiedBy)
            VALUES
                ('Budget', 'BOQ', N'جدول الكميات', 1, 1, GETUTCDATE(), 'system', GETUTCDATE(), 'system'),
                ('Budget', 'CONTRACT', N'العقد', 1, 1, GETUTCDATE(), 'system', GETUTCDATE(), 'system'),
                ('Appropriation', 'BUDGET_APPROVAL', N'موافقة الميزانية', 1, 1, GETUTCDATE(), 'system', GETUTCDATE(), 'system');
        ");

        // ============================================================
        // STEP 11: Seed DocumentSequences for new prefixes
        // ============================================================

        migrationBuilder.Sql(@"
            -- Seed sequences for new prefixes (only if not already present)
            INSERT INTO DocumentSequences (DocumentType, CurrentNumber, IsActive, FiscalYearId)
            SELECT 'Party', 0, 1, fy.Id
            FROM FiscalYears fy
            WHERE fy.Status = 'Active'
            AND NOT EXISTS (SELECT 1 FROM DocumentSequences ds WHERE ds.DocumentType = 'Party');

            INSERT INTO DocumentSequences (DocumentType, CurrentNumber, IsActive, FiscalYearId)
            SELECT 'ReceiptVoucher', 0, 1, fy.Id
            FROM FiscalYears fy
            WHERE fy.Status = 'Active'
            AND NOT EXISTS (SELECT 1 FROM DocumentSequences ds WHERE ds.DocumentType = 'ReceiptVoucher');

            INSERT INTO DocumentSequences (DocumentType, CurrentNumber, IsActive, FiscalYearId)
            SELECT 'DepositSlip', 0, 1, fy.Id
            FROM FiscalYears fy
            WHERE fy.Status = 'Active'
            AND NOT EXISTS (SELECT 1 FROM DocumentSequences ds WHERE ds.DocumentType = 'DepositSlip');

            INSERT INTO DocumentSequences (DocumentType, CurrentNumber, IsActive, FiscalYearId)
            SELECT 'DisbursementRequest', 0, 1, fy.Id
            FROM FiscalYears fy
            WHERE fy.Status = 'Active'
            AND NOT EXISTS (SELECT 1 FROM DocumentSequences ds WHERE ds.DocumentType = 'DisbursementRequest');

            INSERT INTO DocumentSequences (DocumentType, CurrentNumber, IsActive, FiscalYearId)
            SELECT 'Payment', 0, 1, fy.Id
            FROM FiscalYears fy
            WHERE fy.Status = 'Active'
            AND NOT EXISTS (SELECT 1 FROM DocumentSequences ds WHERE ds.DocumentType = 'Payment');
        ");

        // ============================================================
        // STEP 12: Add FK constraints
        // ============================================================

        migrationBuilder.AddForeignKey(
            name: "FK_PaymentOrders_Parties_VendorPartyId",
            table: "PaymentOrders",
            column: "VendorPartyId",
            principalTable: "Parties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Encumbrances_Parties_VendorPartyId",
            table: "Encumbrances",
            column: "VendorPartyId",
            principalTable: "Parties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PurchaseOrders_Parties_SupplierPartyId",
            table: "PurchaseOrders",
            column: "SupplierPartyId",
            principalTable: "Parties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Quotations_Parties_PartyId",
            table: "Quotations",
            column: "PartyId",
            principalTable: "Parties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_RFQSuppliers_Parties_PartyId",
            table: "RFQSuppliers",
            column: "PartyId",
            principalTable: "Parties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        // ============================================================
        // STEP 13: Add indexes on new FK columns
        // ============================================================

        migrationBuilder.CreateIndex(
            name: "IX_PaymentOrders_VendorPartyId",
            table: "PaymentOrders",
            column: "VendorPartyId");

        migrationBuilder.CreateIndex(
            name: "IX_Encumbrances_VendorPartyId",
            table: "Encumbrances",
            column: "VendorPartyId");

        migrationBuilder.CreateIndex(
            name: "IX_PurchaseOrders_SupplierPartyId",
            table: "PurchaseOrders",
            column: "SupplierPartyId");

        migrationBuilder.CreateIndex(
            name: "IX_Quotations_PartyId",
            table: "Quotations",
            column: "PartyId");

        migrationBuilder.CreateIndex(
            name: "IX_RFQSuppliers_PartyId",
            table: "RFQSuppliers",
            column: "PartyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse order: drop FKs, drop tables, restore Suppliers

        migrationBuilder.DropForeignKey(name: "FK_PaymentOrders_Parties_VendorPartyId", table: "PaymentOrders");
        migrationBuilder.DropForeignKey(name: "FK_Encumbrances_Parties_VendorPartyId", table: "Encumbrances");
        migrationBuilder.DropForeignKey(name: "FK_PurchaseOrders_Parties_SupplierPartyId", table: "PurchaseOrders");
        migrationBuilder.DropForeignKey(name: "FK_Quotations_Parties_PartyId", table: "Quotations");
        migrationBuilder.DropForeignKey(name: "FK_RFQSuppliers_Parties_PartyId", table: "RFQSuppliers");

        migrationBuilder.DropIndex(name: "IX_PaymentOrders_VendorPartyId", table: "PaymentOrders");
        migrationBuilder.DropIndex(name: "IX_Encumbrances_VendorPartyId", table: "Encumbrances");
        migrationBuilder.DropIndex(name: "IX_PurchaseOrders_SupplierPartyId", table: "PurchaseOrders");
        migrationBuilder.DropIndex(name: "IX_Quotations_PartyId", table: "Quotations");
        migrationBuilder.DropIndex(name: "IX_RFQSuppliers_PartyId", table: "RFQSuppliers");

        migrationBuilder.DropColumn(name: "VendorPartyId", table: "PaymentOrders");
        migrationBuilder.DropColumn(name: "VendorPartyId", table: "Encumbrances");
        migrationBuilder.DropColumn(name: "SupplierPartyId", table: "PurchaseOrders");
        migrationBuilder.DropColumn(name: "PartyId", table: "Quotations");
        migrationBuilder.DropColumn(name: "PartyId", table: "RFQSuppliers");

        migrationBuilder.DropColumn(name: "ApprovalStep", table: "ApprovalHistory");
        migrationBuilder.DropColumn(name: "Action", table: "ApprovalHistory");

        migrationBuilder.DropColumn(name: "DocumentType", table: "Attachments");
        migrationBuilder.DropColumn(name: "IsRequired", table: "Attachments");
        migrationBuilder.DropColumn(name: "AttachmentTypeCode", table: "Attachments");

        migrationBuilder.DropTable(name: "Parties");
        migrationBuilder.DropTable(name: "DocumentStatusLogs");
        migrationBuilder.DropTable(name: "DocumentAttachmentRequirements");

        // Note: Suppliers table restoration would require a separate reverse-migration
        // that recreates the Suppliers table and restores data from Parties.
    }
}
