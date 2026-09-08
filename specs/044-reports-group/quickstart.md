# Quickstart Validation: Reports Group (RPT-01..06)

**Date**: 2026-09-08

## Prerequisites

- Backend running (`dotnet run --project src/Web`)
- Frontend dev server running (`npm run dev` in `src/Web/ClientApp`)
- Database seeded with test data (fiscal year, budget items, appropriations, encumbrances, payments, receipt vouchers, payment orders, journal entries)
- User logged in with all reporting permissions

## Validation Scenarios

### RPT-01 — Budget Execution Report (existing, verify)

1. Navigate to `/reporting/budget-execution`
2. Select a fiscal year → verify lines load with 4 financial columns + totals row
3. Filter by fund → verify lines filter and totals recompute
4. Click a line → verify detail sheet opens with encumbrances/payments tabs
5. Export Excel → verify matches screen
6. Export PDF → verify RTL rendering

### RPT-02 — Revenue Collections Report

1. Navigate to `/reporting/revenue-collections`
2. Select a period → verify lines load with voucher number, date, party, amount, method, deposit status
3. Filter by party/payment method → verify filtering works
4. Click a voucher → verify detail shows lines + checks + deposit card
5. Verify cancelled vouchers are clearly marked
6. Export → verify matches screen

### RPT-03 — Disbursement Register Report

1. Navigate to `/reporting/disbursement-register`
2. Select a period → verify status counter cards (draft/submitted/approved/paid/rejected) + totals + lines
3. Verify counter totals match line counts
4. Click an order → verify detail shows payments, approver, paidAt
5. Export → verify matches screen

### RPT-04 — Availability Snapshot Report

1. Navigate to `/reporting/availability-snapshot`
2. Select a fiscal year → verify lines load with controlState indicator + amounts
3. Verify controlState displays with colored indicator 100% of the time
4. Click an item → verify detail shows appropriations + encumbrances + payments
5. Export → verify matches screen

### RPT-05 — Trial Balance Report

1. Navigate to `/reporting/trial-balance`
2. Select a fiscal year → verify lines load with 5 columns + 4 totals
3. Verify balance indicator shows when Σdebits = Σcredits
4. Verify red warning when imbalance (if test data supports)
5. Click an account → verify ledger movement entries + totals
6. Verify open period warning "أرقام قابلة للتغير" when applicable
7. Export → verify matches screen

### RPT-06 — Financial Statements

1. **Balance Sheet**: Navigate to `/reporting/financial-statements/balance-sheet` → verify assets/liabilities/equity groups + titleAr in RTL + balanced indicator
2. **Income Statement**: Navigate to `/reporting/financial-statements/income-statement` → verify revenue/expenses + netIncome
3. **General Ledger**: Navigate to `/reporting/financial-statements/general-ledger` → verify paginated lines with runningBalance, page through without freeze
4. **Cash Flow**: Navigate to `/reporting/financial-statements/cash-flow` → verify bilingual sections (titleAr RTL)
5. **Legacy Trial Balance**: Navigate to `/reporting/financial-statements/trial-balance` → verify sections + isBalanced + totals
6. Export any statement → verify matches screen

### Cross-Cutting (all reports)

- **RC-1**: Verify all financial numbers come from server (no client recomputation)
- **RC-2**: Export matches screen for same filters
- **RC-3**: Empty state shows "لا توجد بيانات للفترة المحددة" with Arabic artwork
- **RC-4**: Full result set loaded (no pagination threshold)
- **RC-5**: All views and exports render RTL correctly
- **RC-6**: Unauthorized user sees unauthorized state
- **RC-7**: Fresh computation on every request (no stale data)

## Expected Outcomes

- All 6 report groups accessible from navigation
- All pages load within performance targets (<3s P1, <2s RPT-02)
- Export produces matching Excel + PDF with correct RTL
- Empty states render with Arabic messaging
- Detail drill-down works for all reports with appropriate tabs/sections
