import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Input, Textarea, ErrorState, Skeleton } from '@/components/ui';
import { createSupplierInvoiceSchema, type CreateSupplierInvoiceFormData } from '../shared/schemas';
import { useCreateSupplierInvoice } from '../hooks/useSupplierInvoices';
import { useItems } from '../shared/catalog-hooks';

interface PurchaseOrderLine {
  id: number;
  itemId: number;
  itemName?: string;
  unitId: number;
  unitName?: string;
  orderedQuantity: number;
  receivedQuantity: number;
  remainingQuantity: number;
  unitCost?: number;
}

interface PurchaseOrderForInvoice {
  id: number;
  poNumber: string;
  supplierPartyId: number;
  supplierName?: string;
  status: string;
  currencyCode?: string;
  exchangeRate?: number;
  lines: PurchaseOrderLine[];
}

export default function SupplierInvoiceCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const poIdParam = parseInt(searchParams.get('purchaseOrderId') || '0');
  const createInvoice = useCreateSupplierInvoice();
  const { data: items } = useItems();
  const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderForInvoice | null>(null);
  const [poLoading, setPoLoading] = useState(true);
  const [poError, setPoError] = useState('');

  useEffect(() => {
    if (poIdParam <= 0) {
      setPoLoading(false);
      return;
    }
    const controller = new AbortController();
    async function load() {
      try {
        const res = await fetch(`/api/PurchaseOrders/${poIdParam}`, { signal: controller.signal });
        if (!res.ok) throw new Error('Not found');
        const data = await res.json();
        setPurchaseOrder(data);
      } catch {
        setPoError('لم يتم العثور على أمر الشراء');
      } finally {
        setPoLoading(false);
      }
    }
    load();
    return () => controller.abort();
  }, [poIdParam]);

  const { register, handleSubmit, formState: { errors }, setValue, control } = useForm<CreateSupplierInvoiceFormData>({
    resolver: zodResolver(createSupplierInvoiceSchema),
    defaultValues: {
      purchaseOrderId: poIdParam || 0,
      supplierInvoiceNumber: '',
      invoiceDate: new Date().toISOString().split('T')[0],
      currencyCode: null,
      exchangeRate: null,
      dueDate: null,
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
    if (purchaseOrder) {
      if (purchaseOrder.currencyCode) setValue('currencyCode', purchaseOrder.currencyCode);
      if (purchaseOrder.exchangeRate) setValue('exchangeRate', purchaseOrder.exchangeRate);
    }
  }, [purchaseOrder, setValue]);

  useEffect(() => {
    if (purchaseOrder?.lines && purchaseOrder.lines.length > 0) {
      const eligibleLines = purchaseOrder.lines
        .filter((l) => l.remainingQuantity > 0)
        .map((l) => ({
          purchaseOrderDetailId: l.id,
          itemId: l.itemId,
          quantity: 0,
          unitPrice: l.unitCost ?? 0,
          discountAmount: null as number | null,
          taxAmount: null as number | null,
          notes: null as string | null,
        }));
      replace(eligibleLines);
    }
  }, [purchaseOrder, replace]);

  const onSubmit = async (data: CreateSupplierInvoiceFormData) => {
    try {
      const newId = await createInvoice.mutateAsync(data);
      navigate(`/procurement/supplier-invoices/${newId}`);
    } catch {
      // Error handled by mutation
    }
  };

  if (poIdParam <= 0) return <ErrorState message="يجب تحديد أمر الشراء. يرجى الإنشاء من صفحة أمر الشراء." />;
  if (poLoading) return <Skeleton className="h-96" />;
  if (poError) return <ErrorState message={poError} />;
  if (!purchaseOrder) return <ErrorState message="لم يتم العثور على أمر الشراء" />;

  return (
    <Page title="إنشاء فاتورة مورد">
      <Card>
        <CardHeader>
          <CardTitle>بيانات الفاتورة</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium">رقم أمر الشراء</label>
                <Input value={purchaseOrder.poNumber} disabled />
              </div>

              <div>
                <label className="text-sm font-medium">المورد</label>
                <Input value={purchaseOrder.supplierName ?? `#${purchaseOrder.supplierPartyId}`} disabled />
              </div>

              <div>
                <label className="text-sm font-medium">رقم فاتورة المورد *</label>
                <Input {...register('supplierInvoiceNumber')} placeholder="أدخل رقم فاتورة المورد" />
                {errors.supplierInvoiceNumber && <p className="text-destructive text-sm">{errors.supplierInvoiceNumber.message}</p>}
              </div>

              <div>
                <label className="text-sm font-medium">تاريخ الفاتورة *</label>
                <Input type="date" {...register('invoiceDate')} />
                {errors.invoiceDate && <p className="text-destructive text-sm">{errors.invoiceDate.message}</p>}
              </div>

              <div>
                <label className="text-sm font-medium">العملة</label>
                <Input {...register('currencyCode')} placeholder="أدخل كود العملة" />
              </div>

              <div>
                <label className="text-sm font-medium">سعر الصرف</label>
                <Input type="number" step="0.0001" min="0" {...register('exchangeRate', { valueAsNumber: true })} />
              </div>

              <div>
                <label className="text-sm font-medium">تاريخ الاستحقاق</label>
                <Input type="date" {...register('dueDate')} />
              </div>
            </div>

            <div>
              <label className="text-sm font-medium">ملاحظات</label>
              <Textarea {...register('notes')} />
            </div>

            {fields.length > 0 && (
              <div>
                <h3 className="font-medium mb-2">بنود الفاتورة</h3>
                <div className="overflow-x-auto border rounded">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="bg-muted">
                        <th className="border p-2 text-start">الصنف</th>
                        <th className="border p-2 text-start">الكمية المطلوبة</th>
                        <th className="border p-2 text-start">المتبقي</th>
                        <th className="border p-2 text-start">الكمية *</th>
                        <th className="border p-2 text-start">سعر الوحدة *</th>
                        <th className="border p-2 text-start">الخصم</th>
                        <th className="border p-2 text-start">الضريبة</th>
                        <th className="border p-2 text-start">ملاحظات</th>
                      </tr>
                    </thead>
                    <tbody>
                      {fields.map((field, index) => {
                        const poLine = purchaseOrder.lines?.find((l) => l.id === field.purchaseOrderDetailId);
                        const item = items?.find((i) => i.id === field.itemId);
                        return (
                          <tr key={field.id} className="border-b">
                            <td className="border p-2">{item?.name ?? field.itemId}</td>
                            <td className="border p-2 tabular-nums">{poLine?.orderedQuantity ?? '-'}</td>
                            <td className="border p-2 tabular-nums">{poLine?.remainingQuantity ?? '-'}</td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.quantity`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.unitPrice`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.discountAmount`, { valueAsNumber: true })}
                                className="w-20"
                              />
                            </td>
                            <td className="border p-2">
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                {...register(`lines.${index}.taxAmount`, { valueAsNumber: true })}
                                className="w-20"
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
              <Button type="button" variant="outline" onClick={() => navigate('/procurement/supplier-invoices')}>
                إلغاء
              </Button>
              <Button type="submit" disabled={createInvoice.isPending}>
                {createInvoice.isPending ? 'جاري الإنشاء...' : 'إنشاء'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </Page>
  );
}
