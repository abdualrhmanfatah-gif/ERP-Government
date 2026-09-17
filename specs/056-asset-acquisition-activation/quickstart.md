# Quickstart Validation: Asset Acquisition & Activation

**Feature**: 056-asset-acquisition-activation | **Date**: 2026-09-15

## Prerequisites

- .NET 9 SDK installed
- SQL Server running with latest migrations applied (including JournalEntryId column on Assets)
- Node.js 18+ installed
- User account with `Assets.Update` permission
- At least one AssetGroup with `AccountAssetId` configured
- At least one Draft asset with OriginalValue > 0 and PurchaseDate set
- An open fiscal period for the current date

## Validation Scenarios

### V1: Activate Draft Asset (FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007)

1. Start the application (`dotnet run` from `src/Web`)
2. Navigate to `/assets` in the browser
3. Click on a Draft asset to open its detail page
4. **Expected**: "تفعيل" (Activate) button is visible
5. Click "تفعيل"
6. **Expected**: Activation form opens showing asset details, acquisition cost, and proposed journal entry
7. Modify DepreciationStartDate if needed
8. Click "تأكيد التفعيل" (Confirm Activation)
9. **Expected**: Button shows "جاري التفعيل..." and is disabled
10. **Expected**: Status changes to "نشط" (Active), ActivationDate is set, AcquisitionCost equals OriginalValue
11. **Expected**: Journal entry reference (JRN-xxxxxx) is displayed on the asset card

### V2: Verify Balanced Journal Entry (FR-004)

1. After activation, note the journal entry reference on the asset card
2. Navigate to the journal entry (via link or search)
3. **Expected**: Entry shows two lines:
   - Debit: AssetGroup's AccountAssetId, amount = OriginalValue
   - Credit: Acquisition account, amount = OriginalValue
4. **Expected**: Description is "استحواذ أصل {Code} - {Name}"
5. **Expected**: Entry status is "مرحّل" (Posted)

### V3: Activation Rejected for Non-Draft Asset (FR-011)

1. Open an asset that is already Active
2. **Expected**: "تفعيل" button is NOT visible (or disabled)
3. Attempt to call the activation API directly with an Active asset
4. **Expected**: 400 error with "لا يمكن تفعيل الأصل من حالة Active"

### V4: Activation Rejected for Missing Data (FR-002, FR-003)

1. Create a Draft asset with OriginalValue = 0
2. Attempt to activate
3. **Expected**: Error "القيمة الأصلية يجب أن تكون أكبر من صفر"
4. Create a Draft asset with no PurchaseDate
5. Attempt to activate
6. **Expected**: Error "تاريخ الشراء مطلوب"
7. Create a Draft asset with AssetGroup that has no AccountAssetId
8. Attempt to activate
9. **Expected**: Error "لم يتم تعيين حساب الأصول للمجموعة"

### V5: Concurrent Activation Prevention (FR-012)

1. Open the same Draft asset in two browser tabs
2. Activate in tab 1
3. Attempt to activate in tab 2
4. **Expected**: Tab 2 shows concurrency conflict error

### V6: Activation with Closed Period (FR-015)

1. Set the current date's fiscal period to closed (via admin)
2. Attempt to activate a Draft asset
3. **Expected**: Error "الفترة المحاسبية مغلقة"

### V7: AssetAcquired Event Published (FR-010)

1. After activation, check the OutboxMessages table
2. **Expected**: An OutboxMessage with type "AssetAcquired" exists
3. **Expected**: Event contains SourceEntityId = asset.Id, AssetCode, OriginalValue

### V8: Audit Trail (FR-016)

1. After activation, check the asset's LastModified timestamp
2. **Expected**: Timestamp matches activation time
3. **Expected**: LastModifiedBy shows the activating user

### V9: Frontend Loading State (User Story 1)

1. Open a Draft asset and click "تفعيل"
2. Click "تأكيد التفعيل"
3. **Expected**: Button shows "جاري التفعيل..." and is disabled
4. **Expected**: Form remains visible during processing
5. **Expected**: Button re-enables if activation fails

### V10: Journal Entry Link on Asset Card (FR-014)

1. Open an activated asset's detail page
2. **Expected**: Journal entry reference (JRN-xxxxxx) is displayed
3. **Expected**: Clicking the reference navigates to the journal entry page
4. **Expected**: Journal entry shows correct debit/credit lines

## Backend Unit Test Scenarios

### ActivateAssetCommand

- Success: Draft asset with valid data → status changes to Active, JournalEntryId set, ActivationDate set, AcquisitionCost set
- Failure: Asset not found → returns AssetNotFound error
- Failure: Asset in Active status → returns InvalidStatusTransition error
- Failure: AssetGroup has no AccountAssetId → returns AccountNotConfigured error
- Failure: OriginalValue = 0 → returns validation error
- Failure: Accounting period closed → returns PeriodClosed error
- Failure: Stale RowVersion → returns ConcurrencyConflict error
- Success: Publishes AssetAcquired domain event

### Activation Validation

- Valid command passes validator
- Missing DepreciationStartDate fails validator
- Invalid RowVersion fails validator
