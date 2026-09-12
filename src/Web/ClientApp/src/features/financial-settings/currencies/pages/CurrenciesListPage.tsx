import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Switch, FilterBar, FilterSearch, ConfirmDialog, Badge, StatusBadge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCurrenciesList, useActivateCurrency, useDeactivateCurrency } from '../../hooks/useCurrencies';
import { getActiveStatusLabel } from '@/shared/constants/labels';

export default function CurrenciesListPage() {
  const navigate = useNavigate();
  const canCreate = usePermission(PERMISSIONS.Currencies.Create);
  const [search, setSearch] = useState('');
  const [confirmToggle, setConfirmToggle] = useState<{ id: number; rowVersion: string; isActive: boolean; code: string } | null>(null);

  const { data: items = [], isLoading } = useCurrenciesList();
  const activateMutation = useActivateCurrency();
  const deactivateMutation = useDeactivateCurrency();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.code.toLowerCase().includes(q) && !item.name.toLowerCase().includes(q)) return false;
      }
      return true;
    });
  }, [items, search]);

  function handleToggle(item: typeof items[0]) {
    setConfirmToggle({ id: item.id, rowVersion: item.rowVersion, isActive: item.isActive, code: item.code });
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    const mutation = confirmToggle.isActive ? deactivateMutation : activateMutation;
    mutation.mutate(
      { id: confirmToggle.id, rowVersion: confirmToggle.rowVersion },
      {
        onSuccess: () => {
          notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
          setConfirmToggle(null);
        },
        onError: (err) => notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ' }),
      },
    );
  }

  const columns: DataGridColumn<typeof items[0]>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono font-medium">{row.code}</span> },
    { header: 'الاسم', cell: (row) => row.name },
    { header: 'الرمز', cell: (row) => row.symbol },
    { header: 'الكسور', align: 'start', cell: (row) => row.decimalPlaces },
    { header: 'العملة الأساسية', cell: (row) => row.isBase && <Badge variant="primary">أساسية</Badge> },
    {
      header: 'الحالة',
      cell: (row) => canCreate
        ? <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={getActiveStatusLabel(row.isActive)} />
        : <StatusBadge variant={row.isActive ? 'active' : 'closed'}>{getActiveStatusLabel(row.isActive)}</StatusBadge>,
    },
    {
      header: 'إجراءات',
      cell: (row) => <Button variant="ghost" size="icon" onClick={() => navigate(`/financial-settings/currencies/${row.id}`)} aria-label="عرض" className="cursor-pointer"><Eye size={16} /></Button>,
    },
  ];

  return (
    <Page
      title="العملات"
      description="إدارة العملات ودعمها"
      actions={
        canCreate && (
          <Button onClick={() => navigate('/financial-settings/currencies/new')} icon={<Plus size={16} />} className="cursor-pointer shadow-sm hover:shadow transition-shadow">
            عملة جديدة
          </Button>
        )
      }
      toolbar={
        <FilterBar hasFilters={!!search} onClear={() => setSearch('')}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد عملات بعد"
        rowKey={(row) => row.id}
      />

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        title={confirmToggle?.isActive ? 'تعطيل العملة' : 'تنشيط العملة'}
        message={confirmToggle?.isActive ? `هل تريد تعطيل العملة ${confirmToggle?.code}؟` : `هل تريد تنشيط العملة ${confirmToggle?.code}؟`}
        loading={activateMutation.isPending || deactivateMutation.isPending}
      />
    </Page>
  );
}
