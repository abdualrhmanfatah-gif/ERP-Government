import { useEffect, useMemo, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { DeductionType } from '../web-api-client';
import { deductionTypeLabels } from '../features/payments/payment-orders/shared/types';
import { createPaymentOrderSchema, updatePaymentOrderSchema, type CreatePaymentOrderFormData } from '../features/payments/payment-orders/shared/schemas';
import { useCreatePaymentOrder, useUpdatePaymentOrder } from '../features/payments/payment-orders/hooks/usePaymentOrders';
import { useFundsList } from '../features/budgeting/funds/hooks/useFunds';
import { useFiscalYearByDate } from '../features/accounting/hooks/useFiscalYearByDate';

import { useAccountsList } from '../features/accounting/hooks/useAccountsList';
import { useCurrenciesList } from '../features/accounting/hooks/useCurrenciesList';
import { useBudgetsList } from '../features/budgeting/hooks/useBudgets';
import { useBudgetItemAllocationsList } from '../features/budgeting/hooks/useBudgetItemAllocations';
import { BudgetItemAllocationsClient } from '../web-api-client';
import { useQuery } from '@tanstack/react-query';
import { useBankAccountsList } from '../features/payments/bank-accounts/hooks/useBankAccounts';
import { PaymentMethod, paymentMethodLabels } from '../features/payments/shared/types';
import { notify } from '../features/notifications/notify';
import { Button, Card, Combobox, Input, Select, Badge, MoneyDisplay, Switch } from './ui';
import { toDateInput } from '@/shared/utils/formatters';

interface PaymentOrderDeduction {
  deductionType: DeductionType;
  accountId: number;
  amount: number;
  isMandatory: boolean;
  isTaxDeduction: boolean;
  description: string;
}

function createEmptyDeduction(): PaymentOrderDeduction {
  return { deductionType: DeductionType.Tax, accountId: 0, amount: 0, isMandatory: false, isTaxDeduction: false, description: '' };
}

interface DeductionRaw {
  deductionType?: DeductionType;
  accountId?: number;
  amount?: number;
  isMandatory?: boolean;
  isTaxDeduction?: boolean;
  description?: string;
}

function toDeductionList(raw: unknown[]): PaymentOrderDeduction[] {
  return (raw ?? []).map((d) => {
    const r = d as DeductionRaw;
    return {
      deductionType: r.deductionType ?? DeductionType.Tax,
      accountId: r.accountId ?? 0,
      amount: r.amount ?? 0,
      isMandatory: r.isMandatory ?? false,
      isTaxDeduction: r.isTaxDeduction ?? false,
      description: r.description ?? '',
    };
  });
}

interface PaymentOrderFormProps {
  mode: 'create' | 'detail';
  initialData?: {
    id: number;
    paymentOrderNumber?: string;
    paymentOrderDate?: string;
    dueDate?: string;
    paymentOrderType?: string;
    fundId?: number;
    fiscalYearId?: number;
    budgetItemAllocationId?: number;
    currencyId?: number;
    amountGross?: number;
    deductionAmount?: number;
    beneficiaryName?: string;
    beneficiaryAccountNumber?: string;
    beneficiaryBankName?: string;
    paymentMethod?: string;
    bankAccountId?: number;
    notes?: string;
    accountId?: number;
    accrualJournalEntryId?: number;
    accrualEntryNumber?: string | null;
    deductions?: unknown[];
    rowVersion?: string | number[];
    status?: string;
  };
  editing?: boolean;
  onToggleEditing?: (editing: boolean) => void;
  onStateChange?: (state: { canSave: boolean; isSaving: boolean }) => void;
  onSaved?: () => void;
  onCancel?: () => void;
}

export function PaymentOrderForm({
  mode,
  initialData,
  editing = false,
  onToggleEditing,
  onStateChange,
  onSaved,
  onCancel,
}: PaymentOrderFormProps) {
  const createOrder = useCreatePaymentOrder();
  const updateOrder = useUpdatePaymentOrder();

  const { data: funds = [] } = useFundsList(true);
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const { data: currencies = [] } = useCurrenciesList();
  const { data: bankAccounts = [] } = useBankAccountsList();

  const isDetail = mode === 'detail';
  const isEditable = isDetail ? editing : true;

  const defaultValues: CreatePaymentOrderFormData = isDetail
    ? {
        paymentOrderDate: toDateInput(initialData?.paymentOrderDate),
        dueDate: toDateInput(initialData?.dueDate),
        paymentOrderType: initialData?.paymentOrderType ?? 'Standard',
        fundId: initialData?.fundId ?? 0,
        fiscalYearId: initialData?.fiscalYearId ?? 0,
        budgetItemAllocationId: initialData?.budgetItemAllocationId ?? 0,
        currencyId: initialData?.currencyId ?? 0,
        amountGross: initialData?.amountGross ?? 0,
        deductionAmount: initialData?.deductionAmount ?? 0,
        beneficiaryName: initialData?.beneficiaryName ?? '',
        beneficiaryAccountNumber: initialData?.beneficiaryAccountNumber ?? '',
        beneficiaryBankName: initialData?.beneficiaryBankName ?? '',
        paymentMethod: initialData?.paymentMethod,
        bankAccountId: initialData?.bankAccountId,
        notes: initialData?.notes ?? '',
        accountId: initialData?.accountId ?? undefined,
        accrualJournalEntryId: initialData?.accrualJournalEntryId ?? undefined,
        deductions: [],
      }
    : {
        paymentOrderDate: new Date().toISOString().split('T')[0],
        paymentOrderType: initialData?.paymentOrderType ?? 'Standard',
        fundId: 0,
        fiscalYearId: initialData?.fiscalYearId ?? 0,
        budgetItemAllocationId: 0,
        currencyId: initialData?.currencyId ?? 0,
        amountGross: initialData?.amountGross ?? 0,
        deductionAmount: 0,
        beneficiaryName: initialData?.beneficiaryName ?? '',
        paymentMethod: PaymentMethod.Cash,
        notes: initialData?.notes ?? '',
        accrualJournalEntryId: initialData?.accrualJournalEntryId ?? undefined,
        deductions: [],
      };

  const {
    register,
    handleSubmit,
    watch,
    control,
    reset,
    setValue,
    formState: { errors },
  } = useForm<CreatePaymentOrderFormData>({
    resolver: zodResolver(isDetail ? updatePaymentOrderSchema : createPaymentOrderSchema),
    defaultValues,
  });

  const watchedFundId = watch('fundId');
  const watchedDate = watch('paymentOrderDate');
  const { data: fiscalYearData } = useFiscalYearByDate(watchedDate || '');

  const { data: budgets = [] } = useBudgetsList({
    fundId: watchedFundId || undefined,
    fiscalYearId: fiscalYearData?.fiscalYearId || (isDetail ? initialData?.fiscalYearId : undefined) || undefined,
  });

  const allocationDetailId = isDetail && initialData?.budgetItemAllocationId ? initialData.budgetItemAllocationId : 0;
  const allocationClient = useMemo(() => new BudgetItemAllocationsClient(), []);
  const { data: allocationDetail } = useQuery({
    queryKey: ['payment-order-allocation-detail', allocationDetailId],
    queryFn: () => allocationClient.budgetItemAllocationsGET(allocationDetailId),
    enabled: allocationDetailId > 0,
  });

  const selectedBudgetId = isDetail && initialData?.budgetItemAllocationId && allocationDetail?.budgetId
    ? allocationDetail.budgetId
    : (budgets[0]?.id ?? 0);
  const { data: allocationsData } = useBudgetItemAllocationsList(selectedBudgetId);
  const allocations = Array.isArray(allocationsData) ? allocationsData : [];

  // Auto-set fiscalYearId from date
  useEffect(() => {
    if (fiscalYearData?.fiscalYearId) {
      setValue('fiscalYearId', fiscalYearData.fiscalYearId);
    }
  }, [fiscalYearData, setValue]);

  const [deductions, setDeductions] = useState<PaymentOrderDeduction[]>(() =>
    isDetail ? toDeductionList(initialData?.deductions ?? []) : [],
  );

  useEffect(() => {
    if (isDetail && initialData) {
      reset({
        paymentOrderDate: toDateInput(initialData.paymentOrderDate),
        dueDate: toDateInput(initialData.dueDate),
        paymentOrderType: initialData.paymentOrderType ?? 'Standard',
        fundId: initialData.fundId ?? 0,
        fiscalYearId: initialData.fiscalYearId ?? 0,
        budgetItemAllocationId: initialData.budgetItemAllocationId ?? 0,
        currencyId: initialData.currencyId ?? 0,
        amountGross: initialData.amountGross ?? 0,
        deductionAmount: initialData.deductionAmount ?? 0,
        beneficiaryName: initialData.beneficiaryName ?? '',
        beneficiaryAccountNumber: initialData.beneficiaryAccountNumber ?? '',
        beneficiaryBankName: initialData.beneficiaryBankName ?? '',
        paymentMethod: initialData.paymentMethod,
        bankAccountId: initialData.bankAccountId,
        notes: initialData.notes ?? '',
        accountId: initialData.accountId ?? undefined,
        accrualJournalEntryId: initialData.accrualJournalEntryId ?? undefined,
        deductions: [],
      });
      setDeductions(toDeductionList(initialData.deductions ?? []));
    }
  }, [isDetail, initialData, reset]);

  const watchedAmountGross = watch('amountGross') ?? 0;
  const deductionsSum = useMemo(() => deductions.reduce((s, d) => s + d.amount, 0), [deductions]);
  const netAmount = watchedAmountGross - deductionsSum;
  const canSave = deductionsSum <= watchedAmountGross;
  const isSaving = createOrder.isPending || updateOrder.isPending;

  // Sync deductionAmount from deductions sum
  useEffect(() => {
    setValue('deductionAmount', deductionsSum);
  }, [deductionsSum, setValue]);

  useEffect(() => {
    onStateChange?.({ canSave, isSaving });
  }, [canSave, isSaving, onStateChange]);

  function addDeduction() {
    setDeductions((prev) => [...prev, createEmptyDeduction()]);
  }

  function removeDeduction(i: number) {
    setDeductions((prev) => prev.filter((_, idx) => idx !== i));
  }

  function updateDeduction(i: number, field: keyof PaymentOrderDeduction, value: unknown) {
    setDeductions((prev) => prev.map((d, idx) => (idx === i ? { ...d, [field]: value } : d)));
  }

  async function onSubmit(data: CreatePaymentOrderFormData) {
    try {
      if (isDetail && initialData?.id) {
        await updateOrder.mutateAsync({
          id: initialData.id,
          command: {
            id: initialData.id,
            rowVersion: typeof initialData.rowVersion === 'string'
              ? initialData.rowVersion
              : '',
            paymentOrderDate: new Date(data.paymentOrderDate),
            dueDate: data.dueDate ? new Date(data.dueDate) : undefined,
            paymentOrderType: data.paymentOrderType,
            fundId: data.fundId,
            fiscalYearId: data.fiscalYearId,
            budgetClassificationId: data.budgetClassificationId,
            costCenterId: data.costCenterId,
            purchaseOrderId: data.purchaseOrderId,
            encumbranceId: data.encumbranceId,
            budgetItemAllocationId: data.budgetItemAllocationId,
            currencyId: data.currencyId,
            exchangeRate: data.exchangeRate,
            amountGross: data.amountGross,
            deductionAmount: data.deductionAmount,
            paymentMethod: data.paymentMethod as PaymentMethod | undefined,
            bankAccountId: data.bankAccountId,
            beneficiaryName: data.beneficiaryName,
            beneficiaryAccountNumber: data.beneficiaryAccountNumber,
            beneficiaryBankName: data.beneficiaryBankName,
            notes: data.notes,
            accountId: data.accountId,
            accrualJournalEntryId: data.accrualJournalEntryId,
            deductions: deductions.map((d) => ({
              deductionType: d.deductionType,
              accountId: d.accountId,
              amount: d.amount,
              isMandatory: d.isMandatory,
              isTaxDeduction: d.isTaxDeduction,
              description: d.description,
            })),
          } as any,
        });
        notify({ type: 'success', title: 'تم حفظ التعديلات' });
        onToggleEditing?.(false);
        onSaved?.();
      } else {
        await createOrder.mutateAsync({
          paymentOrderDate: new Date(data.paymentOrderDate),
          dueDate: data.dueDate ? new Date(data.dueDate) : undefined,
          paymentOrderType: data.paymentOrderType,
          fundId: data.fundId,
          fiscalYearId: data.fiscalYearId || undefined,
          budgetItemAllocationId: data.budgetItemAllocationId,
          budgetClassificationId: data.budgetClassificationId,
          costCenterId: data.costCenterId,
          purchaseOrderId: data.purchaseOrderId,
          encumbranceId: data.encumbranceId,
          currencyId: data.currencyId,
          exchangeRate: data.exchangeRate,
          amountGross: data.amountGross,
          deductionAmount: data.deductionAmount,
          paymentMethod: data.paymentMethod as PaymentMethod | undefined,
          bankAccountId: data.bankAccountId,
          beneficiaryName: data.beneficiaryName,
          beneficiaryAccountNumber: data.beneficiaryAccountNumber,
          beneficiaryBankName: data.beneficiaryBankName,
          notes: data.notes,
          accountId: data.accountId,
          accrualJournalEntryId: data.accrualJournalEntryId,
          deductions: deductions.map((d) => ({
            deductionType: d.deductionType,
            accountId: d.accountId,
            amount: d.amount,
            isMandatory: d.isMandatory,
            isTaxDeduction: d.isTaxDeduction,
            description: d.description,
          })),
        } as any);
        notify({ type: 'success', title: 'تم إنشاء أمر الدفع بنجاح' });
        onSaved?.();
      }
    } catch (err: unknown) {
      const problem = err as { detail?: string; message?: string };
      notify({ type: 'error', title: problem.detail ?? problem.message ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  const accountMap = useMemo(() => new Map(accounts.map((a) => [a.id, `${a.code} - ${a.name}`])), [accounts]);

  return (
    <form
      id={isDetail ? 'payment-order-detail-form' : 'payment-order-form'}
      onSubmit={handleSubmit(onSubmit)}
      aria-label={isDetail ? 'تفاصيل أمر الدفع' : 'إنشاء أمر دفع'}
    >
      <input
        type="hidden"
        {...register('accrualJournalEntryId', {
          setValueAs: (value) => value === '' || value == null ? undefined : Number(value),
        })}
      />
      {/* ── Header ─────────────────────────────────────────────────── */}
      <Card variant="default" padding="none">
        <div className="px-4 py-2.5 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <h2 className="text-sm font-bold text-[var(--color-on-surface)]">بيانات أمر الدفع</h2>
        </div>
        <div className="p-4 space-y-3">
          {/* Row 1: dates + fund */}
          <div className="grid grid-cols-12 gap-3">
            <div className="col-span-4">
              <Input
                id="paymentOrderDate"
                type="date"
                label="التاريخ"
                {...register('paymentOrderDate')}
                error={errors.paymentOrderDate?.message}
                disabled={!isEditable}
                required
              />
            </div>
            <div className="col-span-4">
              <Input
                id="dueDate"
                type="date"
                label="تاريخ الاستحقاق"
                {...register('dueDate')}
                disabled={!isEditable}
              />
            </div>
            <div className="col-span-4">
              <Controller
                control={control}
                name="fundId"
                render={({ field }) => (
                  <Combobox
                    label="الصندوق"
                    value={field.value ? String(field.value) : ''}
                    onChange={(val) => field.onChange(val ? Number(val) : 0)}
                    options={funds.map((f) => ({ value: String(f.id), label: `${f.fundNumber} — ${f.fundName}` }))}
                    placeholder="اختر صندوق..."
                    searchPlaceholder="بحث بالرقم أو الاسم..."
                    emptyMessage="لا توجد نتائج"
                    error={errors.fundId?.message}
                    disabled={!isEditable}
                  />
                )}
              />
            </div>
            {isDetail && initialData?.paymentOrderNumber && (
              <div className="col-span-4">
                <Input
                  id="paymentOrderNumber"
                  label="رقم الأمر"
                  value={initialData.paymentOrderNumber}
                  disabled
                />
              </div>
            )}
          </div>

          {/* Row 2: allocation + currency + amount */}
          <div className="grid grid-cols-12 gap-3">
            <div className="col-span-5">
              <Controller
                control={control}
                name="budgetItemAllocationId"
                render={({ field }) => (
                  <Combobox
                    label="التخصيص"
                    value={field.value ? String(field.value) : ''}
                    onChange={(val) => {
                      const allocationId = val ? Number(val) : 0;
                      field.onChange(allocationId);
                      const selected = allocations.find((a) => a.id === allocationId);
                      if (selected) {
                        setValue('accountId', selected.accountId ?? undefined);
                        setValue('costCenterId', selected.costCenterId ?? undefined);
                        setValue('budgetClassificationId', selected.budgetClassificationId ?? undefined);
                      }
                    }}
                    options={allocations.map((a) => ({ value: String(a.id), label: `${a.budgetItemCode} - ${a.budgetItemName}` }))}
                    placeholder="اختر تخصيص..."
                    searchPlaceholder="بحث برقم البند أو الاسم..."
                    emptyMessage="لا توجد تخصيصات"
                    error={errors.budgetItemAllocationId?.message}
                    disabled={!isEditable}
                  />
                )}
              />
            </div>
            <div className="col-span-3">
              <Controller
                control={control}
                name="currencyId"
                render={({ field }) => (
                  <Combobox
                    label="العملة"
                    value={field.value ? String(field.value) : ''}
                    onChange={(val) => field.onChange(val ? Number(val) : 0)}
                    options={currencies.map((c) => ({ value: String(c.id), label: `${c.code} - ${c.name}` }))}
                    placeholder="اختر العملة..."
                    searchPlaceholder="بحث بالرمز أو الاسم..."
                    emptyMessage="لا توجد نتائج"
                    error={errors.currencyId?.message}
                    disabled={!isEditable}
                  />
                )}
              />
            </div>
            <div className="col-span-4">
              <Input
                id="amountGross"
                type="number"
                step="0.01"
                label="المبلغ الإجمالي"
                {...register('amountGross', { valueAsNumber: true })}
                error={errors.amountGross?.message}
                disabled={!isEditable}
                required
              />
            </div>
          </div>
          {watch('budgetItemAllocationId') > 0 && (() => {
            const selected = allocations.find((a) => a.id === watch('budgetItemAllocationId'));
            if (!selected) return null;
            return (
              <div className="flex items-center gap-4 text-xs text-[var(--color-on-surface-variant)]">
                {selected.approvedAmount != null && (
                  <span>الموافق عليه: <MoneyDisplay value={selected.approvedAmount ?? 0} /></span>
                )}
              </div>
            );
          })()}

          {/* Row 3: beneficiary */}
          <div className="grid grid-cols-12 gap-3">
            <div className={watch('paymentMethod') === PaymentMethod.Check ? 'col-span-4' : 'col-span-12'}>
              <Input
                id="beneficiaryName"
                type="text"
                label="اسم المستفيد"
                {...register('beneficiaryName')}
                error={errors.beneficiaryName?.message}
                disabled={!isEditable}
                required
              />
            </div>
            {watch('paymentMethod') === PaymentMethod.Check && (
              <>
                <div className="col-span-4">
                  <Input
                    id="beneficiaryAccountNumber"
                    type="text"
                    label="رقم حساب المستفيد"
                    {...register('beneficiaryAccountNumber')}
                    disabled={!isEditable}
                  />
                </div>
                <div className="col-span-4">
                  <Input
                    id="beneficiaryBankName"
                    type="text"
                    label="بنك المستفيد"
                    {...register('beneficiaryBankName')}
                    disabled={!isEditable}
                  />
                </div>
              </>
            )}
          </div>

          {/* Row 4: payment method + bank + notes */}
          <div className="grid grid-cols-12 gap-3">
            <div className="col-span-3">
              <Controller
                control={control}
                name="paymentMethod"
                render={({ field }) => (
                  <Select
                    label="طريقة الدفع"
                    value={field.value ? String(field.value) : ''}
                    onChange={(e) => {
                      const val = e.target.value || undefined;
                      field.onChange(val);
                      if (val !== PaymentMethod.Check) {
                        setValue('bankAccountId', undefined);
                      }
                    }}
                    options={[
                      { value: PaymentMethod.Cash, label: paymentMethodLabels[PaymentMethod.Cash] },
                      { value: PaymentMethod.Check, label: paymentMethodLabels[PaymentMethod.Check] },
                    ]}
                    disabled={!isEditable}
                  />
                )}
              />
            </div>
            {watch('paymentMethod') === PaymentMethod.Check && (
              <div className="col-span-4">
                <Controller
                  control={control}
                  name="bankAccountId"
                  render={({ field }) => (
                    <Combobox
                      label="الحساب البنكي"
                      value={field.value ? String(field.value) : ''}
                      onChange={(val) => field.onChange(val ? Number(val) : undefined)}
                      options={bankAccounts.map((ba: any) => ({ value: String(ba.id), label: `${ba.bankName} - ${ba.name}` }))}
                      placeholder="اختر الحساب البنكي..."
                      searchPlaceholder="بحث بالاسم..."
                      emptyMessage="لا توجد حسابات بنكية"
                      error={errors.bankAccountId?.message}
                      disabled={!isEditable}
                    />
                  )}
                />
              </div>
            )}
            <div className={watch('paymentMethod') === PaymentMethod.Check ? 'col-span-5' : 'col-span-9'}>
              <Input
                id="notes"
                type="text"
                label="ملاحظات"
                {...register('notes')}
                placeholder="ملاحظات..."
                disabled={!isEditable}
              />
            </div>
          </div>
        </div>
      </Card>

      {/* ── Deductions ──────────────────────────────────────────────── */}
      <Card variant="default" padding="none" className="mt-4">
        <div className="px-4 py-2.5 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)] flex items-center justify-between">
          <h2 className="text-sm font-bold text-[var(--color-on-surface)]">الخصومات</h2>
          <div className="flex items-center gap-3">
            {deductions.length > 0 && (
              <Badge variant={deductionsSum <= watchedAmountGross ? 'success' : 'error'}>
                {deductions.length} خصم
              </Badge>
            )}
            {isEditable && (
              <Button type="button" variant="secondary" size="sm" onClick={addDeduction}>
                + إضافة خصم
              </Button>
            )}
          </div>
        </div>
        <div className="p-4">
          {deductions.length === 0 ? (
            <div className="text-center py-6 rounded-lg border-2 border-dashed border-[var(--color-outline-variant)]">
              <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد خصومات</p>
            </div>
          ) : (
            <div className="overflow-visible rounded-lg border border-[var(--color-outline-variant)]">
              <table className="min-w-full divide-y border-[var(--color-outline-variant)]">
                <thead>
                  <tr className="bg-[var(--color-surface-container-low)]">
                    <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">#</th>
                    <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">النوع</th>
                    <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">الحساب</th>
                    <th className="px-3 py-2 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">المبلغ</th>
                    <th className="px-3 py-2 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">إلزامي</th>
                    {isEditable && <th className="px-3 py-2" />}
                  </tr>
                </thead>
                <tbody className="divide-y border-[var(--color-outline-variant)]">
                  {deductions.map((deduction, index) => (
                    <tr key={index} className="bg-[var(--color-surface)]">
                      <td className="px-3 py-2 text-xs text-[var(--color-on-surface-variant)]">{index + 1}</td>
                      <td className="px-3 py-2">
                        {isEditable ? (
                          <Select
                            label="نوع الخصم"
                            value={String(deduction.deductionType)}
                            onChange={(e) => updateDeduction(index, 'deductionType', e.target.value as DeductionType)}
                            options={Object.entries(deductionTypeLabels).map(([v, l]) => ({ value: v, label: l }))}
                            className="min-w-[100px]"
                          />
                        ) : (
                          <span className="text-xs">{deductionTypeLabels[deduction.deductionType] ?? deduction.deductionType}</span>
                        )}
                      </td>
                      <td className="px-3 py-2">
                        {isEditable ? (
                          <Combobox
                            value={deduction.accountId ? String(deduction.accountId) : ''}
                            onChange={(val) => updateDeduction(index, 'accountId', val ? Number(val) : 0)}
                            options={accounts.map((a) => ({ value: String(a.id), label: `${a.code} - ${a.name}` }))}
                            placeholder="اختر الحساب..."
                            searchPlaceholder="بحث بالرمز أو الاسم..."
                            emptyMessage="لا توجد نتائج"
                          />
                        ) : (
                          <span className="text-xs font-bold">{accountMap.get(deduction.accountId) || deduction.accountId}</span>
                        )}
                      </td>
                      <td className="px-3 py-2">
                        {isEditable ? (
                          <Input
                            type="number"
                            step="0.01"
                            value={deduction.amount || ''}
                            onChange={(e) => updateDeduction(index, 'amount', Number(e.target.value))}
                            className="max-w-[140px]"
                          />
                        ) : (
                          <span className="text-xs tabular-nums font-bold"><MoneyDisplay value={deduction.amount} /></span>
                        )}
                      </td>
                      <td className="px-3 py-2">
                        {isEditable ? (
                          <Switch
                            checked={deduction.isMandatory}
                            onChange={(checked) => updateDeduction(index, 'isMandatory', checked)}
                            size="sm"
                          />
                        ) : (
                          <span className="text-xs">{deduction.isMandatory ? 'نعم' : 'لا'}</span>
                        )}
                      </td>
                      {isEditable && (
                        <td className="px-3 py-2">
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => removeDeduction(index)}
                            className="text-[var(--color-error)] hover:text-[var(--color-error)]"
                          >
                            حذف
                          </Button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {isEditable && (
            <div className="mt-3 text-sm text-[var(--color-on-surface-variant)]">
              مجموع الخصومات: <MoneyDisplay value={deductionsSum} />
            </div>
          )}

          {!isEditable && (
            <div className="mt-3 grid grid-cols-3 gap-4 text-sm">
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الإجمالي:</span>{' '}
                <MoneyDisplay value={watchedAmountGross} />
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الخصومات:</span>{' '}
                <MoneyDisplay value={deductionsSum} />
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الصافي:</span>{' '}
                <MoneyDisplay value={netAmount} />
              </div>
            </div>
          )}
        </div>
      </Card>

      {isDetail && editing && (
        <div className="flex items-center gap-2 mt-4">
          <Button type="submit" variant="primary" disabled={!canSave || isSaving} loading={isSaving}>
            حفظ التعديلات
          </Button>
          <Button type="button" variant="ghost" disabled={isSaving} onClick={onCancel}>
            إلغاء
          </Button>
        </div>
      )}
    </form>
  );
}
