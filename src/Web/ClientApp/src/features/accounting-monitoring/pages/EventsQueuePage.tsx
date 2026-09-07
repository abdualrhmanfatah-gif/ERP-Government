import { useState } from 'react';
import { usePendingEvents } from '../hooks/usePendingEvents';
import { Select, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';

type EventRow = NonNullable<ReturnType<typeof usePendingEvents>['data']>[number];

export function EventsQueuePage() {
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [eventTypeFilter, setEventTypeFilter] = useState<string | undefined>();

  const { data: events, isLoading } = usePendingEvents({
    status: statusFilter,
    eventType: eventTypeFilter,
  });

  const columns: DataGridColumn<EventRow>[] = [
    { key: 'id', header: 'المعرف' },
    { key: 'eventType', header: 'نوع الحدث' },
    {
      key: 'source',
      header: 'المصدر',
      accessorFn: (event) => `${event.sourceTable} #${event.sourceId}`,
    },
    {
      key: 'status',
      header: 'الحالة',
      render: (event) => (
        <Badge
          variant={
            event.status === 'Failed'
              ? 'error'
              : event.status === 'Pending'
              ? 'warning'
              : 'success'
          }
        >
          {event.status}
        </Badge>
      ),
    },
    {
      key: 'errorMessage',
      header: 'الخطأ',
      cellClassName: 'text-[var(--color-error)]',
      accessorFn: (event) => event.errorMessage ?? '-',
    },
    { key: 'retryCount', header: 'المحاولات' },
    {
      key: 'journalEntryId',
      header: 'قيود يومية',
      accessorFn: (event) => (event.journalEntryId ? `#${event.journalEntryId}` : '-'),
    },
  ];

  return (
    <div dir="rtl" className="p-6">
      <h1 className="text-2xl font-bold mb-6 text-[var(--color-on-surface)]">
        طابور الأحداث
      </h1>

      <div className="flex gap-4 mb-6">
        <Select
          label="الحالة"
          value={statusFilter ?? ''}
          onChange={(e) => setStatusFilter(e.target.value || undefined)}
          options={[
            { value: '', label: 'جميع الحالات' },
            { value: 'Pending', label: 'معلق' },
            { value: 'Approved', label: 'معتمد' },
            { value: 'Failed', label: 'فشل' },
          ]}
          className="w-auto"
        />
        <Select
          label="نوع الحدث"
          value={eventTypeFilter ?? ''}
          onChange={(e) => setEventTypeFilter(e.target.value || undefined)}
          options={[
            { value: '', label: 'جميع الأنواع' },
            { value: 'PaymentExecution', label: 'تنفيذ دفعة' },
            { value: 'ReceiptCollection', label: 'تحصيل' },
            { value: 'DepositClearing', label: 'تسوية إيداع' },
          ]}
          className="w-auto"
        />
      </div>

      <DataGrid
        columns={columns}
        data={events ?? []}
        loading={isLoading}
        emptyMessage="لا توجد أحداث"
        rowKey={(event) => event.id}
      />
    </div>
  );
}
