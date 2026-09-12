import { useEffect } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Trash2 } from 'lucide-react';
import {
  Button,
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Input,
  Textarea,
  Select,
  MoneyDisplay,
  FormField,
} from '@/components/ui';
import { createQuotationSchema, type CreateQuotationFormData } from '../shared/schemas';
import type { QuotationDetail } from '../shared/types';

interface SelectOption {
  value: string;
  label: string;
}

interface QuotationFormProps {
  initialData?: QuotationDetail;
  readOnly?: boolean;
  onSubmit?: (data: CreateQuotationFormData) => void;
  onCancel?: () => void;
  onEdit?: () => void;
  isPending?: boolean;
  supplierOptions: SelectOption[];
  currencyOptions: SelectOption[];
  itemOptions: SelectOption[];
  unitOptions: SelectOption[];
}

export function QuotationForm({
  initialData,
  readOnly = false,
  onSubmit,
  onCancel,
  onEdit,
  isPending = false,
  supplierOptions,
  currencyOptions,
  itemOptions,
  unitOptions,
}: QuotationFormProps) {
  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    formState: { errors },
  } = useForm<CreateQuotationFormData>({
    resolver: zodResolver(createQuotationSchema),
    defaultValues: initialData
      ? {
          supplierPartyId: initialData.supplierPartyId,
          quotationDate: initialData.quotationDate?.slice(0, 10) ?? '',
          validUntil: initialData.validUntil?.slice(0, 10) ?? '',
          currencyCode: initialData.currencyCode ?? '',
          exchangeRate: initialData.exchangeRate ?? undefined,
          shippingCost: initialData.shippingCost ?? undefined,
          otherCharges: initialData.otherCharges ?? undefined,
          paymentTerms: initialData.paymentTerms ?? '',
          deliveryTerms: initialData.deliveryTerms ?? '',
          leadTimeDays: initialData.leadTimeDays ?? undefined,
          warrantyPeriodMonths: initialData.warrantyPeriodMonths ?? undefined,
          notes: initialData.notes ?? '',
          lines:
            initialData.lines?.map((l) => ({
              purchaseRequestDetailId: l.purchaseRequestDetailId,
              itemId: l.itemId,
              unitId: l.unitId,
              quantity: l.quantity,
              unitPrice: l.unitPrice ?? 0,
              discountPercent: l.discountPercent ?? undefined,
              taxPercent: l.taxPercent ?? undefined,
              notes: l.notes ?? '',
            })) ?? [],
        }
      : {
          supplierPartyId: 0,
          quotationDate: '',
          validUntil: '',
          currencyCode: '',
          exchangeRate: undefined,
          shippingCost: undefined,
          otherCharges: undefined,
          paymentTerms: '',
          deliveryTerms: '',
          leadTimeDays: undefined,
          warrantyPeriodMonths: undefined,
          notes: '',
          lines: [
            {
              purchaseRequestDetailId: 0,
              itemId: 0,
              unitId: 0,
              quantity: 1,
              unitPrice: 0,
              discountPercent: undefined,
              taxPercent: undefined,
              notes: '',
            },
          ],
        },
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });
  const lines = watch('lines');

  useEffect(() => {
    if (initialData) {
      reset({
        supplierPartyId: initialData.supplierPartyId,
        quotationDate: initialData.quotationDate?.slice(0, 10) ?? '',
        validUntil: initialData.validUntil?.slice(0, 10) ?? '',
        currencyCode: initialData.currencyCode ?? '',
        exchangeRate: initialData.exchangeRate ?? undefined,
        shippingCost: initialData.shippingCost ?? undefined,
        otherCharges: initialData.otherCharges ?? undefined,
        paymentTerms: initialData.paymentTerms ?? '',
        deliveryTerms: initialData.deliveryTerms ?? '',
        leadTimeDays: initialData.leadTimeDays ?? undefined,
        warrantyPeriodMonths: initialData.warrantyPeriodMonths ?? undefined,
        notes: initialData.notes ?? '',
        lines:
          initialData.lines?.map((l) => ({
            purchaseRequestDetailId: l.purchaseRequestDetailId,
            itemId: l.itemId,
            unitId: l.unitId,
            quantity: l.quantity,
            unitPrice: l.unitPrice ?? 0,
            discountPercent: l.discountPercent ?? undefined,
            taxPercent: l.taxPercent ?? undefined,
            notes: l.notes ?? '',
          })) ?? [],
      });
    }
  }, [initialData, reset]);

  const computedTotals = (lines || []).reduce(
    (acc, line) => {
      const qty = line?.quantity || 0;
      const price = line?.unitPrice || 0;
      const disc = line?.discountPercent ? (qty * price * line.discountPercent) / 100 : 0;
      const net = qty * price - disc;
      const tax = line?.taxPercent ? (net * line.taxPercent) / 100 : 0;
      return {
        subTotal: acc.subTotal + net,
        discountAmount: acc.discountAmount + disc,
        taxAmount: acc.taxAmount + tax,
      };
    },
    { subTotal: 0, discountAmount: 0, taxAmount: 0 }
  );
  const grandTotal = computedTotals.subTotal - computedTotals.discountAmount + computedTotals.taxAmount;

  return (
    <form onSubmit={handleSubmit(onSubmit ?? (() => {}))} className="space-y-4">
      <Card>
        <CardHeader className="pb-4">
          <CardTitle className="text-base">بيانات العرض والبنود</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Header Fields */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <FormField label="المورد" required error={errors.supplierPartyId?.message}>
              <Select
                label=""
                options={supplierOptions}
                disabled={readOnly}
                {...register('supplierPartyId', { valueAsNumber: true })}
              />
            </FormField>

            <FormField label="تاريخ العرض" required error={errors.quotationDate?.message}>
              <Input type="date" disabled={readOnly} {...register('quotationDate')} />
            </FormField>

            <FormField label="صالح حتى">
              <Input type="date" disabled={readOnly} {...register('validUntil')} />
            </FormField>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <FormField label="العملة" error={errors.currencyCode?.message}>
              <Select
                label=""
                options={currencyOptions}
                disabled={readOnly}
                {...register('currencyCode')}
              />
            </FormField>

            <FormField label="شروط الدفع">
              <Input disabled={readOnly} {...register('paymentTerms')} />
            </FormField>

            <FormField label="شروط التسليم">
              <Input disabled={readOnly} {...register('deliveryTerms')} />
            </FormField>

            <FormField label="مدة التوريد (يوم)">
              <Input type="number" disabled={readOnly} {...register('leadTimeDays', { valueAsNumber: true })} />
            </FormField>
          </div>

          {/* Lines Table */}
          <div className="border-t pt-4">
            <div className="flex items-center justify-between mb-3">
              <h4 className="text-sm font-medium">بنود العرض</h4>
              {!readOnly && (
                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() =>
                    append({
                      purchaseRequestDetailId: 0,
                      itemId: 0,
                      unitId: 0,
                      quantity: 1,
                      unitPrice: 0,
                      discountPercent: undefined,
                      taxPercent: undefined,
                      notes: '',
                    })
                  }
                >
                  <Plus className="h-4 w-4 ms-1" />
                  إضافة بند
                </Button>
              )}
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--color-border-container)]">
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">الصنف</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">الوحدة</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">الكمية</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">سعر الوحدة</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">خصم %</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">ضريبة %</th>
                    <th className="p-2 text-start text-[var(--color-on-surface-variant)] font-medium">الإجمالي</th>
                    {!readOnly && <th className="p-2"></th>}
                  </tr>
                </thead>
                <tbody>
                  {fields.map((field, index) => {
                    const line = lines?.[index];
                    const qty = line?.quantity || 0;
                    const price = line?.unitPrice || 0;
                    const disc = line?.discountPercent ? (qty * price * line.discountPercent) / 100 : 0;
                    const net = qty * price - disc;
                    const tax = line?.taxPercent ? (net * line.taxPercent) / 100 : 0;
                    const total = net + tax;

                    return (
                      <tr key={field.id} className="border-b border-[var(--color-border-container)]">
                        <td className="p-1.5">
                          {readOnly ? (
                            <span className="text-sm">{itemOptions.find((o) => o.value === String(line?.itemId))?.label ?? '-'}</span>
                          ) : (
                            <select
                              className="w-full px-2 py-1.5 text-sm border border-[var(--color-border-input)] rounded-md bg-[var(--color-surface-container-lowest)]"
                              {...register(`lines.${index}.itemId`, { valueAsNumber: true })}
                            >
                              <option value="0">اختر...</option>
                              {itemOptions.map((opt) => (
                                <option key={opt.value} value={opt.value}>
                                  {opt.label}
                                </option>
                              ))}
                            </select>
                          )}
                        </td>
                        <td className="p-1.5">
                          {readOnly ? (
                            <span className="text-sm">{unitOptions.find((o) => o.value === String(line?.unitId))?.label ?? '-'}</span>
                          ) : (
                            <select
                              className="w-full px-2 py-1.5 text-sm border border-[var(--color-border-input)] rounded-md bg-[var(--color-surface-container-lowest)]"
                              {...register(`lines.${index}.unitId`, { valueAsNumber: true })}
                            >
                              <option value="0">اختر...</option>
                              {unitOptions.map((opt) => (
                                <option key={opt.value} value={opt.value}>
                                  {opt.label}
                                </option>
                              ))}
                            </select>
                          )}
                        </td>
                        <td className="p-1.5">
                          {readOnly ? (
                            <span className="text-sm">{line?.quantity}</span>
                          ) : (
                            <Input
                              type="number"
                              step="0.01"
                              className="w-20"
                              {...register(`lines.${index}.quantity`, { valueAsNumber: true })}
                            />
                          )}
                        </td>
                        <td className="p-1.5">
                          {readOnly ? (
                            <MoneyDisplay value={line?.unitPrice ?? 0} />
                          ) : (
                            <Input
                              type="number"
                              step="0.01"
                              className="w-24"
                              {...register(`lines.${index}.unitPrice`, { valueAsNumber: true })}
                            />
                          )}
                        </td>
                        <td className="p-1.5">
                          {readOnly ? (
                            <span className="text-sm">{line?.discountPercent ?? '-'}</span>
                          ) : (
                            <Input
                              type="number"
                              step="0.01"
                              className="w-16"
                              {...register(`lines.${index}.discountPercent`, { valueAsNumber: true })}
                            />
                          )}
                        </td>
                        <td className="p-1.5">
                          {readOnly ? (
                            <span className="text-sm">{line?.taxPercent ?? '-'}</span>
                          ) : (
                            <Input
                              type="number"
                              step="0.01"
                              className="w-16"
                              {...register(`lines.${index}.taxPercent`, { valueAsNumber: true })}
                            />
                          )}
                        </td>
                        <td className="p-1.5">
                          <MoneyDisplay value={total} className="font-medium" />
                        </td>
                        {!readOnly && (
                          <td className="p-1.5">
                            {fields.length > 1 && (
                              <Button type="button" size="sm" variant="ghost" onClick={() => remove(index)}>
                                <Trash2 className="h-4 w-4 text-[var(--color-error)]" />
                              </Button>
                            )}
                          </td>
                        )}
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {errors.lines && (
              <p className="text-[var(--color-error)] text-xs mt-2">{errors.lines.message}</p>
            )}
          </div>

          {/* Totals */}
          <div className="border-t pt-4 grid grid-cols-2 md:grid-cols-4 gap-3">
            <div>
              <span className="text-xs text-[var(--color-on-surface-variant)]">المجموع الفرعي</span>
              <MoneyDisplay value={computedTotals.subTotal} className="block text-sm font-medium" />
            </div>
            <div>
              <span className="text-xs text-[var(--color-on-surface-variant)]">الخصم</span>
              <MoneyDisplay value={computedTotals.discountAmount} className="block text-sm font-medium" />
            </div>
            <div>
              <span className="text-xs text-[var(--color-on-surface-variant)]">الضريبة</span>
              <MoneyDisplay value={computedTotals.taxAmount} className="block text-sm font-medium" />
            </div>
            <div>
              <span className="text-xs text-[var(--color-on-surface-variant)]">الإجمالي شامل الضريبة</span>
              <MoneyDisplay value={grandTotal} className="block text-sm font-bold text-[var(--color-primary)]" />
            </div>
          </div>

          {/* Notes */}
          <FormField label="ملاحظات">
            <Textarea rows={2} disabled={readOnly} {...register('notes')} />
          </FormField>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-end gap-2">
        {readOnly ? (
          <Button type="button" variant="outline" onClick={onEdit}>
            تعديل
          </Button>
        ) : (
          <>
            <Button type="button" variant="outline" onClick={onCancel}>
              إلغاء
            </Button>
            <Button type="submit" disabled={isPending} loading={isPending}>
              حفظ
            </Button>
          </>
        )}
      </div>
    </form>
  );
}
