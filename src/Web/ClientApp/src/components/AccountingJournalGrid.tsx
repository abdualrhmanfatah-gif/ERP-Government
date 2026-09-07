import { useNavigate } from 'react-router-dom';
import { DataGrid } from '@/components/ui/DataGrid';
import type { JournalDto } from '../web-api-client';

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
    cell: (row: JournalDto) =>
      row.isActive ? (
        <span className="text-[var(--color-success)]">نشط</span>
      ) : (
        <span className="text-[var(--color-on-surface-variant)]">غير نشط</span>
      ),
  },
];

interface JournalGridProps {
  data: JournalDto[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
}

export function JournalGrid({ data, loading, error, onRetry }: JournalGridProps) {
  const navigate = useNavigate();

  return (
    <DataGrid
      columns={columns}
      data={data}
      loading={loading}
      error={error}
      onRetry={onRetry}
      emptyMessage="لا توجد دفاتر"
      rowKey={(row) => row.id ?? 0}
      onRowClick={(row) => navigate(`/accounting/journals/${row.id}`)}
    />
  );
}
