# Data Model: Asset Acquisition & Activation

**Feature**: 056-asset-acquisition-activation | **Date**: 2026-09-15

## Entity Changes

### Entity: Asset (Modified)

**Table**: `Assets` | **Source**: `src/Domain/Assets/Entities/Asset.cs`

#### New Field

| Property | Type | Nullable | Constraints | Notes |
|----------|------|----------|-------------|-------|
| JournalEntryId | int? | Yes | FK → JournalEntries (SetNull) | Links to acquisition journal entry created during activation |

#### Migration Impact

- Add nullable `JournalEntryId` column to `Assets` table
- Add FK constraint to `JournalEntries` table with `ON DELETE SET NULL`
- Add index on `JournalEntryId` for query performance

#### Updated Relationships

```
Asset ──FK──> AssetGroup (Restrict)
Asset ──FK──> Location (nullable)
Asset ──FK──> Fund (nullable)
Asset ──FK──> CostCenter (nullable)
Asset ──FK──> User/Custodian (nullable)
Asset ──FK──> JournalEntry (SetNull) [NEW]
```

### Entity: JournalEntry (Existing)

**Table**: `JournalEntries` | **Source**: `src/Domain/Accounting/Entities/JournalEntry.cs`

No schema changes required. The activation command will create journal entries following the existing pattern.

### Entity: JournalEntryLine (Existing)

**Table**: `JournalEntryLines` | **Source**: `src/Domain/Accounting/Entities/JournalEntryLine.cs`

No schema changes required. Two lines will be created per activation (debit + credit).

## Journal Entry Structure for Activation

### Header

| Field | Value | Source |
|-------|-------|--------|
| EntryNumber | Auto-generated (JRN-xxxxxx) | IDocumentSequenceService |
| DocumentDate | Current date | DateTime.Today |
| EntryStatus | Draft | Default |
| EntryType | SystemGenerated | Constant |
| PeriodId | Active fiscal period | FiscalPeriods lookup |
| FiscalYearId | From active period | FiscalPeriod.FiscalYearId |
| IsSystemGenerated | true | Constant |
| Narration | "استحواذ أصل {Code} - {Name}" | Auto-generated |

### Lines

| Line | Account | Debit | Credit | Description |
|------|---------|-------|--------|-------------|
| 1 | AssetGroup.AccountAssetId | OriginalValue | 0 | "استحواذ أصل {Code} - {Name}" |
| 2 | AcquisitionAccount (hardcoded) | 0 | OriginalValue | "استحواذ أصل {Code} - {Name}" |

## State Transitions

### Asset Status During Activation

```
Draft ──────> Active (upon successful activation)
```

Validation:
- Only Draft assets can be activated (FR-001, FR-011)
- OriginalValue > 0 required (FR-002)
- AssetGroup must have AccountAssetId configured (FR-003)
- Accounting period for activation date must be open (FR-015)

### Journal Entry Lifecycle During Activation

```
[Created as Draft] ──> [Posted] (single step)
```

The activation command creates the journal entry directly in Posted status (following the programmatic pattern from RecordPaymentCommand). This is appropriate because:
- The activation is a single-step operation (per user decision Q2)
- The entry is system-generated and validated before creation
- No manual review/approval is needed for acquisition entries

## Validation Rules

### Activation Command Validation

| Rule | Error Code | Message |
|------|------------|---------|
| Asset must exist | Assets.AssetNotFound | الأصل بالمعرف {Id} غير موجود |
| Asset must be in Draft status | Assets.InvalidStatusTransition | لا يمكن تفعيل الأصل من حالة {Status} |
| AssetGroup must exist | Assets.AssetGroupNotFound | المجموعة غير موجودة |
| AccountAssetId must be configured | Assets.AccountNotConfigured |لم يتم تعيين حساب الأصول للمجموعة |
| OriginalValue must be > 0 | Assets.InvalidValue | القيمة الأصلية يجب أن تكون أكبر من صفر |
| PurchaseDate must be set | Assets.InvalidDate | تاريخ الشراء مطلوب |
| Acquisition account must exist | Assets.AcquisitionAccountNotFound | حساب الاستحواذ غير مُعد |
| Accounting period must be open | Assets.PeriodClosed | الفترة المحاسبية مغلقة |
| RowVersion must match | ConcurrencyConflict | تم تعديل الأصل من مستخدم آخر |

### Error Codes (New)

| Code | Category | Description |
|------|----------|-------------|
| Assets.AccountNotConfigured | Validation | AssetGroup has no AccountAssetId |
| Assets.AcquisitionAccountNotFound | Configuration | Hardcoded acquisition account not found |
| Assets.PeriodClosed | Validation | Accounting period for activation date is closed |
| Assets.AlreadyActivated | Validation | Asset is already Active |

## Field Locking After Activation

Once an asset is activated (Status = Active), the following fields become read-only:
- OriginalValue
- PurchaseDate
- AssetGroupId

This is already enforced by the existing UpdateAssetCommandHandler (LockedStatuses check).

## Audit Trail

The activation operation will be logged with:
- Actor: Current user (CreatedBy/LastModifiedBy from BaseAuditableEntity)
- Timestamp: Activation date (LastModified)
- Journal Entry Reference: JournalEntryId stored on the Asset entity

The journal entry itself provides a complete audit trail with:
- Entry number (JRN-xxxxxx)
- Debit/credit lines with account references
- System-generated flag
- Posted timestamp
