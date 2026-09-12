import type { ReactNode } from 'react';
import { DashboardPage } from '../features/dashboard/pages/DashboardPage';
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

import ItemsListPage from '../features/inventory/items/pages/ItemsListPage';
import ItemCreatePage from '../features/inventory/items/pages/ItemCreatePage';
import ItemDetailPage from '../features/inventory/items/pages/ItemDetailPage';
import ItemEditPage from '../features/inventory/items/pages/ItemEditPage';
import ItemCategoriesListPage from '../features/inventory/item-categories/pages/ItemCategoriesListPage';
import UnitsListPage from '../features/inventory/units/pages/UnitsListPage';
import WarehousesListPage from '../features/inventory/warehouses/pages/WarehousesListPage';

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

import EncumbrancesListPage from '../features/budgeting/encumbrances/pages/EncumbrancesListPage';
import EncumbranceCreatePage from '../features/budgeting/encumbrances/pages/EncumbranceCreatePage';
import { RecurringEntriesListPage } from '../features/accounting/recurring-entries';
import { RecurringEntryDetailPage } from '../features/accounting/recurring-entries';
import RecurringEntryCreatePage from '../features/accounting/recurring-entries/pages/RecurringEntryCreatePage';
import { PaymentOrdersListPage } from '../features/payments/payment-orders/pages/PaymentOrdersListPage';
import { PaymentOrderCreatePage } from '../features/payments/payment-orders/pages/PaymentOrderCreatePage';
import PaymentOrderDetailPage from '../features/payments/payment-orders/pages/PaymentOrderDetailPage';
import { DisbursementRequestsListPage } from '../features/payments/disbursement-requests/pages/DisbursementRequestsListPage';
import { DisbursementRequestCreatePage } from '../features/payments/disbursement-requests/pages/DisbursementRequestCreatePage';
import DisbursementRequestDetailPage from '../features/payments/disbursement-requests/pages/DisbursementRequestDetailPage';
import { PaymentsListPage } from '../features/payments/payments/pages/PaymentsListPage';
import PaymentSuccessPage from '../features/payments/payments/pages/PaymentSuccessPage';
import { BankAccountsListPage } from '../features/payments/bank-accounts/pages/BankAccountsListPage';
import BankAccountDetailPage from '../features/payments/bank-accounts/pages/BankAccountDetailPage';
import BudgetExecutionReportPage from '../features/reporting/budget-execution-report/pages/BudgetExecutionReportPage';
import RevenueCollectionsReportPage from '../features/reporting/revenue-collections-report/pages/RevenueCollectionsReportPage';
import DisbursementRegisterReportPage from '../features/reporting/disbursement-register-report/pages/DisbursementRegisterReportPage';
import AvailabilitySnapshotReportPage from '../features/reporting/availability-snapshot-report/pages/AvailabilitySnapshotReportPage';
import TrialBalanceReportPage from '../features/reporting/trial-balance-report/pages/TrialBalanceReportPage';
import BalanceSheetReportPage from '../features/reporting/financial-statements/pages/BalanceSheetReportPage';
import IncomeStatementReportPage from '../features/reporting/financial-statements/pages/IncomeStatementReportPage';
import CashFlowStatementReportPage from '../features/reporting/financial-statements/pages/CashFlowStatementReportPage';
import GeneralLedgerReportPage from '../features/reporting/financial-statements/pages/GeneralLedgerReportPage';

import ProcurementDashboardPage from '../features/procurement/pages/ProcurementDashboardPage';
import PurchaseRequestsListPage from '../features/procurement/purchase-requests/pages/PurchaseRequestsListPage';
import PurchaseRequestDetailPage from '../features/procurement/purchase-requests/pages/PurchaseRequestDetailPage';
import PurchaseRequestCreatePage from '../features/procurement/purchase-requests/pages/PurchaseRequestCreatePage';
import PurchaseRequestEditPage from '../features/procurement/purchase-requests/pages/PurchaseRequestEditPage';
import QuotationsListPage from '../features/procurement/quotations/pages/QuotationsListPage';
import QuotationCreatePage from '../features/procurement/quotations/pages/QuotationCreatePage';
import QuotationDetailPage from '../features/procurement/quotations/pages/QuotationDetailPage';
import QuotationEditPage from '../features/procurement/quotations/pages/QuotationEditPage';
import PurchaseOrdersListPage from '../features/procurement/purchase-orders/pages/PurchaseOrdersListPage';
import PurchaseOrderCreatePage from '../features/procurement/purchase-orders/pages/PurchaseOrderCreatePage';
import PurchaseOrderDetailPage from '../features/procurement/purchase-orders/pages/PurchaseOrderDetailPage';
import PurchaseOrderEditPage from '../features/procurement/purchase-orders/pages/PurchaseOrderEditPage';
import GRNsListPage from '../features/procurement/goods-receipt-notes/pages/GRNsListPage';
import GRNCreatePage from '../features/procurement/goods-receipt-notes/pages/GRNCreatePage';
import GRNDetailPage from '../features/procurement/goods-receipt-notes/pages/GRNDetailPage';
import SupplierInvoicesListPage from '../features/procurement/supplier-invoices/pages/SupplierInvoicesListPage';
import SupplierInvoiceCreatePage from '../features/procurement/supplier-invoices/pages/SupplierInvoiceCreatePage';
import SupplierInvoiceDetailPage from '../features/procurement/supplier-invoices/pages/SupplierInvoiceDetailPage';

export interface RouteConfig {
  path: string;
  element: ReactNode;
  layout?: React.FC<{ children: ReactNode }>;
  label?: string;
  protected?: boolean;
  requiredPermission?: string;
}

export const AppRoutes: RouteConfig[] = [
  {
    path: '/',
    element: <DashboardPage />,
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
  // Procurement
  {
    path: '/procurement/dashboard',
    element: <ProcurementDashboardPage />,
    label: 'لوحة المشتريات',
    protected: true,
  },
  {
    path: '/procurement/purchase-requests',
    element: <PurchaseRequestsListPage />,
    label: 'طلبات الشراء',
    protected: true,
  },
  {
    path: '/procurement/purchase-requests/create',
    element: <PurchaseRequestCreatePage />,
    label: 'طلب شراء جديد',
    protected: true,
  },
  {
    path: '/procurement/purchase-requests/:id/edit',
    element: <PurchaseRequestEditPage />,
    label: 'تعديل طلب الشراء',
    protected: true,
  },
  {
    path: '/procurement/purchase-requests/:id',
    element: <PurchaseRequestDetailPage />,
    label: 'تفاصيل طلب الشراء',
    protected: true,
  },
  {
    path: '/procurement/quotations',
    element: <QuotationsListPage />,
    label: 'عروض الأسعار',
    protected: true,
  },
  {
    path: '/procurement/quotations/create',
    element: <QuotationCreatePage />,
    label: 'عرض سعر جديد',
    protected: true,
  },
  {
    path: '/procurement/quotations/:id',
    element: <QuotationDetailPage />,
    label: 'تفاصيل عرض السعر',
    protected: true,
  },
  {
    path: '/procurement/quotations/:id/edit',
    element: <QuotationEditPage />,
    label: 'تعديل عرض السعر',
    protected: true,
  },
  {
    path: '/procurement/purchase-orders',
    element: <PurchaseOrdersListPage />,
    label: 'أوامر الشراء',
    protected: true,
  },
  {
    path: '/procurement/purchase-orders/create',
    element: <PurchaseOrderCreatePage />,
    label: 'أمر شراء جديد',
    protected: true,
  },
  {
    path: '/procurement/purchase-orders/:id',
    element: <PurchaseOrderDetailPage />,
    label: 'تفاصيل أمر الشراء',
    protected: true,
  },
  {
    path: '/procurement/purchase-orders/:id/edit',
    element: <PurchaseOrderEditPage />,
    label: 'تعديل أمر الشراء',
    protected: true,
  },
  {
    path: '/procurement/goods-receipt-notes',
    element: <GRNsListPage />,
    label: 'سندات استلام البضاعة',
    protected: true,
  },
  {
    path: '/procurement/goods-receipt-notes/create',
    element: <GRNCreatePage />,
    label: 'إنشاء باردة استلام',
    protected: true,
  },
  {
    path: '/procurement/goods-receipt-notes/:id',
    element: <GRNDetailPage />,
    label: 'تفاصيل باردة الاستلام',
    protected: true,
  },
  {
    path: '/procurement/supplier-invoices',
    element: <SupplierInvoicesListPage />,
    label: 'فواتير الموردين',
    protected: true,
  },
  {
    path: '/procurement/supplier-invoices/create',
    element: <SupplierInvoiceCreatePage />,
    label: 'فاتورة مورد جديدة',
    protected: true,
  },
  {
    path: '/procurement/supplier-invoices/:id',
    element: <SupplierInvoiceDetailPage />,
    label: 'تفاصيل فاتورة المورد',
    protected: true,
  },

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
    label: 'الموردون',
    protected: true,
  },
  {
    path: '/parties/create',
    element: <PartyCreatePage />,
    label: 'مورد جديد',
    protected: true,
  },
  {
    path: '/parties/:id',
    element: <PartyDetailPage />,
    label: 'تفاصيل المورد',
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
  // Reporting — Budget Execution (RPT-01)
  {
    path: '/reporting/budget-execution',
    element: <BudgetExecutionReportPage />,
    label: 'تقرير تنفيذ الموازنة',
    protected: true,
    requiredPermission: 'Reporting.ViewBudgetExecution',
  },
  // Reporting — Revenue Collections (RPT-02)
  {
    path: '/reporting/revenue-collections',
    element: <RevenueCollectionsReportPage />,
    label: 'سجل التحصيلات',
    protected: true,
    requiredPermission: 'Reporting.ViewRevenueCollections',
  },
  // Reporting — Disbursement Register (RPT-03)
  {
    path: '/reporting/disbursement-register',
    element: <DisbursementRegisterReportPage />,
    label: 'سجل الصرف',
    protected: true,
    requiredPermission: 'Reporting.ViewDisbursementRegister',
  },
  // Reporting — Availability Snapshot (RPT-04)
  {
    path: '/reporting/availability-snapshot',
    element: <AvailabilitySnapshotReportPage />,
    label: 'لقطة التوفر',
    protected: true,
    requiredPermission: 'Reporting.ViewAvailabilitySnapshot',
  },
  // Reporting — Trial Balance (RPT-05)
  {
    path: '/reporting/trial-balance',
    element: <TrialBalanceReportPage />,
    label: 'ميزان المراجعة',
    protected: true,
    requiredPermission: 'Reporting.ViewTrialBalance',
  },
  // Reporting — Financial Statements (RPT-06)
  {
    path: '/reporting/financial-statements/balance-sheet',
    element: <BalanceSheetReportPage />,
    label: 'الميزانية العمومية',
    protected: true,
    requiredPermission: 'Reporting.ViewFinancialStatements',
  },
  {
    path: '/reporting/financial-statements/income-statement',
    element: <IncomeStatementReportPage />,
    label: 'قائمة الدخل',
    protected: true,
    requiredPermission: 'Reporting.ViewFinancialStatements',
  },
  {
    path: '/reporting/financial-statements/cash-flow',
    element: <CashFlowStatementReportPage />,
    label: 'قائمة التدفقات النقدية',
    protected: true,
    requiredPermission: 'Reporting.ViewFinancialStatements',
  },
  {
    path: '/reporting/financial-statements/general-ledger',
    element: <GeneralLedgerReportPage />,
    label: 'دفتر الأستاذ العام',
    protected: true,
    requiredPermission: 'Reporting.ViewFinancialStatements',
  },
  // ─── Payments ──────────────────────────────────────────────────
  {
    path: '/payments/payment-orders',
    element: <PaymentOrdersListPage />,
    label: 'أوامر الدفع',
    protected: true,
    requiredPermission: 'PaymentOrders.View',
  },
  {
    path: '/payments/payment-orders/create',
    element: <PaymentOrderCreatePage />,
    label: 'أمر دفع جديد',
    protected: true,
    requiredPermission: 'PaymentOrders.Create',
  },
  {
    path: '/payments/payment-orders/:id',
    element: <PaymentOrderDetailPage />,
    label: 'تفاصيل أمر الدفع',
    protected: true,
    requiredPermission: 'PaymentOrders.View',
  },
  {
    path: '/payments/disbursement-requests',
    element: <DisbursementRequestsListPage />,
    label: 'طلبات الصرف',
    protected: true,
    requiredPermission: 'DisbursementRequests.View',
  },
  {
    path: '/payments/disbursement-requests/create',
    element: <DisbursementRequestCreatePage />,
    label: 'طلب صرف جديد',
    protected: true,
    requiredPermission: 'DisbursementRequests.Create',
  },
  {
    path: '/payments/disbursement-requests/:id',
    element: <DisbursementRequestDetailPage />,
    label: 'تفاصيل طلب الصرف',
    protected: true,
    requiredPermission: 'DisbursementRequests.View',
  },
  {
    path: '/payments/payments',
    element: <PaymentsListPage />,
    label: 'المدفوعات',
    protected: true,
    requiredPermission: 'Payments.View',
  },
  {
    path: '/payments/payments/:id',
    element: <PaymentSuccessPage />,
    label: 'تفاصيل الدفعة',
    protected: true,
    requiredPermission: 'Payments.View',
  },
  {
    path: '/payments/bank-accounts',
    element: <BankAccountsListPage />,
    label: 'الحسابات البنكية',
    protected: true,
    requiredPermission: 'BankAccounts.View',
  },
  {
    path: '/payments/bank-accounts/:id',
    element: <BankAccountDetailPage />,
    label: 'تفاصيل الحساب البنكي',
    protected: true,
    requiredPermission: 'BankAccounts.View',
  },
  {
    path: '/inventory/items',
    element: <ItemsListPage />,
    label: 'الأصناف',
    protected: true,
    requiredPermission: 'Items.View',
  },
  {
    path: '/inventory/items/create',
    element: <ItemCreatePage />,
    label: 'صنف جديد',
    protected: true,
    requiredPermission: 'Items.Create',
  },
  {
    path: '/inventory/items/:id',
    element: <ItemDetailPage />,
    label: 'تفاصيل الصنف',
    protected: true,
    requiredPermission: 'Items.View',
  },
  {
    path: '/inventory/items/:id/edit',
    element: <ItemEditPage />,
    label: 'تعديل الصنف',
    protected: true,
    requiredPermission: 'Items.Update',
  },
  {
    path: '/inventory/item-categories',
    element: <ItemCategoriesListPage />,
    label: 'تصنيفات الأصناف',
    protected: true,
    requiredPermission: 'ItemCategories.View',
  },
  {
    path: '/inventory/units',
    element: <UnitsListPage />,
    label: 'الوحدات',
    protected: true,
    requiredPermission: 'Units.View',
  },
  {
    path: '/inventory/warehouses',
    element: <WarehousesListPage />,
    label: 'المستودعات',
    protected: true,
    requiredPermission: 'Warehouses.View',
  },
];
