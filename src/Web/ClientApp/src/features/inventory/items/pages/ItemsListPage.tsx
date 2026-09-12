import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, Badge, FilterBar, FilterSearch, FilterSelect, DataGrid } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { useItemsList } from '../hooks/useItems';
import { itemTypeLabels, type ItemType } from '../shared/types';

const itemTypeFilterOptions = Object.entries(itemTypeLabels).map(([value, label]) => ({ value, label }));

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

const underReorderOptions = [
  { value: 'true', label: 'تحت مستوى إعادة الطلب' },
];

export default function ItemsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [unitFilter, setUnitFilter] = useState('');
  const [itemTypeFilter, setItemTypeFilter] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [underReorderFilter, setUnderReorderFilter] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize] = useState(20);

  const params = useMemo(() => ({
    search: search || undefined,
    categoryId: categoryFilter ? Number(categoryFilter) : undefined,
    unitId: unitFilter ? Number(unitFilter) : undefined,
    itemType: itemTypeFilter || undefined,
    isActive: isActiveFilter ? isActiveFilter === 'true' : undefined,
    underReorderLevel: underReorderFilter ? underReorderFilter === 'true' : undefined,
    page: page + 1,
    pageSize,
  }), [search, categoryFilter, unitFilter, itemTypeFilter, isActiveFilter, underReorderFilter, page, pageSize]);

  const { data, isLoading, error, refetch } = useItemsList(params);
  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

  const hasFilters = !!search || !!categoryFilter || !!unitFilter || !!itemTypeFilter || !!isActiveFilter || !!underReorderFilter;

  function handleClearFilters() {
    setSearch('');
    setCategoryFilter('');
    setUnitFilter('');
    setItemTypeFilter('');
    setIsActiveFilter('');
    setUnderReorderFilter('');
    setPage(0);
  }

  const columns: DataGridColumn<typeof items[number]>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono font-medium whitespace-nowrap" dir="ltr">{row.code}</span> },
    { header: 'الاسم', cell: (row) => <span className="max-w-[200px] truncate">{row.name}</span> },
    { header: 'التصنيف', cell: (row) => row.categoryName ?? '—' },
    { header: 'الوحدة', cell: (row) => row.unitName },
    { header: 'الباركود', cell: (row) => <span dir="ltr">{row.barcode ?? '—'}</span> },
    { header: 'النوع', cell: (row) => <Badge variant="outline">{itemTypeLabels[row.itemType as ItemType] ?? row.itemType}</Badge> },
    { header: 'الكمية المتاحة', cell: (row) => <span className="tabular-nums">{row.availableQuantity ?? '—'}</span>, align: 'right' },
    { header: 'الكمية المحجوزة', cell: (row) => <span className="tabular-nums">{row.reservedQuantity ?? '—'}</span>, align: 'right' },
    { header: 'متوسط التكلفة', cell: (row) => <span className="tabular-nums">{row.averageCost != null ? Number(row.averageCost).toFixed(2) : '—'}</span>, align: 'right' },
    { header: 'الحد الأدنى', cell: (row) => <span className="tabular-nums">{row.minimumStock ?? '—'}</span>, align: 'right' },
    { header: 'الحد الأقصى', cell: (row) => <span className="tabular-nums">{row.maximumStock ?? '—'}</span>, align: 'right' },
    { header: 'إعادة الطلب', cell: (row) => <span className="tabular-nums">{row.reorderLevel ?? '—'}</span>, align: 'right' },
    {
      header: 'الحالة',
      cell: (row) => (
        <Badge variant={row.isActive ? 'success' : 'danger'}>
          {row.isActive ? 'نشط' : 'غير نشط'}
        </Badge>
      ),
    },
    {
      header: 'عرض',
      cell: (row) => (
        <Button variant="ghost" size="icon" onClick={() => navigate(`/inventory/items/${row.id}`)} aria-label="عرض الصنف">
          <Eye size={16} />
        </Button>
      ),
    },
  ];

  return (
    <Page
      title="الأصناف"
      description="إدارة أصناف المخزون"
      actions={
        <Button onClick={() => navigate('/inventory/items/create')} icon={<Plus size={16} />}>
          صنف جديد
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
          <FilterSelect value={categoryFilter} onChange={setCategoryFilter} options={[]} placeholder="التصنيف" label="التصنيف" />
          <FilterSelect value={unitFilter} onChange={setUnitFilter} options={[]} placeholder="الوحدة" label="الوحدة" />
          <FilterSelect value={itemTypeFilter} onChange={setItemTypeFilter} options={itemTypeFilterOptions} placeholder="نوع الصنف" label="نوع الصنف" />
          <FilterSelect value={isActiveFilter} onChange={setIsActiveFilter} options={isActiveOptions} placeholder="الحالة" label="الحالة" />
          <FilterSelect value={underReorderFilter} onChange={setUnderReorderFilter} options={underReorderOptions} placeholder="إعادة الطلب" label="إعادة الطلب" />
        </FilterBar>
      }
      loading={isLoading}
      error={error ? 'خطأ في تحميل الأصناف' : undefined}
      onRetry={error ? () => refetch() : undefined}
    >
      <DataGrid
        columns={columns}
        data={items}
        loading={isLoading}
        emptyMessage="لا توجد أصناف بعد"
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/inventory/items/${row.id}`)}
        pagination={{ pageIndex: page, pageSize }}
        totalItems={totalCount}
        onPageChange={(p) => setPage(p)}
      />
    </Page>
  );
}
