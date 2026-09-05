import { useNavigate } from 'react-router-dom';
import { DataGrid } from '@/components/ui/DataGrid';
import { StatusBadge } from '@/components/ui/StatusBadge';
import type { MoveDto } from '../types';
import type { PaginationState } from '@tanstack/react-table';

interface MovesGridProps {
  data: MoveDto[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
  totalItems: number;
  pagination: PaginationState;
  onPageChange: (page: number) => void;
}

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    Draft: 'مسودة',
    Submitted: 'مرسل',
    Approved: 'موافق عليه',
    Posted: 'مرحل',
    Reversed: 'ملغى عكسي',
    Cancelled: 'ملغى',
  };
  return map[status] ?? status;
}

function statusVariant(status: string): 'draft' | 'pending' | 'approved' | 'active' | 'closed' {
  const map: Record<string, 'draft' | 'pending' | 'approved' | 'active' | 'closed'> = {
    Draft: 'draft',
    Submitted: 'pending',
    Approved: 'approved',
    Posted: 'active',
    Reversed: 'closed',
    Cancelled: 'closed',
  };
  return map[status] ?? 'draft';
}

export function MovesGrid({ data, loading, error, onRetry, totalItems, pagination, onPageChange }: MovesGridProps) {
  const navigate = useNavigate();

  const columns = [
    {
      key: 'entryNumber',
      header: 'رقم القيد',
      accessorKey: 'entryNumber' as const,
      align: 'left' as const,
      sortable: true,
    },
    {
      key: 'documentDate',
      header: 'التاريخ',
      accessorKey: 'documentDate' as const,
      align: 'left' as const,
      sortable: true,
    },
    {
      key: 'journalName',
      header: 'اليومية',
      accessorKey: 'journalName' as const,
      align: 'left' as const,
      cell: (row: MoveDto) => row.journalName ?? '—',
    },
    {
      key: 'isSystemGenerated',
      header: 'تلقائي',
      align: 'center' as const,
      cell: (row: MoveDto) =>
        row.isSystemGenerated ? <StatusBadge variant="draft">تلقائي</StatusBadge> : null,
    },
    {
      key: 'entryStatus',
      header: 'الحالة',
      align: 'center' as const,
      cell: (row: MoveDto) => (
        <StatusBadge variant={statusVariant(row.entryStatus)}>
          {statusLabel(row.entryStatus)}
        </StatusBadge>
      ),
    },
  ];

  return (
    <DataGrid<MoveDto>
      columns={columns}
      data={data}
      loading={loading}
      error={error}
      onRetry={onRetry}
      rowKey={(row) => row.id}
      onRowClick={(row) => navigate(`/accounting/journal-entries/${row.id}`)}
      pagination={pagination}
      totalItems={totalItems}
      onPageChange={onPageChange}
      emptyMessage="لا توجد قيود"
    />
  );
}
