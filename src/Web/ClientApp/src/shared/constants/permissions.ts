/**
 * Permission policy strings.
 * Source: src/Application/Common/Security/PermissionCodes.cs
 */

export const PERMISSIONS = {
  Currencies: {
    View: 'Currencies.View',
    Create: 'Currencies.Create',
    Update: 'Currencies.Update',
    Activate: 'Currencies.Activate',
    Deactivate: 'Currencies.Deactivate',
  },
  ExchangeRates: {
    View: 'ExchangeRates.View',
    Create: 'ExchangeRates.Create',
    Update: 'ExchangeRates.Update',
    Activate: 'ExchangeRates.Activate',
    Deactivate: 'ExchangeRates.Deactivate',
  },
  FiscalYears: {
    View: 'FiscalYears.View',
    Create: 'FiscalYears.Create',
    Update: 'FiscalYears.Update',
    Open: 'FiscalYears.Open',
    Close: 'FiscalYears.Close',
  },
  FiscalPeriods: {
    View: 'FiscalPeriods.View',
    Create: 'FiscalPeriods.Create',
    Update: 'FiscalPeriods.Update',
    Lock: 'FiscalPeriods.Lock',
    Unlock: 'FiscalPeriods.Unlock',
  },
  DocumentSequences: {
    View: 'DocumentSequences.View',
    Create: 'DocumentSequences.Create',
    Update: 'DocumentSequences.Update',
    Deactivate: 'DocumentSequences.Deactivate',
  },
  ClosingEntries: {
    View: 'ClosingEntries.View',
    Generate: 'ClosingEntries.Generate',
    Approve: 'ClosingEntries.Approve',
    Reverse: 'ClosingEntries.Reverse',
  },
  Accounting: {
    ChartOfAccounts: {
      Read: 'Accounting.ChartOfAccounts.Read',
      Create: 'Accounting.ChartOfAccounts.Create',
      Edit: 'Accounting.ChartOfAccounts.Edit',
    },
    Journals: {
      Read: 'Accounting.Journals.Read',
      Create: 'Accounting.Journals.Create',
      Edit: 'Accounting.Journals.Edit',
    },
    JournalEntries: {
      Read: 'Accounting.JournalEntries.Read',
      Create: 'Accounting.JournalEntries.Create',
      Submit: 'Accounting.JournalEntries.Submit',
      Approve: 'Accounting.JournalEntries.Approve',
      Post: 'Accounting.JournalEntries.Post',
      Reverse: 'Accounting.JournalEntries.Reverse',
      UpdateLines: 'Accounting.JournalEntries.UpdateLines',
      Cancel: 'Accounting.JournalEntries.Cancel',
    },
    Reports: {
      ViewBalanceSheet: 'Accounting.Reports.BalanceSheet',
      ViewIncomeStatement: 'Accounting.Reports.IncomeStatement',
      ViewGeneralLedger: 'Accounting.Reports.GeneralLedger',
      ViewCashFlow: 'Accounting.Reports.CashFlow',
      Export: 'Accounting.Reports.Export',
      Print: 'Accounting.Reports.Print',
    },
  },
  Suppliers: {
    View: 'Suppliers.View',
    Create: 'Suppliers.Create',
    Update: 'Suppliers.Update',
  },
  PurchaseRequests: {
    View: 'PurchaseRequests.View',
    Create: 'PurchaseRequests.Create',
    Submit: 'PurchaseRequests.Submit',
    Approve: 'PurchaseRequests.Approve',
    Reject: 'PurchaseRequests.Reject',
  },
  Quotations: {
    View: 'Quotations.View',
    Create: 'Quotations.Create',
  },
  PurchaseOrders: {
    View: 'PurchaseOrders.View',
    Create: 'PurchaseOrders.Create',
    Submit: 'PurchaseOrders.Submit',
    Approve: 'PurchaseOrders.Approve',
    Issue: 'PurchaseOrders.Issue',
    Cancel: 'PurchaseOrders.Cancel',
    Close: 'PurchaseOrders.Close',
  },
  Vendors: {
    View: 'Suppliers.View',
    Create: 'Suppliers.Create',
    Update: 'Suppliers.Update',
    Activate: 'Suppliers.Activate',
    Deactivate: 'Suppliers.Deactivate',
  },
  Roles: {
    View: 'Roles.View',
    Create: 'Roles.Create',
    Edit: 'Roles.Edit',
  },
} as const;

// ─── Assets ────────────────────────────────────────────────────────

export const ASSET_PERMISSIONS = {
  Assets: {
    View: 'Assets.View',
    Create: 'Assets.Create',
    Update: 'Assets.Update',
  },
  AssetGroups: {
    View: 'AssetGroups.View',
    Create: 'AssetGroups.Create',
    Update: 'AssetGroups.Update',
  },
  AssetMovements: {
    View: 'AssetMovements.View',
    Create: 'AssetMovements.Create',
    Approve: 'AssetMovements.Approve',
  },
  AssetDisposals: {
    View: 'AssetDisposals.View',
    Create: 'AssetDisposals.Create',
    Approve: 'AssetDisposals.Approve',
    Post: 'AssetDisposals.Post',
  },
  AssetRevaluations: {
    View: 'AssetRevaluations.View',
    Create: 'AssetRevaluations.Create',
    Approve: 'AssetRevaluations.Approve',
    Post: 'AssetRevaluations.Post',
  },
  AssetImpairments: {
    View: 'AssetImpairments.View',
    Create: 'AssetImpairments.Create',
    Approve: 'AssetImpairments.Approve',
    Post: 'AssetImpairments.Post',
  },
  Depreciation: {
    View: 'Depreciation.View',
    Post: 'Depreciation.Post',
    Reverse: 'Depreciation.Reverse',
  },
} as const;

// ─── Inventory ─────────────────────────────────────────────────────

export const INVENTORY_PERMISSIONS = {
  Items: {
    View: 'Items.View',
    Create: 'Items.Create',
    Update: 'Items.Update',
  },
  ItemCategories: {
    View: 'ItemCategories.View',
    Create: 'ItemCategories.Create',
    Update: 'ItemCategories.Update',
  },
  Units: {
    View: 'Units.View',
    Create: 'Units.Create',
    Update: 'Units.Update',
  },
  Warehouses: {
    View: 'Warehouses.View',
    Create: 'Warehouses.Create',
    Update: 'Warehouses.Update',
  },
  Locations: {
    View: 'Locations.View',
    Create: 'Locations.Create',
    Update: 'Locations.Update',
  },

  StockTakes: {
    View: 'StockTakes.View',
    Create: 'StockTakes.Create',
    Start: 'StockTakes.Start',
    Complete: 'StockTakes.Complete',
    Approve: 'StockTakes.Approve',
  },
  StockTransactions: {
    View: 'StockTransactions.View',
  },
} as const;

export type AssetPermission =
  | typeof ASSET_PERMISSIONS.Assets[keyof typeof ASSET_PERMISSIONS.Assets]
  | typeof ASSET_PERMISSIONS.AssetGroups[keyof typeof ASSET_PERMISSIONS.AssetGroups]
  | typeof ASSET_PERMISSIONS.AssetMovements[keyof typeof ASSET_PERMISSIONS.AssetMovements]
  | typeof ASSET_PERMISSIONS.AssetDisposals[keyof typeof ASSET_PERMISSIONS.AssetDisposals]
  | typeof ASSET_PERMISSIONS.AssetRevaluations[keyof typeof ASSET_PERMISSIONS.AssetRevaluations]
  | typeof ASSET_PERMISSIONS.AssetImpairments[keyof typeof ASSET_PERMISSIONS.AssetImpairments]
  | typeof ASSET_PERMISSIONS.Depreciation[keyof typeof ASSET_PERMISSIONS.Depreciation];

// ─── Reporting (RPT-01) ─────────────────────────────────────────────

export const REPORTING_PERMISSIONS = {
  ViewBudgetExecution: 'Reporting.ViewBudgetExecution',
  ExportReports: 'Reporting.ExportReports',
} as const;

// ─── Budgeting ─────────────────────────────────────────────────────

export const BUDGET_PERMISSIONS = {
  BudgetTypes: {
    View: 'BudgetTypes.View',
    Create: 'BudgetTypes.Create',
    Update: 'BudgetTypes.Update',
  },
  Funds: {
    View: 'Funds.View',
    Create: 'Funds.Create',
    Update: 'Funds.Update',
    ToggleActive: 'Funds.ToggleActive',
  },
  BudgetClassifications: {
    View: 'BudgetClassifications.View',
    Create: 'BudgetClassifications.Create',
    Update: 'BudgetClassifications.Update',
  },
  Budgets: {
    View: 'Budgets.View',
    Create: 'Budgets.Create',
    Update: 'Budgets.Update',
    Submit: 'Budgets.Submit',
    Approve: 'Budgets.Approve',
    Activate: 'Budgets.Activate',
    Suspend: 'Budgets.Suspend',
    Close: 'Budgets.Close',
    Cancel: 'Budgets.Cancel',
  },
  BudgetItems: {
    View: 'BudgetItems.View',
    Create: 'BudgetItems.Create',
    Update: 'BudgetItems.Update',
    Delete: 'BudgetItems.Delete',
  },
  Appropriations: {
    View: 'Appropriations.View',
    Create: 'Appropriations.Create',
    Update: 'Appropriations.Update',
    Delete: 'Appropriations.Delete',
    Submit: 'Appropriations.Submit',
    Approve: 'Appropriations.Approve',
    Activate: 'Appropriations.Activate',
    Suspend: 'Appropriations.Suspend',
    Close: 'Appropriations.Close',
    Cancel: 'Appropriations.Cancel',
    Reverse: 'Appropriations.Reverse',
  },
  Encumbrances: {
    View: 'Encumbrances.View',
    Create: 'Encumbrances.Create',
    Submit: 'Encumbrances.Submit',
    Approve: 'Encumbrances.Approve',
    Activate: 'Encumbrances.Activate',
    Release: 'Encumbrances.Release',
    Close: 'Encumbrances.Close',
    Cancel: 'Encumbrances.Cancel',
    Reverse: 'Encumbrances.Reverse',
  },
  BudgetTransactions: {
    View: 'BudgetTransactions.View',
    Create: 'BudgetTransactions.Create',
    Update: 'BudgetTransactions.Update',
    Delete: 'BudgetTransactions.Delete',
    Submit: 'BudgetTransactions.Submit',
    Approve: 'BudgetTransactions.Approve',
    Post: 'BudgetTransactions.Post',
    Cancel: 'BudgetTransactions.Cancel',
    Reverse: 'BudgetTransactions.Reverse',
  },
} as const;

// ─── Revenue / Treasury (TRE-01) ───────────────────────────────────

export const RECEIPT_VOUCHER_PERMISSIONS = {
  ReceiptVouchers: {
    View: 'ReceiptVouchers.View',
    Create: 'ReceiptVouchers.Create',
    Update: 'ReceiptVouchers.Update',
    Submit: 'ReceiptVouchers.Submit',
    Approve: 'ReceiptVouchers.Approve',
    Cancel: 'ReceiptVouchers.Cancel',
  },
} as const;

export type TreasuryPermission =
  | typeof RECEIPT_VOUCHER_PERMISSIONS.ReceiptVouchers[keyof typeof RECEIPT_VOUCHER_PERMISSIONS.ReceiptVouchers];

// ─── Procurement (PROC) ─────────────────────────────────────────────

export const PROCUREMENT_PERMISSIONS = {
  PurchaseRequests: {
    View: 'PurchaseRequests.View',
    Create: 'PurchaseRequests.Create',
    Update: 'PurchaseRequests.Update',
    Submit: 'PurchaseRequests.Submit',
    Approve: 'PurchaseRequests.Approve',
    Cancel: 'PurchaseRequests.Cancel',
  },
  GoodsReceipts: {
    View: 'GoodsReceipts.View',
    Create: 'GoodsReceipts.Create',
    Confirm: 'GoodsReceipts.Confirm',
    Reject: 'GoodsReceipts.Reject',
  },
  SupplierInvoices: {
    View: 'SupplierInvoices.View',
    Create: 'SupplierInvoices.Create',
    Submit: 'SupplierInvoices.Submit',
    Match: 'SupplierInvoices.Match',
    Cancel: 'SupplierInvoices.Cancel',
  },
} as const;

export type ProcurementPermission =
  | typeof PROCUREMENT_PERMISSIONS.PurchaseRequests[keyof typeof PROCUREMENT_PERMISSIONS.PurchaseRequests]
  | typeof PROCUREMENT_PERMISSIONS.GoodsReceipts[keyof typeof PROCUREMENT_PERMISSIONS.GoodsReceipts]
  | typeof PROCUREMENT_PERMISSIONS.SupplierInvoices[keyof typeof PROCUREMENT_PERMISSIONS.SupplierInvoices];

export type BudgetPermission =
  | typeof BUDGET_PERMISSIONS.BudgetTypes[keyof typeof BUDGET_PERMISSIONS.BudgetTypes]
  | typeof BUDGET_PERMISSIONS.Funds[keyof typeof BUDGET_PERMISSIONS.Funds]
  | typeof BUDGET_PERMISSIONS.BudgetClassifications[keyof typeof BUDGET_PERMISSIONS.BudgetClassifications]
  | typeof BUDGET_PERMISSIONS.Budgets[keyof typeof BUDGET_PERMISSIONS.Budgets]
  | typeof BUDGET_PERMISSIONS.BudgetItems[keyof typeof BUDGET_PERMISSIONS.BudgetItems]
  | typeof BUDGET_PERMISSIONS.Appropriations[keyof typeof BUDGET_PERMISSIONS.Appropriations]
  | typeof BUDGET_PERMISSIONS.Encumbrances[keyof typeof BUDGET_PERMISSIONS.Encumbrances]
  | typeof BUDGET_PERMISSIONS.BudgetTransactions[keyof typeof BUDGET_PERMISSIONS.BudgetTransactions];

export type InventoryPermission =
  | typeof INVENTORY_PERMISSIONS.Items[keyof typeof INVENTORY_PERMISSIONS.Items]
  | typeof INVENTORY_PERMISSIONS.ItemCategories[keyof typeof INVENTORY_PERMISSIONS.ItemCategories]
  | typeof INVENTORY_PERMISSIONS.Units[keyof typeof INVENTORY_PERMISSIONS.Units]
  | typeof INVENTORY_PERMISSIONS.Warehouses[keyof typeof INVENTORY_PERMISSIONS.Warehouses]
  | typeof INVENTORY_PERMISSIONS.Locations[keyof typeof INVENTORY_PERMISSIONS.Locations]
  | typeof INVENTORY_PERMISSIONS.StockTakes[keyof typeof INVENTORY_PERMISSIONS.StockTakes]
  | typeof INVENTORY_PERMISSIONS.StockTransactions[keyof typeof INVENTORY_PERMISSIONS.StockTransactions];
