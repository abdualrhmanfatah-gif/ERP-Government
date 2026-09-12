import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Card, CardHeader, CardTitle, CardContent, Input, Select, Textarea, Combobox, EmptyState } from '@/components/ui';
import { Plus, Trash2, Pencil } from 'lucide-react';
import { createPurchaseRequestSchema, type CreatePurchaseRequestFormData } from '../shared/schemas';
import { purchaseRequestPriorityLabels } from '../shared/types';
import type { ComboboxOption } from '@/components/ui/Combobox';

const priorityOptions = Object.entries(purchaseRequestPriorityLabels).map(([value, label]) => ({
  value,
  label,
}));

interface PurchaseRequestFormProps {
  initialData?: {
    requestDate: string;
    requiredDate?: string | null;
    departmentId?: number | null;
    costCenterId?: number | null;
    requesterName: string;
    priority: string;
    notes?: string | null;
    lines: Array<{
      itemId: number;
      unitId: number;
      requestedQuantity: number;
      unitCostEstimate?: number | null;
      notes?: string | null;
    }>;
  };
  onSubmit: (data: CreatePurchaseRequestFormData) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  isCreate?: boolean;
  onEdit?: () => void;
  itemOptions: ComboboxOption[];
  unitOptions: ComboboxOption[];
  departmentOptions: ComboboxOption[];
  costCenterOptions: ComboboxOption[];
}

export function PurchaseRequestForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  isCreate = false,
  onEdit,
  itemOptions,
  unitOptions,
  departmentOptions,
  costCenterOptions,
}: PurchaseRequestFormProps) {
  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<CreatePurchaseRequestFormData>({
    resolver: zodResolver(createPurchaseRequestSchema),
    defaultValues: initialData
      ? {
          requestDate: initialData.requestDate,
          requiredDate: initialData.requiredDate ?? null,
          departmentId: initialData.departmentId ?? null,
          costCenterId: initialData.costCenterId ?? null,
          requesterName: initialData.requesterName,
          priority: initialData.priority as 'Low' | 'Normal' | 'High' | 'Urgent',
          notes: initialData.notes ?? null,
          lines: initialData.lines.map((l) => ({
            itemId: l.itemId,
            unitId: l.unitId,
            requestedQuantity: l.requestedQuantity,
            unitCostEstimate: l.unitCostEstimate ?? null,
            notes: l.notes ?? null,
          })),
        }
      : {
          requestDate: new Date().toISOString().split('T')[0],
          requesterName: '',
          priority: 'Normal',
          lines: [],
        },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'lines',
  });

  const watchedLines = watch('lines');
  const estimatedTotal = watchedLines.reduce(
    (sum, line) => sum + (line.unitCostEstimate ?? 0) * (line.requestedQuantity ?? 0),
    0,
  );

  function addLine() {
    append({ itemId: 0, unitId: 0, requestedQuantity: 1, unitCostEstimate: null, notes: null }, { shouldFocus: false });
  }

  if (readOnly) {
    const itemMap = new Map(itemOptions.map((o) => [o.value, o.label]));
    const unitMap = new Map(unitOptions.map((o) => [o.value, o.label]));

    return (
      <div className="space-y-4">
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>بيانات طلب الشراء</CardTitle>
              {onEdit && (
                <Button variant="outline" size="sm" onClick={onEdit}>
                  <Pencil className="h-4 w-4 ms-2" /> تعديل
                </Button>
              )}
            </div>
          </CardHeader>
          <CardContent className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">تاريخ الطلب</span>
              <span>{new Date(initialData!.requestDate).toLocaleDateString('ar-YE')}</span>
            </div>
            {initialData!.requiredDate && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">التاريخ المطلوب</span>
                <span>{new Date(initialData!.requiredDate).toLocaleDateString('ar-YE')}</span>
              </div>
            )}
            {initialData!.departmentId && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">القسم</span>
                <span>{departmentOptions.find((d) => d.value === String(initialData!.departmentId))?.label ?? `#${initialData!.departmentId}`}</span>
              </div>
            )}
            {initialData!.costCenterId && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">مركز التكلفة</span>
                <span>{costCenterOptions.find((c) => c.value === String(initialData!.costCenterId))?.label ?? `#${initialData!.costCenterId}`}</span>
              </div>
            )}
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">مقدم الطلب</span>
              <span>{initialData!.requesterName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">الأولوية</span>
              <span>{purchaseRequestPriorityLabels[initialData!.priority as keyof typeof purchaseRequestPriorityLabels]}</span>
            </div>
            {initialData!.notes && (
              <div className="pt-2 border-t border-[var(--color-outline-variant)]">
                <span className="text-[var(--color-on-surface-variant)]">ملاحظات</span>
                <p className="mt-1">{initialData!.notes}</p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>بنود الطلب</CardTitle>
              <span className="text-sm text-[var(--color-on-surface-variant)]">{initialData!.lines.length} بند</span>
            </div>
          </CardHeader>
          <CardContent>
            {initialData!.lines.length === 0 ? (
              <EmptyState message="لا توجد بنود" />
            ) : (
              <div className="overflow-x-auto border border-[var(--color-outline-variant)] rounded-xl">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="border-b-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
                      <th className="text-start px-4 py-3 font-semibold w-10">#</th>
                      <th className="text-start px-4 py-3 font-semibold">الصنف</th>
                      <th className="text-start px-4 py-3 font-semibold">الوحدة</th>
                      <th className="text-start px-4 py-3 font-semibold">الكمية</th>
                      <th className="text-start px-4 py-3 font-semibold">سعر الوحدة</th>
                      <th className="text-start px-4 py-3 font-semibold">الإجمالي</th>
                      {initialData!.lines.some((l) => l.notes) && (
                        <th className="text-start px-4 py-3 font-semibold">ملاحظات</th>
                      )}
                    </tr>
                  </thead>
                  <tbody>
                    {initialData!.lines.map((line, idx) => {
                      const lineTotal = (line.unitCostEstimate ?? 0) * (line.requestedQuantity ?? 0);
                      return (
                        <tr key={idx} className="border-b border-[var(--color-outline-variant)] last:border-b-0">
                          <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{idx + 1}</td>
                          <td className="px-4 py-3">{itemMap.get(String(line.itemId)) ?? `بند #${line.itemId}`}</td>
                          <td className="px-4 py-3">{unitMap.get(String(line.unitId)) ?? `وحدة #${line.unitId}`}</td>
                          <td className="px-4 py-3 tabular-nums">{line.requestedQuantity}</td>
                          <td className="px-4 py-3 tabular-nums font-mono">
                            {line.unitCostEstimate != null ? line.unitCostEstimate.toLocaleString('ar-YE') : '-'}
                          </td>
                          <td className="px-4 py-3 tabular-nums font-mono">
                            {lineTotal > 0 ? lineTotal.toLocaleString('ar-YE') : '-'}
                          </td>
                          {initialData!.lines.some((l) => l.notes) && (
                            <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{line.notes ?? '-'}</td>
                          )}
                        </tr>
                      );
                    })}
                  </tbody>
                  <tfoot>
                    <tr className="bg-[var(--color-surface-container-low)] font-semibold border-t-2 border-[var(--color-outline-variant)]">
                      <td colSpan={5} className="px-4 py-3 text-start">الإجمالي التقديري</td>
                      <td className="px-4 py-3 tabular-nums font-mono">
                        {estimatedTotal > 0 ? estimatedTotal.toLocaleString('ar-YE') : '-'}
                      </td>
                      {initialData!.lines.some((l) => l.notes) && <td />}
                    </tr>
                  </tfoot>
                </table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} aria-label="طلب شراء">
      <Card className="mb-6">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>بيانات طلب الشراء</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={addLine} aria-label="إضافة صنف">
              <Plus className="h-4 w-4 ms-2" /> إضافة صنف
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mb-6">
            <Input
              label="تاريخ الطلب *"
              type="date"
              {...register('requestDate')}
              error={errors.requestDate?.message}
            />
            <Input
              label="التاريخ المطلوب"
              type="date"
              {...register('requiredDate')}
              error={errors.requiredDate?.message}
            />
            <Select
              label="الأولوية *"
              options={priorityOptions}
              {...register('priority')}
              error={errors.priority?.message}
            />
            <Combobox
              label="القسم"
              options={departmentOptions}
              value={String(watch('departmentId') || '')}
              onChange={(val) => setValue('departmentId', Number(val) || null)}
              placeholder="اختر القسم..."
              searchPlaceholder="بحث..."
              error={errors.departmentId?.message}
            />
            <Combobox
              label="مركز التكلفة"
              options={costCenterOptions}
              value={String(watch('costCenterId') || '')}
              onChange={(val) => setValue('costCenterId', Number(val) || null)}
              placeholder="اختر مركز التكلفة..."
              searchPlaceholder="بحث..."
              error={errors.costCenterId?.message}
            />
            <div className="col-span-2 md:col-span-2">
              <Input
                label="مقدم الطلب *"
                {...register('requesterName')}
                error={errors.requesterName?.message}
              />
            </div>
            <div className="col-span-2 md:col-span-5">
              <Textarea
                label="ملاحظات"
                rows={1}
                {...register('notes')}
                error={errors.notes?.message}
              />
            </div>
          </div>

          <div className="border-t border-[var(--color-outline-variant)] pt-4">
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm font-semibold text-[var(--color-on-surface)]">بنود الطلب</span>
            </div>
            {errors.lines?.message && (
              <p className="text-sm text-[var(--color-error)] mb-4">{errors.lines.message}</p>
            )}
            {fields.length === 0 ? (
              <EmptyState message="لا توجد بنود. اضغط &quot;إضافة صنف&quot; لإضافة صنف أول." />
            ) : (
              <>
                <div className="overflow-x-auto border border-[var(--color-outline-variant)] rounded-xl">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="border-b-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
                        <th className="text-start px-4 py-3 font-semibold w-10">#</th>
                        <th className="text-start px-4 py-3 font-semibold w-[30%]">الصنف</th>
                        <th className="text-start px-4 py-3 font-semibold w-[18%]">الوحدة</th>
                        <th className="text-start px-4 py-3 font-semibold w-[12%]">الكمية</th>
                        <th className="text-start px-4 py-3 font-semibold w-[14%]">تقدير سعر الوحدة</th>
                        <th className="text-start px-4 py-3 font-semibold w-[14%]">الإجمالي</th>
                        <th className="text-start px-4 py-3 font-semibold w-[10%]">ملاحظات</th>
                        <th className="w-12" />
                      </tr>
                    </thead>
                    <tbody>
                      {fields.map((field, index) => {
                        const lineTotal = (watchedLines[index]?.unitCostEstimate ?? 0) * (watchedLines[index]?.requestedQuantity ?? 0);
                        return (
                          <tr key={field.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_3%,transparent)]">
                            <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{index + 1}</td>
                            <td className="px-4 py-3">
                              <Combobox
                                options={itemOptions}
                                value={String(field.itemId || '')}
                                onChange={(val) => setValue(`lines.${index}.itemId`, Number(val))}
                                placeholder="اختر الصنف..."
                                searchPlaceholder="بحث..."
                                error={errors.lines?.[index]?.itemId?.message}
                              />
                            </td>
                            <td className="px-4 py-3">
                              <Combobox
                                options={unitOptions}
                                value={String(field.unitId || '')}
                                onChange={(val) => setValue(`lines.${index}.unitId`, Number(val))}
                                placeholder="اختر الوحدة..."
                                searchPlaceholder="بحث..."
                                error={errors.lines?.[index]?.unitId?.message}
                              />
                            </td>
                            <td className="px-4 py-3">
                              <Input
                                type="number"
                                min="0.01"
                                step="0.01"
                                {...register(`lines.${index}.requestedQuantity`, { valueAsNumber: true })}
                                error={errors.lines?.[index]?.requestedQuantity?.message}
                              />
                            </td>
                            <td className="px-4 py-3">
                              <Input
                                type="number"
                                min="0"
                                step="0.01"
                                {...register(`lines.${index}.unitCostEstimate`, { valueAsNumber: true })}
                                error={errors.lines?.[index]?.unitCostEstimate?.message}
                              />
                            </td>
                            <td className="px-4 py-3 tabular-nums font-mono text-[var(--color-on-surface-variant)]">
                              {lineTotal > 0 ? lineTotal.toLocaleString('ar-YE') : '-'}
                            </td>
                            <td className="px-4 py-3">
                              <Input
                                {...register(`lines.${index}.notes`)}
                                error={errors.lines?.[index]?.notes?.message}
                              />
                            </td>
                            <td className="px-4 py-3">
                              <Button
                                type="button"
                                variant="ghost"
                                size="icon"
                                onClick={() => remove(index)}
                                aria-label="حذف الصنف"
                              >
                                <Trash2 className="h-4 w-4 text-[var(--color-error)]" />
                              </Button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                    <tfoot>
                      <tr className="bg-[var(--color-surface-container-low)] font-semibold border-t-2 border-[var(--color-outline-variant)]">
                        <td colSpan={5} className="px-4 py-3 text-start">الإجمالي التقديري</td>
                        <td className="px-4 py-3 tabular-nums font-mono">
                          {estimatedTotal > 0 ? estimatedTotal.toLocaleString('ar-YE') : '-'}
                        </td>
                        <td colSpan={2} />
                      </tr>
                    </tfoot>
                  </table>
                </div>
                <p className="text-xs text-[var(--color-on-surface-variant)] mt-2">
                  * الحقول المطلوبة — يجب اختيار الصنف والوحدة والكمية لكل صنف
                </p>
              </>
            )}
          </div>
        </CardContent>
      </Card>

      {errors.root && (
        <p className="text-sm text-[var(--color-error)] mb-4" role="alert">{errors.root.message}</p>
      )}

      <div className="flex justify-end gap-2">
        <Button variant="ghost" type="button" onClick={onCancel}>
          إلغاء
        </Button>
        <Button
          variant="primary"
          type="submit"
          loading={isPending}
        >
          {isCreate ? 'إنشاء طلب' : 'حفظ'}
        </Button>
      </div>
    </form>
  );
}
