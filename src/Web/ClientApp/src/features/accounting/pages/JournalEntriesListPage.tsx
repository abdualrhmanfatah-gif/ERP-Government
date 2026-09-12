import { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useJournalEntriesList } from '../hooks/useJournalEntries';
import { StatusBadge } from '@/components/AccountingStatusBadge';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import type { JournalEntryDto } from '@/web-api-client';
import { Button, Card, Input, Badge, Page } from '@/components/ui';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { statusFilters } from '../shared/types';
import { downloadBlobExport, buildExportUrl } from '@/shared/utils/download';

export function JournalEntriesListPage() {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState('');
  const [exportingAll, setExportingAll] = useState(false);

  const { data: entries = [], isLoading } = useJournalEntriesList({ entryStatus: statusFilter || undefined });

  const filteredEntries = useMemo(() => {
    if (!searchQuery) return entries;
    const q = searchQuery.toLowerCase();
    return entries.filter((e) => (e.entryNumber ?? '').toLowerCase().includes(q) || (e.ref && e.ref.toLowerCase().includes(q)));
  }, [entries, searchQuery]);

  const handleExportEntry = useCallback(async (entry: JournalEntryDto, format: 'xlsx' | 'pdf') => {
    try {
      const url = buildExportUrl('/api/JournalEntries/export', {
        format,
        entryId: entry.id,
      });
      await downloadBlobExport(url, `JournalEntry-${entry.entryNumber ?? entry.id}.${format}`);
      notify({ type: 'success', title: 'تم تصدير القيد بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير القيد' });
    }
  }, []);

  const handleExportAll = useCallback(async (format: 'xlsx' | 'pdf') => {
    setExportingAll(true);
    try {
      const url = buildExportUrl('/api/JournalEntries/export', {
        format,
        status: statusFilter || undefined,
      });
      await downloadBlobExport(url, `JournalEntries-${new Date().toISOString().slice(0, 10)}.${format}`);
      notify({ type: 'success', title: 'تم تصدير القيود بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير القيود' });
    } finally {
      setExportingAll(false);
    }
  }, [statusFilter]);

  const columns: DataGridColumn<JournalEntryDto>[] = [
    {
      id: 'entryNumber',
      key: 'entryNumber',
      header: 'رقم القيد',
      cell: (row) => (
        <span className="flex items-center gap-2">
          {row.entryNumber}
          {row.isSystemGenerated && (
            <Badge variant="default">نظام</Badge>
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
        return String(v);
      },
    },
    {
      id: 'entryStatus',
      key: 'entryStatus',
      header: 'الحالة',
      cell: (row) => <StatusBadge status={row.entryStatus as any} />,
    },
    {
      id: 'journalName',
      key: 'journalName',
      header: 'اليومية',
      cell: (row) => row.journalName || '-',
    },
    {
      id: 'totalBaseDebit',
      key: 'totalBaseDebit',
      header: 'مدين',
      align: 'left',
      cell: (row) => (row.totalBaseDebit ?? 0).toLocaleString('ar-YE'),
    },
    {
      id: 'totalBaseCredit',
      key: 'totalBaseCredit',
      header: 'دائن',
      align: 'left',
      cell: (row) => (row.totalBaseCredit ?? 0).toLocaleString('ar-YE'),
    },
    {
      id: 'export',
      key: 'export',
      header: 'تصدير',
      align: 'center',
      cell: (row) => (
        <div className="flex gap-1 justify-center">
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); handleExportEntry(row, 'xlsx'); }}>
            <Download size={14} />
          </Button>
          <Button variant="ghost" size="sm" onClick={(e) => { e.stopPropagation(); handleExportEntry(row, 'pdf'); }}>
            <FileText size={14} />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <Page
      title="قيود اليومية"
      description="إدارة ومراجعة قيود اليومية المحاسبية"
      actions={
        <div className="flex gap-2">
          <Button variant="outline" size="sm" disabled={exportingAll} onClick={() => handleExportAll('xlsx')}>
            <Download size={16} className="ms-1" />
            تصدير الكل Excel
          </Button>
          <Button variant="outline" size="sm" disabled={exportingAll} onClick={() => handleExportAll('pdf')}>
            <FileText size={16} className="ms-1" />
            تصدير الكل PDF
          </Button>
          <Button variant="primary" onClick={() => navigate('/accounting/journal-entries/create')}>+ قيد جديد</Button>
        </div>
      }
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
          rowKey={(row) => row.id ?? 0}
          onRowClick={(row) => navigate(`/accounting/journal-entries/${row.id}`)}
        />
      </Card>
    </Page>
  );
}
