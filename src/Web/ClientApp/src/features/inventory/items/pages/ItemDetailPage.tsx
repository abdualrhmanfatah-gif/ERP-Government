import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, Badge, DataGrid, ConfirmDialog, Switch } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { useItemById, useToggleItemActive, useRemoveItemUnit } from '../hooks/useItems';
import { useItemCategoriesList } from '../../item-categories/hooks/useItemCategories';
import { useUnitsList } from '../../units/hooks/useUnits';
import { itemTypeLabels, type ItemType, type ItemUnit } from '../shared/types';
import { notify } from '@/features/notifications/notify';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { ItemForm } from '../components/ItemForm';

export default function ItemDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const itemId = Number(id);

  const { data: item, isLoading, error } = useItemById(itemId);
  const { data: categories = [] } = useItemCategoriesList();
  const { data: units = [] } = useUnitsList();
  const toggleMutation = useToggleItemActive();
  const removeUnitMutation = useRemoveItemUnit();

  const [confirmToggle, setConfirmToggle] = useState(false);
  const [removeUnit, setRemoveUnit] = useState<ItemUnit | null>(null);

  const categoryOptions = categories.map((c) => ({ value: String(c.id), label: c.name }));
  const unitOptions = units.map((u) => ({ value: String(u.id), label: u.name }));

  function handleToggleActive() {
    toggleMutation.mutate(itemId, {
      onSuccess: () => {
        notify({ type: 'success', title: item?.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
        setConfirmToggle(false);
      },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التبديل' }),
    });
  }

  function handleConfirmRemoveUnit() {
    if (!removeUnit) return;
    removeUnitMutation.mutate(
      { itemId, id: removeUnit.id },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم حذف الوحدة بنجاح' });
          setRemoveUnit(null);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الحذف' }),
      },
    );
  }

  const unitColumns: DataGridColumn<ItemUnit>[] = [
    { header: 'الوحدة', cell: (row) => row.unitName },
    { header: 'معامل التحويل', cell: (row) => <span className="tabular-nums">{row.conversionFactor}</span>, align: 'right' },
    {
      header: 'الأساسية',
      cell: (row) => (
        <Badge variant={row.isBase ? 'success' : 'outline'}>
          {row.isBase ? 'أساسية' : '辅助'}
        </Badge>
      ),
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <div className="flex items-center gap-1">
          {!row.isBase && (
            <Button
              variant="ghost"
              size="icon-xs"
              onClick={() => navigate(`/inventory/items/${itemId}/units/${row.id}/edit`)}
              aria-label="تعديل الوحدة"
            >
              <Pencil size={14} />
            </Button>
          )}
          {!row.isBase && (
            <Button
              variant="ghost"
              size="icon-xs"
              onClick={() => setRemoveUnit(row)}
              aria-label="حذف الوحدة"
            >
              <Trash2 size={14} />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <Page
      title={item?.name ?? 'تفاصيل الصنف'}
      description={item ? `${item.code} — ${item.name}` : undefined}
      loading={isLoading}
      error={error || (!item && !isLoading) ? 'حدث خطأ أثناء تحميل بيانات الصنف' : undefined}
      breadcrumbs={[{ label: 'الأصناف', path: '/inventory/items' }, { label: item?.name ?? '' }]}
      actions={
        item ? (
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              icon={<Pencil size={16} />}
              onClick={() => navigate(`/inventory/items/${itemId}/edit`)}
              aria-label="تعديل الصنف"
            >
              تعديل
            </Button>
            <Switch
              checked={item.isActive}
              onChange={() => setConfirmToggle(true)}
              label={getActiveStatusLabel(item.isActive)}
            />
          </div>
        ) : undefined
      }
    >
      {!item ? null : (
        <div className="space-y-6">
          <ItemForm
            initialData={item}
            readOnly
            onEdit={() => navigate(`/inventory/items/${itemId}/edit`)}
            onSubmit={() => {}}
            onCancel={() => navigate('/inventory/items')}
            categoryOptions={categoryOptions}
            unitOptions={unitOptions}
          />

          <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-sm font-semibold">وحدات الصنف</h2>
              <Button
                size="sm"
                icon={<Plus size={14} />}
                onClick={() => navigate(`/inventory/items/${itemId}/units/create`)}
                aria-label="إضافة وحدة"
              >
                إضافة وحدة
              </Button>
            </div>
            <DataGrid
              columns={unitColumns}
              data={item.itemUnits ?? []}
              emptyMessage="لا توجد وحدات مرتبطة"
              rowKey={(row) => row.id}
            />
          </div>
        </div>
      )}

      <ConfirmDialog
        open={confirmToggle}
        onClose={() => setConfirmToggle(false)}
        onConfirm={handleToggleActive}
        message={item?.isActive ? 'هل تريد تعطيل هذا الصنف؟' : 'هل تريد تفعيل هذا الصنف؟'}
        title="تأكيد تغيير الحالة"
        loading={toggleMutation.isPending}
      />

      <ConfirmDialog
        open={!!removeUnit}
        onClose={() => setRemoveUnit(null)}
        onConfirm={handleConfirmRemoveUnit}
        message={`هل تريد حذف الوحدة "${removeUnit?.unitName}"؟`}
        title="تأكيد الحذف"
        destructive
        loading={removeUnitMutation.isPending}
      />
    </Page>
  );
}
