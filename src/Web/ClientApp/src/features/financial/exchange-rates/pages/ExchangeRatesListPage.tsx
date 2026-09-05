import { useState } from 'react';
import { Plus, Pencil, Power, ChevronDown, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';
import { useQuery } from '@tanstack/react-query';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { FilterBar, FilterSelect, FilterDate, FilterToggle } from '@/components/ui';
import { ExchangeRateForm } from '../components/ExchangeRateForm';
import { ExchangeRateHistoryRow } from '../components/ExchangeRateHistoryRow';
import { useExchangeRatesList, type ExchangeRateFilters as Filters } from '../hooks/useExchangeRatesList';
import { useCreateExchangeRate } from '../hooks/useCreateExchangeRate';
import { useUpdateExchangeRate } from '../hooks/useUpdateExchangeRate';
import { useActivateExchangeRate } from '../hooks/useActivateExchangeRate';
import { useDeactivateExchangeRate } from '../hooks/useDeactivateExchangeRate';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { CurrenciesClient } from '../../../../web-api-client';
import { authFetchFn } from '../../../../shared/utils/auth-fetch';
import type { ExchangeRateDto } from '../../../../web-api-client';

const currenciesClient = new CurrenciesClient('', authFetchFn);

function useCurrencies() {
  return useQuery({
    queryKey: ['currencies'],
    queryFn: () => currenciesClient.currenciesAll(undefined),
  });
}


export function ExchangeRatesListPage() {
  const [filters, setFilters] = useState<Filters>({});
  const { data: rates = [], isLoading, error, refetch } = useExchangeRatesList(filters);
  const { data: currencies = [] } = useCurrencies();

  const { hasPermission: canCreate } = usePermission(PERMISSIONS.ExchangeRates.Create);
  const { hasPermission: canUpdate } = usePermission(PERMISSIONS.ExchangeRates.Update);
  const { hasPermission: canActivate } = usePermission(PERMISSIONS.ExchangeRates.Activate);
  const { hasPermission: canDeactivate } = usePermission(PERMISSIONS.ExchangeRates.Deactivate);

  const currencyNameMap = new Map(currencies.map((c: { id?: number; name?: string; code?: string }) => [c.id, c.name ?? c.code ?? '']));
  const getCurrencyName = (id?: number) => id != null ? currencyNameMap.get(id) ?? '—' : '—';
  const createMutation = useCreateExchangeRate();
  const updateMutation = useUpdateExchangeRate();
  const activateMutation = useActivateExchangeRate();
  const deactivateMutation = useDeactivateExchangeRate();

  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [editingRate, setEditingRate] = useState<ExchangeRateDto | null>(null);
  const [confirmAction, setConfirmAction] = useState<{ type: 'activate' | 'deactivate'; rate: ExchangeRateDto } | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);

  const formatDate = (d: Date | string | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  const formatRateType = (t: string | number | undefined) => {
    if (t === 'official' || t === 'Official' || t === 0) return 'رسمي';
    return 'سوق';
  };

  const handleCreate = async (data: Record<string, unknown>) => {
    try {
      await createMutation.mutateAsync(data as never);
      setShowCreateDialog(false);
      toast.success('تم إنشاء سعر الصرف بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء سعر الصرف';
      toast.error(message);
    }
  };

  const handleEdit = async (data: Record<string, unknown>) => {
    if (!editingRate) return;
    try {
      await updateMutation.mutateAsync({
        id: editingRate.id!,
        data: { ...data, id: editingRate.id, rowVersion: editingRate.rowVersion } as never,
      });
      setEditingRate(null);
      toast.success('تم تحديث سعر الصرف بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تحديث سعر الصرف';
      toast.error(message);
    }
  };

  const handleConfirmAction = async () => {
    if (!confirmAction) return;
    const { type, rate } = confirmAction;
    try {
      if (type === 'activate') {
        await activateMutation.mutateAsync({ id: rate.id!, rowVersion: rate.rowVersion });
        toast.success('تم تفعيل سعر الصرف بنجاح');
      } else {
        await deactivateMutation.mutateAsync({ id: rate.id!, rowVersion: rate.rowVersion });
        toast.success('تم تعطيل سعر الصرف بنجاح');
      }
      setConfirmAction(null);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تنفيذ الإجراء';
      toast.error(message);
    }
  };

  const hasFilters = 
    filters.rateType !== undefined || 
    filters.currencyId !== undefined || 
    filters.fromDate !== undefined || 
    filters.toDate !== undefined || 
    filters.isActive !== undefined;

  return (
    <div>
      <PageHeader
        title="أسعار الصرف"
        description="إدارة أسعار الصرف بين العملات"
        actions={
          canCreate ? (
            <Button
              variant="primary"
              size="sm"
              icon={<Plus size={16} />}
              onClick={() => setShowCreateDialog(true)}
            >
              سعر صرف جديد
            </Button>
          ) : undefined
        }
      />
      <FilterBar hasFilters={hasFilters} onClear={() => setFilters({})}>
        <FilterSelect
          label="نوع السعر"
          value={filters.rateType !== undefined ? String(filters.rateType) : ''}
          onChange={(v) => setFilters({ ...filters, rateType: v === '' ? undefined : Number(v) })}
          options={[
            { value: '0', label: 'رسمي' },
            { value: '1', label: 'سوق' },
          ]}
        />
        <FilterSelect
          label="العملة"
          value={filters.currencyId !== undefined ? String(filters.currencyId) : ''}
          onChange={(v) => setFilters({ ...filters, currencyId: v === '' ? undefined : Number(v) })}
          options={currencies.map((c) => ({
            value: String(c.id),
            label: c.code ?? '',
          }))}
        />
        <FilterDate
          label="من تاريخ"
          value={filters.fromDate ? new Date(filters.fromDate).toISOString().split('T')[0] : ''}
          onChange={(v) => setFilters({ ...filters, fromDate: v ? new Date(v) : undefined })}
          max={filters.toDate ? new Date(filters.toDate).toISOString().split('T')[0] : undefined}
        />
        <FilterDate
          label="إلى تاريخ"
          value={filters.toDate ? new Date(filters.toDate).toISOString().split('T')[0] : ''}
          onChange={(v) => setFilters({ ...filters, toDate: v ? new Date(v) : undefined })}
          min={filters.fromDate ? new Date(filters.fromDate).toISOString().split('T')[0] : undefined}
        />
        <FilterToggle
          label="النشطة فقط"
          checked={filters.isActive === true}
          onChange={(checked) => setFilters({ ...filters, isActive: checked ? true : undefined })}
        />
      </FilterBar>
      <DataGrid<ExchangeRateDto>
        columns={[
          {
            key: '_expand',
            header: '',
            width: 40,
            align: 'center',
            cell: (row) => {
              const isExpanded = expandedId === row.id;
              return (
                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    setExpandedId(isExpanded ? null : row.id ?? null);
                  }}
                  aria-label={isExpanded ? 'طي' : 'توسيع'}
                  aria-expanded={isExpanded}
                  className="p-2 min-w-9 min-h-9 hover:bg-[var(--color-surface-container-low)] rounded-lg"
                >
                  {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </button>
              );
            },
          },
          { key: 'baseCurrencyCode', header: 'العملة الأساسية', width: 150, cell: (row) => getCurrencyName(row.baseCurrencyId) },
          { key: 'currencyCode', header: 'العملة الهدف', width: 150, cell: (row) => getCurrencyName(row.currencyId) },
          {
            key: 'rateDate',
            header: 'التاريخ',
            width: 120,
            cell: (row) => formatDate(row.rateDate),
          },
          {
            key: 'rateType',
            header: 'النوع',
            width: 100,
            cell: (row) => formatRateType(row.rateType),
          },
          {
            key: 'rate',
            header: 'السعر',
            width: 120,
            cell: (row) => <span className="tabular-nums">{row.rate?.toLocaleString('ar-EG') ?? '—'}</span>,
          },
          {
            key: 'isActive',
            header: 'الحالة',
            width: 90,
            cell: (row) => (
              <StatusBadge variant={row.isActive ? 'active' : 'closed'}>
                {row.isActive ? 'نشط' : 'غير نشط'}
              </StatusBadge>
            ),
          },
          {
            key: '_actions',
            header: 'الإجراءات',
            width: 180,
            cell: (row) => (
              <div className="flex gap-1">
                {canUpdate ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Pencil size={14} />}
                    onClick={(e) => { e.stopPropagation(); setEditingRate(row); }}
                    aria-label={`تعديل ${row.baseCurrencyCode}/${row.currencyCode}`}
                  >
                    تعديل
                  </Button>
                ) : null}
                {row.isActive && canDeactivate ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Power size={14} />}
                    onClick={(e) => { e.stopPropagation(); setConfirmAction({ type: 'deactivate', rate: row }); }}
                    aria-label={`تعطيل ${row.baseCurrencyCode}/${row.currencyCode}`}
                  >
                    تعطيل
                  </Button>
                ) : null}
                {!row.isActive && canActivate ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Power size={14} />}
                    onClick={(e) => { e.stopPropagation(); setConfirmAction({ type: 'activate', rate: row }); }}
                    aria-label={`تفعيل ${row.baseCurrencyCode}/${row.currencyCode}`}
                  >
                    تفعيل
                  </Button>
                ) : null}
              </div>
            ),
          },
        ]}
        data={rates}
        loading={isLoading}
        error={error ? 'فشل تحميل بيانات أسعار الصرف' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد أسعار صرف"
        rowKey={(row) => String(row.id ?? 0)}
        onRowClick={(row) => setExpandedId(expandedId === row.id ? null : row.id ?? null)}
      />

      {expandedId != null && (
        <div className="mt-2 border border-[var(--color-border-container)] rounded">
          <ExchangeRateHistoryRow
            rates={rates}
            baseCurrencyId={rates.find((r) => r.id === expandedId)?.baseCurrencyId}
            currencyId={rates.find((r) => r.id === expandedId)?.currencyId}
          />
        </div>
      )}

      <Dialog
        open={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        title="سعر صرف جديد"
      >
        <ExchangeRateForm
          currencies={currencies}
          onSubmit={handleCreate}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء سعر الصرف' : undefined}
          loading={createMutation.isPending}
        />
      </Dialog>

      <Dialog
        open={!!editingRate}
        onClose={() => setEditingRate(null)}
        title="تعديل سعر الصرف"
      >
        {editingRate ? (
          <ExchangeRateForm
            currencies={currencies}
            initialData={{
              baseCurrencyId: editingRate.baseCurrencyId,
              currencyId: editingRate.currencyId,
              rateDate: editingRate.rateDate,
              rateType: typeof editingRate.rateType === 'string' ? (editingRate.rateType === 'Market' ? 1 : 0) : (editingRate.rateType != null ? Number(editingRate.rateType) : 0),
              rate: editingRate.rate,
            }}
            onSubmit={handleEdit}
            serverError={updateMutation.error ? 'حدث خطأ أثناء تحديث سعر الصرف' : undefined}
            loading={updateMutation.isPending}
          />
        ) : null}
      </Dialog>

      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={handleConfirmAction}
        message={
          confirmAction?.type === 'activate'
            ? `هل تريد تفعيل سعر الصرف ${confirmAction?.rate.baseCurrencyCode}/${confirmAction?.rate.currencyCode}؟`
            : `هل تريد تعطيل سعر الصرف ${confirmAction?.rate.baseCurrencyCode}/${confirmAction?.rate.currencyCode}؟`
        }
        destructive={confirmAction?.type === 'deactivate'}
      />
    </div>
  );
}
