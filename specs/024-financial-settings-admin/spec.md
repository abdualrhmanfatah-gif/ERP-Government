# Feature Specification: Financial Settings Administration UI

**Feature Branch**: `024-financial-settings-admin`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "Financial settings administration for the ERP: currencies, exchange rates, fiscal years with their periods, document numbering sequences, and general-ledger closing entries per fiscal year. Backend is fully built; this feature is the admin UI."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Fiscal Year Lifecycle (Priority: P1)

As an administrator, I need to manage fiscal years and their periods so that the financial system operates within valid time boundaries. I can create a fiscal year in Draft status, open it for posting, bulk-generate monthly periods, lock/unlock individual periods, and close the year. The system prevents overlapping open years and enforces lifecycle rules.

**Why this priority**: Fiscal years are the foundation of all financial posting. Without the admin UI to create and manage them, no other financial activity can proceed. This is the most critical administrative function.

**Independent Test**: Can be fully tested by creating a fiscal year, generating periods, opening it, and verifying period statuses. Delivers immediate value by enabling financial operations.

**Acceptance Scenarios**:

1. **Given** no open fiscal year overlaps the intended dates, **When** an admin creates a fiscal year (name, start, end), **Then** it appears as Draft and can be opened.
2. **Given** an open fiscal year, **When** the admin opens a second overlapping year, **Then** the system rejects it with the overlap reason.
3. **Given** an open year with locked periods, **When** the admin closes the year, **Then** it becomes read-only for setup but closing still requires the year-end processes elsewhere.
4. **Given** a fiscal year, **When** the admin bulk-generates its periods (monthly), **Then** periods are created with correct date ranges and statuses.
5. **Given** a period, **When** the admin locks it, **Then** posting into it is blocked; unlock restores it.

---

### User Story 2 - Document Sequences Administration (Priority: P1)

As an administrator, I need to view and manage document numbering sequences so that document numbering remains consistent and traceable. I can view all seeded sequences (BGT, APR, ENC, PO, PE, PTY, RCV, DSL, DSB, PAY), edit settings, and deactivate sequences. Changes apply only to future allocations.

**Why this priority**: Document sequences are essential for every financial transaction. Admins must be able to see sequence state at all times and adjust settings without breaking existing document numbers.

**Independent Test**: Can be fully tested by viewing the sequences list, editing a sequence name, and deactivating one. Verifies that already-issued numbers are untouched.

**Acceptance Scenarios**:

1. **Given** the seeded sequences (BGT, APR, ENC, PO, PE, PTY, RCV, DSL, DSB, PAY), **When** an admin views the sequences list, **Then** each shows prefix, next number, format, and active state.
2. **Given** a sequence, **When** the admin edits its settings or deactivates it, **Then** changes apply to future allocations only; already-issued numbers are untouched.
3. **Given** a deactivated sequence, **When** a document of that type is created, **Then** the error surfaces the deactivated sequence.

---

### User Story 3 - Currency Management (Priority: P2)

As an administrator, I need to manage currencies from an ISO-4217 reference list so that the system supports multi-currency operations. I can create currencies (code, name, symbol, decimals), activate them for use, and deactivate them while preserving historical data.

**Why this priority**: Currencies are needed before exchange rates can be configured, but are not blocking other admin functions in the short term.

**Independent Test**: Can be fully tested by creating a currency from the ISO list, activating it, and deactivating it. Verifies existing documents retain the currency.

**Acceptance Scenarios**:

1. **Given** ISO-4217 reference list, **When** an admin creates a currency (code, name, symbol, decimals), **Then** it appears inactive until activated.
2. **Given** a currency in use, **When** the admin deactivates it, **Then** existing documents keep it but new selection excludes it.

---

### User Story 4 - Exchange Rate Management (Priority: P2)

As an administrator, I need to record and manage exchange rates for currency pairs so that financial documents use correct conversion rates. I can record rates with effective dates, activate/deactivate rates, and the system resolves the applicable rate for any given date.

**Why this priority**: Exchange rates depend on currencies being set up. They are needed for multi-currency transactions but are secondary to fiscal year and sequence setup.

**Independent Test**: Can be fully tested by recording rates for a currency pair, activating one and verifying conflict deactivation, and looking up a rate by date.

**Acceptance Scenarios**:

1. **Given** a currency pair and effective date, **When** an admin records a rate, **Then** lookup by date returns the applicable rate.
2. **Given** multiple rates for overlapping dates, **When** activating one, **Then** others in conflict deactivate.
3. **Given** a rate, **When** deactivated, **Then** historical documents keep their recorded rate.

---

### User Story 5 - Closing Entries (Priority: P2)

As an administrator, I need to generate and manage year-end closing entries so that fiscal year results are properly carried forward. I can generate a closing proposal, approve it to post, and reverse a posted entry if needed.

**Why this priority**: Closing entries are the final step in the fiscal year lifecycle. They depend on a full year of posted activity and are less frequent than other admin tasks.

**Independent Test**: Can be fully tested by generating a closing entry for a fiscal year with posted activity, approving it, and reversing it. Verifies balanced lines and proper status transitions.

**Acceptance Scenarios**:

1. **Given** a fiscal year with posted activity, **When** an admin generates the closing entry, **Then** the system proposes balanced closing lines for revenue/expense accounts.
2. **Given** a proposed closing entry, **When** approved, **Then** it posts; the year's result carries to the closing account.
3. **Given** a posted closing entry, **When** reversed, **Then** a reversal entry restores the accounts and the original is marked reversed.

---

### Edge Cases

- **Overlap rejection message**: When opening a fiscal year that overlaps an existing open year, the system displays a clear message identifying the conflicting year and the overlapping date range.
- **Bulk-generate twice**: When bulk-generating periods for a fiscal year that already has periods, the system displays an idempotency message and makes no changes.
- **Lock on period with in-flight documents**: When locking a period that has unposted documents, the system warns the admin and requires confirmation before proceeding.
- **Deactivate currency with active exchange rates**: When deactivating a currency that has active exchange rates, the system blocks the action and lists the active rates that must be deactivated first.
- **Generate closing entry twice**: When generating a closing entry for a year that already has one, the system rejects with a message indicating an entry already exists.
- **Reverse already-reversed closing entry**: When reversing an entry that is already a reversal, the system rejects with a message indicating the entry has already been reversed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST gate every action by permission codes defined in `PermissionCodes.cs`.
- **FR-002**: System MUST show audit info (created/modified by, at) on every record.
- **FR-003**: System MUST never renumber issued documents when sequence settings change.
- **FR-004**: System MUST block posting UI hints on locked periods.
- **FR-005**: System MUST show effective-rate resolution in exchange rate lookup (which rate applies for a given date).
- **FR-006**: System MUST provide a dedicated admin section for Financial Settings with sub-sections for Currencies, Exchange Rates, Fiscal Years, Document Sequences, and Closing Entries.
- **FR-007**: System MUST display all fiscal year statuses (Draft, Open, SoftClosed, HardClosed) with appropriate visual indicators.
- **FR-008**: System MUST support bulk period generation with monthly Arabic-named periods.
- **FR-009**: System MUST display document sequence state (prefix, next number, format, active) in a persistent list view.
- **FR-010**: System MUST generate closing entry proposals showing balanced revenue/expense lines before posting.
- **FR-011**: System MUST support RTL layout across all financial settings screens.
- **FR-012**: System MUST use the shared UI component library (Badge, StatusBadge, Dialog, ConfirmDialog, FilterBar, etc.).
- **FR-013**: System MUST format monetary values using the shared MoneyDisplay component and Arabic locale conventions.
- **FR-014**: System MUST display exchange rate lookup results showing the effective rate and its source date.
- **FR-015**: System MUST support activating/deactivating currencies and exchange rates with confirmation dialogs.
- **FR-016**: System MUST provide fiscal year detail view showing periods, their lock status, and available lifecycle actions.
- **FR-017**: System MUST display closing entry line details (account, debit, credit) for review before approval.
- **FR-018**: System MUST show the reversal status of closing entries (original entry marked reversed, reversal entry linked).

### Key Entities

- **FiscalYear**: name, yearNumber, startDate, endDate, status (Draft/Open/SoftClosed/HardClosed), isClosed, closingJournalEntryId
- **FiscalPeriod**: fiscalYearId, periodNumber, name, startDate, endDate, isLockedForPosting
- **DocumentSequence**: name, documentType, fiscalYearId?, currentNumber, resetPolicy (Yearly/Never), isActive
- **Currency**: code, name, symbol, decimalPlaces, roundingPrecision, isBase, isActive
- **ExchangeRate**: baseCurrencyId, currencyId, rateDate, rateType (Official/Market), rate, isActive
- **YearEndClosingEntry**: closingEntryNumber, fiscalYearId, closingDate, description, status (Draft/PendingApproval/Approved/Posted/Cancelled), isReversal, reversalOfId, journalEntryId

### Field Contract

#### FiscalYear

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `name` | string | Yes (create + edit) | Required |
| `yearNumber` | number | No (auto) | Display only |
| `startDate` | Date | Yes (create) | Required |
| `endDate` | Date | Yes (create) | Required |
| `status` | enum | No (lifecycle) | Draft/Open/SoftClosed/HardClosed |
| `isClosed` | boolean | No | Derived from status |
| `closingJournalEntryId` | number? | No | Link to closing entry |

#### FiscalPeriod

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `fiscalYearId` | number | No (parent) | FK |
| `periodNumber` | number | No (auto) | 1-12 |
| `name` | string | No (auto) | Arabic month name |
| `startDate` | Date | No (auto) | Generated |
| `endDate` | Date | No (auto) | Generated |
| `isLockedForPosting` | boolean | Yes (lock/unlock) | Toggle |

#### DocumentSequence

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `name` | string | Yes (edit) | Sequence name |
| `documentType` | string | No | BGT/APR/ENC/PO/PE/PTY/RCV/DSL/DSB/PAY |
| `fiscalYearId` | number? | No | Optional FK |
| `currentNumber` | number | No (auto) | Next number |
| `resetPolicy` | enum | Yes (edit) | Yearly/Never |
| `isActive` | boolean | Yes (deactivate) | Toggle |

#### Currency

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `code` | string | No (from ISO picker) | ISO-4217 |
| `name` | string | Yes (create) | Arabic name |
| `symbol` | string | Yes (create) | Required |
| `decimalPlaces` | number | No (from ISO) | Auto from ISO |
| `roundingPrecision` | number | Yes (create + edit) | Default 0.01 |
| `isBase` | boolean | Yes (create + edit) | Base currency flag |
| `isActive` | boolean | Yes (activate/deactivate) | Toggle |

#### ExchangeRate

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `baseCurrencyId` | number | Yes (create) | Base currency |
| `currencyId` | number | Yes (create) | Target currency |
| `rateDate` | Date | Yes (create) | Effective date |
| `rateType` | enum | Yes (create) | Official/Market |
| `rate` | number | Yes (create + edit) | Exchange rate |
| `isActive` | boolean | Yes (activate/deactivate) | Toggle |

#### ClosingEntry

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `closingEntryNumber` | string | No (auto) | System-generated |
| `fiscalYearId` | number | No (auto) | FK |
| `closingDate` | Date | No (auto) | Set at generation |
| `description` | string | Yes (generate) | Optional |
| `status` | enum | No (lifecycle) | Draft/PendingApproval/Approved/Posted/Cancelled |
| `isReversal` | boolean | No | Flag |
| `reversalOfId` | number? | No | Link to original |
| `journalEntryId` | number? | No | Link to posted JE |

### Permissions

- **FR-PERM-001**: `FiscalYears.View` — View list + detail
- **FR-PERM-002**: `FiscalYears.Create` — Create fiscal year
- **FR-PERM-003**: `FiscalYears.Update` — Edit fiscal year
- **FR-PERM-004**: `FiscalYears.Open` — Open fiscal year
- **FR-PERM-005**: `FiscalYears.Close` — Close fiscal year
- **FR-PERM-006**: `FiscalPeriods.View` — View periods
- **FR-PERM-007**: `FiscalPeriods.Create` — Create period
- **FR-PERM-008**: `FiscalPeriods.Update` — Edit period
- **FR-PERM-009**: `FiscalPeriods.Lock` — Lock period
- **FR-PERM-010**: `FiscalPeriods.Unlock` — Unlock period
- **FR-PERM-011**: `DocumentSequences.View` — View sequences
- **FR-PERM-012**: `DocumentSequences.Create` — Create sequence
- **FR-PERM-013**: `DocumentSequences.Update` — Edit sequence (ADDED — was missing)
- **FR-PERM-014**: `DocumentSequences.Deactivate` — Deactivate sequence (ADDED — was missing)
- **FR-PERM-015**: `Currencies.View` — View currencies
- **FR-PERM-016**: `Currencies.Create` — Create currency
- **FR-PERM-017**: `Currencies.Update` — Edit currency
- **FR-PERM-018**: `Currencies.Activate` — Activate currency
- **FR-PERM-019**: `Currencies.Deactivate` — Deactivate currency
- **FR-PERM-020**: `ExchangeRates.View` — View rates
- **FR-PERM-021**: `ExchangeRates.Create` — Create rate
- **FR-PERM-022**: `ExchangeRates.Update` — Edit rate
- **FR-PERM-023**: `ExchangeRates.Activate` — Activate rate
- **FR-PERM-024**: `ExchangeRates.Deactivate` — Deactivate rate
- **FR-PERM-025**: `ClosingEntries.View` — View closing entries
- **FR-PERM-026**: `ClosingEntries.Generate` — Generate closing entry
- **FR-PERM-027**: `ClosingEntries.Approve` — Approve closing entry
- **FR-PERM-028**: `ClosingEntries.Reverse` — Reverse closing entry

### Implementation Note

Currency screens + ISO picker are ALREADY implemented in specs/027. US3/US4 consume them — no duplicate build.

### UI States Required

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| FiscalYears List | Skeleton | "لا توجد سنوات" | Toast | Guard | N/A | DataGrid |
| FiscalYear Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + periods + actions |
| FiscalYear Create | N/A | N/A | Toast | Guard | N/A | Form |
| DocumentSequences List | Skeleton | "لا توجد تسلسلات" | Toast | Guard | N/A | DataGrid |
| Currencies List | Skeleton | "لا توجد عملات" | Toast | Guard | N/A | DataGrid |
| Currency Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + edit + audit |
| Currency Create | N/A | N/A | Toast | Guard | N/A | ISO picker + form |
| ExchangeRates List | Skeleton | "لا توجد أسعار" | Toast | Guard | N/A | DataGrid |
| ExchangeRate Create | N/A | N/A | Toast | Guard | N/A | Form |
| ClosingEntries List | Skeleton | "لا توجد قيود إغلاق" | Toast | Guard | N/A | DataGrid |
| ClosingEntry Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + lines + actions |

### Tests Expected

| ID | Page | Test |
|----|------|------|
| T-024-001 | FiscalYears List | Renders with status badges |
| T-024-002 | FiscalYear Detail | Shows periods table |
| T-024-003 | FiscalYear Create | Form renders and validates |
| T-024-004 | FiscalYearStatusBadge | All 4 status variants |
| T-024-005 | PeriodLockIndicator | Locked/unlocked states |
| T-024-006 | DocumentSequences List | Renders all 10 seeded sequences |
| T-024-007 | Currencies List | Renders with activate/deactivate |
| T-024-008 | Currency Create | ISO picker search + form |
| T-024-009 | Currency Detail | Audit trail displayed |
| T-024-010 | ExchangeRates List | Renders with filters |
| T-024-011 | ExchangeRate Create | Form renders |
| T-024-012 | ExchangeRate Lookup | Shows effective rate |
| T-024-013 | ClosingEntries List | Renders with status |
| T-024-014 | ClosingEntry Detail | Lines table renders |
| T-024-015 | ClosingEntryLines | Balanced totals |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An admin can set up a full fiscal year (create, generate periods, open) in under 5 minutes.
- **SC-002**: Document sequence state is always visible without navigating away from the sequences list.
- **SC-003**: Closing entry generation shows a reviewable proposal with balanced lines before any posting occurs.
- **SC-004**: All financial settings screens render correctly in RTL layout with Arabic text.
- **SC-005**: Exchange rate lookup displays the effective rate and its source date within 1 second of request.
- **SC-006**: 100% of mutating actions require explicit permission authorization.
- **SC-007**: Every record displayed includes audit metadata (created/modified by and timestamp).
- **SC-008**: Locking a period immediately reflects in all posting-related UI hints across the system.

## Assumptions

- Backend APIs for all financial settings operations are fully implemented and tested.
- The frontend uses the existing shared UI component library (Badge, StatusBadge, Dialog, ConfirmDialog, FilterBar, etc.).
- The frontend follows the established pattern of feature folders with pages/, components/, hooks/, and shared/ directories.
- API client generation via NSwag (`npm run generate-api`) will produce typed clients for all FinancialSettings endpoints.
- Arabic is the primary UI language; all labels, messages, and status text are in Arabic.
- The existing permission system (`usePermission` hook) will gate all actions; backend permissions are the authority.
- Document sequence seeding (BGT, APR, ENC, PO, PE, PTY, RCV, DSL, DSB, PAY) is handled by the backend; the UI displays them read-only with edit capability.
- ISO-4217 currency list is available via the backend `GetIso4217CodesQuery` endpoint.
- Closing entry generation, approval, and reversal use the existing backend commands with their lifecycle transitions.
