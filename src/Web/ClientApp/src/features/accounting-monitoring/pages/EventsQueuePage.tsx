import { useState } from 'react';
import { usePendingEvents } from '../hooks/usePendingEvents';

export function EventsQueuePage() {
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
  const [eventTypeFilter, setEventTypeFilter] = useState<string | undefined>();

  const { data: events, isLoading } = usePendingEvents({
    status: statusFilter,
    eventType: eventTypeFilter,
  });

  return (
    <div dir="rtl" className="p-6">
      <h1 className="text-2xl font-bold mb-6" style={{ color: 'var(--color-onSurface)' }}>
        طابور الأحداث
      </h1>

      <div className="flex gap-4 mb-6">
        <select
          value={statusFilter ?? ''}
          onChange={(e) => setStatusFilter(e.target.value || undefined)}
          className="border rounded px-3 py-2"
        >
          <option value="">جميع الحالات</option>
          <option value="Pending">معلق</option>
          <option value="Processing">قيد المعالجة</option>
          <option value="Posted">تم الترحيل</option>
          <option value="Failed">فشل</option>
        </select>
        <select
          value={eventTypeFilter ?? ''}
          onChange={(e) => setEventTypeFilter(e.target.value || undefined)}
          className="border rounded px-3 py-2"
        >
          <option value="">جميع الأنواع</option>
          <option value="PaymentExecution">تنفيذ دفعة</option>
          <option value="ReceiptCollection">تحصيل</option>
          <option value="DepositClearing">تسوية إيداع</option>
        </select>
      </div>

      {isLoading ? (
        <p>جاري التحميل...</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse" style={{ color: 'var(--color-onSurface)' }}>
            <thead>
              <tr style={{ backgroundColor: 'var(--color-surface)' }}>
                <th className="border p-2 text-right">المعرف</th>
                <th className="border p-2 text-right">نوع الحدث</th>
                <th className="border p-2 text-right">المصدر</th>
                <th className="border p-2 text-right">الحالة</th>
                <th className="border p-2 text-right">الخطأ</th>
                <th className="border p-2 text-right">المحاولات</th>
                <th className="border p-2 text-right">قيود يومية</th>
              </tr>
            </thead>
            <tbody>
              {events?.map((event) => (
                <tr key={event.id}>
                  <td className="border p-2">{event.id}</td>
                  <td className="border p-2">{event.eventType}</td>
                  <td className="border p-2">{event.sourceTable} #{event.sourceId}</td>
                  <td className="border p-2">
                    <span
                      className="px-2 py-1 rounded text-sm"
                      style={{
                        backgroundColor:
                          event.status === 'Failed'
                            ? 'var(--color-error)'
                            : event.status === 'Pending'
                            ? 'var(--color-statusPending)'
                            : 'var(--color-statusApproved)',
                        color: 'white',
                      }}
                    >
                      {event.status}
                    </span>
                  </td>
                  <td className="border p-2" style={{ color: 'var(--color-error)' }}>
                    {event.errorMessage ?? '-'}
                  </td>
                  <td className="border p-2">{event.retryCount}</td>
                  <td className="border p-2">
                    {event.journalEntryId ? `#${event.journalEntryId}` : '-'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
