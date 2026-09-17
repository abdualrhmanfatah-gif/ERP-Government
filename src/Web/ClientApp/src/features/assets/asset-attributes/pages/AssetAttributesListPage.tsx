import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Page,
  Button,
  Badge,
  FilterBar,
  FilterSearch,
  FilterSelect,
  ConfirmDialog,
  DataGrid,
  StatusBadge,
} from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { useAssetAttributesList, useUpdateAssetAttribute } from '../hooks/useAssetAttributes';
import { dataTypeLabels, dataTypeOptions, type AssetAttributeDefinition } from '../shared/types';
import { getActiveBadge } from '../../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { activeStatusLabels } from '@/shared/constants/labels';

const isActiveFilterOptions = [
  { value: 'true', label: activeStatusLabels.active },
  { value: 'false', label: activeStatusLabels.inactive },
];

export default function AssetAttributesListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [dataTypeFilter, setDataTypeFilter] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [page, setPage] = useState(0);
  const [pageSize] = useState(20);
  const [toggleTarget, setToggleTarget] = useState<AssetAttributeDefinition | null>(null);

  const params = useMemo(() => ({
    search: search || undefined,
    dataType: dataTypeFilter || undefined,
    isActive: isActiveFilter ? isActiveFilter === 'true' : undefined,
    page: page + 1,
    pageSize,
  }), [search, dataTypeFilter, isActiveFilter, page, pageSize]);

  const { data, isLoading, error, refetch } = useAssetAttributesList(params);
  const updateMutation = useUpdateAssetAttribute();
  const definitions = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

  const hasFilters = !!search || !!dataTypeFilter || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setDataTypeFilter('');
    setIsActiveFilter('');
    setPage(0);
  }

  async function confirmToggle() {
    if (!toggleTarget) return;
    const target = toggleTarget;
    setToggleTarget(null);
    try {
      await updateMutation.mutateAsync({
        id: target.id,
        data: {
          name: target.name,
          description: target.description || undefined,
          unit: target.unit || undefined,
          sortOrder: target.sortOrder,
          attributeDataType: target.attributeDataType,
          isActive: !target.isActive,
          rowVersion: target.rowVersion,
        },
      });
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  const columns: DataGridColumn<AssetAttributeDefinition>[] = [
    {
      header: 'الكود',
      cell: (row) => <span className="font-mono font-medium whitespace-nowrap" dir="ltr">{row.code}</span>,
    },
    {
      header: 'الاسم',
      cell: (row) => <span className="max-w-[240px] truncate">{row.name}</span>,
    },
    {
      header: 'النوع',
      cell: (row) => <Badge variant="outline">{dataTypeLabels[row.attributeDataType] ?? row.attributeDataType}</Badge>,
    },
    {
      header: 'الوحدة',
      cell: (row) => row.unit ?? '—',
    },
    {
      header: 'الحالة',
      cell: (row) => {
        const badge = getActiveBadge(row.isActive);
        return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
      },
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <div className="flex justify-end gap-1">
          {!row.isActive ? (
            <Button
              variant="ghost"
              size="sm"
              onClick={(e) => { e.stopPropagation(); setToggleTarget(row); }}
            >
              تفعيل
            </Button>
          ) : (
            <Button
              variant="ghost"
              size="sm"
              className="text-[var(--color-error)]"
              onClick={(e) => { e.stopPropagation(); setToggleTarget(row); }}
            >
              تعطيل
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            aria-label="تعديل المواصفة"
            onClick={(e) => { e.stopPropagation(); navigate(`/assets/attributes/${row.id}/edit`); }}
          >
            <Pencil size={16} />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <Page
      title="تعريفات المواصفات"
      description="إدارة تعريفات مواصفات الأصول وربطها بمجموعات الأصول"
      actions={
        <Button
          variant="primary"
          size="sm"
          icon={<Plus size={16} />}
          onClick={() => navigate('/assets/attributes/create')}
        >
          إضافة تعريف مواصفة
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch
            value={search}
            onChange={(value) => { setSearch(value); setPage(0); }}
            placeholder="بحث بالكود أو الاسم..."
            className="flex-1 min-w-48"
          />
          <FilterSelect
            label="النوع"
            value={dataTypeFilter}
            onChange={(value) => { setDataTypeFilter(value); setPage(0); }}
            options={dataTypeOptions}
          />
          <FilterSelect
            label="الحالة"
            value={isActiveFilter}
            onChange={(value) => { setIsActiveFilter(value); setPage(0); }}
            options={isActiveFilterOptions}
          />
        </FilterBar>
      }
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
      onRetry={error ? () => refetch() : undefined}
    >
      <DataGrid<AssetAttributeDefinition>
        columns={columns}
        data={definitions}
        loading={isLoading}
        emptyMessage="لا توجد تعريفات مواصفات بعد"
        rowKey={(row) => row.id}
        onRowClick={(row) => navigate(`/assets/attributes/${row.id}/edit`)}
        pagination={{ pageIndex: page, pageSize }}
        totalItems={totalCount}
        onPageChange={(p) => setPage(p)}
      />

      <ConfirmDialog
        open={!!toggleTarget}
        onClose={() => setToggleTarget(null)}
        title={toggleTarget?.isActive ? 'تعطيل المواصفة' : 'تفعيل المواصفة'}
        message={
          toggleTarget?.isActive
            ? 'هل أنت متأكد من تعطيل هذه المواصفة؟ لن تظهر في قوائم الربط بالمجموعات الجديدة.'
            : 'هل أنت متأكد من تفعيل هذه المواصفة؟'
        }
        confirmLabel={toggleTarget?.isActive ? 'تعطيل' : 'تفعيل'}
        onConfirm={confirmToggle}
        destructive={!!toggleTarget?.isActive}
        loading={updateMutation.isPending}
      />
    </Page>
  );
}
