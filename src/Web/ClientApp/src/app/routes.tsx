import type { ReactNode } from 'react';
import { Home } from '../components/Home';
import { LoginPage } from '../features/auth/LoginPage';
import { RegisterPage } from '../features/auth/RegisterPage';
import { AccountsListPage } from '../features/accounting/pages/AccountsListPage';
import { AccountDetailPage } from '../features/accounting/pages/AccountDetailPage';
import { AccountCreatePage } from '../features/accounting/pages/AccountCreatePage';
import { AccountEditPage } from '../features/accounting/pages/AccountEditPage';
import { JournalEntriesListPage } from '../features/accounting/pages/JournalEntriesListPage';
import { JournalEntryCreatePage } from '../features/accounting/pages/JournalEntryCreatePage';
import { JournalEntryDetailPage } from '../features/accounting/pages/JournalEntryDetailPage';
import { JournalsListPage } from '../features/accounting/journals/pages/JournalsListPage';
import { JournalCreatePage } from '../features/accounting/journals/pages/JournalCreatePage';
import { JournalEditPage } from '../features/accounting/journals/pages/JournalEditPage';
import { TemplatesListPage } from '../features/accounting/templates/pages/TemplatesListPage';
import { TemplateCreatePage } from '../features/accounting/templates/pages/TemplateCreatePage';
import { TemplateEditPage } from '../features/accounting/templates/pages/TemplateEditPage';
import { PostingRulesListPage } from '../features/accounting-monitoring/pages/PostingRulesListPage';

import { RolesListPage } from '../features/security/rbac/pages/RolesListPage';
import { RoleCreatePage } from '../features/security/rbac/pages/RoleCreatePage';
import { RoleEditPage } from '../features/security/rbac/pages/RoleEditPage';
import { RoleDetailPage } from '../features/security/rbac/pages/RoleDetailPage';
import { UserListPage } from '../features/security/users/pages/UserListPage';
import { UserDetailPage } from '../features/security/users/pages/UserDetailPage';
import { AuthLayout } from '../layouts/AuthLayout';
import { OrgUnitsListPage } from '../features/organization/pages/OrgUnitsListPage';
import { OrgUnitCreatePage } from '../features/organization/pages/OrgUnitCreatePage';
import { OrgUnitEditPage } from '../features/organization/pages/OrgUnitCreatePage';
import { EmployeesListPage } from '../features/organization/pages/EmployeesListPage';
import { EmployeeCreatePage } from '../features/organization/pages/EmployeeCreatePage';
import { EmployeeEditPage } from '../features/organization/pages/EmployeeCreatePage';
import { CostCentersListPage } from '../features/organization/pages/CostCentersListPage';
import { ProjectsListPage } from '../features/organization/pages/ProjectsListPage';
import { ProjectCreatePage } from '../features/organization/pages/ProjectCreatePage';
import { ProjectEditPage } from '../features/organization/pages/ProjectCreatePage';

import { AccountGroupsListPage } from '../features/accounting/account-groups/pages/AccountGroupsListPage';
import { AccountGroupDetailPage } from '../features/accounting/account-groups/pages/AccountGroupDetailPage';
import FiscalYearsListPage from '../features/financial-settings/fiscal-years/pages/FiscalYearsListPage';
import FiscalYearDetailPage from '../features/financial-settings/fiscal-years/pages/FiscalYearDetailPage';
import FiscalYearCreatePage from '../features/financial-settings/fiscal-years/pages/FiscalYearCreatePage';
import DocumentSequencesListPage from '../features/financial-settings/document-sequences/pages/DocumentSequencesListPage';
import CurrenciesListPage from '../features/financial-settings/currencies/pages/CurrenciesListPage';
import CurrencyCreatePage from '../features/financial-settings/currencies/pages/CurrencyCreatePage';
import CurrencyDetailPage from '../features/financial-settings/currencies/pages/CurrencyDetailPage';
import ExchangeRatesListPage from '../features/financial-settings/exchange-rates/pages/ExchangeRatesListPage';
import ExchangeRateCreatePage from '../features/financial-settings/exchange-rates/pages/ExchangeRateCreatePage';
import ClosingEntriesListPage from '../features/financial-settings/closing-entries/pages/ClosingEntriesListPage';
import ClosingEntryDetailPage from '../features/financial-settings/closing-entries/pages/ClosingEntryDetailPage';

import BudgetTypesListPage from '../features/budgeting/budget-types/pages/BudgetTypesListPage';
import FundsListPage from '../features/budgeting/funds/pages/FundsListPage';
import FundDetailPage from '../features/budgeting/funds/pages/FundDetailPage';
import ClassificationsListPage from '../features/budgeting/classifications/pages/ClassificationsListPage';
import PartiesListPage from '../features/parties/pages/PartiesListPage';
import PartyCreatePage from '../features/parties/pages/PartyCreatePage';
import PartyDetailPage from '../features/parties/pages/PartyDetailPage';
import BudgetsListPage from '../features/budgeting/budgets/pages/BudgetsListPage';
import { ReceiptVouchersListPage, CreateReceiptVoucherPage, ReceiptVoucherDetailPage, DepositSlipsListPage, CreateDepositSlipPage, DepositSlipDetailPage, MonthlyStatementPage } from '../features/treasury';
import ChecksListPage from '../features/treasury/checks/pages/ChecksListPage';import BudgetDetailPage from '../features/budgeting/budgets/pages/BudgetDetailPage';
import BudgetCreatePage from '../features/budgeting/budgets/pages/BudgetCreatePage';
import AppropriationsListPage from '../features/budgeting/appropriations/pages/AppropriationsListPage';
import AppropriationCreatePage from '../features/budgeting/appropriations/pages/AppropriationCreatePage';
import AppropriationDetailPage from '../features/budgeting/appropriations/pages/AppropriationDetailPage';
import EncumbrancesListPage from '../features/budgeting/encumbrances/pages/EncumbrancesListPage';
import EncumbranceCreatePage from '../features/budgeting/encumbrances/pages/EncumbranceCreatePage';
import { RecurringEntriesListPage } from '../features/accounting/recurring-entries';
import { RecurringEntryDetailPage } from '../features/accounting/recurring-entries';
import RecurringEntryCreatePage from '../features/accounting/recurring-entries/pages/RecurringEntryCreatePage';

export interface RouteConfig {
  path: string;
  element: ReactNode;
  layout?: React.FC<{ children: ReactNode }>;
  label?: string;
  protected?: boolean;
}

export const AppRoutes: RouteConfig[] = [
  {
    path: '/',
    element: <Home />,
    label: 'الرئيسية',
    protected: true,
  },
  {
    path: '/login',
    element: <LoginPage />,
    layout: AuthLayout,
    label: 'تسجيل الدخول',
  },
  {
    path: '/register',
    element: <RegisterPage />,
    layout: AuthLayout,
    label: 'إنشاء حساب',
  },
  {
    path: '/accounting/accounts',
    element: <AccountsListPage />,
    label: 'الحسابات',
    protected: true,
  },
  {
    path: '/accounting/accounts/create',
    element: <AccountCreatePage />,
    label: 'حساب جديد',
    protected: true,
  },
  {
    path: '/accounting/accounts/:id/edit',
    element: <AccountEditPage />,
    label: 'تعديل الحساب',
    protected: true,
  },
  {
    path: '/accounting/accounts/:id',
    element: <AccountDetailPage />,
    label: 'تفاصيل الحساب',
    protected: true,
  },
  // Accounting — Journal Entries
  {
    path: '/accounting/journal-entries',
    element: <JournalEntriesListPage />,
    label: 'قيود اليومية',
    protected: true,
  },
  {
    path: '/accounting/journal-entries/create',
    element: <JournalEntryCreatePage />,
    label: 'قيد جديد',
    protected: true,
  },
  {
    path: '/accounting/journal-entries/:id',
    element: <JournalEntryDetailPage />,
    label: 'تفاصيل القيد',
    protected: true,
  },
  // Accounting — Journals (ACC-03)
  {
    path: '/accounting/journals',
    element: <JournalsListPage />,
    label: 'دفاتر اليومية',
    protected: true,
  },
  {
    path: '/accounting/journals/create',
    element: <JournalCreatePage />,
    label: 'دفتر جديد',
    protected: true,
  },
  {
    path: '/accounting/journals/:id',
    element: <JournalEditPage />,
    label: 'تعديل الدفتر',
    protected: true,
  },
  // Accounting — Templates (ACC-03)
  {
    path: '/accounting/templates',
    element: <TemplatesListPage />,
    label: 'قوالب القيود',
    protected: true,
  },
  {
    path: '/accounting/templates/create',
    element: <TemplateCreatePage />,
    label: 'قالب جديد',
    protected: true,
  },
  {
    path: '/accounting/templates/:id',
    element: <TemplateEditPage />,
    label: 'تعديل القالب',
    protected: true,
  },
  // Accounting — Recurring Entries (ACC-04)
  {
    path: '/accounting/recurring-entries',
    element: <RecurringEntriesListPage />,
    label: 'القيود الدورية',
    protected: true,
  },
  {
    path: '/accounting/recurring-entries/new',
    element: <RecurringEntryCreatePage />,
    label: 'جدول دوري جديد',
    protected: true,
  },
  {
    path: '/accounting/recurring-entries/:id',
    element: <RecurringEntryDetailPage />,
    label: 'تفاصيل الجدول الدوري',
    protected: true,
  },
  // Accounting — Monitoring (ACC-05)
  {
    path: '/accounting/posting-rules',
    element: <PostingRulesListPage />,
    label: 'قواعد الترحيل',
    protected: true,
  },
  {
    path: '/security/roles',
    element: <RolesListPage />,
    label: 'الأدوار',
    protected: true,
  },
  {
    path: '/security/roles/create',
    element: <RoleCreatePage />,
    label: 'دور جديد',
    protected: true,
  },
  {
    path: '/security/roles/:id',
    element: <RoleDetailPage />,
    label: 'تفاصيل الدور',
    protected: true,
  },
  {
    path: '/security/roles/:id/edit',
    element: <RoleEditPage />,
    label: 'تعديل الدور',
    protected: true,
  },
  {
    path: '/security/users',
    element: <UserListPage />,
    label: 'المستخدمون',
    protected: true,
  },
  {
    path: '/security/users/:id',
    element: <UserDetailPage />,
    label: 'تفاصيل المستخدم',
    protected: true,
  },
  // Organization — US5: Org Units
  {
    path: '/organization/units',
    element: <OrgUnitsListPage />,
    label: 'الوحدات التنظيمية',
    protected: true,
  },
  {
    path: '/organization/units/create',
    element: <OrgUnitCreatePage />,
    label: 'وحدة جديدة',
    protected: true,
  },
  {
    path: '/organization/units/:id/edit',
    element: <OrgUnitEditPage />,
    label: 'تعديل الوحدة',
    protected: true,
  },
  // Organization — US6: Employees
  {
    path: '/organization/employees',
    element: <EmployeesListPage />,
    label: 'الموظفون',
    protected: true,
  },
  {
    path: '/organization/employees/create',
    element: <EmployeeCreatePage />,
    label: 'موظف جديد',
    protected: true,
  },
  {
    path: '/organization/employees/:id/edit',
    element: <EmployeeEditPage />,
    label: 'تعديل الموظف',
    protected: true,
  },
  // Organization — US7: Cost Centers
  {
    path: '/organization/cost-centers',
    element: <CostCentersListPage />,
    label: 'مراكز التكلفة',
    protected: true,
  },
  // Organization — US8: Projects
  {
    path: '/organization/projects',
    element: <ProjectsListPage />,
    label: 'المشاريع',
    protected: true,
  },
  {
    path: '/organization/projects/create',
    element: <ProjectCreatePage />,
    label: 'مشروع جديد',
    protected: true,
  },
  {
    path: '/organization/projects/:id/edit',
    element: <ProjectEditPage />,
    label: 'تعديل المشروع',
    protected: true,
  },
  // Procurement — Purchase Orders
 

  // Assets — Asset Register
  // Account Groups (US1-US5)
  {
    path: '/accounting/account-groups',
    element: <AccountGroupsListPage />,
    label: 'مجموعات الحسابات',
    protected: true,
  },
  {
    path: '/accounting/account-groups/:id',
    element: <AccountGroupDetailPage />,
    label: 'تفاصيل المجموعة',
    protected: true,
  },
  // Budgeting —_budget-types & Funds (Foundation spec #011)
  {
    path: '/budgeting/budget-types',
    element: <BudgetTypesListPage />,
    label: 'أنواع الموازنات',
    protected: true,
  },
  {
    path: '/budgeting/funds',
    element: <FundsListPage />,
    label: 'الصناديق',
    protected: true,
  },
  {
    path: '/budgeting/funds/:id',
    element: <FundDetailPage />,
    label: 'تفاصيل الصندوق',
    protected: true,
  },
  {
    path: '/budgeting/budget-classifications',
    element: <ClassificationsListPage />,
    label: 'التصنيفات المالية',
    protected: true,
  },
  // Budgeting — Budgets
  {
    path: '/budgeting/budgets',
    element: <BudgetsListPage />,
    label: 'الموازنات',
    protected: true,
  },
  {
    path: '/budgeting/budgets/create',
    element: <BudgetCreatePage />,
    label: 'موازنة جديدة',
    protected: true,
  },
  {
    path: '/budgeting/budgets/:id',
    element: <BudgetDetailPage />,
    label: 'تفاصيل الموازنة',
    protected: true,
  },
  // Budgeting — Appropriations
  {
    path: '/budgeting/appropriations',
    element: <AppropriationsListPage />,
    label: 'التخصيصات',
    protected: true,
  },
  {
    path: '/budgeting/appropriations/create',
    element: <AppropriationCreatePage />,
    label: 'تخصيص جديد',
    protected: true,
  },
  {
    path: '/budgeting/appropriations/:id',
    element: <AppropriationDetailPage />,
    label: 'تفاصيل التخصيص',
    protected: true,
  },
  // Budgeting — Encumbrances
  {
    path: '/budgeting/encumbrances',
    element: <EncumbrancesListPage />,
    label: 'الالتزامات',
    protected: true,
  },
  {
    path: '/budgeting/encumbrances/create',
    element: <EncumbranceCreatePage />,
    label: 'التزام جديد',
    protected: true,
  },
  // Parties
  {
    path: '/parties',
    element: <PartiesListPage />,
    label: 'الأطراف',
    protected: true,
  },
  {
    path: '/parties/create',
    element: <PartyCreatePage />,
    label: 'طرف جديد',
    protected: true,
  },
  {
    path: '/parties/:id',
    element: <PartyDetailPage />,
    label: 'تفاصيل الطرف',
    protected: true,
  },
  // Financial Settings
  {
    path: '/financial-settings/fiscal-years',
    element: <FiscalYearsListPage />,
    label: 'السنوات المالية',
    protected: true,
  },
  {
    path: '/financial-settings/fiscal-years/create',
    element: <FiscalYearCreatePage />,
    label: 'سنة مالية جديدة',
    protected: true,
  },
  {
    path: '/financial-settings/fiscal-years/:id',
    element: <FiscalYearDetailPage />,
    label: 'تفاصيل السنة المالية',
    protected: true,
  },
  {
    path: '/financial-settings/document-sequences',
    element: <DocumentSequencesListPage />,
    label: 'تسلسل الوثائق',
    protected: true,
  },
  {
    path: '/financial-settings/currencies',
    element: <CurrenciesListPage />,
    label: 'العملات',
    protected: true,
  },
  {
    path: '/financial-settings/currencies/new',
    element: <CurrencyCreatePage />,
    label: 'عملة جديدة',
    protected: true,
  },
  {
    path: '/financial-settings/currencies/:id',
    element: <CurrencyDetailPage />,
    label: 'تفاصيل العملة',
    protected: true,
  },
  {
    path: '/financial-settings/exchange-rates',
    element: <ExchangeRatesListPage />,
    label: 'أسعار الصرف',
    protected: true,
  },
  {
    path: '/financial-settings/exchange-rates/new',
    element: <ExchangeRateCreatePage />,
    label: 'سعر صرف جديد',
    protected: true,
  },
  {
    path: '/financial-settings/closing-entries',
    element: <ClosingEntriesListPage />,
    label: 'قيود الإغلاق',
    protected: true,
  },
  {
    path: '/financial-settings/closing-entries/:id',
    element: <ClosingEntryDetailPage />,
    label: 'تفاصيل قيد الإغلاق',
    protected: true,
  },
  // Treasury — Receipt Vouchers (TRE-01)
  {
    path: '/treasury/receipt-vouchers',
    element: <ReceiptVouchersListPage />,
    label: 'سندات القبض',
    protected: true,
  },
  {
    path: '/treasury/receipt-vouchers/create',
    element: <CreateReceiptVoucherPage />,
    label: 'سند قبض جديد',
    protected: true,
  },
  {
    path: '/treasury/receipt-vouchers/:id',
    element: <ReceiptVoucherDetailPage />,
    label: 'تفاصيل سند القبض',
    protected: true,
  },
  // Treasury — Deposit Slips (TRE-02)
  {
    path: '/treasury/deposit-slips',
    element: <DepositSlipsListPage />,
    label: 'بطاقات الإيداع',
    protected: true,
  },
  {
    path: '/treasury/deposit-slips/create',
    element: <CreateDepositSlipPage />,
    label: 'إنشاء بطاقة إيداع',
    protected: true,
  },
  {
    path: '/treasury/deposit-slips/:id',
    element: <DepositSlipDetailPage />,
    label: 'تفاصيل بطاقة الإيداع',
    protected: true,
  },
  {
    path: '/treasury/monthly-statement',
    element: <MonthlyStatementPage />,
    label: 'كشف حساب شهري',
    protected: true,
  },
  // Treasury — Checks (TRE-03)
  {
    path: '/treasury/checks',
    element: <ChecksListPage />,
    label: 'الشيكات',
    protected: true,
  },
];
