import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRecurringEntries } from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS, STATUS_LABELS } from '../shared/types';
import type { RecurringEntryListFilters } from '../shared/client';
import { Page, Button, Select, StatusBadge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';

type EntryRow = NonNullable<ReturnType<typeof useRecurringEntries>['data']>[number];

export default function RecurringEntriesListPage() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<RecurringEntryListFilters>({});
  const { data: entries, isLoading } = useRecurringEntries(filters);

  const columns: DataGridColumn<EntryRow>[] = [
    { key: 'entryNumber', header: 'رقم القيد', cellClassName: 'font-mono' },
    { key: 'name', header: 'الاسم' },
    { key: 'journalName', header: 'الدفتر' },
    { key: 'frequency', header: 'الدورية', accessorFn: (e) => FREQUENCY_LABELS[e.frequency ?? ''] },
    {
      key: 'nextExecutionDate',
      header: 'التاريخ القادم',
      accessorFn: (e) => (e.nextExecutionDate ? new Date(e.nextExecutionDate).toLocaleDateString('ar-YE') : '-'),
    },
    {
      key: 'status',
      header: 'الحالة',
      render: (entry) => (
        <StatusBadge variant={entry.status === 'Active' ? 'active' : entry.status === 'Paused' ? 'draft' : 'closed'}>
          {STATUS_LABELS[entry.status ?? '']}
        </StatusBadge>
      ),
    },
    {
      key: 'actions',
      header: 'إجراءات',
      render: (entry) => (
        <Button variant="ghost" size="sm" onClick={() => navigate(`/accounting/recurring-entries/${entry.id}`)}>
          التفاصيل
        </Button>
      ),
    },
  ];

  return (
    <Page
      title="القيود الدورية"
      actions={
        <Button variant="primary" size="sm" onClick={() => navigate('/accounting/recurring-entries/new')}>
          إنشاء جدول جديد
        </Button>
      }
      toolbar={
        <div className="flex gap-4">
          <Select
            label="الحالة"
            value={filters.status ?? ''}
            onChange={(e) => setFilters({ ...filters, status: e.target.value || undefined })}
            options={[
              { value: '', label: 'الكل' },
              ...Object.entries(STATUS_LABELS).map(([key, label]) => ({ value: key, label })),
            ]}
            className="w-auto"
          />
          <Select
            label="الدورية"
            value={filters.frequency ?? ''}
            onChange={(e) => setFilters({ ...filters, frequency: e.target.value || undefined })}
            options={[
              { value: '', label: 'الكل' },
              ...Object.entries(FREQUENCY_LABELS).map(([key, label]) => ({ value: key, label })),
            ]}
            className="w-auto"
          />
        </div>
      }
      loading={isLoading}
    >
      <DataGrid
        columns={columns}
        data={entries ?? []}
        loading={isLoading}
        emptyMessage="لا توجد جداول دورية"
        rowKey={(entry) => entry.id}
      />
    </Page>
  );
}
