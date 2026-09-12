import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, FilterBar, FilterSearch, FilterSelect } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { quotationStatusLabels, type Quotation, type QuotationStatus } from '../shared/types';
import { useQuotationsList } from '../hooks/useQuotations';

const statusOptions = Object.entries(quotationStatusLabels).map(([value, label]) => ({ value, label }));

export default function QuotationsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  const { data: response, isLoading } = useQuotationsList({
    search: search || undefined,
    status: statusFilter || undefined,
  });

  const items = response?.items ?? [];

  const columns: DataGridColumn<Quotation>[] = useMemo(() => [
    { key: 'quotationNumber', header: 'رقم العرض', width: '150px', render: (v) => <span dir="ltr">{v}</span> },
    { key: 'supplierPartyId', header: 'المورد', width: '100px' },
    { key: 'quotationDate', header: 'التاريخ', width: '120px', render: (v) => new Date(v).toLocaleDateString('ar-YE') },
    { key: 'validUntil', header: 'صالح حتى', width: '120px', render: (v) => v ? new Date(v).toLocaleDateString('ar-YE') : '-' },
    { key: 'status', header: 'الحالة', width: '130px', render: (v) => quotationStatusLabels[v as QuotationStatus] },
    { key: 'grandTotal', header: 'الإجمالي', width: '120px', render: (v) => v != null ? <span className="tabular-nums">{v.toLocaleString('ar-YE')}</span> : '-' },
    {
      key: 'actions',
      header: 'إجراءات',
      width: '80px',
      render: (_, row) => (
        <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/quotations/${row.id}`)}>
          <Eye className="h-4 w-4" />
        </Button>
      ),
    },
  ], [navigate]);

  function handleClearFilters() {
    setSearch('');
    setStatusFilter('');
  }

  const hasFilters = !!search || !!statusFilter;

  return (
    <Page title="عروض الأسعار" actions={<Button onClick={() => navigate('/procurement/quotations/create')}><Plus className="h-4 w-4 ms-1" />عرض جديد</Button>}>
      <FilterBar onClear={handleClearFilters} hasFilters={hasFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم العرض..." />
        <FilterSelect value={statusFilter} onChange={setStatusFilter} options={statusOptions} placeholder="الحالة" />
      </FilterBar>

      <div className="mt-4">
        <DataGrid
          data={items}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="لا توجد عروض أسعار"
        />
      </div>
    </Page>
  );
}
