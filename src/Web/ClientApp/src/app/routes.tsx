import type { ReactNode } from 'react';
import { Home } from '../components/Home';
import { LoginPage } from '../features/auth/LoginPage';
import { RegisterPage } from '../features/auth/RegisterPage';
import { AccountsListPage } from '../features/accounting/pages/AccountsListPage';
import { AccountDetailPage } from '../features/accounting/pages/AccountDetailPage';
import { AccountCreatePage } from '../features/accounting/pages/AccountCreatePage';
import { AccountEditPage } from '../features/accounting/pages/AccountEditPage';
import { MovesListPage } from '../features/accounting/pages/MovesListPage';
import { MoveCreatePage } from '../features/accounting/pages/MoveCreatePage';
import { MoveDetailPage } from '../features/accounting/pages/MoveDetailPage';
import { CurrenciesListPage } from '../features/financial/pages/CurrenciesListPage';
import { ExchangeRatesListPage } from '../features/financial/exchange-rates/pages/ExchangeRatesListPage';
import { FiscalYearsListPage } from '../features/financial/fiscal-years/pages/FiscalYearsListPage';
import { FiscalYearDetailPage } from '../features/financial/fiscal-years/pages/FiscalYearDetailPage';
import { DocumentSequencesListPage } from '../features/financial/document-sequences/pages/DocumentSequencesListPage';
import { DocumentSequenceCreatePage } from '../features/financial/document-sequences/pages/DocumentSequenceCreatePage';
import { DocumentSequenceEditPage } from '../features/financial/document-sequences/pages/DocumentSequenceEditPage';
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
import { BalanceSheetPage } from '../features/reports/pages/BalanceSheetPage';
import BudgetTypesListPage from '../features/budgeting/budget-types/pages/BudgetTypesListPage';
import FundsListPage from '../features/budgeting/funds/pages/FundsListPage';
import FundDetailPage from '../features/budgeting/funds/pages/FundDetailPage';
import ClassificationsListPage from '../features/budgeting/classifications/pages/ClassificationsListPage';
import { IncomeStatementPage } from '../features/reports/pages/IncomeStatementPage';
import { GeneralLedgerPage } from '../features/reports/pages/GeneralLedgerPage';
import { CashFlowStatementPage } from '../features/reports/pages/CashFlowStatementPage';
import { TrialBalancePage } from '../features/reports/pages/TrialBalancePage';

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
  {
    path: '/accounting/journal-entries',
    element: <MovesListPage />,
    label: 'قيود اليومية',
    protected: true,
  },
  {
    path: '/accounting/journal-entries/create',
    element: <MoveCreatePage />,
    label: 'قيد جديد',
    protected: true,
  },
  {
    path: '/accounting/journal-entries/:id',
    element: <MoveDetailPage />,
    label: 'تفاصيل القيد',
    protected: true,
  },
  {
    path: '/financial/currencies',
    element: <CurrenciesListPage />,
    label: 'العملات',
    protected: true,
  },
  {
    path: '/financial/exchange-rates',
    element: <ExchangeRatesListPage />,
    label: 'أسعار الصرف',
    protected: true,
  },
  {
    path: '/financial/fiscal-years',
    element: <FiscalYearsListPage />,
    label: 'السنوات المالية',
    protected: true,
  },
  {
    path: '/financial/fiscal-years/:id',
    element: <FiscalYearDetailPage />,
    label: 'تفاصيل السنة المالية',
    protected: true,
  },
  {
    path: '/financial/document-sequences',
    element: <DocumentSequencesListPage />,
    label: 'تسلسل المستندات',
    protected: true,
  },
  {
    path: '/financial/document-sequences/create',
    element: <DocumentSequenceCreatePage />,
    label: 'تسلسل جديد',
    protected: true,
  },
  {
    path: '/financial/document-sequences/:id/edit',
    element: <DocumentSequenceEditPage />,
    label: 'تعديل التسلسل',
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
  // Reports
  {
    path: '/reports/balance-sheet',
    element: <BalanceSheetPage />,
    label: 'الميزانية العمومية',
    protected: true,
  },
  {
    path: '/reports/income-statement',
    element: <IncomeStatementPage />,
    label: 'قائمة الدخل',
    protected: true,
  },
  {
    path: '/reports/general-ledger',
    element: <GeneralLedgerPage />,
    label: 'دفتر الأستاذ',
    protected: true,
  },
  {
    path: '/reports/cash-flow',
    element: <CashFlowStatementPage />,
    label: 'قائمة التدفقات النقدية',
    protected: true,
  },
  {
    path: '/reports/trial-balance',
    element: <TrialBalancePage />,
    label: 'ميزان المراجعة',
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
];
