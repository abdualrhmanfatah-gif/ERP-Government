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
    ],
  },
  {
    label: 'المالية',
    items: [
      { path: '/financial/currencies', label: 'العملات', permission: 'FinancialSettings.Currencies.Read' },
      { path: '/financial/exchange-rates', label: 'أسعار الصرف', permission: 'FinancialSettings.ExchangeRates.Read' },
      { path: '/financial/fiscal-years', label: 'السنوات المالية', permission: 'FinancialSettings.FiscalYears.Read' },
      { path: '/financial/document-sequences', label: 'تسلسل المستندات', permission: 'FinancialSettings.DocumentSequences.Read' },
    ],
  },
  {
    label: 'الأمان',
    items: [
      { path: '/security/users', label: 'المستخدمون', permission: 'Security.Users.Read' },
      { path: '/security/roles', label: 'الأدوار', permission: 'Security.Roles.Read' },
    ],
  },
  {
    label: 'التنظيم',
    items: [
      { path: '/organization/units', label: 'الوحدات التنظيمية', permission: 'Organization.OrgUnits.Read' },
      { path: '/organization/employees', label: 'الموظفون', permission: 'Organization.Employees.Read' },
      { path: '/organization/cost-centers', label: 'مراكز التكلفة', permission: 'Organization.CostCenters.Read' },
      { path: '/organization/projects', label: 'المشاريع', permission: 'Organization.Projects.Read' },
    ],
  },
  {
    label: 'الموازنة',
    items: [
      { path: '/budgeting/budget-types', label: 'أنواع الموازنات', permission: 'BudgetTypes.View' },
      { path: '/budgeting/funds', label: 'الصناديق', permission: 'Funds.View' },
      { path: '/budgeting/budget-classifications', label: 'التصنيفات المالية', permission: 'BudgetClassifications.View' },
    ],
  },
  {
    label: 'المشتريات',
    items: [
      { path: '/procurement/orders', label: 'أوامر الشراء', permission: 'Procurement.PurchaseOrders.Read' },
    ],
  },
  {
    label: 'التقارير',
    items: [
      { path: '/reports/balance-sheet', label: 'الميزانية العمومية', permission: 'Accounting.Reports.BalanceSheet' },
      { path: '/reports/income-statement', label: 'قائمة الدخل', permission: 'Accounting.Reports.IncomeStatement' },
      { path: '/reports/general-ledger', label: 'دفتر الأستاذ', permission: 'Accounting.Reports.GeneralLedger' },
      { path: '/reports/cash-flow', label: 'قائمة التدفقات النقدية', permission: 'Accounting.Reports.CashFlow' },
      { path: '/reports/trial-balance', label: 'ميزان المراجعة', permission: 'Accounting.Reports.TrialBalance' },
    ],
  },
];
