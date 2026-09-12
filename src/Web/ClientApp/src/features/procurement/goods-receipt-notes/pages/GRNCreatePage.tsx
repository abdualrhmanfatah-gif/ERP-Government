import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Input, Textarea, Skeleton, Combobox } from '@/components/ui';
import { createGRNSchema, type CreateGRNFormData } from '../shared/schemas';
import { useCreateGRN } from '../hooks/useGRNs';
import { useWarehouses, useLocations, usePurchaseOrderForGRN } from '../shared/catalog-hooks';
import type { ComboboxOption } from '@/components/ui/Combobox';

export default function GRNCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const poIdParam = parseInt(searchParams.get('purchaseOrderId') || '0');
  const createGRN = useCreateGRN();
  const { data: warehouses, isLoading: warehousesLoading } = useWarehouses();
  const { data: locations } = useLocations();
  const { data: purchaseOrder, isLoading: poLoading } = usePurchaseOrderForGRN(poIdParam);

  const { register, handleSubmit, formState: { errors }, setValue, control, watch } = useForm<CreateGRNFormData>({
    resolver: zodResolver(createGRNSchema),
    defaultValues: {
      purchaseOrderId: poIdParam || 0,
      warehouseId: 0,
      locationId: null,
      notes: '',
      lines: [],
    },
  });

  const { fields, replace } = useFieldArray({ control, name: 'lines' });

  useEffect(() => {
    if (poIdParam > 0) {
      setValue('purchaseOrderId', poIdParam);
    }
  }, [poIdParam, setValue]);

  useEffect(() => {
    if (purchaseOrder?.lines && purchaseOrder.lines.length > 0) {
      const eligibleLines = purchaseOrder.lines
        .filter((l) => l.remainingQuantity > 0)
        .map((l) => ({
          purchaseOrderDetailId: l.id,
          itemId: l.itemId,
          unitId: l.unitId,
          receivedQuantity: 0,
          acceptedQuantity: null as number | null,
          rejectedQuantity: null as number | null,
          batchNumber: null as string | null,
          expiryDate: null as string | null,
          notes: null as string | null,
        }));
      replace(eligibleLines);
    }
  }, [purchaseOrder, replace]);

  const onSubmit = async (data: CreateGRNFormData) => {
    try {
      await createGRN.mutateAsync(data);
      navigate('/procurement/goods-receipt-notes');
    } catch {
      // error handled by mutation onError
    }
  };

  if (warehousesLoading || poLoading) return <Skeleton className="h-96" />;
  if (poIdParam > 0 && !purchaseOrder) return <Page title="خطأ" error="لم يتم العثور على أمر الشراء" />;

  const warehouseOptions: ComboboxOption[] = (warehouses ?? []).map((w) => ({ value: String(w.id), label: `${w.code} - ${w.name}` }));
  const locationOptions: ComboboxOption[] = (locations ?? []).map((l) => ({ value: String(l.id), label: l.name }));

  return (
    <Page title="إنشاء إشعار استلام">
      <Card>
        <CardHeader>
          <CardTitle>بيانات إشعار الاستلام</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium">أمر الشراء</label>
                <Input
                  type="number"
                  {...register('purchaseOrderId', { valueAsNumber: true })}
                  disabled={poIdParam > 0}
                />
                {errors.purchaseOrderId && <p className="text-destructive text-sm">{errors.purchaseOrderId.message}</p>}
              </div>

              <div>
                <Combobox
                  label="المستودع *"
                  options={warehouseOptions}
                  value={String(watch('warehouseId') || '')}
                  onChange={(val) => setValue('warehouseId', Number(val) || 0, { shouldValidate: true })}
                  placeholder="اختر المستودع..."
                  searchPlaceholder="بحث..."
                  error={errors.warehouseId?.message}
                />
              </div>

              <div>
                <Combobox
                  label="الموقع"
                  options={locationOptions}
                  value={String(watch('locationId') || '')}
                  onChange={(val) => setValue('locationId', val ? Number(val) : null, { shouldValidate: true })}
                  placeholder="اختر الموقع..."
                  searchPlaceholder="بحث..."
                />
              </div>
            </div>

            <div>
              <label className="text-sm font-medium">ملاحظات</label>
              <Textarea {...register('notes')} />
            </div>

            {fields.length > 0 && (
              <div>
                <h3 className="font-medium mb-2">بنود الاستلام</h3>
                <div className="overflow-x-auto border border-[var(--color-outline-variant)] rounded-xl">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="border-b-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
                        <th className="px-3 py-3 text-start font-semibold w-10">#</th>
                        <th className="px-3 py-3 text-start font-semibold">الصنف</th>
                        <th className="px-3 py-3 text-start font-semibold">الوحدة</th>
                        <th className="px-3 py-3 text-start font-semibold">المطلوب</th>
                        <th className="px-3 py-3 text-start font-semibold">المتبقي</th>
                        <th className="px-3 py-3 text-start font-semibold">المستلمة</th>
                        <th className="px-3 py-3 text-start font-semibold">المقبولة</th>
                        <th className="px-3 py-3 text-start font-semibold">المرفوضة</th>
                        <th className="px-3 py-3 text-start font-semibold">الدفعة</th>
                        <th className="px-3 py-3 text-start font-semibold">تاريخ الانتهاء</th>
                        <th className="px-3 py-3 text-start font-semibold">ملاحظات</th>
                      </tr>
                    </thead>
                    <tbody>
                      {fields.map((field, index) => {
                        const poLine = purchaseOrder?.lines?.find((l) => l.id === field.purchaseOrderDetailId);
                        return (
                          <tr key={field.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_3%,transparent)]">
                            <td className="px-3 py-3 text-[var(--color-on-surface-variant)]">{index + 1}</td>
                            <td className="px-3 py-3">{poLine?.itemId ?? field.itemId}</td>
                            <td className="px-3 py-3">{poLine?.unitId ?? field.unitId}</td>
                            <td className="px-3 py-3 tabular-nums">{poLine?.orderedQuantity ?? '-'}</td>
                            <td className="px-3 py-3 tabular-nums">{poLine?.remainingQuantity ?? '-'}</td>
                            <td className="px-3 py-3">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.receivedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="px-3 py-3">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.acceptedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="px-3 py-3">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.rejectedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="px-3 py-3">
                              <Input
                                {...register(`lines.${index}.batchNumber`)}
                                className="w-24"
                                placeholder="الدفعة"
                              />
                            </td>
                            <td className="px-3 py-3">
                              <Input
                                type="date"
                                {...register(`lines.${index}.expiryDate`)}
                                className="w-32"
                              />
                            </td>
                            <td className="px-3 py-3">
                              <Input
                                {...register(`lines.${index}.notes`)}
                                className="w-32"
                                placeholder="ملاحظات"
                              />
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
                {errors.lines && <p className="text-destructive text-sm mt-1">{errors.lines.message}</p>}
              </div>
            )}

            <div className="flex gap-2 justify-end">
              <Button type="button" variant="outline" onClick={() => navigate('/procurement/goods-receipt-notes')}>
                إلغاء
              </Button>
              <Button type="submit" disabled={createGRN.isPending} loading={createGRN.isPending}>
                إنشاء
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </Page>
  );
}
