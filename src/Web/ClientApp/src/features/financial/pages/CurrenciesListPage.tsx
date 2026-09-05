import { useState } from 'react';
import { Plus, Pencil, Power, Star } from 'lucide-react';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { FilterBar, FilterSelect } from '@/components/ui';
import { CurrencyForm } from '../components/CurrencyForm';
import { useCurrenciesList } from '../hooks/useCurrenciesList';
import { useCreateCurrency } from '../hooks/useCreateCurrency';
import { useUpdateCurrency } from '../hooks/useUpdateCurrency';
import { useActivateCurrency } from '../hooks/useActivateCurrency';
import { useDeactivateCurrency } from '../hooks/useDeactivateCurrency';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import type { CurrencyDto } from '../types';

export function CurrenciesListPage() {
  const [filters, setFilters] = useState<{ isActive?: boolean }>({});

  const { hasPermission: canCreate } = usePermission(PERMISSIONS.Currencies.Create);
  const { hasPermission: canUpdate } = usePermission(PERMISSIONS.Currencies.Update);
  const { hasPermission: canActivate } = usePermission(PERMISSIONS.Currencies.Activate);
  const { hasPermission: canDeactivate } = usePermission(PERMISSIONS.Currencies.Deactivate);

  const { data: currencies = [], isLoading, error, refetch } = useCurrenciesList(filters);

  const createMutation = useCreateCurrency();
  const updateMutation = useUpdateCurrency();
  const activateMutation = useActivateCurrency();
  const deactivateMutation = useDeactivateCurrency();

  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [editingCurrency, setEditingCurrency] = useState<CurrencyDto | null>(null);
  const [confirmAction, setConfirmAction] = useState<{ type: 'activate' | 'deactivate'; currency: CurrencyDto } | null>(null);

  const handleCreate = async (data: Record<string, unknown>) => {
    try {
      await createMutation.mutateAsync(data);
      setShowCreateDialog(false);
      toast.success('تم إنشاء العملة بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء العملة';
      toast.error(message);
    }
  };

  const handleEdit = async (data: Record<string, unknown>) => {
    if (!editingCurrency) return;
    try {
      await updateMutation.mutateAsync({ id: editingCurrency.id!, data: { ...data, id: editingCurrency.id } });
      setEditingCurrency(null);
      toast.success('تم تعديل العملة بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعديل العملة';
      toast.error(message);
    }
  };

  const handleConfirmAction = async () => {
    if (!confirmAction) return;
    const { type, currency } = confirmAction;
    try {
      if (type === 'activate') {
        await activateMutation.mutateAsync({ id: currency.id!, rowVersion: undefined });
        toast.success('تم تفعيل العملة بنجاح');
      } else {
        await deactivateMutation.mutateAsync({ id: currency.id!, rowVersion: undefined });
        toast.success('تم تعطيل العملة بنجاح');
      }
      setConfirmAction(null);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تنفيذ الإجراء';
      toast.error(message);
    }
  };

  const hasFilter = filters.isActive !== undefined;

  return (
    <div>
      <PageHeader
        title="العملات"
        description="إدارة العملات"
        actions={
          canCreate ? (
            <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => setShowCreateDialog(true)}>
              عملة جديدة
            </Button>
          ) : undefined
        }
      />
      <FilterBar hasFilters={hasFilter} onClear={() => setFilters({})}>
        <FilterSelect
          label="الحالة"
          value={filters.isActive === undefined ? '' : String(filters.isActive)}
          onChange={(v) => setFilters(v === '' ? {} : { isActive: v === 'true' })}
          options={[
            { value: 'true', label: 'نشط' },
            { value: 'false', label: 'غير نشط' },
          ]}
        />
      </FilterBar>
      <DataGrid<CurrencyDto>
        columns={[
          {
            key: 'code',
            header: 'الرمز',
            width: 100,
            cell: (row) => <span className="tabular-nums">{row.code ?? '—'}</span>,
          },
          {
            key: 'name',
            header: 'الاسم',
            width: 180,
            cell: (row) => row.name ?? '—',
          },
          {
            key: 'symbol',
            header: 'الرمز',
            width: 80,
            cell: (row) => row.symbol ?? '—',
          },
          {
            key: 'decimalPlaces',
            header: 'الحالات العشرية',
            width: 100,
            align: 'center',
            cell: (row) => <span className="tabular-nums">{row.decimalPlaces ?? '—'}</span>,
          },
          {
            key: 'roundingPrecision',
            header: 'دقة التحديد',
            width: 120,
            align: 'center',
            cell: (row) => <span className="tabular-nums">{row.roundingPrecision ?? '—'}</span>,
          },
          {
            key: 'isBase',
            header: 'العملة الأساسية',
            width: 100,
            align: 'center',
            cell: (row) =>
              row.isBase ? (
                <Star size={16} fill="var(--color-secondary)" color="var(--color-secondary)" aria-label="العملة الأساسية" />
              ) : null,
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
                    onClick={(e) => { e.stopPropagation(); setEditingCurrency(row); }}
                    aria-label={`تعديل ${row.code}`}
                  >
                    تعديل
                  </Button>
                ) : null}
                {canActivate && !row.isActive ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Power size={14} />}
                    onClick={(e) => { e.stopPropagation(); setConfirmAction({ type: 'activate', currency: row }); }}
                    aria-label={`تفعيل ${row.code}`}
                  >
                    تفعيل
                  </Button>
                ) : null}
                {canDeactivate && row.isActive && !row.isBase ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Power size={14} />}
                    onClick={(e) => { e.stopPropagation(); setConfirmAction({ type: 'deactivate', currency: row }); }}
                    aria-label={`تعطيل ${row.code}`}
                  >
                    تعطيل
                  </Button>
                ) : null}
              </div>
            ),
          },
        ]}
        data={currencies}
        loading={isLoading}
        error={error ? 'فشل تحميل بيانات العملات' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد عملات"
        rowKey={(row) => String(row.id ?? 0)}
      />

      <Dialog
        open={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        title="عملة جديدة"
      >
        <CurrencyForm
          onSubmit={handleCreate}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء العملة' : undefined}
          loading={createMutation.isPending}
        />
      </Dialog>

      <Dialog
        open={!!editingCurrency}
        onClose={() => setEditingCurrency(null)}
        title="تعديل العملة"
      >
        {editingCurrency ? (
          <CurrencyForm
            initialData={editingCurrency}
            onSubmit={handleEdit}
            serverError={updateMutation.error ? 'حدث خطأ أثناء تعديل العملة' : undefined}
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
            ? `هل تريد تفعيل العملة ${confirmAction?.currency.code}؟`
            : `هل تريد تعطيل العملة ${confirmAction?.currency.code}؟`
        }
        destructive={confirmAction?.type === 'deactivate'}
      />
    </div>
  );
}
