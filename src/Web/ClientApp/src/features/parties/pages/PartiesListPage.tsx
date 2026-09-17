import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePartiesList, useTogglePartyActive } from '../hooks/useParties';
import { PartyType, PARTY_TYPE_LABELS, type PartyFilters, type PartyResponse } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, StatusBadge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus } from 'lucide-react';
import { activeStatusLabels } from '@/shared/constants/labels';

const partyTypeOptions = Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({
  value: Number(value) as PartyType,
  label,
}));

export default function PartiesListPage() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<PartyFilters>({});
  const { data: parties, isLoading, isError, refetch } = usePartiesList(filters);
  const toggleActive = useTogglePartyActive(0);
  const { hasPermission: canView } = usePermission('Parties.View');
  const { hasPermission: canCreate } = usePermission('Parties.Create');
  const { hasPermission: canUpdate } = usePermission('Parties.Update');

  if (!canView) {
    return (
      <div className="p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">غير مصرح بالوصول</p>
      </div>
    );
  }

  function handleSearch(value: string) {
    setFilters((prev) => ({ ...prev, search: value || undefined }));
  }

  function handleTypeFilter(partyType: PartyType | undefined) {
    setFilters((prev) => ({ ...prev, partyType }));
  }

  function handleActiveFilter(isActive: boolean | undefined) {
    setFilters((prev) => ({ ...prev, isActive }));
  }

  async function handleToggleActive(id: number) {
    await toggleActive.mutateAsync(id);
  }

  const hasActiveFilters = !!filters.search || filters.partyType !== undefined || filters.isActive !== undefined;

  const columns: DataGridColumn<PartyResponse>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono">{row.partyCode}</span> },
    { header: 'الاسم', cell: (row) => row.nameAr },
    { header: 'النوع', cell: (row) => PARTY_TYPE_LABELS[row.partyType] },
    { header: 'الرقم الضريبي', cell: (row) => <span className="font-mono" dir="ltr">{row.taxNumber ?? '—'}</span> },
    { header: 'الحالة', cell: (row) => <StatusBadge variant={row.isActive ? 'active' : 'inactive'}>{row.isActive ? 'نشط' : 'غير نشط'}</StatusBadge> },
    {
      header: 'إجراءات',
      cell: (row) => canUpdate
        ? <Button variant="ghost" size="sm" onClick={() => handleToggleActive(row.id)}>{row.isActive ? 'تعطيل' : 'تفعيل'}</Button>
        : null,
    },
  ];

  return (
    <Page
      title="الأطراف"
      error={isError ? 'حدث خطأ أثناء تحميل الأطراف' : undefined}
      onRetry={isError ? () => refetch() : undefined}
      actions={
        canCreate ? (
          <Button variant="primary" size="sm" onClick={() => navigate('/parties/create')}>
            <Plus size={16} className="ms-1" />
            طرف جديد
          </Button>
        ) : undefined
      }
      toolbar={
        <FilterBar
          hasFilters={hasActiveFilters}
          onClear={() => setFilters({})}
        >
          <FilterSearch
            value={filters.search ?? ''}
            onChange={handleSearch}
            placeholder="بحث بالاسم أو الرقم الضريبي"
          />
          <FilterSelect
            label="النوع"
            value={filters.partyType !== undefined ? String(filters.partyType) : ''}
            onChange={(val) => handleTypeFilter(val ? Number(val) as PartyType : undefined)}
            options={[
              { value: '', label: 'الكل' },
              ...partyTypeOptions.map((opt) => ({ value: String(opt.value), label: opt.label })),
            ]}
          />
          <FilterSelect
            label="الحالة"
            value={filters.isActive === undefined ? '' : String(filters.isActive)}
            onChange={(val) => handleActiveFilter(val === '' ? undefined : val === 'true')}
            options={[
              { value: '', label: 'الكل' },
              { value: 'true', label: activeStatusLabels.active },
              { value: 'false', label: activeStatusLabels.inactive },
            ]}
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={parties ?? []}
        loading={isLoading}
        emptyMessage={hasActiveFilters ? 'لا توجد نتائج مطابقة لمعايير البحث' : 'لا توجد أطراف بعد'}
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/parties/${row.id}`)}
      />
    </Page>
  );
}
