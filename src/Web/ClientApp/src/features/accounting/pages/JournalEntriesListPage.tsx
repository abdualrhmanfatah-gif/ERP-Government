import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useJournalEntriesList } from '../hooks/useJournalEntries';
import { StatusBadge } from '../components/StatusBadge';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import type { JournalEntryDto } from '../shared/client';

const statusFilters = [
  { key: '', label: 'الكل' },
  { key: 'Draft', label: 'مسودة' },
  { key: 'Submitted', label: 'مقدم' },
  { key: 'Approved', label: 'موافق عليه' },
  { key: 'Posted', label: 'مسجل' },
  { key: 'Reversed', label: 'معكوس' },
  { key: 'Cancelled', label: 'ملغى' },
];

const columns: DataGridColumn<JournalEntryDto>[] = [
  {
    id: 'entryNumber',
    key: 'entryNumber',
    header: 'رقم القيد',
    cell: (row) => (
      <span className="flex items-center gap-2">
        {row.entryNumber}
        {row.isSystemGenerated && (
          <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-[var(--color-surfaceContainerHigh)] text-[var(--color-onSurfaceVariant)]">نظام</span>
        )}
      </span>
    ),
  },
  {
    id: 'documentDate',
    key: 'documentDate',
    header: 'التاريخ',
    cell: (row) => {
      const v = row.documentDate;
      if (!v) return '-';
      if (typeof v === 'string') return v;
      if (v instanceof Date) return v.toLocaleDateString('ar-YE');
      return String(v);
    },
  },
  {
    id: 'entryStatus',
    key: 'entryStatus',
    header: 'الحالة',
    cell: (row) => <StatusBadge status={row.entryStatus} />,
  },
  {
    id: 'journalName',
    key: 'journalName',
    header: 'اليومية',
    cell: (row) => row.journalName || '-',
  },
  {
    id: 'totalDebit',
    key: 'totalDebit',
    header: 'مدين',
    align: 'left',
    cell: (row) => (row.totalDebit ?? 0).toLocaleString('ar-YE'),
  },
  {
    id: 'totalCredit',
    key: 'totalCredit',
    header: 'دائن',
    align: 'left',
    cell: (row) => (row.totalCredit ?? 0).toLocaleString('ar-YE'),
  },
];

export function JournalEntriesListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState('');

  const { data: entries = [], isLoading } = useJournalEntriesList({ entryStatus: statusFilter || undefined });

  const filteredEntries = useMemo(() => {
    if (!searchQuery) return entries;
    const q = searchQuery.toLowerCase();
    return entries.filter((e) => e.entryNumber.toLowerCase().includes(q) || (e.ref && e.ref.toLowerCase().includes(q)));
  }, [entries, searchQuery]);

  return (
    <div className="max-w-6xl mx-auto py-8 px-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight" style={{ color: 'var(--color-onSurface)' }}>قيود اليومية</h1>
          <p className="mt-1 text-sm" style={{ color: 'var(--color-onSurfaceVariant)' }}>إدارة ومراجعة قيود اليومية المحاسبية</p>
        </div>
        <button onClick={() => navigate('/accounting/journal-entries/create')}
          className="px-5 py-2.5 rounded-lg text-sm font-bold transition-all duration-200 cursor-pointer hover:shadow-md"
          style={{ backgroundColor: 'var(--color-primary)', color: 'var(--color-on-primary)' }}>+ قيد جديد</button>
      </div>

      <div className="rounded-xl border overflow-hidden" style={{ backgroundColor: 'var(--color-surface)', borderColor: 'var(--color-outlineVariant)' }}>
        <div className="px-6 py-4 border-b" style={{ borderColor: 'var(--color-outlineVariant)', backgroundColor: 'var(--color-surfaceContainerLow)' }}>
          <div className="flex items-center justify-between gap-4">
            <div className="flex flex-wrap gap-2">
              {statusFilters.map((sf) => (
                <button key={sf.key} type="button" onClick={() => setStatusFilter(sf.key)}
                  className="px-3 py-1.5 rounded-full text-xs font-bold transition-all duration-200 cursor-pointer hover:shadow-sm"
                  style={{
                    backgroundColor: statusFilter === sf.key ? 'var(--color-primary)' : 'var(--color-surface)',
                    color: statusFilter === sf.key ? 'var(--color-on-primary)' : 'var(--color-onSurface)',
                    borderColor: statusFilter === sf.key ? 'transparent' : 'var(--color-outlineVariant)',
                  }}>{sf.label}</button>
              ))}
            </div>
            <input type="text" value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} placeholder="بحث برقم القيد..."
              className="w-64 px-3 py-2 rounded-lg border text-sm focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:border-[var(--color-focus-ring)] transition-colors duration-200"
              style={{ backgroundColor: 'var(--color-surface)', color: 'var(--color-onSurface)', borderColor: 'var(--color-outlineVariant)' }} />
          </div>
        </div>

        <DataGrid<JournalEntryDto>
          columns={columns}
          data={filteredEntries}
          loading={isLoading}
          emptyMessage="لا توجد قيود"
          rowKey={(row) => row.id}
          onRowClick={(row) => navigate(`/accounting/journal-entries/${row.id}`)}
        />
      </div>
    </div>
  );
}
