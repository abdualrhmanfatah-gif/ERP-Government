import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Page, Button, Card, Input, Select, Switch, Textarea, Skeleton } from '@/components/ui';
import { createItemSchema, type CreateItemInput } from '../shared/schemas';
import { itemTypeOptions, type Item } from '../shared/types';
import type { ComboboxOption } from '@/components/ui/Combobox';

interface ItemFormProps {
  initialData?: Item;
  onSubmit: (data: CreateItemInput) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  onEdit?: () => void;
  categoryOptions: ComboboxOption[];
  unitOptions: ComboboxOption[];
  isLoading?: boolean;
}

export function ItemForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  onEdit,
  categoryOptions,
  unitOptions,
  isLoading = false,
}: ItemFormProps) {
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<CreateItemInput>({
    resolver: zodResolver(createItemSchema),
    defaultValues: initialData
      ? {
          name: initialData.name,
          nameEn: initialData.nameEn ?? '',
          description: initialData.description ?? '',
          categoryId: initialData.categoryId ?? undefined,
          unitId: initialData.unitId,
          barcode: initialData.barcode ?? '',
          itemType: initialData.itemType,
          openingStock: initialData.openingStock ?? undefined,
          minimumStock: initialData.minimumStock ?? undefined,
          maximumStock: initialData.maximumStock ?? undefined,
          reorderLevel: initialData.reorderLevel ?? undefined,
          reorderQuantity: initialData.reorderQuantity ?? undefined,
          leadTimeDays: initialData.leadTimeDays ?? undefined,
          isActive: initialData.isActive,
        }
      : { isActive: true },
  });

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-48" />
        <Skeleton className="h-32" />
        <Skeleton className="h-24" />
      </div>
    );
  }

  if (readOnly && initialData) {
    return (
      <div className="space-y-6">
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold">البيانات الأساسية</h2>
            {onEdit && (
              <Button variant="outline" size="sm" onClick={onEdit}>
                تعديل
              </Button>
            )}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الكود</span>
              <span className="block font-mono text-sm text-[var(--color-on-surface)]">{initialData.code}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الاسم</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{initialData.name}</span>
            </div>
            {initialData.nameEn && (
              <div>
                <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الاسم الإنجليزي</span>
                <span className="block text-sm text-[var(--color-on-surface)]">{initialData.nameEn}</span>
              </div>
            )}
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">النوع</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{itemTypeOptions.find((o) => o.value === initialData.itemType)?.label ?? initialData.itemType}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">التصنيف</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{initialData.categoryName ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الوحدة</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{initialData.unitName}</span>
            </div>
            {initialData.barcode && (
              <div>
                <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الباركود</span>
                <span className="block font-mono text-sm text-[var(--color-on-surface)]">{initialData.barcode}</span>
              </div>
            )}
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الحالة</span>
              <span className="block text-sm text-[var(--color-on-surface)]">{initialData.isActive ? 'نشط' : 'معطل'}</span>
            </div>
            {initialData.description && (
              <div className="md:col-span-2">
                <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الوصف</span>
                <span className="block text-sm text-[var(--color-on-surface)]">{initialData.description}</span>
              </div>
            )}
          </div>
        </Card>

        <Card>
          <h2 className="text-sm font-semibold mb-4">مستويات المخزون</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الحد الأدنى</span>
              <span className="block text-sm text-[var(--color-on-surface)] tabular-nums">{initialData.minimumStock ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الحد الأقصى</span>
              <span className="block text-sm text-[var(--color-on-surface)] tabular-nums">{initialData.maximumStock ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">مستوى إعادة الطلب</span>
              <span className="block text-sm text-[var(--color-on-surface)] tabular-nums">{initialData.reorderLevel ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">كمية إعادة الطلب</span>
              <span className="block text-sm text-[var(--color-on-surface)] tabular-nums">{initialData.reorderQuantity ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">أيام waktu التسليم</span>
              <span className="block text-sm text-[var(--color-on-surface)] tabular-nums">{initialData.leadTimeDays ?? '—'}</span>
            </div>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6" aria-label="نموذج الصنف">
      <Card>
        <h2 className="text-sm font-semibold mb-4">البيانات الأساسية</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label="الاسم *" {...register('name')} error={errors.name?.message} required />
          <Input label="الاسم الإنجليزي" {...register('nameEn')} error={errors.nameEn?.message} />
          <Textarea label="الوصف" {...register('description')} rows={3} />
          <Select
            label="التصنيف"
            options={categoryOptions}
            {...register('categoryId', { valueAsNumber: true })}
            error={errors.categoryId?.message}
          />
          <Select
            label="الوحدة *"
            options={unitOptions}
            {...register('unitId', { valueAsNumber: true })}
            error={errors.unitId?.message}
          />
          <Input label="الباركود" {...register('barcode')} error={errors.barcode?.message} />
          <Select
            label="نوع الصنف *"
            options={itemTypeOptions}
            {...register('itemType')}
            error={errors.itemType?.message}
          />
        </div>
      </Card>

      <Card>
        <h2 className="text-sm font-semibold mb-4">مستويات المخزون</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Input label="الرصيد الافتتاحي" type="number" step="0.01" {...register('openingStock', { valueAsNumber: true })} />
          <Input label="الحد الأدنى" type="number" step="0.01" {...register('minimumStock', { valueAsNumber: true })} error={errors.minimumStock?.message} />
          <Input label="الحد الأقصى" type="number" step="0.01" {...register('maximumStock', { valueAsNumber: true })} error={errors.maximumStock?.message} />
          <Input label="مستوى إعادة الطلب" type="number" step="0.01" {...register('reorderLevel', { valueAsNumber: true })} />
          <Input label="كمية إعادة الطلب" type="number" step="0.01" {...register('reorderQuantity', { valueAsNumber: true })} />
          <Input label="أيام وقت التسليم" type="number" {...register('leadTimeDays', { valueAsNumber: true })} />
        </div>
      </Card>

      <Card>
        <h2 className="text-sm font-semibold mb-4">الحالة</h2>
        <Switch
          label="نشط"
          checked={watch('isActive')}
          onChange={(v) => setValue('isActive', v)}
        />
      </Card>

      <div className="flex gap-4 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>
          إلغاء
        </Button>
        <Button type="submit" disabled={isPending}>
          {isPending ? 'جاري الحفظ...' : 'حفظ'}
        </Button>
      </div>
    </form>
  );
}
