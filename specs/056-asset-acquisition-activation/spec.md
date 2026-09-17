# Feature Specification: Asset Acquisition & Activation (الاستحواذ والتفعيل)

**Feature Branch**: `056-asset-acquisition-activation`

**Created**: 2026-09-15

**Status**: Draft

**Input**: User description: "تقسيم وحدة الأصول إلى features صغيرة. F3: دورة الاستحواذ — تحويل الأصل المسجل (Draft من F2) إلى أصل مفعّل (Active) بإنشاء القيد المحاسبي للتكلفة، مع استخدام حدث `AssetAcquired` الموجود غير المستخدم."

## Clarifications

### Session 2026-09-15

- Q: What is the credit account source for the acquisition journal entry? → A: Single global account configured in general settings (e.g., "Asset Purchases" or Payables account)
- Q: Is activation a single-step or two-step process? → A: Single step with Post permission — activation is not a standalone document
- Q: How is DepreciationStartDate determined during activation? → A: Defaults to activation date, editable before posting — supports pro-rata daily calculation
- Q: Where is the acquisition account configured? → A: Hardcoded reference to a specific AccountId in constants file or configuration
- Q: What loading indicator should the activation form display? → A: Disable the "تفعيل" button and show "جاري التفعيل..." inline while processing; keep form visible to prevent duplicate submissions
- Q: What description text should appear on the activation journal entry lines? → A: Auto-generated: "استحواذ أصل {Code} - {Name}" (Acquisition of asset {Code} - {Name})

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Activate a Draft Asset (Priority: P1)

As an **Asset Accountant (محاسب الأصول)**, I want to activate a draft asset by creating an acquisition journal entry so that the asset's value is reflected in the general ledger and depreciation can begin.

**Why this priority**: Without activation, assets remain as draft records with no accounting impact. This is the bridge between the asset register and the financial statements.

**Independent Test**: Can be tested by activating a draft asset and verifying: (1) status changes to Active, (2) a balanced journal entry is created, (3) AcquisitionCost and ActivationDate are set, (4) the asset appears in financial reports.

**Acceptance Scenarios**:

1. **Given** an asset accountant views a Draft asset with complete financial data, **When** they click "تفعيل" (Activate), **Then** an activation form opens showing the asset details, acquisition cost, and proposed journal entry
2. **Given** the activation form is displayed, **When** the accountant confirms the activation, **Then** the "تفعيل" button is disabled and shows "جاري التفعيل..." (Processing...) inline while the system creates a balanced journal entry (Debit: Asset Group's AccountAssetId, Credit: configured acquisition account) with description "استحواذ أصل {Code} - {Name}", sets status to Active, sets ActivationDate and AcquisitionCost, and publishes the AssetAcquired event
3. **Given** the activation is processed, **When** viewing the asset card, **Then** the linked journal entry reference is displayed along with activation date and acquisition cost
4. **Given** an asset is in Draft status, **When** the accountant attempts to activate it without required financial data (OriginalValue, PurchaseDate), **Then** the system shows a validation error indicating missing required fields
5. **Given** an asset is in Active status, **When** any user attempts to activate it, **Then** the system rejects the operation with "الأصل مفعل بالفعل" (Asset already activated)

### User Story 2 - View Acquisition Journal Entry (Priority: P2)

As an **Auditor (المراجع)**, I want to view the acquisition journal entry linked to an activated asset so that I can trace the accounting impact of asset acquisition.

**Why this priority**: Audit trail requires visibility into the accounting entries created during activation.

**Independent Test**: Can be tested by viewing an activated asset's detail card and verifying the journal entry reference, amounts, and posting status are displayed.

**Acceptance Scenarios**:

1. **Given** an activated asset is displayed, **When** the user views the asset card, **Then** the journal entry reference (JE number) is shown with a link to the full entry
2. **Given** the journal entry link is clicked, **When** the entry page opens, **Then** the debit and credit lines are displayed with account names, amounts, and posting status

### User Story 3 - Reject Activation of Incomplete Asset (Priority: P2)

As an **Asset Accountant**, I want the system to prevent activation of assets with missing or invalid data so that incomplete records don't enter the financial ledger.

**Why this priority**: Data integrity protection prevents accounting errors.

**Independent Test**: Can be tested by attempting to activate assets with various missing fields and verifying appropriate error messages.

**Acceptance Scenarios**:

1. **Given** a Draft asset has no AssetGroup assigned, **When** activation is attempted, **Then** the system shows "مجموعة الأصل مطلوبة" (Asset group is required)
2. **Given** a Draft asset's AssetGroup has no AccountAssetId configured, **When** activation is attempted, **Then** the system shows "لم يتم تعيين حساب الأصول للمجموعة" (Asset account not assigned to group)
3. **Given** a Draft asset has OriginalValue of zero or negative, **When** activation is attempted, **Then** the system shows "القيمة الأصلية يجب أن تكون أكبر من صفر" (Original value must be greater than zero)

### Edge Cases

- What happens when the configured acquisition account doesn't exist? → System MUST return an error indicating the acquisition account is not configured
- What happens when the journal entry creation fails (e.g., accounting period closed)? → System MUST NOT change asset status and MUST show the accounting error
- What happens when two users attempt to activate the same asset simultaneously? → System MUST use optimistic concurrency (RowVersion) to prevent double activation
- What happens when the asset group's AccountAssetId is changed after activation? → System MUST use the account ID from the time of activation (stored in the journal entry), not the current group configuration
- What happens when the user wants to reverse an activation? → This is out of scope for v1; the user should contact an administrator (non-functional requirement)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an "Activate" action only for assets in Draft status
- **FR-002**: System MUST validate that the asset has complete financial data before activation: OriginalValue > 0, PurchaseDate set, AssetGroup assigned
- **FR-003**: System MUST validate that the asset's AssetGroup has an AccountAssetId configured before activation
- **FR-004**: System MUST create a balanced journal entry upon activation: Debit = AssetGroup.AccountAssetId, Credit = acquisition account (hardcoded AccountId reference in configuration/constants), with auto-generated description "استحواذ أصل {Code} - {Name}"
- **FR-005**: System MUST set asset status to "Active" upon successful activation
- **FR-006**: System MUST set ActivationDate to the current date upon activation
- **FR-007**: System MUST set AcquisitionCost equal to OriginalValue upon activation
- **FR-008**: System MUST allow the user to modify DepreciationStartDate during activation (defaults to ActivationDate)
- **FR-009**: System MUST link the created journal entry to the asset (store JournalEntryId on the asset record)
- **FR-010**: System MUST publish an AssetAcquired domain event upon successful activation
- **FR-011**: System MUST prevent activation of assets that are already Active, UnderMaintenance, Disposed, or WrittenOff
- **FR-012**: System MUST use optimistic concurrency (RowVersion) to prevent double activation
- **FR-013**: System MUST require Assets.Update permission for activation (reuse existing permission)
- **FR-014**: System MUST display the linked journal entry reference on the asset detail card
- **FR-015**: System MUST NOT allow activation if the accounting period for the activation date is closed
- **FR-016**: System MUST log the activation operation for audit purposes (actor, timestamp, journal entry reference)

### Key Entities

- **Asset**: The core entity. Key fields for activation: Status (Draft → Active), ActivationDate, AcquisitionCost, DepreciationStartDate, JournalEntryId (new field linking to the created entry)
- **JournalEntry**: The accounting entry created during activation. Contains debit/credit lines with account references and amounts
- **AssetGroup**: Provides the debit account (AccountAssetId) for the acquisition entry
- **AcquisitionAccount**: A hardcoded account reference in configuration/constants that serves as the credit counterpart (e.g., "مشتريات أصول" or "ذمم دائنة")

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Asset Accountant can activate a draft asset in under 2 minutes (from opening activation form to successful save)
- **SC-002**: 100% of activation attempts produce balanced journal entries (debit = credit)
- **SC-003**: Activation fails gracefully with clear Arabic error messages when data is incomplete
- **SC-004**: Linked journal entry is accessible from the asset detail card within 2 clicks
- **SC-005**: No double activations occur under concurrent user scenarios (optimistic concurrency)
- **SC-006**: All activation operations produce audit log entries with actor, timestamp, and journal entry reference

## Assumptions

- The Asset entity already contains ActivationDate, AcquisitionCost, and DepreciationStartDate fields (from F2)
- The AssetAcquired domain event is already defined in `src/Domain/Events/Assets/AssetAcquired.cs`
- The AssetGroup entity already contains AccountAssetId and other account fields
- A journal entry creation service exists in `src/Application/Accounting/` that can be reused
- The acquisition account is hardcoded as a reference in configuration/constants (not dynamically configurable)
- The existing Assets.Update permission can be reused for activation (no new permission needed)
- The accounting period validation service exists and can check if a date falls within an open period
- The asset's RowVersion field is available for optimistic concurrency
- The F2 Asset Register CRUD feature is complete and assets can be created in Draft status
- The journal entry follows the existing pattern: JournalEntry entity with JournalEntryLine items (debit/credit)
- The AssetConfiguration already maps AssetGroup to GL accounts (AccountAssetId, AccountDepreciationId, etc.)

## Non-Goals (Explicit Exclusions)

| Non-Goal | Rationale |
|----------|-----------|
| Integration with procurement/warehouse systems | Cross-module integration is deferred to v2 |
| Installment purchase handling | Adds financial complexity beyond v1 scope |
| Modifying acquisition entry after posting | Only reversal and re-activation allowed — preserves entry integrity |
| Bulk activation from list view | Requires different review design — Parking Lot |
| Self-constructed assets (cost accumulation) | Scope expansion — typically deferred |
| Reversing/correcting activation | Out of scope for v1; administrator action required |
