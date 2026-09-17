export interface NavItem {
  path: string;
  label: string;
  permission?: string;
}

export interface ModuleGroup {
  label: string;
  items: NavItem[];
}

export const moduleGroups: ModuleGroup[] = [
  {
    label: 'اداره الحسابات',
    items: [
      { path: '/accounting/account-groups', label: 'مجموعات الحسابات', permission: 'Accounting.ChartOfAccounts.Read' },
      { path: '/accounting/accounts', label: 'الحسابات', permission: 'Accounting.ChartOfAccounts.Read' },
      { path: '/accounting/journal-entries', label: 'قيود اليومية', permission: 'Accounting.JournalEntries.Read' },
      { path: '/accounting/journals', label: 'دفاتر اليومية', permission: 'Accounting.Journals.Read' },
      { path: '/accounting/templates', label: 'قوالب القيود', permission: 'Accounting.Templates.Read' },
      { path: '/accounting/recurring-entries', label: 'القيود الدورية', permission: 'Accounting.RecurringEntries.Read' },
    ],
  },

  {
    label: 'الموازنة',
    items: [
      { path: '/budgeting/budget-types', label: 'أنواع الموازنات', permission: 'BudgetTypes.View' },
      { path: '/budgeting/funds', label: 'الصناديق', permission: 'Funds.View' },
      { path: '/budgeting/budget-classifications', label: 'التصنيفات المالية', permission: 'BudgetClassifications.View' },
      { path: '/budgeting/budgets', label: 'الموازنات', permission: 'Budgets.View' },


      { path: '/budgeting/encumbrances', label: 'الالتزامات', permission: 'Encumbrances.View' },
    ],
  },
  {
    label: 'الخزينة',
    items: [
      { path: '/treasury/revenue-claims', label: 'المطالبات الإيرادية', permission: 'RevenueClaims.View' },
      { path: '/treasury/collection-orders', label: 'أوامر التحصيل', permission: 'CollectionOrders.View' },
      { path: '/treasury/receipt-vouchers', label: 'سندات القبض', permission: 'ReceiptVouchers.View' },
      { path: '/treasury/deposit-slips', label: 'بطاقات الإيداع', permission: 'DepositSlips.View' },
      { path: '/treasury/checks', label: 'الشيكات', permission: 'Checks.View' },
      { path: '/treasury/monthly-statement', label: 'كشف حساب شهري', permission: 'Statements.View' },
    ],
  },
  {
    label: 'التقارير',
    items: [
      { path: '/reporting/budget-execution', label: 'تقرير تنفيذ الموازنة', permission: 'Reporting.ViewBudgetExecution' },
      { path: '/reporting/revenue-collections', label: 'سجل التحصيلات', permission: 'Reporting.ViewRevenueCollections' },
      { path: '/reporting/disbursement-register', label: 'سجل الصرف', permission: 'Reporting.ViewDisbursementRegister' },
      { path: '/reporting/trial-balance', label: 'ميزان المراجعة', permission: 'Reporting.ViewTrialBalance' },
      { path: '/reporting/financial-statements/balance-sheet', label: 'الميزانية العمومية', permission: 'Accounting.Reports.BalanceSheet' },
      { path: '/reporting/financial-statements/income-statement', label: 'قائمة الدخل', permission: 'Accounting.Reports.IncomeStatement' },
      { path: '/reporting/financial-statements/cash-flow', label: 'قائمة التدفقات النقدية', permission: 'Accounting.Reports.CashFlow' },
      { path: '/reporting/financial-statements/general-ledger', label: 'دفتر الأستاذ العام', permission: 'Accounting.Reports.GeneralLedger' },
    ],
  },
  {
    label: 'الإعدادات المالية',
    items: [
      { path: '/financial-settings/fiscal-years', label: 'السنوات المالية', permission: 'FiscalYears.View' },
      { path: '/financial-settings/document-sequences', label: 'تسلسل الوثائق', permission: 'DocumentSequences.View' },
      { path: '/financial-settings/currencies', label: 'العملات', permission: 'Currencies.View' },
      { path: '/financial-settings/exchange-rates', label: 'أسعار الصرف', permission: 'ExchangeRates.View' },
      { path: '/financial-settings/closing-entries', label: 'قيود الإغلاق', permission: 'ClosingEntries.View' },
      { path: '/security/users', label: 'المستخدمون', permission: 'Security.Users.Read' },
      { path: '/security/roles', label: 'الأدوار', permission: 'Security.Roles.Read' },
      { path: '/organization/units', label: 'الوحدات التنظيمية', permission: 'Organization.OrgUnits.Read' },
      { path: '/organization/employees', label: 'الموظفون', permission: 'Organization.Employees.Read' },
      { path: '/organization/cost-centers', label: 'مراكز التكلفة', permission: 'Organization.CostCenters.Read' },
      { path: '/organization/projects', label: 'المشاريع', permission: 'Organization.Projects.Read' },
    ],
  },
  {
    label: 'المدفوعات',
    items: [
      { path: '/payments/payment-orders', label: 'أوامر الدفع', permission: 'PaymentOrders.View' },
      { path: '/payments/disbursement-requests', label: 'طلبات الصرف', permission: 'DisbursementRequests.View' },
      { path: '/payments/payments', label: 'المدفوعات', permission: 'Payments.View' },
      { path: '/payments/bank-accounts', label: 'الحسابات البنكية', permission: 'BankAccounts.View' },
    ],
  },
  {
    label: 'المشتريات',
    items: [
      { path: '/procurement/dashboard', label: 'لوحة المشتريات', permission: 'PurchaseRequests.View' },
      { path: '/parties', label: 'الموردون', permission: 'Parties.View' },
      { path: '/procurement/purchase-requests', label: 'طلبات الشراء', permission: 'PurchaseRequests.View' },
      { path: '/procurement/quotations', label: 'عروض الأسعار', permission: 'Quotations.View' },
      { path: '/procurement/purchase-orders', label: 'أوامر الشراء', permission: 'PurchaseOrdersCreate' },
      { path: '/procurement/goods-receipt-notes', label: 'سندات استلام البضاعة', permission: 'GoodsReceiptsCreate' },
      { path: '/procurement/supplier-invoices', label: 'فواتير الموردين', permission: 'SupplierInvoicesCreate' },
    ],
  },
  {
    label: 'المخزون',
    items: [
      { path: '/inventory/items', label: 'الأصناف', permission: 'Items.View' },
      { path: '/inventory/item-categories', label: 'تصنيفات الأصناف', permission: 'ItemCategories.View' },
      { path: '/inventory/units', label: 'الوحدات', permission: 'Units.View' },
      { path: '/inventory/warehouses', label: 'المستودعات', permission: 'Warehouses.View' },
      { path: '/inventory/locations', label: 'المواقع', permission: 'Locations.View' },
    ],
  },
  {
    label: 'الأصول',
    items: [
      { path: '/assets', label: 'سجل الأصول', permission: 'Assets.View' },
      { path: '/assets/asset-groups', label: 'مجموعات الأصول', permission: 'AssetGroups.View' },
      { path: '/assets/attributes', label: 'تعريفات المواصفات', permission: 'AssetGroups.View' },
      { path: '/assets/transfers', label: 'نقل الأصول', permission: 'AssetTransfers.View' },
      { path: '/assets/depreciation', label: 'الإهلاك', permission: 'AssetDepreciation.View' },
      { path: '/assets/disposals', label: 'التخلص', permission: 'AssetDisposals.View' },
      { path: '/assets/revaluations', label: 'التقييمات', permission: 'AssetRevaluations.View' },
      { path: '/assets/impairments', label: 'الإنخفاض', permission: 'AssetImpairments.View' },
      { path: '/assets/counts', label: 'الجرد', permission: 'AssetCounts.View' },
    ],
  },
];
