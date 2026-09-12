import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePartiesList, useTogglePartyActive } from '../hooks/useParties';
import { PartyType, PARTY_TYPE_LABELS, type PartyFilters } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Badge, Card, Input, Select } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Search } from 'lucide-react';
import { activeStatusLabels, getActiveStatusLabel } from '@/shared/constants/labels';

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
      <Page title="الأطراف" error="حدث خطأ أثناء تحميل الأطراف" onRetry={() => refetch()}>
        <Card className="bg-[var(--color-surface-container-lowest)] text-center">
          <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل الأطراف</p>
          <Button variant="ghost" size="sm" onClick={() => refetch()} className="mt-2">
            إعادة المحاولة
          </Button>
        </Card>
      </Page>
    );
  }

  const columns: DataGridColumn<any>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono cursor-pointer" onClick={() => navigate(`/parties/${row.id}`)}>{row.partyCode}</span> },
    { header: 'الاسم', cell: (row) => <span className="cursor-pointer" onClick={() => navigate(`/parties/${row.id}`)}>{row.nameAr}</span> },
    { header: 'النوع', cell: (row) => PARTY_TYPE_LABELS[row.partyType] },
    { header: 'الرقم الضريبي', cell: (row) => <span className="font-mono">{row.taxNumber ?? '—'}</span> },
    { header: 'الحالة', cell: (row) => <Badge variant={row.isActive ? 'success' : 'default'}>{getActiveStatusLabel(row.isActive)}</Badge> },
    {
      header: 'إجراءات',
      cell: (row) => canUpdate
        ? <Button variant="ghost" size="sm" onClick={() => handleToggleActive(row.id)}>{row.isActive ? 'تعطيل' : 'تفعيل'}</Button>
        : null,
    },
  ];

  return (
    <Page
      title="الموردون"
      actions={
        canCreate ? (
          <Button variant="primary" size="sm" onClick={() => navigate('/parties/create')}>
            <Plus size={16} className="ms-1" />
            مورد جديد
          </Button>
        ) : undefined
      }
      toolbar={
        <div className="flex gap-3 items-center">
          <div className="relative flex-1 max-w-md">
            <Search size={16} className="absolute end-3 top-1/2 -translate-y-1/2 text-[var(--color-on-surface-variant)]" />
            <Input
              type="text"
              placeholder="بحث بالاسم أو الرقم الضريبي"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              className="pe-9"
            />
          </div>
          <Select
            label="النوع"
            value={filters.partyType ?? ''}
            onChange={(e) => handleTypeFilter(e.target.value ? Number(e.target.value) as PartyType : undefined)}
            options={[
              { value: '', label: 'كل الأنواع' },
              ...partyTypeOptions.map((opt) => ({ value: String(opt.value), label: opt.label })),
            ]}
            className="w-auto"
          />
          <Select
            label="الحالة"
            value={filters.isActive === undefined ? '' : String(filters.isActive)}
            onChange={(e) => handleActiveFilter(e.target.value === '' ? undefined : e.target.value === 'true')}
            options={[
              { value: '', label: 'الكل' },
              { value: 'true', label: activeStatusLabels.active },
              { value: 'false', label: activeStatusLabels.inactive },
            ]}
            className="w-auto"
          />
        </div>
      }
    >
      <DataGrid
        columns={columns}
        data={parties ?? []}
        loading={isLoading}
        emptyMessage="لا توجد أطراف بعد"
        rowKey={(row) => row.id}
      />
    </Page>
  );
}
