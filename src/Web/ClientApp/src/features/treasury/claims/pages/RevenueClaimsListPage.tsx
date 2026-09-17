import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, FilterBar, FilterSelect, Badge, EmptyState, MoneyDisplay, Page } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { RevenueClaimsClient, ClaimStatus } from '@/web-api-client';
import { formatDate } from '@/shared/utils/formatters';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { partiesClient } from '@/features/parties/shared/client';
import { claimStatusLabels, claimStatusBadgeVariant } from '../../shared/types';
import type { PartyResponse } from '@/features/parties/shared/types';
import type { RevenueClaimDto } from '../../shared/types';

const client = new RevenueClaimsClient();

export default function RevenueClaimsListPage() {
  const navigate = useNavigate();
  const [partyId, setPartyId] = useState('');
  const [status, setStatus] = useState('');

  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });

  const { data: claims = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ['revenue-claims', 'list', partyId, status],
    queryFn: () =>
      client.revenueClaimsAll(
        partyId ? Number(partyId) : undefined,
        status ? (status as ClaimStatus) : undefined,
      ),
  });

  const columns: DataGridColumn<RevenueClaimDto>[] = useMemo(
    () => [
      {
        key: 'claimNumber',
        header: 'رقم المطالبة',
        render: (row) => <span className="font-medium tabular-nums">{row.claimNumber}</span>,
      },
      {
        key: 'claimDate',
        header: 'التاريخ',
        render: (row) => formatDate(row.claimDate),
      },
      { key: 'partyName', header: 'الجهة' },
      {
        key: 'totalAmount',
        header: 'المبلغ',
        render: (row) => <MoneyDisplay value={row.totalAmount ?? 0} />,
      },
      {
        key: 'collectedAmount',
        header: 'المحصّل',
        render: (row) => <MoneyDisplay value={row.collectedAmount ?? 0} />,
      },
      {
        key: 'availableAmount',
        header: 'المتاح للتحصيل',
        render: (row) => (
          <span className="tabular-nums text-[var(--color-success)]">
            <MoneyDisplay value={row.availableAmount ?? 0} />
          </span>
        ),
      },
      {
        key: 'status',
        header: 'الحالة',
        render: (row) => (
          <Badge variant={claimStatusBadgeVariant[row.status ?? 'Draft']}>
            {claimStatusLabels[row.status ?? 'Draft']}
          </Badge>
        ),
      },
      {
        key: 'actions',
        header: '',
        render: (row) => (
          <Button
            variant="ghost"
            size="icon"
            aria-label={`عرض المطالبة ${row.claimNumber}`}
            onClick={() => navigate(`/treasury/revenue-claims/${row.id}`)}
          >
            <Eye size={16} />
          </Button>
        ),
      },
    ],
    [navigate],
  );

  const hasFilters = !!partyId || !!status;
  const partyOptions: { value: string; label: string }[] = (parties as PartyResponse[]).map((p) => ({
    value: String(p.id),
    label: p.nameAr,
  }));

  return (
    <Page
      title="المطالبات الإيرادية"
      actions={
        <Button onClick={() => navigate('/treasury/revenue-claims/create')}>
          <Plus size={16} />
          مطالبة جديدة
        </Button>
      }
      toolbar={
        <FilterBar
          hasFilters={hasFilters}
          onClear={() => {
            setPartyId('');
            setStatus('');
          }}
        >
          <FilterSelect
            label="الجهة"
            value={partyId}
            onChange={setPartyId}
            options={[{ value: '', label: 'الكل' }, ...partyOptions]}
          />
          <FilterSelect
            label="الحالة"
            value={status}
            onChange={setStatus}
            options={[
              { value: '', label: 'الكل' },
              ...Object.entries(claimStatusLabels).map(([value, label]) => ({ value, label })),
            ]}
          />
        </FilterBar>
      }
      loading={isLoading}
    >
      {isError ? (
        <div className="p-4 text-center flex flex-col items-center gap-2">
          <p className="text-sm text-[var(--color-error)]">{getQueryErrorMessage(error)}</p>
          <Button variant="outline" size="sm" onClick={() => refetch()}>إعادة المحاولة</Button>
        </div>
      ) : claims.length === 0 ? (
        <EmptyState message="لا توجد مطالبات إيرادية" />
      ) : (
        <DataGrid columns={columns} data={claims} rowKey={(row) => row.id ?? 0} />
      )}
    </Page>
  );
}
