# Research: ERP Read-Only Reporting & Oversight

**Feature**: 020-erp-read-only-reporting
**Date**: 2026-09-06

## R1: How to query budget execution across appropriations, encumbrances, and payments?

**Decision**: LINQ joins over Appropriation, Encumbrance, and PaymentOrder entities via IApplicationDbContext. Group by BudgetItem + Fund + Program + Project dimensions.

**Rationale**: The existing `BudgetAvailabilityService.GetAvailabilityBreakdownAsync` already computes appropriation/encumbered/paid per dimension. The budget execution report extends this by also showing the full budget line listing (not just one item's breakdown).

**Alternatives Considered**:
- Database views: rejected — Constitution requires migrations-only schema changes, and views add persistence complexity.
- Stored procedures: rejected — violates "no stored aggregates" constraint.
- Separate aggregate tables: rejected — violates "no new tables" constraint.

**Query Pattern**:
```csharp
// Pseudocode — actual implementation uses EF Core LINQ
var report = await dbContext.Appropriations
    .AsNoTracking()
    .Include(a => a.BudgetItem)
    .Include(a => a.Budget)
    .Where(filters)
    .GroupBy(a => new { a.BudgetItem.Id, a.Budget.FundId, ... })
    .Select(g => new BudgetExecutionLineDto {
        Appropriated = g.Sum(a => a.Amount),
        Encumbered = /* join Encumbrances */,
        Paid = /* join PaymentOrders */,
        Available = Appropriated - Encumbered - Paid
    })
    .ToListAsync();
```

## R2: How to query revenue collections with deposit-slip and check-clearing status?

**Decision**: Join ReceiptVoucher → ReceiptVoucherLine (revenue account) → Check (clearing status) → DepositSlip (deposit status). Filter by RevenueAccountId, PartyId, PaymentMethod, and date range.

**Rationale**: ReceiptVoucher entity already has navigation properties to Lines, Checks, and DepositSlip. The existing `GetMonthlyStatementQuery` demonstrates this pattern.

**Alternatives Considered**:
- Query RevenueReceipt instead: rejected — RevenueReceipt is the posted revenue record; ReceiptVoucher is the collection instrument with deposit/check details needed for US2.
- Join through JournalEntryLine: rejected — ReceiptVoucher has direct relationships; no need for ledger detour.

## R3: How to handle drill-down from summary to detail?

**Decision**: Separate detail queries per report type. Summary endpoint returns aggregated rows; detail endpoint accepts the grouping key (e.g., BudgetItemId + FundId) and returns individual source records.

**Rationale**: Drill-down requires fetching individual source records (encumbrances, payments, receipt vouchers, journal entries). A separate query per drill-down keeps summary queries fast and detail queries targeted.

**Alternatives Considered**:
- Single query with optional detail flag: rejected — adds unnecessary complexity to the summary query path.
- Client-side join: rejected — violates server-side data access pattern.

## R4: How to implement report export?

**Decision**: Reuse existing `IReportExporter` interface with `ExcelReportExporter` (ClosedXML) and `PdfReportExporter` (QuestPDF). Each report query returns a DTO; export endpoint converts DTO to `ReportResult` and streams to client.

**Rationale**: Export infrastructure already exists in `src/Infrastructure/Services/`. The existing `Reports.cs` endpoint demonstrates the pattern: send query → get result → create MemoryStream → exporter writes → `Results.File()`.

**Alternatives Considered**:
- CSV export: spec mentions Excel/PDF only. No existing CSV infrastructure.
- Client-side export: rejected — server-side stream is faster for large datasets and avoids client memory pressure.

## R5: How to ensure performance under 2 seconds?

**Decision**: 
1. `AsNoTracking()` on all queries — no change tracking overhead.
2. Filter by fiscal year (partition-sized) before dimension joins.
3. Use `ProjectBy` (anonymous type projections) to select only needed columns.
4. Index hints: ensure covering indexes on (FundId, FiscalYearId, BudgetItemId) and (VoucherDate, FundId).

**Rationale**: The 2-second target is achievable for ~50k-200k rows with proper indexing and no-change-tracking. The existing `BudgetAvailabilityService` already performs similar aggregations.

**Alternatives Considered**:
- Materialized views / pre-computed aggregates: rejected — violates "no stored aggregates" constraint.
- Caching: acceptable as an optimization but not required for initial implementation; query performance should meet target without caching.

## R6: How to handle authorization for report endpoints?

**Decision**: Register new permission codes in `PermissionCodes.cs` and wire endpoints with `.RequireAuthorization(PermissionCodes.XXX)`. Follow existing placeholder pattern (`RequireAssertion(_ => true)`) until RBAC enforcement spec (DEP-020) lands.

**Rationale**: Constitution Principle VII requires named permissions on every endpoint. Current state has all policies as open placeholders. New endpoints must declare permissions for consistency.

**Permission Codes to Add**:
- `Reporting.ViewBudgetExecution`
- `Reporting.ViewRevenueCollections`
- `Reporting.ViewDisbursementRegister`
- `Reporting.ViewAvailabilitySnapshot`
- `Reporting.ViewTrialBalanceReport`
- `Reporting.ExportReports`

## R7: How to scope reports by fund and period?

**Decision**: All report queries accept `FundId?` and `FiscalYearId` / `FiscalPeriodId` as required filter parameters. Fund filter joins through Budget.FundId or PaymentOrder.FundId or ReceiptVoucher → RevenueReceipt.FundId depending on the report.

**Rationale**: Period and fund are the primary scoping dimensions per spec FR-006. Optional filters (program, project, budget item, party, method, status, approver) narrow further.

## R8: How to handle multi-currency in reports?

**Decision**: Display base-currency amounts only (clarified in spec session 2026-09-06). JournalEntryLine carries both original amounts and base-currency equivalents. Reports query `Debit`/`Credit` columns (already in base currency per Constitution Principle IV).

**Rationale**: Constitution Principle IV mandates base-currency totals. The clarification confirmed users do not need original-currency display at summary level.
