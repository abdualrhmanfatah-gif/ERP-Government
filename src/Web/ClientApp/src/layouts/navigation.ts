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
    label: 'الرئيسية',
    items: [
      { path: '/', label: 'لوحة التحكم' },
    ],
  },
  {
    label: 'اداره الحسابات',
    items: [
      { path: '/accounting/account-groups', label: 'مجموعات الحسابات', permission: 'Accounting.ChartOfAccounts.Read' },
      { path: '/accounting/accounts', label: 'الحسابات', permission: 'Accounting.ChartOfAccounts.Read' },
      { path: '/accounting/journal-entries', label: 'قيود اليومية', permission: 'Accounting.JournalEntries.Read' },
      { path: '/accounting/journals', label: 'دفاتر اليومية', permission: 'Accounting.Journals.Read' },
      { path: '/accounting/templates', label: 'قوالب القيود', permission: 'Accounting.Templates.Read' },
      { path: '/accounting/recurring-entries', label: 'القيود الدورية', permission: 'Accounting.RecurringEntries.Read' },
      { path: '/accounting/events', label: 'طابور الأحداث', permission: 'Accounting.AccountingEvents.Read' },
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
    label: 'الإعدادات المالية',
    items: [
      { path: '/financial-settings/fiscal-years', label: 'السنوات المالية', permission: 'FiscalYears.View' },
      { path: '/financial-settings/document-sequences', label: 'تسلسل الوثائق', permission: 'DocumentSequences.View' },
      { path: '/financial-settings/currencies', label: 'العملات', permission: 'Currencies.View' },
      { path: '/financial-settings/exchange-rates', label: 'أسعار الصرف', permission: 'ExchangeRates.View' },
      { path: '/financial-settings/closing-entries', label: 'قيود الإغلاق', permission: 'ClosingEntries.View' },
    ],
  },
];
