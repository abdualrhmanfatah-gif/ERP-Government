import { useEffect } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Card, CardHeader, CardTitle, CardContent, Input, Textarea, Combobox, EmptyState } from '@/components/ui';
import { Plus, Trash2 } from 'lucide-react';
import { createPurchaseOrderSchema, type CreatePurchaseOrderFormData } from '@/features/procurement/purchase-orders/shared/schemas';
import { useExchangeRateLookup } from '@/features/procurement/purchase-orders/shared/catalog-hooks';
import type { ComboboxOption } from '@/components/ui/Combobox';

interface PurchaseOrdersFormProps {
  initialData?: CreatePurchaseOrderFormData;
  onSubmit: (data: CreatePurchaseOrderFormData) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  onEdit?: () => void;
  isCreate?: boolean;
  supplierOptions: ComboboxOption[];
  itemOptions: ComboboxOption[];
  unitOptions: ComboboxOption[];
  warehouseOptions: ComboboxOption[];
  locationOptions: ComboboxOption[];
  currencyOptions: ComboboxOption[];
  currencies: { id: number; code: string; nameAr: string; isBase: boolean }[];
}

function computeLineTotals(line: {
  orderedQuantity: number;
  unitPrice: number;
  discountPercent?: number | null;
  taxPercent?: number | null;
}) {
  const qty = line.orderedQuantity || 0;
  const price = line.unitPrice || 0;
  const sub = qty * price;
  const discPct = line.discountPercent ?? 0;
  const discAmt = sub * (discPct / 100);
  const net = sub - discAmt;
  const taxPct = line.taxPercent ?? 0;
  const taxAmt = net * (taxPct / 100);
  const total = net;
  const totalWithTax = net + taxAmt;
  return { subTotal: sub, discountAmount: discAmt, netUnitPrice: price * (1 - discPct / 100), taxAmount: taxAmt, lineTotal: total, lineTotalWithTax: totalWithTax };
}

export function ProcurementPurchaseOrdersForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  onEdit,
  isCreate = false,
  supplierOptions,
  itemOptions,
  unitOptions,
  warehouseOptions,
  locationOptions,
  currencyOptions,
  currencies,
}: PurchaseOrdersFormProps) {
  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<CreatePurchaseOrderFormData>({
    resolver: zodResolver(createPurchaseOrderSchema),
    defaultValues: initialData ?? {
      supplierPartyId: 0,
      lines: [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'lines',
  });

  const watchedLines = watch('lines');

  const computedLines = watchedLines.map((line) => computeLineTotals({
    orderedQuantity: line.orderedQuantity ?? 0,
    unitPrice: line.unitPrice ?? 0,
    discountPercent: line.discountPercent,
    taxPercent: line.taxPercent,
  }));

  const totals = computedLines.reduce(
    (acc, cl) => ({
      subTotal: acc.subTotal + cl.subTotal,
      discountAmount: acc.discountAmount + cl.discountAmount,
      taxAmount: acc.taxAmount + cl.taxAmount,
    }),
    { subTotal: 0, discountAmount: 0, taxAmount: 0 },
  );
  const grandTotal = totals.subTotal - totals.discountAmount + totals.taxAmount;

  const watchedCurrencyCode = watch('currencyCode');
  const baseCurrency = currencies.find((c) => c.isBase);
  const selectedCurrency = currencies.find((c) => c.code === watchedCurrencyCode);
  const isNonBaseCurrency = selectedCurrency && baseCurrency && selectedCurrency.id !== baseCurrency.id;

  const exchangeRateQuery = useExchangeRateLookup(
    baseCurrency?.id ?? 0,
    selectedCurrency?.id ?? 0,
    !!isNonBaseCurrency,
  );

  useEffect(() => {
    if (exchangeRateQuery.data?.rate) {
      setValue('exchangeRate', exchangeRateQuery.data.rate, { shouldValidate: true });
    }
  }, [exchangeRateQuery.data?.rate, setValue]);

  function addLine() {
    append({
      purchaseRequestDetailId: null,
      quotationDetailId: null,
      itemId: 0,
      unitId: 0,
      orderedQuantity: 1,
      unitPrice: 0,
      discountPercent: null,
      taxPercent: null,
      expectedDeliveryDate: null,
      notes: null,
    });
  }

  if (readOnly) {
    const itemMap = new Map(itemOptions.map((o) => [o.value, o.label]));
    const unitMap = new Map(unitOptions.map((o) => [o.value, o.label]));

    return (
      <div className="space-y-4">
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>بيانات أمر الشراء</CardTitle>
              {onEdit && (
                <Button variant="outline" size="sm" onClick={onEdit}>
                  تعديل
                </Button>
              )}
            </div>
          </CardHeader>
          <CardContent className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">المورد</span>
              <span>{supplierOptions.find((s) => s.value === String(initialData!.supplierPartyId))?.label ?? `#${initialData!.supplierPartyId}`}</span>
            </div>
            {initialData!.warehouseId && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">المستودع</span>
                <span>{warehouseOptions.find((w) => w.value === String(initialData!.warehouseId))?.label ?? `#${initialData!.warehouseId}`}</span>
              </div>
            )}
            {initialData!.paymentTerms && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">شروط الدفع</span>
                <span>{initialData!.paymentTerms}</span>
              </div>
            )}
            {initialData!.deliveryTerms && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">شروط التسليم</span>
                <span>{initialData!.deliveryTerms}</span>
              </div>
            )}
            {initialData!.expectedDeliveryDate && (
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">تاريخ التسليم المتوقع</span>
                <span>{new Date(initialData!.expectedDeliveryDate).toLocaleDateString('ar-YE')}</span>
              </div>
            )}
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
              <CardTitle>بنود أمر الشراء</CardTitle>
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
                      <th className="text-start px-4 py-3 font-semibold">الخصم %</th>
                      <th className="text-start px-4 py-3 font-semibold">الضريبة %</th>
                      <th className="text-start px-4 py-3 font-semibold">الإجمالي</th>
                    </tr>
                  </thead>
                  <tbody>
                    {initialData!.lines.map((line, idx) => {
                      const cl = computeLineTotals({
                        orderedQuantity: line.orderedQuantity,
                        unitPrice: line.unitPrice,
                        discountPercent: line.discountPercent,
                        taxPercent: line.taxPercent,
                      });
                      return (
                        <tr key={idx} className="border-b border-[var(--color-outline-variant)] last:border-b-0">
                          <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{idx + 1}</td>
                          <td className="px-4 py-3">{itemMap.get(String(line.itemId)) ?? `بند #${line.itemId}`}</td>
                          <td className="px-4 py-3">{unitMap.get(String(line.unitId)) ?? `وحدة #${line.unitId}`}</td>
                          <td className="px-4 py-3 tabular-nums">{line.orderedQuantity}</td>
                          <td className="px-4 py-3 tabular-nums font-mono">{line.unitPrice.toLocaleString('ar-YE')}</td>
                          <td className="px-4 py-3 tabular-nums">{line.discountPercent ?? '-'}</td>
                          <td className="px-4 py-3 tabular-nums">{line.taxPercent ?? '-'}</td>
                          <td className="px-4 py-3 tabular-nums font-mono">{cl.lineTotalWithTax.toLocaleString('ar-YE')}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                  <tfoot>
                    <tr className="bg-[var(--color-surface-container-low)] font-semibold border-t-2 border-[var(--color-outline-variant)]">
                      <td colSpan={7} className="px-4 py-3 text-start">الإجمالي</td>
                      <td className="px-4 py-3 tabular-nums font-mono font-bold">{grandTotal.toLocaleString('ar-YE')}</td>
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
    <form onSubmit={handleSubmit(onSubmit)} aria-label="أمر شراء">
      <Card className="mb-6">
        <CardHeader>
          <CardTitle>بيانات أمر الشراء</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
            <Combobox
              label="المورد *"
              options={supplierOptions}
              value={String(watch('supplierPartyId') || '')}
              onChange={(val) => setValue('supplierPartyId', Number(val) || 0, { shouldValidate: true })}
              placeholder="اختر المورد..."
              searchPlaceholder="بحث..."
              error={errors.supplierPartyId?.message}
            />
            <Combobox
              label="المستودع"
              options={warehouseOptions}
              value={String(watch('warehouseId') || '')}
              onChange={(val) => setValue('warehouseId', Number(val) || 0, { shouldValidate: true })}
              placeholder="اختر المستودع..."
              searchPlaceholder="بحث..."
              error={errors.warehouseId?.message}
            />
            <Combobox
              label="موقع التسليم"
              options={locationOptions}
              value={String(watch('deliveryLocationId') || '')}
              onChange={(val) => setValue('deliveryLocationId', Number(val) || 0, { shouldValidate: true })}
              placeholder="اختر الموقع..."
              searchPlaceholder="بحث..."
              error={errors.deliveryLocationId?.message}
            />
            <Combobox
              label="العملة"
              options={currencyOptions}
              value={String(watch('currencyCode') || '')}
              onChange={(val) => setValue('currencyCode', val || '', { shouldValidate: true })}
              placeholder="اختر العملة..."
              searchPlaceholder="بحث..."
              error={errors.currencyCode?.message}
            />
            <Input
              label="تاريخ التسليم المتوقع"
              type="date"
              {...register('expectedDeliveryDate')}
              error={errors.expectedDeliveryDate?.message}
            />
            <div className="col-span-2">
              <Input
                label="شروط الدفع"
                {...register('paymentTerms')}
                error={errors.paymentTerms?.message}
              />
            </div>
            <div className="col-span-2">
              <Input
                label="شروط التسليم"
                {...register('deliveryTerms')}
                error={errors.deliveryTerms?.message}
              />
            </div>
            <div className="col-span-4">
              <Textarea
                label="ملاحظات"
                rows={1}
                {...register('notes')}
                error={errors.notes?.message}
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="mb-6">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>بنود أمر الشراء</CardTitle>
            <Button type="button" variant="outline" size="sm" onClick={addLine} aria-label="إضافة بند">
              <Plus className="h-4 w-4 ms-2" /> إضافة بند
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {errors.lines?.message && (
            <p className="text-sm text-[var(--color-error)] mb-4">{errors.lines.message}</p>
          )}
          {fields.length === 0 ? (
            <EmptyState message="لا توجد بنود. اضغط &quot;إضافة بند&quot; لإضافة بند أول." />
          ) : (
            <>
              <div className="overflow-x-auto border border-[var(--color-outline-variant)] rounded-xl">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="border-b-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
                      <th className="text-start px-3 py-3 font-semibold w-10">#</th>
                      <th className="text-start px-3 py-3 font-semibold w-[22%]">الصنف</th>
                      <th className="text-start px-3 py-3 font-semibold w-[14%]">الوحدة</th>
                      <th className="text-start px-3 py-3 font-semibold w-[10%]">الكمية</th>
                      <th className="text-start px-3 py-3 font-semibold w-[12%]">سعر الوحدة</th>
                      <th className="text-start px-3 py-3 font-semibold w-[9%]">خصم %</th>
                      <th className="text-start px-3 py-3 font-semibold w-[9%]">ضريبة %</th>
                      <th className="text-start px-3 py-3 font-semibold w-[12%]">الإجمالي</th>
                      <th className="w-10" />
                    </tr>
                  </thead>
                  <tbody>
                    {fields.map((field, index) => {
                      const cl = computedLines[index] ?? { lineTotalWithTax: 0 };
                      return (
                        <tr key={field.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_3%,transparent)]">
                          <td className="px-3 py-3 text-[var(--color-on-surface-variant)]">{index + 1}</td>
                          <td className="px-3 py-3">
                            <Combobox
                              options={itemOptions}
                              value={String(field.itemId || '')}
                              onChange={(val) => setValue(`lines.${index}.itemId`, Number(val), { shouldValidate: true })}
                              placeholder="اختر..."
                              searchPlaceholder="بحث..."
                              error={errors.lines?.[index]?.itemId?.message}
                            />
                          </td>
                          <td className="px-3 py-3">
                            <Combobox
                              options={unitOptions}
                              value={String(field.unitId || '')}
                              onChange={(val) => setValue(`lines.${index}.unitId`, Number(val), { shouldValidate: true })}
                              placeholder="اختر..."
                              searchPlaceholder="بحث..."
                              error={errors.lines?.[index]?.unitId?.message}
                            />
                          </td>
                          <td className="px-3 py-3">
                            <Input
                              type="number"
                              min="0.01"
                              step="0.01"
                              {...register(`lines.${index}.orderedQuantity`, { valueAsNumber: true })}
                              error={errors.lines?.[index]?.orderedQuantity?.message}
                            />
                          </td>
                          <td className="px-3 py-3">
                            <Input
                              type="number"
                              min="0"
                              step="0.01"
                              {...register(`lines.${index}.unitPrice`, { valueAsNumber: true })}
                              error={errors.lines?.[index]?.unitPrice?.message}
                            />
                          </td>
                          <td className="px-3 py-3">
                            <Input
                              type="number"
                              min="0"
                              max="100"
                              step="0.01"
                              {...register(`lines.${index}.discountPercent`, { valueAsNumber: true })}
                              error={errors.lines?.[index]?.discountPercent?.message}
                            />
                          </td>
                          <td className="px-3 py-3">
                            <Input
                              type="number"
                              min="0"
                              max="100"
                              step="0.01"
                              {...register(`lines.${index}.taxPercent`, { valueAsNumber: true })}
                              error={errors.lines?.[index]?.taxPercent?.message}
                            />
                          </td>
                          <td className="px-3 py-3 tabular-nums font-mono text-[var(--color-on-surface-variant)]">
                            {cl.lineTotalWithTax > 0 ? cl.lineTotalWithTax.toLocaleString('ar-YE') : '-'}
                          </td>
                          <td className="px-3 py-3">
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon"
                              onClick={() => remove(index)}
                              aria-label="حذف البند"
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
                      <td colSpan={5} className="px-4 py-3 text-start">المجموع الفرعي</td>
                      <td colSpan={2} className="px-4 py-3 tabular-nums font-mono">{totals.subTotal.toLocaleString('ar-YE')}</td>
                      <td />
                    </tr>
                    <tr className="bg-[var(--color-surface-container-low)]">
                      <td colSpan={5} className="px-4 py-3 text-start">الخصم</td>
                      <td colSpan={2} className="px-4 py-3 tabular-nums font-mono text-[var(--color-error)]">-{totals.discountAmount.toLocaleString('ar-YE')}</td>
                      <td />
                    </tr>
                    <tr className="bg-[var(--color-surface-container-low)]">
                      <td colSpan={5} className="px-4 py-3 text-start">الضريبة</td>
                      <td colSpan={2} className="px-4 py-3 tabular-nums font-mono">+{totals.taxAmount.toLocaleString('ar-YE')}</td>
                      <td />
                    </tr>
                    <tr className="bg-[var(--color-surface-container-low)] font-bold border-t-2 border-[var(--color-outline-variant)]">
                      <td colSpan={5} className="px-4 py-3 text-start">الإجمالي</td>
                      <td colSpan={2} />
                      <td className="px-4 py-3 tabular-nums font-mono text-lg">{grandTotal.toLocaleString('ar-YE')}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
              <p className="text-xs text-[var(--color-on-surface-variant)] mt-2">
                * الحقول المطلوبة — يجب اختيار الصنف والوحدة والكمية وسعر الوحدة لكل بند
              </p>
            </>
          )}
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
          {isCreate ? 'إنشاء أمر الشراء' : 'حفظ'}
        </Button>
      </div>
    </form>
  );
}
