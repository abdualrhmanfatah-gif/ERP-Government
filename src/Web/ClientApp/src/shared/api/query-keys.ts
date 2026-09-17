// Query key factory — centralized cache keys for TanStack Query.
// Reporting keys follow the [domain, entity, ...params] convention.

export const reportingKeys = {
  // RPT-01 — Budget Execution
  budgetExecution: (filters: unknown) => ['reporting', 'budget-execution', filters] as const,
  budgetExecutionDetail: (budgetItemId: number) =>
    ['reporting', 'budget-execution-detail', budgetItemId] as const,

  // RPT-02 — Revenue Collections
  revenueCollections: (filters: unknown) => ['reporting', 'revenue-collections', filters] as const,
  revenueCollectionsDetail: (receiptVoucherId: number) =>
    ['reporting', 'revenue-collections-detail', receiptVoucherId] as const,

  // RPT-03 — Disbursement Register
  disbursementRegister: (filters: unknown) => ['reporting', 'disbursement-register', filters] as const,
  disbursementRegisterDetail: (paymentOrderId: number) =>
    ['reporting', 'disbursement-register-detail', paymentOrderId] as const,

  // RPT-05 — Trial Balance
  trialBalance: (filters: unknown) => ['reporting', 'trial-balance', filters] as const,
  ledgerMovement: (accountId: number, fiscalYearId: number) =>
    ['reporting', 'ledger-movement', accountId, fiscalYearId] as const,

  // RPT-06 — Financial Statements
  balanceSheet: (params: unknown) => ['reporting', 'balance-sheet', params] as const,
  incomeStatement: (params: unknown) => ['reporting', 'income-statement', params] as const,
  generalLedger: (params: unknown) => ['reporting', 'general-ledger', params] as const,
  cashFlowStatement: (params: unknown) => ['reporting', 'cash-flow-statement', params] as const,
  trialBalanceLegacy: (params: unknown) => ['reporting', 'trial-balance-legacy', params] as const,
};
