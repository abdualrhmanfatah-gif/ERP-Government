import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Input, Textarea, ErrorState, Skeleton } from '@/components/ui';
import { createGRNSchema, type CreateGRNFormData } from '../shared/schemas';
import { useCreateGRN } from '../hooks/useGRNs';
import { useWarehouses, useLocations, usePurchaseOrderForGRN } from '../shared/catalog-hooks';

export default function GRNCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const poIdParam = parseInt(searchParams.get('purchaseOrderId') || '0');
  const createGRN = useCreateGRN();
  const { data: warehouses, isLoading: warehousesLoading } = useWarehouses();
  const { data: locations } = useLocations();
  const { data: purchaseOrder, isLoading: poLoading } = usePurchaseOrderForGRN(poIdParam);

  const { register, handleSubmit, formState: { errors }, setValue, control } = useForm<CreateGRNFormData>({
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
      // Error handled by mutation
    }
  };

  if (warehousesLoading || poLoading) return <Skeleton className="h-96" />;
  if (poIdParam > 0 && !purchaseOrder) return <ErrorState message="لم يتم العثور على أمر الشراء" />;

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
                <label className="text-sm font-medium">المستودع</label>
                <select {...register('warehouseId', { valueAsNumber: true })} className="w-full border rounded p-2">
                  <option value="">اختر المستودع</option>
                  {warehouses?.map((w) => (
                    <option key={w.id} value={w.id}>{w.name}</option>
                  ))}
                </select>
                {errors.warehouseId && <p className="text-destructive text-sm">{errors.warehouseId.message}</p>}
              </div>

              <div>
                <label className="text-sm font-medium">الموقع</label>
                <select {...register('locationId', { valueAsNumber: true })} className="w-full border rounded p-2">
                  <option value="">اختر الموقع</option>
                  {locations?.map((l) => (
                    <option key={l.id} value={l.id}>{l.name}</option>
                  ))}
                </select>
              </div>
            </div>

            <div>
              <label className="text-sm font-medium">ملاحظات</label>
              <Textarea {...register('notes')} />
            </div>

            {fields.length > 0 && (
              <div>
                <h3 className="font-medium mb-2">بنود الاستلام</h3>
                <div className="overflow-x-auto border rounded">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="bg-muted">
                        <th className="border p-2 text-start">الصنف</th>
                        <th className="border p-2 text-start">الوحدة</th>
                        <th className="border p-2 text-start">الكمية المطلوبة</th>
                        <th className="border p-2 text-start">المتبقي</th>
                        <th className="border p-2 text-start">الكمية المستلمة</th>
                        <th className="border p-2 text-start">المقبولة</th>
                        <th className="border p-2 text-start">المرفوضة</th>
                        <th className="border p-2 text-start">الدفعة</th>
                        <th className="border p-2 text-start">تاريخ الانتهاء</th>
                        <th className="border p-2 text-start">ملاحظات</th>
                      </tr>
                    </thead>
                    <tbody>
                      {fields.map((field, index) => {
                        const poLine = purchaseOrder?.lines?.find((l) => l.id === field.purchaseOrderDetailId);
                        return (
                          <tr key={field.id} className="border-b">
                            <td className="border p-2">{field.itemId}</td>
                            <td className="border p-2">{field.unitId}</td>
                            <td className="border p-2 tabular-nums">{poLine?.orderedQuantity ?? '-'}</td>
                            <td className="border p-2 tabular-nums">{poLine?.remainingQuantity ?? '-'}</td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.receivedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.acceptedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.rejectedQuantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                {...register(`lines.${index}.batchNumber`)}
                                className="w-24"
                                placeholder="الدفعة"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="date"
                                {...register(`lines.${index}.expiryDate`)}
                                className="w-32"
                              />
                            </td>
                            <td className="border p-2">
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
              <Button type="submit" disabled={createGRN.isPending}>
                {createGRN.isPending ? 'جاري الإنشاء...' : 'إنشاء'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </Page>
  );
}
