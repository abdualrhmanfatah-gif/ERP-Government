import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Badge, FilterBar, FilterSearch, FilterSelect, Switch, ConfirmDialog } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useExchangeRatesList, useActivateExchangeRate, useDeactivateExchangeRate } from '../../hooks/useExchangeRates';
import { exchangeRateTypeLabels, ExchangeRateType } from '../../shared/types';
import { getActiveStatusLabel } from '@/shared/constants/labels';

const rateTypeOptions = Object.entries(exchangeRateTypeLabels).map(([value, label]) => ({ value, label }));

export default function ExchangeRatesListPage() {
  const navigate = useNavigate();
  const canCreate = usePermission(PERMISSIONS.ExchangeRates.Create);
  const [search, setSearch] = useState('');
  const [rateTypeFilter, setRateTypeFilter] = useState('');
  const [confirmToggle, setConfirmToggle] = useState<{ id: number; rowVersion: string; isActive: boolean } | null>(null);

  const { data: items = [], isLoading } = useExchangeRatesList();
  const activateMutation = useActivateExchangeRate();
  const deactivateMutation = useDeactivateExchangeRate();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.baseCurrencyCode.toLowerCase().includes(q) && !item.currencyCode.toLowerCase().includes(q)) return false;
      }
      if (rateTypeFilter && item.rateType !== rateTypeFilter) return false;
      return true;
    });
  }, [items, search, rateTypeFilter]);

  function handleToggle(item: typeof items[0]) {
    setConfirmToggle({ id: item.id, rowVersion: item.rowVersion, isActive: item.isActive });
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    const mutation = confirmToggle.isActive ? deactivateMutation : activateMutation;
    mutation.mutate(
      { id: confirmToggle.id, rowVersion: confirmToggle.rowVersion },
      {
        onSuccess: () => {
          notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل' : 'تم التنشيط' });
          setConfirmToggle(null);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ' }),
      },
    );
  }

  const columns: DataGridColumn<typeof items[0]>[] = [
    { header: 'العملة الأساسية', cell: (row) => <span className="font-mono">{row.baseCurrencyCode}</span> },
    { header: 'العملة', cell: (row) => <span className="font-mono">{row.currencyCode}</span> },
    { header: 'التاريخ', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-YE', { dateStyle: 'medium' }).format(new Date(row.rateDate))}</span> },
    { header: 'النوع', cell: (row) => exchangeRateTypeLabels[ExchangeRateType[row.rateType as keyof typeof ExchangeRateType] as ExchangeRateType] ?? row.rateType },
    { header: 'السعر', align: 'left', cell: (row) => <span className="font-mono">{row.rate.toLocaleString('ar-YE', { minimumFractionDigits: 2, maximumFractionDigits: 6 })}</span> },
    {
      header: 'الحالة',
      cell: (row) => canCreate
        ? <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={getActiveStatusLabel(row.isActive)} />
        : <Badge variant={row.isActive ? 'success' : 'danger'}>{getActiveStatusLabel(row.isActive)}</Badge>,
    },
    {
      header: 'إجراءات',
      cell: (row) => <Button variant="ghost" size="icon" onClick={() => navigate(`/financial-settings/exchange-rates/${row.id}`)} aria-label="عرض" className="cursor-pointer"><Eye size={16} /></Button>,
    },
  ];

  return (
    <Page
      title="أسعار الصرف"
      description="إدارة أسعار الصرف بين العملات"
      actions={
        canCreate && (
          <Button onClick={() => navigate('/financial-settings/exchange-rates/new')} icon={<Plus size={16} />} className="cursor-pointer shadow-sm hover:shadow transition-shadow">
            سعر صرف جديد
          </Button>
        )
      }
      toolbar={
        <FilterBar hasFilters={!!search || !!rateTypeFilter} onClear={() => { setSearch(''); setRateTypeFilter(''); }}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالعملة..." />
          <FilterSelect value={rateTypeFilter} onChange={setRateTypeFilter} options={rateTypeOptions} placeholder="نوع السعر" label="نوع السعر" />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد أسعار صرف بعد"
        rowKey={(row) => row.id}
      />

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        title={confirmToggle?.isActive ? 'تعطيل سعر الصرف' : 'تنشيط سعر الصرف'}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا السعر؟' : 'هل تريد تنشيط هذا السعر؟'}
        loading={activateMutation.isPending || deactivateMutation.isPending}
      />
    </Page>
  );
}
