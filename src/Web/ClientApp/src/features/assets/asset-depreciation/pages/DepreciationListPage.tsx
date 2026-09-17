import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Play } from 'lucide-react';
import { Button, DataGrid, FilterBar, FilterSelect, MoneyDisplay, Page, StatusBadge } from '@/components/ui';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { formatDate } from '@/shared/utils/formatters';
import { useDepreciationRuns } from '../hooks/useDepreciation';
import { getDepreciationRunStatusBadge } from '../shared/status';
import { depreciationStatusOptions, type DepreciationRun } from '../shared/types';

export function DepreciationListPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, error, refetch } = useDepreciationRuns({ status: status || undefined, page });
  const items = useMemo(() => data?.items ?? [], [data]);

  return (
    <Page
      title="إهلاك الأصول"
      description="عمليات الإهلاك الدورية لجميع الأصول"
      maxWidth="full"
      loading={isLoading && !data}
      actions={
        <Button variant="primary" size="sm" icon={<Play size={16} />} onClick={() => navigate('/assets/depreciation/run')}>
          تشغيل الإهلاك
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={!!status} onClear={() => { setStatus(''); setPage(1); }}>
          <FilterSelect label="الحالة" value={status} onChange={(value) => { setStatus(value); setPage(1); }} options={depreciationStatusOptions} />
        </FilterBar>
      }
    >
      <DataGrid<DepreciationRun>
        data={items}
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/assets/depreciation/${row.id}`)}
        loading={isFetching && !isLoading}
        error={error ? getQueryErrorMessage(error) : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد عمليات إهلاك"
        pagination={{ pageIndex: page - 1, pageSize: data?.pageSize ?? 20 }}
        totalItems={data?.totalCount ?? 0}
        onPageChange={(pageIndex) => setPage(pageIndex + 1)}
        columns={[
          { id: 'runNumber', accessorKey: 'runNumber', header: 'رقم العملية', width: 130, cell: (row) => <span dir="ltr" className="font-mono tabular-nums">{row.runNumber}</span> },
          { id: 'fiscalYear', accessorKey: 'fiscalYear', header: 'السنة المالية' },
          { id: 'periodNumber', accessorKey: 'periodNumber', header: 'الفترة', width: 80 },
          { id: 'depreciationDate', accessorKey: 'depreciationDate', header: 'تاريخ الإهلاك', width: 120, cell: (row) => formatDate(row.depreciationDate) },
          { id: 'assetsCount', accessorKey: 'assetsCount', header: 'الأصول', width: 80 },
          { id: 'totalDepreciation', accessorKey: 'totalDepreciation', header: 'إجمالي الإهلاك', align: 'right', cell: (row) => <MoneyDisplay value={row.totalDepreciation} /> },
          { id: 'status', accessorKey: 'status', header: 'الحالة', width: 110, cell: (row) => { const badge = getDepreciationRunStatusBadge(row.status); return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>; } },
          { id: 'journalEntryId', accessorKey: 'journalEntryId', header: 'قيد الترحيل', width: 110, cell: (row) => row.journalEntryId ? <span dir="ltr" className="tabular-nums">{row.journalEntryId}</span> : '—' },
        ]}
      />
    </Page>
  );
}
