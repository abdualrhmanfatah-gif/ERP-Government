import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { FilterBar, FilterSelect, FilterDate } from '@/components/ui';
import { MovesGrid } from '../components/MovesGrid';
import { useMovesList } from '../hooks/useMoves';
import { useJournalsList } from '../hooks/useJournalsList';

export function MovesListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const status = searchParams.get('status') ?? '';
  const fromDate = searchParams.get('fromDate') ?? '';
  const toDate = searchParams.get('toDate') ?? '';
  const journalId = searchParams.get('journalId') ?? '';

  const filters = {
    entryStatus: status || undefined,
    journalId: journalId ? Number(journalId) : undefined,
    fromDate: fromDate || undefined,
    toDate: toDate || undefined,
  };

  const { data = [], isLoading, error, refetch, totalItems = 0 } = useMovesList(filters, { page, pageSize });
  const { data: journalsData } = useJournalsList({ isActive: true });
  const journals = journalsData ?? [];

  const hasFilters = !!(status || fromDate || toDate || journalId);

  useEffect(() => {
    setPage(1);
  }, [status, fromDate, toDate, journalId]);

  const updateParam = (key: string, value: string) => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev);
      if (value) {
        next.set(key, value);
      } else {
        next.delete(key);
      }
      return next;
    });
  };

  const clearAll = () => {
    setSearchParams(new URLSearchParams());
  };

  return (
    <div className="space-y-4">
      <PageHeader
        title="قيود اليومية"
        description="إدارة قيود اليومية — إنشاء ومراجعة وترحيل"
        actions={
          <Button variant="primary" size="sm" onClick={() => navigate('/accounting/journal-entries/create')}>
            قيد جديد
          </Button>
        }
      />

      <FilterBar hasFilters={hasFilters} onClear={clearAll}>
        <FilterSelect
          label="الحالة"
          value={status}
          onChange={(v) => updateParam('status', v)}
          options={[
            { value: 'Draft', label: 'مسودة' },
            { value: 'Submitted', label: 'مرسل' },
            { value: 'Approved', label: 'موافق عليه' },
            { value: 'Posted', label: 'مرحل' },
            { value: 'Reversed', label: 'ملغى عكسي' },
            { value: 'Cancelled', label: 'ملغى' },
          ]}
          placeholder="كل الحالات"
        />

        <FilterDate
          label="من تاريخ"
          value={fromDate}
          onChange={(v) => updateParam('fromDate', v)}
        />

        <FilterDate
          label="إلى تاريخ"
          value={toDate}
          onChange={(v) => updateParam('toDate', v)}
          min={fromDate}
        />

        <FilterSelect
          label="اليومية"
          value={journalId}
          onChange={(v) => updateParam('journalId', v)}
          options={journals.map((j) => ({
            value: String(j.id),
            label: `${j.code} — ${j.name}`,
          }))}
          placeholder="كل الأيام"
        />
      </FilterBar>

      <MovesGrid
        data={data}
        loading={isLoading}
        error={error ? (error as Error).message : undefined}
        onRetry={() => refetch()}
        totalItems={totalItems}
        pagination={{ pageIndex: page - 1, pageSize }}
        onPageChange={(p) => setPage(p + 1)}
      />
    </div>
  );
}
