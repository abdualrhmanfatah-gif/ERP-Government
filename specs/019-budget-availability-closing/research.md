# Research: Financial Control Layer

**Feature**: 019-budget-availability-closing | **Date**: 2026-09-05

## Research Items

### R1: Multi-Dimension Availability Query Design

**Decision**: Extend `BudgetAvailabilityService.GetAvailabilitySummaryAsync` with a new method `GetAvailabilityBreakdownAsync(int budgetItemId, int fiscalYearId)` that returns `List<AvailabilityBreakdownDto>`.

**Rationale**: The existing service already queries Appropriations and Encumbrances by BudgetItemId. The breakdown adds a GROUP BY on the dimension columns (FundId from BudgetItem, and program/project dimensions from BudgetClassification or CostCenter). The query filters Appropriations by fiscal year via the Budget → FiscalYear relationship.

**Alternatives considered**:
- Separate service class — rejected for code duplication.
- Database view/materialized view — rejected per "zero stored computed" requirement.
- In-memory grouping after existing query — rejected because it would load all records before grouping.

**Implementation approach**:
```csharp
public record AvailabilityBreakdownDto(
    int FundId, string FundCode, string FundName,
    int? ProgramId, string? ProgramCode,
    int? ProjectId, string? ProjectCode,
    int BudgetItemId, string ItemCode,
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);
```

Query joins: Appropriation → BudgetItem → Fund; groups by FundId, program/project dimensions. Encumbrances summed per appropriation. Payments (completed disbursements) summed per encumbrance.

### R2: Year-Boundary Document Splitting

**Decision**: Proportional allocation based on days.

**Rationale**: For an encumbrance or payment spanning two fiscal years, split the amount proportionally by the number of days in each year. Example: an encumbrance of 365,000 covering Jan 1 – Dec 31, split at June 30 gives 182,500 to the first half (182 days) and 182,500 to the second half (183 days).

**Formula**: `amount × (days_in_year / total_span_days)`

**Alternatives considered**:
- Equal split — rejected because it doesn't reflect actual time-weighted commitment.
- Cash-flow based split — rejected because cash flow data may not be available at split time.
- Manual split by user — rejected because it requires manual intervention and is error-prone.

**Edge case**: If the document starts or ends exactly on the year boundary, no split is needed — the entire amount belongs to one year.

### R3: Lapse Run Locking Strategy

**Decision**: Application-level optimistic concurrency via RowVersion on FiscalYear, plus a check for existing Completed YearClosingRun before proceeding.

**Rationale**: SQL Server row-level locking is implicit with EF Core transactions. The explicit check for an existing Completed run prevents duplicate lapses. During the lapse transaction, the FiscalYear's RowVersion is verified to prevent concurrent modifications.

**Alternatives considered**:
- Advisory locks — rejected because they require database-specific features.
- Distributed locks — rejected because this is a single-server application.
- Pessimistic locking (SELECT FOR UPDATE) — rejected because EF Core doesn't natively support it; would require raw SQL.

### R4: Closing Entry Account Mapping

**Decision**: Closing entries transfer revenue/expense account balances to a "Final Account" equity account (e.g., account code 9001 — Retained Earnings / Budget Surplus).

**Rationale**: Standard government accounting practice. Revenue accounts are debited (zeroed), expense accounts are credited (zeroed), and the net is posted to the final account equity account. The posting rule template maps each revenue/expense account to the final account.

**Implementation**: A `PostingRule` with `EventType = ClosingEntry` that generates balanced journal entries from account balances.

### R5: FiscalYear Lifecycle Integration

**Decision**: The lapse command operates on a FiscalYear with `Status = Open` (or equivalent active status). After lapse, the FiscalYear's `IsClosed` flag is set to `true`. The final account generation requires `IsClosed = true`. Reopening sets `IsClosed = false`.

**Rationale**: The existing `FiscalYear` entity already has `IsClosed` and `ClosingJournalEntryId` fields. The lapse operation uses these existing fields plus the new `YearClosingRun` append-only log for detailed tracking.

**Note**: The existing `FiscalYears.Close` permission is for period-level closing. The new `FinancialControl.LapseYear` is for the year-end lapse operation (different scope).

### R6: Payment Blocking Against Lapsed Items

**Decision**: The lapse command marks encumbrances with `Status = Cancelled` (using a reason of "lapsed") and sets appropriation balances to zero. Any subsequent payment or disbursement request against a lapsed budget item is blocked by checking the `YearClosingRun` status for the fiscal year.

**Rationale**: Rather than adding a new "Lapsed" status to every entity, the blocking is done at the service level: before processing a payment, check if the fiscal year has been lapsed. This is simpler and more maintainable.

**Implementation**: A guard method in the payment/disbursement handler: `await CheckFiscalYearNotLapsedAsync(fiscalYearId)`.
