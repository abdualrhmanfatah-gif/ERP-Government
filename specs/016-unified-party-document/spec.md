# Feature Specification: Unified Party + Document Infrastructure

**Feature Branch**: `016-unified-party-document`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Unified Party + Document Infrastructure — Approvals, Status Log, Attachments, Sequences + PaymentOrder Approval-Column Strip"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Migrate Suppliers to Unified Party (Priority: P1)

As a system administrator, all existing suppliers are consolidated into a single Party entity with a party code (PTY prefix), preserving their name, contact info, tax number, and active status. All downstream references (PaymentOrder, Encumbrance, PurchaseOrder, Quotation, RFQSupplier) now point to the Party record instead of the legacy Supplier table.

**Why this priority**: Foundation for all other features; without a unified Party, no downstream migration or new party types are possible. Highest blast radius if done incorrectly.

**Independent Test**: Verify Supplier rows appear as Party records with correct PartyType=Supplier, PartyCode generated per row, all vendor/supplier FK references migrated, and zero remaining references to the Supplier entity or Suppliers table.

**Acceptance Scenarios**:

1. **Given** existing Supplier records with name, contact, tax, notes, active status, **When** migration runs, **Then** each Supplier becomes a Party record with PartyType=Supplier, PartyCode=PTY-NNN, and all fields mapped.
2. **Given** duplicate Suppliers sharing a TaxNumber, **When** migration runs, **Then** only one Party is created per TaxNumber (dedup by TaxNumber first, then normalized NameAr + PartyType).
3. **Given** Suppliers with no TaxNumber, **When** migration runs, **Then** dedup proceeds on normalized NameAr + PartyType; unmatched Suppliers produce new Party rows.
4. **Given** a PaymentOrder referencing VendorId, **When** migration completes, **Then** VendorId is replaced by VendorPartyId (FK → Parties) and validation uses context.Parties.FindAsync + PartyType check.
5. **Given** a PurchaseOrder referencing SupplierId, **When** migration completes, **Then** SupplierId is replaced by SupplierPartyId (FK → Parties).
6. **Given** all references migrated, **When** code review is performed, **Then** zero "Supplier" references exist in src/.

---

### User Story 2 - ApprovalHistory as Sole Approval Source (Priority: P1)

As an auditor, every approval decision is recorded in ApprovalHistory with a step number, an authoritative Action enum (Submit/Approve/Reject/Return/Cancel), and the original Decision string retained as a legacy note. The PaymentOrder entity no longer carries inline approval columns.

**Why this priority**: Establishes the single source of truth for approvals; PaymentOrder column removal must happen atomically with ApprovalHistory adoption.

**Independent Test**: Verify ApprovalHistory records for every approval event include ApprovalStep and Action, PaymentOrder approval columns are dropped, and all approve/reject/cancel/void commands write only to ApprovalHistory.

**Acceptance Scenarios**:

1. **Given** an existing ApprovalHistory row with Decision="Approved", **When** backfill runs, **Then** Action=Approve and Decision is preserved in Reason when Reason is empty.
2. **Given** an existing ApprovalHistory row with Decision="Draft -> Submitted", **When** backfill runs, **Then** Action=Submit (inferred from status transition).
3. **Given** an existing ApprovalHistory row with Decision="* -> Approved", **When** backfill runs, **Then** Action=Approve.
4. **Given** an existing ApprovalHistory row with Decision="* -> Cancelled", **When** backfill runs, **Then** Action=Cancel.
5. **Given** a PaymentOrder approval command, **When** approver acts, **Then** a single SaveChanges writes both ApprovalHistory (Action + ApprovalStep + real RequiredRole) and DocumentStatusLog row.
6. **Given** PaymentOrder columns ApprovedById/ApprovedAt/RejectedById/RejectedAt/RejectionReason/CancelledById/CancelledAt/CancellationReason/VoidedById/VoidedAt/VoidReason, **When** migration runs, **Then** all columns are dropped.

---

### User Story 3 - Append-Only Document Status Log (Priority: P1)

As an auditor, every document state transition is recorded in an append-only DocumentStatusLog with from-status, to-status, actor, timestamp, and optional reason. Status log rows can never be updated or deleted.

**Why this priority**: Core audit trail for all document lifecycle transitions; required by governance principles.

**Independent Test**: Verify DocumentStatusLog rows are created on every FSM transition, no update/delete API exists, and all 14 writer sites produce status log rows.

**Acceptance Scenarios**:

1. **Given** a document transitioning from Draft to Submitted, **When** the transition completes, **Then** a DocumentStatusLog row is created with FromStatus=Draft, ToStatus=Submitted, ChangedById, ChangedAt, and Reason.
2. **Given** a DocumentStatusLog row, **When** an update or delete is attempted via API, **Then** the operation is rejected (405 or 403).
3. **Given** all 14 writer sites (ApprovalService + 13 budgeting handlers), **When** each executes, **Then** each creates a DocumentStatusLog row via the IDocumentStatusLogger service (no scattered manual rows).

---

### User Story 4 - Formalized Attachments with Mandatory Requirements (Priority: P2)

As a document approver, I can view which attachments are mandatory for a given document type, and I am blocked from approving if required attachments are missing.

**Why this priority**: Attachment requirements gate critical approval transitions; must be wired before approval workflows are complete.

**Independent Test**: Verify DocumentAttachmentRequirement records define mandatory codes per DocumentType, and Submitted→Approved transitions check for required attachments.

**Acceptance Scenarios**:

1. **Given** a DocumentAttachmentRequirement with DocumentType="Budget", AttachmentTypeCode="BOQ", IsMandatory=true, **When** a Budget document reaches Submitted→Approved transition, **Then** the system verifies at least one Attachment with DocumentType="Budget" and code="BOQ" exists; if absent, approval is blocked with a clear message.
2. **Given** a DocumentAttachmentRequirement with IsMandatory=false, **When** the document reaches approval, **Then** the attachment is not required (informational only).
3. **Given** mandatory attachments are present, **When** approval is attempted, **Then** the transition proceeds normally.
4. **Given** a document type with no DocumentAttachmentRequirement records, **When** approval is attempted, **Then** no attachment check is performed (backward compatible).

---

### User Story 5 - Party CRUD and Management (Priority: P2)

As an administrator, I can create, view, update, and toggle-active status for any Party record. Parties support multiple types (Supplier, Customer, GovEntity, TaxAuthority, Other) with Arabic-first name fields and bilingual support.

**Why this priority**: Enables ongoing management of the unified Party entity post-migration.

**Independent Test**: Verify full CRUD operations on Party, filtering by PartyType and IsActive, search by name and tax number, and toggle-active behavior.

**Acceptance Scenarios**:

1. **Given** a new Party with PartyType=Customer, NameAr="شركة X", **When** POST /api/Parties is called, **Then** a PartyCode (PTY sequence) is assigned and the record is persisted.
2. **Given** existing Parties, **When** GET /api/Parties is called with PartyType=Supplier and IsActive=true, **Then** only active Supplier parties are returned.
3. **Given** an existing Party, **When** PATCH /api/Parties/{id}/toggle-active is called, **Then** IsActive flips and the change is recorded.
4. **Given** a Party with TaxNumber="123456789", **When** search by tax is performed, **Then** the Party is found.

---

### User Story 6 - Generic Document Endpoints (Priority: P2)

As an API consumer, I can query approvals, status logs, and attachments for any entity type via a generic document API, and upload real files as attachments.

**Why this priority**: Provides a unified API surface for document-related operations across all modules.

**Independent Test**: Verify GET/POST/DELETE for attachments with file upload, GET for approvals and status logs by entity name and ID, and GET for attachment requirements.

**Acceptance Scenarios**:

1. **Given** a Budget document with Id=42, **When** GET /api/Documents/budgets/42/approvals is called, **Then** all ApprovalHistory records for that document are returned.
2. **Given** a Budget document with Id=42, **When** GET /api/Documents/budgets/42/status-log is called, **Then** all DocumentStatusLog records are returned ordered by ChangedAt.
3. **Given** a file upload, **When** POST /api/Documents/budgets/42/attachments is called with a file stream, **Then** a StoragePath is generated, the file is stored, and an Attachment row is created.
4. **Given** a document type "Budget", **When** GET /api/Documents/budgets/42/attachments/requirements is called, **Then** mandatory vs. available attachments are listed.
5. **Given** an attachment with Id=7, **When** DELETE /api/Documents/budgets/42/attachments/7 is called, **Then** the Attachment row and stored file are removed.

---

### User Story 7 - Document Sequence Prefixes (Priority: P3)

As a system, document sequence prefixes are extended to cover Party (PTY), ReceiptVoucher (RCV), DepositSlip (DSL), DisbursementRequest (DSB), and Payment (PAY), with seeds for the active fiscal year.

**Why this priority**: Required for unique code generation across new entity types; lower urgency since existing sequences already work.

**Independent Test**: Verify sequence generation for each new prefix produces unique, gap-free numbers within the active fiscal year.

**Acceptance Scenarios**:

1. **Given** the active fiscal year 2026, **When** a new Party is created, **Then** PartyCode is assigned as PTY-00001, PTY-00002, etc.
2. **Given** the active fiscal year 2026, **When** a new ReceiptVoucher is created, **Then** the code uses RCV prefix.
3. **Given** two concurrent requests for the same prefix, **When** both are processed, **Then** both receive unique sequence numbers (atomicity guarantee).

---

### Edge Cases

- What happens when a Supplier has no NameAr (required field)? → Migration rejects or defaults to a synthetic name.
- What happens when the same TaxNumber exists across different Supplier records with conflicting data? → Dedup merges by TaxNumber using first-wins strategy: the Supplier with the earliest Created date becomes the primary record; missing fields are filled from later duplicates.
- What happens when an ApprovalHistory row has a Decision string that does not match any inference rule? → Default to Action=Approve; log a warning.
- What happens when a PaymentOrder void command runs after ApprovalHistory-only refactor? → Void writes Action=Cancel to ApprovalHistory and a DocumentStatusLog row; no inline columns checked.
- What happens when an attachment upload fails mid-stream? → Transaction rolls back; no partial Attachment row or orphaned file.
- What happens when DocumentSequenceService encounters a duplicate number? → Reject with a clear error; atomicity prevents this under normal conditions.
- What happens when a document type has no DocumentAttachmentRequirement but an attachment is uploaded? → Attachment is allowed (not blocked); IsRequired on Attachment defaults to false; AttachmentTypeCode is still required for all new attachments.
- What happens when two users simultaneously update the same Party or DocumentAttachmentRequirement? → System returns 409 Conflict with problem-details body; caller must re-fetch and retry.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create a unified Party entity with PartyCode (PTY sequence), PartyType enum, Arabic-first name fields, and standard auditable fields.
- **FR-002**: System MUST migrate all Supplier records to Party records with PartyType=Supplier, deduplicating by TaxNumber first (first-wins: earliest Created date is primary, missing fields filled from later duplicates), then by normalized NameAr + PartyType.
- **FR-003**: System MUST replace all VendorId/SupplierId references (PaymentOrder, Encumbrance, PurchaseOrder, Quotation, RFQSupplier) with Party foreign keys.
- **FR-004**: System MUST delete the Supplier entity, configuration, DbSet, and Suppliers table with zero remaining references in src/.
- **FR-005**: System MUST add ApprovalStep (int, required, default 1) and Action enum (ApprovalAction: Submit/Approve/Reject/Return/Cancel, stored int) to ApprovalHistory.
- **FR-006**: System MUST retain the Decision string in ApprovalHistory as a legacy note while Action is the authoritative field.
- **FR-007**: System MUST backfill existing ApprovalHistory rows: "Approved" → Approve; "{from} -> {to}" → inferred Action; original string preserved in Reason when empty.
- **FR-008**: System MUST create an append-only DocumentStatusLog entity with EntityName, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt, Reason.
- **FR-009**: System MUST reject any update or delete operation on DocumentStatusLog rows (enforced at API layer).
- **FR-010**: System MUST add DocumentType (string, required), IsRequired (bool, default false), and AttachmentTypeCode (string, required) to the Attachment entity.
- **FR-011**: System MUST create a DocumentAttachmentRequirement entity with DocumentType, AttachmentTypeCode, TitleAr, IsMandatory, IsActive, and a unique constraint on (DocumentType, AttachmentTypeCode).
- **FR-012**: System MUST drop PaymentOrder columns: ApprovedById, ApprovedAt, RejectedById, RejectedAt, RejectionReason, CancelledById, CancelledAt, CancellationReason, VoidedById, VoidedAt, VoidReason.
- **FR-013**: System MUST refactor all PaymentOrder approval commands (Approve, Reject, Cancel, Void) to write only to ApprovalHistory and DocumentStatusLog via a single SaveChanges.
- **FR-014**: System MUST inject IDocumentStatusLogger into all 14 writer sites (ApprovalService + 13 budgeting lifecycle handlers) for consistent status log creation.
- **FR-015**: System MUST enforce an attachment gate on Submitted→Approved transitions: mandatory DocumentAttachmentRequirement codes with no corresponding attachment block with a clear error message.
- **FR-016**: System MUST extend DocumentSequenceService with prefixes Party→PTY, ReceiptVoucher→RCV, DepositSlip→DSL, DisbursementRequest→DSB, Payment→PAY, with seeds for the active fiscal year.
- **FR-017**: System MUST provide CRUD API endpoints for Party (GET list with filters, GET by id, POST, PUT, PATCH toggle-active).
- **FR-018**: System MUST provide generic document API endpoints for approvals, status-log, attachments (GET/POST/DELETE with real file upload), and attachment requirements.
- **FR-019**: System MUST provide CRUD API endpoints for DocumentAttachmentRequirements (admin).
- **FR-020**: System MUST wire the attachment gate into Budget and Appropriation approval transitions now; other modules adopt in their specs.
- **FR-021**: System MUST record approval history with real RequiredRole (never string.Empty) for all approval transitions.

### Key Entities

- **Party**: Unified entity representing external or internal parties (Supplier, Customer, GovEntity, TaxAuthority, Other). Key attributes: PartyCode, PartyType, NameAr (required), NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes, IsActive.
- **ApprovalHistory**: Extended approval decision record. Key attributes: DocumentType, DocumentId, ApprovalStep, Action (enum), Decision (legacy string), ApproverUserId, RequiredRole, Reason, EvaluationSnapshot, DecisionAt.
- **DocumentStatusLog**: Append-only audit trail for document state transitions. Key attributes: EntityName (plural lowercase, e.g., "budgets", "paymentorders"), DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt, Reason.
- **Attachment**: Document attachment with file storage reference. Key attributes: EntityName, DocumentId, DocumentType, AttachmentTypeCode, IsRequired, StoragePath.
- **DocumentAttachmentRequirement**: Defines which attachment types are mandatory per document type. Key attributes: DocumentType, AttachmentTypeCode, TitleAr, IsMandatory, IsActive.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing Supplier records are successfully migrated to Party with zero data loss (verified by row count comparison).
- **SC-002**: Zero references to the Supplier entity or Suppliers table remain in src/ after migration.
- **SC-003**: All 14 approval writer sites produce DocumentStatusLog rows with zero scattered manual inserts.
- **SC-004**: PaymentOrder approval column drop migration completes without data loss (approval history preserved in ApprovalHistory).
- **SC-005**: Attachment gate blocks approval within 100ms of detecting missing mandatory attachments.
- **SC-006**: DocumentSequenceService generates unique codes for all 5 new prefixes (PTY, RCV, DSL, DSB, PAY) with zero duplicates under concurrent load.
- **SC-007**: All new API endpoints return responses conforming to the published OpenAPI contract.
- **SC-008**: Party CRUD operations complete within 500ms for lists up to 10,000 records.
- **SC-009**: File upload attachment handling supports streams up to 50MB without timeout or memory exhaustion.
- **SC-010**: Backfill of ApprovalHistory Action enum completes for all existing rows with zero mismatches.

## Clarifications

### Session 2026-09-05

- Q: When the same TaxNumber exists across multiple Suppliers with conflicting data during migration, which field wins as the source of truth for NameAr, Phone, Email, and Address? → A: First-wins — pick the Supplier with the earliest Created date; fill missing fields from later duplicates.
- Q: What file storage mechanism should the attachment upload handler use to persist uploaded files? → A: Local filesystem relative to a configured base directory.
- Q: When two users simultaneously update the same Party record or DocumentAttachmentRequirement, how should the system handle the conflict? → A: Return 409 Conflict with problem-details body; frontend retries after re-fetching.
- Q: How should an Attachment row be linked to a DocumentAttachmentRequirement's AttachmentTypeCode for the mandatory-attachment gate check? → A: Add AttachmentTypeCode (string, required) to Attachment; gate matches on (DocumentType + AttachmentTypeCode).
- Q: Should the DocumentStatusLog's EntityName values use plural lowercase form or PascalCase singular? → A: Plural lowercase (e.g., "budgets", "paymentorders", "appropriations") — matches the existing route pattern.

## Assumptions

- The existing Supplier entity has no application layer or endpoints, making migration purely a data/infrastructure concern.
- Employee entity remains in Organization module and is NOT a Party type.
- The active fiscal year for sequence seeding is the current year (2026).
- Existing ApprovalHistory Decision strings follow the documented patterns ("Approved", "{from} -> {to}") for backfill inference.
- The Supplier table has no cascading dependencies that would complicate deletion.
- DocumentStatusLog append-only enforcement is implemented at the API layer (no database-level trigger required for v1).
- File storage for attachments uses local filesystem relative to a configured base directory; StoragePath stores the relative path from that base.
- PartyCode sequence generation reuses the existing DocumentSequenceService infrastructure.
- The attachment gate applies only to Submitted→Approved transitions (not other transitions) per the specification.
- PaymentOrder void operations are treated as Cancel actions in the ApprovalHistory backfill.
