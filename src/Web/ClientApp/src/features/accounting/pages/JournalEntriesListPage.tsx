import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useJournalEntriesList } from '../hooks/useJournalEntries';
import { StatusBadge } from '@/components/AccountingStatusBadge';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import type { JournalEntryDto } from '../shared/client';
import { Button, Card, Input } from '@/components/ui';

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
          <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-[var(--color-surface-container-high)] text-[var(--color-on-surface-variant)]">نظام</span>
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
    <Page
      title="قيود اليومية"
      description="إدارة ومراجعة قيود اليومية المحاسبية"
      actions={<Button variant="primary" onClick={() => navigate('/accounting/journal-entries/create')}>+ قيد جديد</Button>}
      loading={isLoading}
    >
      <Card variant="default">
        <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <div className="flex items-center justify-between gap-4">
            <div className="flex flex-wrap gap-2">
              {statusFilters.map((sf) => (
                <Button key={sf.key} type="button" variant={statusFilter === sf.key ? 'primary' : 'ghost'} size="sm" onClick={() => setStatusFilter(sf.key)}>
                  {sf.label}
                </Button>
              ))}
            </div>
            <Input type="text" value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} placeholder="بحث برقم القيد..." className="w-64" />
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
      </Card>
    </Page>
  );
}
