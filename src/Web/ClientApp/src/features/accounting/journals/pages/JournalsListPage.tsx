import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, Badge, DataGrid, FilterBar, FilterSelect } from '@/components/ui';
import { useJournalsList } from '../../hooks/useJournalsList';
import { JournalType, type JournalDto } from '../../../../web-api-client';
import { activeStatusLabels } from '@/shared/constants/labels';

const journalTypeLabels: Record<string, string> = {
  General: 'عامة',
  Purchase: 'مشتريات',
  Sale: 'مبيعات',
  Cash: 'نقدية',
  Bank: 'بنكية',
  Adjustment: 'تسوية',
  Closing: 'إقفال',
};

const columns = [
  { key: 'code', header: 'الرمز', accessorKey: 'code' as const, width: 120 },
  { key: 'name', header: 'الاسم', accessorKey: 'name' as const, width: 200 },
  {
    key: 'type',
    header: 'النوع',
    accessorKey: 'type' as const,
    width: 120,
    cell: (row: JournalDto) => journalTypeLabels[row.type ?? ''] ?? row.type ?? '—',
  },
  { key: 'sequenceId', header: 'السلسلة', accessorKey: 'sequenceId' as const, width: 100, cell: (row: JournalDto) => row.sequenceId ?? '—' },
  {
    key: 'requireApprovalBeforePosting',
    header: 'اشتراط الاعتماد',
    accessorKey: 'requireApprovalBeforePosting' as const,
    width: 120,
    cell: (row: JournalDto) => row.requireApprovalBeforePosting ? 'نعم' : 'لا',
  },
  {
    key: 'isActive',
    header: 'نشط',
    accessorKey: 'isActive' as const,
    width: 80,
    cell: (row: JournalDto) => (
      <Badge variant={row.isActive ? 'success' : 'default'}>
        {row.isActive ? 'نشط' : 'غير نشط'}
      </Badge>
    ),
  },
];

const journalTypeOptions = [
  { value: '', label: 'الكل' },
  { value: JournalType.General, label: 'عامة' },
  { value: JournalType.Purchase, label: 'مشتريات' },
  { value: JournalType.Sale, label: 'مبيعات' },
  { value: JournalType.Cash, label: 'نقدية' },
  { value: JournalType.Bank, label: 'بنكية' },
  { value: JournalType.Adjustment, label: 'تسوية' },
  { value: JournalType.Closing, label: 'إقفال' },
];

const activeOptions = [
  { value: '', label: 'الكل' },
  { value: 'true', label: activeStatusLabels.active },
  { value: 'false', label: activeStatusLabels.inactive },
];

export function JournalsListPage() {
  const navigate = useNavigate();
  const [type, setType] = useState<string>('');
  const [isActive, setIsActive] = useState<string>('');

  const { data: journals = [], isLoading, error, refetch } = useJournalsList({
    type: type ? (type as JournalType) : undefined,
    isActive: isActive === '' ? undefined : isActive === 'true',
  });

  const hasFilters = !!type || !!isActive;

  const clearAll = () => {
    setType('');
    setIsActive('');
  };

  return (
    <Page
      title="دفاتر اليومية"
      description="إدارة دفاتر اليومية المحاسبية"
      actions={
        <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/accounting/journals/create')}>
          إنشاء دفتر
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={clearAll}>
          <FilterSelect
            label="النوع"
            value={type}
            onChange={setType}
            options={journalTypeOptions}
          />
          <FilterSelect
            label="الحالة"
            value={isActive}
            onChange={setIsActive}
            options={activeOptions}
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={journals}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد دفاتر"
        rowKey={(row) => row.id ?? 0}
        onRowClick={(row) => navigate(`/accounting/journals/${row.id}`)}
      />
    </Page>
  );
}
