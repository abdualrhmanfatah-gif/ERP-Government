// Deposit slips list — US1: status/formType filters
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, FilterBar, FilterSelect, Badge, EmptyState, MoneyDisplay, Page } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { DepositSlipsClient } from '../../../web-api-client';

const client = new DepositSlipsClient();

const statusOptions = [
  { value: '', label: 'الكل' },
  { value: 'Draft', label: 'مسودة' },
  { value: 'Approved', label: 'معتمدة' },
];

const formTypeOptions = [
  { value: '', label: 'الكل' },
  { value: 'Form47', label: 'نقدية (47)' },
  { value: 'Form48', label: 'شيكات (48)' },
];

const statusBadgeVariant: Record<string, string> = {
  Draft: 'warning',
  Approved: 'success',
};

const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Approved: 'معتمدة',
};

const formTypeLabels: Record<string, string> = {
  Form47: 'نقدية (47)',
  Form48: 'شيكات (48)',
};

export default function DepositSlipsListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState('');
  const [formTypeFilter, setFormTypeFilter] = useState('');

  const { data: slips = [], isLoading } = useQuery({
    queryKey: ['deposit-slips', 'list', statusFilter, formTypeFilter],
    queryFn: () =>
      client.depositSlipsAll(
        statusFilter as any || undefined,
        formTypeFilter as any || undefined,
        undefined,
        undefined,
        1,
        50,
      ),
  });

  const columns: DataGridColumn<Record<string, unknown>>[] = useMemo(
    () => [
      {
        key: 'slipNumber',
        header: 'رقم البطاقة',
        render: (row) => <span className="font-medium tabular-nums">{row.slipNumber as string}</span>,
      },
      {
        key: 'slipDate',
        header: 'التاريخ',
        render: (row) => new Date(row.slipDate as string).toLocaleDateString('ar-YE'),
      },
      {
        key: 'formType',
        header: 'النوع',
        render: (row) => formTypeLabels[row.formType as string] ?? '—',
      },
      {
        key: 'status',
        header: 'الحالة',
        render: (row) => (
          <Badge variant={(statusBadgeVariant[row.status as string] ?? 'default') as any}>
            {statusLabels[row.status as string] ?? '—'}
          </Badge>
        ),
      },
      {
        key: 'totalAmount',
        header: 'الإجمالي',
        render: (row) => <MoneyDisplay value={(row.totalAmount as number) ?? 0} />,
      },
      {
        key: 'actions',
        header: '',
        render: (row) => (
          <Button variant="ghost" size="sm" onClick={() => navigate(`/treasury/deposit-slips/${row.id}`)}>
            <Eye className="h-4 w-4" />
          </Button>
        ),
      },
    ],
    [navigate],
  );

  return (
    <Page
      title="بطاقات الإيداع"
      actions={
        <Button onClick={() => navigate('/treasury/deposit-slips/create')}>
          <Plus className="h-4 w-4 ml-2" /> إنشاء بطاقة
        </Button>
      }
      toolbar={
        <FilterBar>
          <FilterSelect
            label="الحالة"
            value={statusFilter}
            onChange={setStatusFilter}
            options={statusOptions}
          />
          <FilterSelect
            label="النوع"
            value={formTypeFilter}
            onChange={setFormTypeFilter}
            options={formTypeOptions}
          />
        </FilterBar>
      }
      loading={isLoading}
    >
      {slips.length === 0 ? (
        <EmptyState message="لا توجد بطاقات إيداع" />
      ) : (
        <DataGrid columns={columns} data={slips as any} rowKey={(row: any) => row.id} />
      )}
    </Page>
  );
}
