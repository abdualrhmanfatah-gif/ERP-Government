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
      { path: '/accounting/posting-rules', label: 'قواعد الترحيل', permission: 'Accounting.PostingRules.Read' },
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
    label: 'الأطراف',
    items: [
      { path: '/parties', label: 'الأطراف', permission: 'Parties.View' },
    ],
  },
  {
    label: 'الموازنة',
    items: [
      { path: '/budgeting/budget-types', label: 'أنواع الموازنات', permission: 'BudgetTypes.View' },
      { path: '/budgeting/funds', label: 'الصناديق', permission: 'Funds.View' },
      { path: '/budgeting/budget-classifications', label: 'التصنيفات المالية', permission: 'BudgetClassifications.View' },
      { path: '/budgeting/budgets', label: 'الموازنات', permission: 'Budgets.View' },
      { path: '/budgeting/appropriations', label: 'التخصيصات', permission: 'Appropriations.View' },
      { path: '/budgeting/encumbrances', label: 'الالتزامات', permission: 'Encumbrances.View' },
    ],
  },
  {
    label: 'الخزينة',
    items: [
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
      { path: '/reporting/availability-snapshot', label: 'لقطة التوفر', permission: 'Reporting.ViewAvailabilitySnapshot' },
      { path: '/reporting/trial-balance', label: 'ميزان المراجعة', permission: 'Reporting.ViewTrialBalance' },
      { path: '/reporting/financial-statements/balance-sheet', label: 'الميزانية العمومية', permission: 'Reporting.ViewFinancialStatements' },
      { path: '/reporting/financial-statements/income-statement', label: 'قائمة الدخل', permission: 'Reporting.ViewFinancialStatements' },
      { path: '/reporting/financial-statements/cash-flow', label: 'قائمة التدفقات النقدية', permission: 'Reporting.ViewFinancialStatements' },
      { path: '/reporting/financial-statements/general-ledger', label: 'دفتر الأستاذ العام', permission: 'Reporting.ViewFinancialStatements' },
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
];
