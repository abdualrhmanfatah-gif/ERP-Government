# Quickstart Validation: Financial Settings Admin UI

**Feature**: 024-financial-settings-admin
**Date**: 2026-09-06

## Prerequisites

- Backend running (`dotnet run` from `src/Web/` or via Aspire AppHost)
- Frontend dev server running (`npm run dev` from `src/Web/ClientApp/`)
- Admin user logged in with FinancialSettings permissions
- Browser set to RTL layout

## Validation Scenarios

### V1: Fiscal Year Lifecycle (US1)

1. Navigate to Financial Settings > Fiscal Years
2. Click "New Fiscal Year" → enter name "2026", start "2026-01-01", end "2026-12-31"
3. **Expected**: Year appears in list with Draft status
4. Click on year → detail page shows periods section (empty)
5. Click "Generate Periods" → 12 monthly periods created
6. **Expected**: Periods table shows 12 rows with correct date ranges, all unlocked
7. Click "Open Year" → status changes to Open
8. **Expected**: Status badge shows "مفتوح" (Open)
9. Try creating another year overlapping 2026 → click Open
10. **Expected**: Error message showing overlap with existing open year
11. Lock a period → posting hints are blocked for that period
12. Click "Close Year" → status changes to HardClosed

### V2: Document Sequences (US2)

1. Navigate to Financial Settings > Document Sequences
2. **Expected**: 10 seeded sequences visible (BGT, APR, ENC, PO, PE, PTY, RCV, DSL, DSB, PAY)
3. **Expected**: Each shows prefix, next number, format, active state
4. Edit a sequence name → save
5. **Expected**: Name updated, current number unchanged
6. Deactivate a sequence
7. **Expected**: Sequence marked inactive, existing numbers preserved

### V3: Currency Management (US3)

1. Navigate to Financial Settings > Currencies
2. Click "New Currency" → search ISO 4217 list for "YER"
3. Select YER → fill symbol, confirm decimals
4. **Expected**: Currency appears as inactive
5. Click activate → currency becomes active
6. **Expected**: isActive = true
7. Click deactivate
8. **Expected**: Currency deactivated (if no active exchange rates reference it)

### V4: Exchange Rates (US4)

1. Navigate to Financial Settings > Exchange Rates
2. Click "New Exchange Rate" → select base currency, target currency, date, rate type, rate
3. **Expected**: Rate appears in list as inactive
4. Click activate → rate becomes active
5. Create another rate for overlapping period → activate it
6. **Expected**: Previous overlapping rate is deactivated
7. Use lookup endpoint with a date → returns the applicable active rate
8. **Expected**: Lookup shows rate value and source date

### V5: Closing Entries (US5)

1. Navigate to Financial Settings > Closing Entries (for a fiscal year with posted activity)
2. Click "Generate Closing Entry"
3. **Expected**: Entry appears with Draft/PendingApproval status
4. Open entry detail → review closing lines (account, debit, credit)
5. **Expected**: Lines are balanced (total debits = total credits)
6. Click "Approve" → entry status changes to Posted
7. **Expected**: Year result carried to closing account
8. Click "Reverse" → reversal entry created
9. **Expected**: Original entry marked as reversed, reversal entry linked

### V6: RTL and Arabic

1. All screens render correctly in RTL layout
2. All status labels display in Arabic
3. Monetary values formatted with Arabic locale
4. Date pickers use Arabic calendar conventions

## Test Commands

```bash
# Frontend tests
cd src/Web/ClientApp
npm run test -- --run src/features/financial-settings/__tests__/

# Frontend lint
npm run lint

# Frontend build
npm run build
```

## Expected Outcomes

- All 5 validation scenarios pass
- No console errors in browser
- All tests pass
- Build succeeds with no warnings
- RTL layout renders correctly on all screens
