import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useBankAccountsList } from '../hooks/useBankAccounts';
import { bankAccountStatusLabels } from '../shared/types';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { Page, Button, FilterBar, MoneyDisplay, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { Plus } from 'lucide-react';
import type { BankAccountDto } from '../../../../web-api-client';

const statusOptions = [
  { value: '', label: 'الكل' },
  ...Object.entries(bankAccountStatusLabels).map(([value, label]) => ({ value, label })),
];

export function BankAccountsListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState('');
  const { data: accounts = [], isLoading } = useBankAccountsList();

  const filtered = statusFilter
    ? accounts.filter(a => a.isActive === (statusFilter === 'active'))
    : accounts;

  const columns: DataGridColumn<BankAccountDto>[] = [
    {
      header: 'الاسم',
      cell: (row) => (
        <Link to={`/payments/bank-accounts/${row.id}`} className="text-[var(--color-primary)] hover:underline">
          {row.name}
        </Link>
      ),
    },
    { header: 'البنك', cell: (row) => <span className="text-sm">{row.bankName}</span> },
    { header: 'رقم الحساب', cell: (row) => <span className="font-mono">{row.accountNumber}</span> },
    { header: 'العملة', cell: (row) => <span className="text-sm">{row.currencyId}</span> },
    { header: 'الرصيد', cell: (row) => <MoneyDisplay value={row.currentBalance ?? 0} /> },
    { header: 'افتراضي', cell: (row) => <span>{row.isDefault ? '✓' : ''}</span> },
    {
      header: 'الحالة',
      cell: (row) => (
        <Badge variant={row.isActive ? 'success' : 'default'}>
          {getActiveStatusLabel(row.isActive ?? false)}
        </Badge>
      ),
    },
    {
      header: 'آخر تسوية',
      cell: (row) => <span className="text-sm">{row.lastReconciliationDate ? new Date(row.lastReconciliationDate).toLocaleDateString('ar-YE') : '—'}</span>,
    },
  ];

  return (
    <Page
      title="الحسابات البنكية"
      actions={
        <Button variant="primary" size="sm">
          <Plus size={16} className="ms-1" />
          حساب جديد
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={!!statusFilter} onClear={() => setStatusFilter('')}>
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={statusOptions}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد حسابات بنكية"
        rowKey={(row) => row.id ?? 0}
        onRowClick={(row) => navigate(`/payments/bank-accounts/${row.id}`)}
      />
    </Page>
  );
}
