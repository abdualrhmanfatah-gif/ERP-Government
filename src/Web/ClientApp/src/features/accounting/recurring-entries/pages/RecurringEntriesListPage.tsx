import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRecurringEntries } from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS, STATUS_LABELS } from '../shared/types';
import type { RecurringEntryListFilters } from '../shared/client';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui';
import { Loading } from '@/components/ui/Loading';
import { StatusBadge } from '@/components/ui/StatusBadge';

export default function RecurringEntriesListPage() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<RecurringEntryListFilters>({});
  const { data: entries, isLoading } = useRecurringEntries(filters);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">القيود الدورية</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/accounting/recurring-entries/new')}>
          إنشاء جدول جديد
        </Button>
      </div>

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

      {isLoading ? (
        <Loading />
      ) : (
        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b">
              <th className="text-right p-3">رقم القيد</th>
              <th className="text-right p-3">الاسم</th>
              <th className="text-right p-3">الدفتر</th>
              <th className="text-right p-3">الدورية</th>
              <th className="text-right p-3">التاريخ القادم</th>
              <th className="text-right p-3">الحالة</th>
              <th className="text-right p-3">إجراءات</th>
            </tr>
          </thead>
          <tbody>
            {entries?.map((entry) => (
              <tr key={entry.id} className="border-b hover:bg-gray-50">
                <td className="p-3 font-mono">{entry.entryNumber}</td>
                <td className="p-3">{entry.name}</td>
                <td className="p-3">{entry.journalName}</td>
                <td className="p-3">{FREQUENCY_LABELS[entry.frequency ?? '']}</td>
                <td className="p-3">
                  {entry.nextExecutionDate
                    ? new Date(entry.nextExecutionDate).toLocaleDateString('ar-YE')
                    : '-'}
                </td>
                <td className="p-3">
                  <StatusBadge variant={entry.status === 'Active' ? 'active' : entry.status === 'Paused' ? 'draft' : 'closed'}>
                    {STATUS_LABELS[entry.status ?? '']}
                  </StatusBadge>
                </td>
                <td className="p-3">
                  <Button variant="ghost" size="sm" onClick={() => navigate(`/accounting/recurring-entries/${entry.id}`)}>
                    التفاصيل
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
