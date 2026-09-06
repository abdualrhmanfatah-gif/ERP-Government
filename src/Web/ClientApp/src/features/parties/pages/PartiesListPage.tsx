import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePartiesList, useTogglePartyActive } from '../hooks/useParties';
import { PartyType, PARTY_TYPE_LABELS, type PartyFilters } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Button, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Search } from 'lucide-react';

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

  const [search, setSearch] = useState('');

  if (!canView) {
    return (
      <div className="p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">غير مصرح بالوصول</p>
      </div>
    );
  }

  function handleSearch() {
    setFilters((prev) => ({ ...prev, search: search || undefined }));
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

  if (isError) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">الأطراف</h1>
        </div>
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6 text-center">
          <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل الأطراف</p>
          <Button variant="ghost" size="sm" onClick={() => refetch()} className="mt-2">
            إعادة المحاولة
          </Button>
        </div>
      </div>
    );
  }

  const columns: DataGridColumn<any>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono cursor-pointer" onClick={() => navigate(`/parties/${row.id}`)}>{row.partyCode}</span> },
    { header: 'الاسم', cell: (row) => <span className="cursor-pointer" onClick={() => navigate(`/parties/${row.id}`)}>{row.nameAr}</span> },
    { header: 'النوع', cell: (row) => PARTY_TYPE_LABELS[row.partyType] },
    { header: 'الرقم الضريبي', cell: (row) => <span className="font-mono">{row.taxNumber ?? '—'}</span> },
    { header: 'الحالة', cell: (row) => <Badge variant={row.isActive ? 'success' : 'default'}>{row.isActive ? 'نشط' : 'غير نشط'}</Badge> },
    {
      header: 'إجراءات',
      cell: (row) => canUpdate
        ? <Button variant="ghost" size="sm" onClick={() => handleToggleActive(row.id)}>{row.isActive ? 'تعطيل' : 'تفعيل'}</Button>
        : null,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">الأطراف</h1>
        {canCreate && (
          <Button variant="primary" size="sm" onClick={() => navigate('/parties/create')}>
            <Plus size={16} className="ms-1" />
            طرف جديد
          </Button>
        )}
      </div>

      <div className="flex gap-3 items-center">
        <div className="relative flex-1 max-w-md">
          <Search size={16} className="absolute end-3 top-1/2 -translate-y-1/2 text-[var(--color-on-surface-variant)]" />
          <input
            type="text"
            placeholder="بحث بالاسم أو الرقم الضريبي"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className="w-full rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 pe-9 text-sm"
          />
        </div>
        <select
          value={filters.partyType ?? ''}
          onChange={(e) => handleTypeFilter(e.target.value ? Number(e.target.value) as PartyType : undefined)}
          className="rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
        >
          <option value="">كل الأنواع</option>
          {partyTypeOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
        <select
          value={filters.isActive === undefined ? '' : String(filters.isActive)}
          onChange={(e) => handleActiveFilter(e.target.value === '' ? undefined : e.target.value === 'true')}
          className="rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-3 py-2 text-sm"
        >
          <option value="">الكل</option>
          <option value="true">نشط</option>
          <option value="false">غير نشط</option>
        </select>
      </div>

      <DataGrid
        columns={columns}
        data={parties ?? []}
        loading={isLoading}
        emptyMessage="لا توجد أطراف بعد"
        rowKey={(row) => row.id}
      />
    </div>
  );
}
