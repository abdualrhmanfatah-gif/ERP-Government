import { useState } from 'react';

interface UsePermissionResult {
  hasPermission: boolean;
  isLoading: boolean;
}

// Policy strings confirmed from PermissionCodes.cs
type PolicyString =
  | 'Currencies.View' | 'Currencies.Create' | 'Currencies.Update' | 'Currencies.Activate' | 'Currencies.Deactivate'
  | 'ExchangeRates.View' | 'ExchangeRates.Create' | 'ExchangeRates.Update' | 'ExchangeRates.Activate' | 'ExchangeRates.Deactivate'
  | 'FiscalYears.View' | 'FiscalYears.Create' | 'FiscalYears.Update' | 'FiscalYears.Open' | 'FiscalYears.Close'
  | 'FiscalPeriods.View' | 'FiscalPeriods.Create' | 'FiscalPeriods.Update' | 'FiscalPeriods.Lock' | 'FiscalPeriods.Unlock'
  | 'DocumentSequences.View' | 'DocumentSequences.Create'
  | 'Suppliers.View' | 'Suppliers.Create' | 'Suppliers.Update' | 'Suppliers.Activate' | 'Suppliers.Deactivate'
  | 'PurchaseRequests.View' | 'PurchaseRequests.Create' | 'PurchaseRequests.Submit' | 'PurchaseRequests.Approve' | 'PurchaseRequests.Reject'
  | 'RFQ.View' | 'RFQ.Create' | 'RFQ.Publish' | 'RFQ.Complete' | 'RFQ.Cancel'
  | 'Quotations.View' | 'Quotations.Create'
  | 'PurchaseOrders.View' | 'PurchaseOrders.Create' | 'PurchaseOrders.Submit' | 'PurchaseOrders.Approve' | 'PurchaseOrders.Cancel'
  | 'Assets.View' | 'Assets.Create' | 'Assets.Update'
  | 'AssetGroups.View' | 'AssetGroups.Create' | 'AssetGroups.Update'
  | 'AssetMovements.View' | 'AssetMovements.Create' | 'AssetMovements.Approve'
  | 'AssetDisposals.View' | 'AssetDisposals.Create' | 'AssetDisposals.Approve' | 'AssetDisposals.Post'
  | 'AssetRevaluations.View' | 'AssetRevaluations.Create' | 'AssetRevaluations.Approve' | 'AssetRevaluations.Post'
  | 'AssetImpairments.View' | 'AssetImpairments.Create' | 'AssetImpairments.Approve' | 'AssetImpairments.Post'
  | 'Depreciation.View' | 'Depreciation.Post' | 'Depreciation.Reverse'
  | 'Items.View' | 'Items.Create' | 'Items.Update'
  | 'ItemCategories.View' | 'ItemCategories.Create' | 'ItemCategories.Update'
  | 'Units.View' | 'Units.Create' | 'Units.Update'
  | 'Warehouses.View' | 'Warehouses.Create' | 'Warehouses.Update'
  | 'Locations.View' | 'Locations.Create' | 'Locations.Update'
  | 'GoodsReceiptNotes.View' | 'GoodsReceiptNotes.Create' | 'GoodsReceiptNotes.Approve'
  | 'StockTakes.View' | 'StockTakes.Create' | 'StockTakes.Start' | 'StockTakes.Complete' | 'StockTakes.Approve'
  | 'StockTransactions.View'
  | 'BudgetTypes.View' | 'BudgetTypes.Create' | 'BudgetTypes.Update'
  | 'Funds.View' | 'Funds.Create' | 'Funds.Update' | 'Funds.Activate' | 'Funds.Deactivate'
  | 'BudgetClassifications.View' | 'BudgetClassifications.Create' | 'BudgetClassifications.Update'
  | 'Budgets.View' | 'Budgets.Create' | 'Budgets.Update' | 'Budgets.Submit' | 'Budgets.Approve'
  | 'Budgets.Activate' | 'Budgets.Suspend' | 'Budgets.Close' | 'Budgets.Cancel'
  | 'BudgetItems.View' | 'BudgetItems.Create' | 'BudgetItems.Update' | 'BudgetItems.Delete'
  | 'Appropriations.View' | 'Appropriations.Create' | 'Appropriations.Update' | 'Appropriations.Delete'
  | 'Appropriations.Submit' | 'Appropriations.Approve' | 'Appropriations.Activate'
  | 'Appropriations.Suspend' | 'Appropriations.Close' | 'Appropriations.Cancel'
  | 'Encumbrances.View' | 'Encumbrances.Create' | 'Encumbrances.Submit' | 'Encumbrances.Approve'
  | 'Encumbrances.Activate' | 'Encumbrances.Release' | 'Encumbrances.Close' | 'Encumbrances.Cancel'
  | 'Encumbrances.Reverse'
  | 'Accounting.ChartOfAccounts.Read' | 'Accounting.ChartOfAccounts.Create' | 'Accounting.ChartOfAccounts.Edit'
  | 'Accounting.JournalEntries.Read' | 'Accounting.JournalEntries.Create' | 'Accounting.JournalEntries.Submit'
  | 'Accounting.JournalEntries.Approve' | 'Accounting.JournalEntries.Post' | 'Accounting.JournalEntries.Reverse'
  | 'Accounting.JournalEntries.UpdateLines' | 'Accounting.JournalEntries.Cancel'
  | 'Accounting.Reports.BalanceSheet' | 'Accounting.Reports.IncomeStatement'
  | 'Accounting.Reports.GeneralLedger' | 'Accounting.Reports.CashFlow'
  | 'Accounting.Reports.Export' | 'Accounting.Reports.Print';

// Stub permission set — always grants all permissions.
// When backend provides real permissions, replace this with API call.
const STUB_GRANTED_POLICIES: Set<string> = new Set(
  Object.entries({
    Currencies: ['View', 'Create', 'Update', 'Activate', 'Deactivate'],
    ExchangeRates: ['View', 'Create', 'Update', 'Activate', 'Deactivate'],
    FiscalYears: ['View', 'Create', 'Update', 'Open', 'Close'],
    FiscalPeriods: ['View', 'Create', 'Update', 'Lock', 'Unlock'],
    DocumentSequences: ['View', 'Create'],
    Suppliers: ['View', 'Create', 'Update', 'Activate', 'Deactivate'],
    PurchaseRequests: ['View', 'Create', 'Submit', 'Approve', 'Reject'],
    RFQ: ['View', 'Create', 'Publish', 'Complete', 'Cancel'],
    Quotations: ['View', 'Create'],
    PurchaseOrders: ['View', 'Create', 'Submit', 'Approve', 'Cancel'],
    Assets: ['View', 'Create', 'Update'],
    AssetGroups: ['View', 'Create', 'Update'],
    AssetMovements: ['View', 'Create', 'Approve'],
    AssetDisposals: ['View', 'Create', 'Approve', 'Post'],
    AssetRevaluations: ['View', 'Create', 'Approve', 'Post'],
    AssetImpairments: ['View', 'Create', 'Approve', 'Post'],
    Depreciation: ['View', 'Post', 'Reverse'],
    Items: ['View', 'Create', 'Update'],
    ItemCategories: ['View', 'Create', 'Update'],
    Units: ['View', 'Create', 'Update'],
    Warehouses: ['View', 'Create', 'Update'],
    Locations: ['View', 'Create', 'Update'],
    GoodsReceiptNotes: ['View', 'Create', 'Approve'],
    StockTakes: ['View', 'Create', 'Start', 'Complete', 'Approve'],
    StockTransactions: ['View'],
    BudgetTypes: ['View', 'Create', 'Update'],
    Funds: ['View', 'Create', 'Update', 'Activate', 'Deactivate'],
    BudgetClassifications: ['View', 'Create', 'Update'],
    Budgets: ['View', 'Create', 'Update', 'Submit', 'Approve', 'Activate', 'Suspend', 'Close', 'Cancel'],
    BudgetItems: ['View', 'Create', 'Update', 'Delete'],
    Appropriations: ['View', 'Create', 'Update', 'Delete', 'Submit', 'Approve', 'Activate', 'Suspend', 'Close', 'Cancel'],
    Encumbrances: ['View', 'Create', 'Submit', 'Approve', 'Activate', 'Release', 'Close', 'Cancel', 'Reverse'],
    'Accounting.ChartOfAccounts': ['Read', 'Create', 'Edit'],
    'Accounting.JournalEntries': ['Read', 'Create', 'Submit', 'Approve', 'Post', 'Reverse', 'UpdateLines', 'Cancel'],
    'Accounting.Reports': ['BalanceSheet', 'IncomeStatement', 'GeneralLedger', 'CashFlow', 'Export', 'Print'],
  }).flatMap(([module, actions]) => actions.map(action => `${module}.${action}`))
);

/**
 * Permission hook — STUB implementation.
 * Always returns true for all policies during development.
 * Wire to backend permission endpoint when available.
 *
 * @param policy - Permission policy string (e.g., 'Currencies.Create')
 * @returns { hasPermission: boolean, isLoading: boolean }
 */
export function usePermission(policy?: PolicyString): UsePermissionResult {
  const [isLoading] = useState(false);

  // Stub: always returns true so UI is usable during development.
  // When backend provides real permissions, replace STUB_GRANTED_POLICIES
  // with API response and add loading state.
  const hasPermission = policy ? STUB_GRANTED_POLICIES.has(policy) : true;

  return { hasPermission, isLoading };
}

/**
 * Check permission without hook context (for use in callbacks, renders).
 * Uses the same stub logic as usePermission.
 */
export function hasPermission(policy: PolicyString): boolean {
  return STUB_GRANTED_POLICIES.has(policy);
}
