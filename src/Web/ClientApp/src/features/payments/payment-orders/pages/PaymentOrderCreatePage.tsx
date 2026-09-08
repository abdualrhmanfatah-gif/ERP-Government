import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { PaymentOrdersClient, CreatePaymentOrderCommand, PaymentOrderLineType, DeductionType } from '../../../../web-api-client';
import { createPaymentOrderSchema, type CreatePaymentOrderFormData } from '../shared/schemas';
import { Page, Button, Input, Select } from '@/components/ui';
import { ArrowRight, Plus, Trash2 } from 'lucide-react';

const client = new PaymentOrdersClient();

const lineTypeLabels: Record<number, string> = {
  [PaymentOrderLineType.Invoice]: 'فاتورة',
  [PaymentOrderLineType.Contract]: 'عقد',
  [PaymentOrderLineType.PurchaseOrder]: 'طلب شراء',
  [PaymentOrderLineType.Other]: 'أخرى',
  [PaymentOrderLineType.Advance]: 'سلفة',
};

const deductionTypeLabels: Record<number, string> = {
  [DeductionType.Tax]: 'ضريبة',
  [DeductionType.Insurance]: 'تأمين',
  [DeductionType.Retention]: 'احتباس',
  [DeductionType.AdvancePayment]: 'دفعة مقدمة',
  [DeductionType.LoanDeduction]: 'قرض',
  [DeductionType.Penalty]: 'غرامة',
  [DeductionType.Other]: 'أخرى',
};

function createEmptyLine() {
  return {
    lineType: PaymentOrderLineType.Invoice,
    accountId: 0,
    amount: 0,
    description: '',
  };
}

function createEmptyDeduction() {
  return {
    deductionType: DeductionType.Tax,
    accountId: 0,
    amount: 0,
    isMandatory: false,
    isTaxDeduction: false,
    description: '',
  };
}

export function PaymentOrderCreatePage() {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [errors, setErrors] = useState<Record<string, string>>({});

  const [form, setForm] = useState<CreatePaymentOrderFormData>({
    paymentOrderDate: new Date().toISOString().split('T')[0],
    paymentOrderType: 'Standard',
    vendorId: 0,
    fundId: 0,
    fiscalYearId: 0,
    appropriationId: 0,
    currencyId: 0,
    amountGross: 0,
    deductionAmount: 0,
    beneficiaryName: '',
    lines: [createEmptyLine()],
    deductions: [],
  });

  const createMutation = useMutation({
    mutationFn: (cmd: CreatePaymentOrderCommand) => client.paymentOrdersPOST(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      navigate('/payments/payment-orders');
    },
  });

  const netAmount = useMemo(() => form.amountGross - form.deductionAmount, [form.amountGross, form.deductionAmount]);
  const linesSum = useMemo(() => form.lines.reduce((s, l) => s + l.amount, 0), [form.lines]);
  const deductionsSum = useMemo(() => form.deductions.reduce((s, d) => s + d.amount, 0), [form.deductions]);

  function updateField<K extends keyof CreatePaymentOrderFormData>(
    field: K,
    value: CreatePaymentOrderFormData[K],
  ) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
  }

  function updateLine(index: number, field: string, value: string | number | boolean) {
    setForm((prev) => {
      const lines = [...prev.lines];
      lines[index] = { ...lines[index], [field]: value };
      return { ...prev, lines };
    });
  }

  function addLine() {
    setForm((prev) => ({ ...prev, lines: [...prev.lines, createEmptyLine()] }));
  }

  function removeLine(index: number) {
    setForm((prev) => ({ ...prev, lines: prev.lines.filter((_, i) => i !== index) }));
  }

  function updateDeduction(index: number, field: string, value: string | number | boolean) {
    setForm((prev) => {
      const deductions = [...prev.deductions];
      deductions[index] = { ...deductions[index], [field]: value };
      return { ...prev, deductions };
    });
  }

  function addDeduction() {
    setForm((prev) => ({ ...prev, deductions: [...prev.deductions, createEmptyDeduction()] }));
  }

  function removeDeduction(index: number) {
    setForm((prev) => ({ ...prev, deductions: prev.deductions.filter((_, i) => i !== index) }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = createPaymentOrderSchema.safeParse(form);
    if (!result.success) {
      const fieldErrors: Record<string, string> = {};
      for (const issue of result.error.issues) {
        const key = issue.path[0] as string;
        if (!fieldErrors[key]) fieldErrors[key] = issue.message;
      }
      setErrors(fieldErrors);
      return;
    }

    try {
      await createMutation.mutateAsync({
        paymentOrderDate: new Date(result.data.paymentOrderDate),
        dueDate: result.data.dueDate ? new Date(result.data.dueDate) : undefined,
        paymentOrderType: result.data.paymentOrderType,
        vendorId: result.data.vendorId,
        fundId: result.data.fundId,
        fiscalYearId: result.data.fiscalYearId,
        appropriationId: result.data.appropriationId,
        budgetClassificationId: result.data.budgetClassificationId,
        costCenterId: result.data.costCenterId,
        projectId: result.data.projectId,
        currencyId: result.data.currencyId,
        exchangeRate: result.data.exchangeRate,
        amountGross: result.data.amountGross,
        deductionAmount: result.data.deductionAmount,
        beneficiaryName: result.data.beneficiaryName,
        beneficiaryIban: result.data.beneficiaryIban,
        beneficiaryAccountNumber: result.data.beneficiaryAccountNumber,
        beneficiaryBankName: result.data.beneficiaryBankName,
        notes: result.data.notes,
        lines: result.data.lines.map((l) => ({
          lineType: l.lineType as PaymentOrderLineType,
          accountId: l.accountId,
          amount: l.amount,
          description: l.description,
        })),
        deductions: result.data.deductions.map((d) => ({
          deductionType: d.deductionType as DeductionType,
          accountId: d.accountId,
          amount: d.amount,
          isMandatory: d.isMandatory ?? false,
          isTaxDeduction: d.isTaxDeduction ?? false,
          description: d.description,
        })),
      });
    } catch (err: unknown) {
      const problem = err as { detail?: string; message?: string };
      setErrors({ submit: problem.detail ?? problem.message ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  return (
    <Page
      title="أمر دفع جديد"
      maxWidth="lg"
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Header section */}
        <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <h2 className="text-sm font-semibold mb-4">بيانات أمر الدفع</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Input
              label="التاريخ *"
              type="date"
              value={form.paymentOrderDate}
              onChange={(e) => updateField('paymentOrderDate', e.target.value)}
            />
            {errors.paymentOrderDate && <p className="text-xs text-[var(--color-error)]">{errors.paymentOrderDate}</p>}

            <Input
              label="تاريخ الاستحقاق"
              type="date"
              value={form.dueDate ?? ''}
              onChange={(e) => updateField('dueDate', e.target.value || undefined)}
            />

            <Input
              label="المورد *"
              type="number"
              value={form.vendorId || ''}
              onChange={(e) => updateField('vendorId', Number(e.target.value))}
            />
            {errors.vendorId && <p className="text-xs text-[var(--color-error)]">{errors.vendorId}</p>}

            <Input
              label="الصندوق *"
              type="number"
              value={form.fundId || ''}
              onChange={(e) => updateField('fundId', Number(e.target.value))}
            />
            {errors.fundId && <p className="text-xs text-[var(--color-error)]">{errors.fundId}</p>}

            <Input
              label="السنة المالية *"
              type="number"
              value={form.fiscalYearId || ''}
              onChange={(e) => updateField('fiscalYearId', Number(e.target.value))}
            />
            {errors.fiscalYearId && <p className="text-xs text-[var(--color-error)]">{errors.fiscalYearId}</p>}

            <Input
              label="التخصيص *"
              type="number"
              value={form.appropriationId || ''}
              onChange={(e) => updateField('appropriationId', Number(e.target.value))}
            />
            {errors.appropriationId && <p className="text-xs text-[var(--color-error)]">{errors.appropriationId}</p>}

            <Input
              label="المبلغ الإجمالي *"
              type="number"
              step="0.01"
              value={form.amountGross || ''}
              onChange={(e) => updateField('amountGross', Number(e.target.value))}
            />
            {errors.amountGross && <p className="text-xs text-[var(--color-error)]">{errors.amountGross}</p>}

            <Input
              label="مبلغ الخصم"
              type="number"
              step="0.01"
              value={form.deductionAmount || ''}
              onChange={(e) => updateField('deductionAmount', Number(e.target.value))}
            />

            <Input
              label="اسم المستفيد *"
              type="text"
              value={form.beneficiaryName}
              onChange={(e) => updateField('beneficiaryName', e.target.value)}
            />
            {errors.beneficiaryName && <p className="text-xs text-[var(--color-error)]">{errors.beneficiaryName}</p>}
          </div>
        </section>

        {/* Lines section */}
        <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold">الأسطر</h2>
            <Button type="button" variant="ghost" size="sm" onClick={addLine}>
              <Plus size={14} className="ms-1" />
              إضافة سطر
            </Button>
          </div>
          {errors.lines && <p className="text-xs text-[var(--color-error)] mb-2">{errors.lines}</p>}
          <div className="space-y-3">
            {form.lines.map((line, i) => (
              <div key={i} className="grid grid-cols-1 md:grid-cols-4 gap-3 items-end border border-[var(--color-outline-variant)] rounded p-3">
                <Select
                  label="النوع"
                  value={String(line.lineType)}
                  onChange={(e) => updateLine(i, 'lineType', Number(e.target.value))}
                  options={Object.entries(lineTypeLabels).map(([v, l]) => ({ value: v, label: l }))}
                />
                <Input
                  label="الحساب *"
                  type="number"
                  value={line.accountId || ''}
                  onChange={(e) => updateLine(i, 'accountId', Number(e.target.value))}
                />
                <Input
                  label="المبلغ *"
                  type="number"
                  step="0.01"
                  value={line.amount || ''}
                  onChange={(e) => updateLine(i, 'amount', Number(e.target.value))}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeLine(i)}
                  disabled={form.lines.length <= 1}
                  aria-label="حذف السطر"
                >
                  <Trash2 size={14} />
                </Button>
              </div>
            ))}
          </div>
          <div className="mt-3 text-sm text-[var(--color-on-surface-variant)]">
            مجموع الأسطر: <span className="font-mono">{linesSum.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
            {' · '}
            الصافي: <span className="font-mono">{netAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
        </section>

        {/* Deductions section */}
        <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold">الخصومات</h2>
            <Button type="button" variant="ghost" size="sm" onClick={addDeduction}>
              <Plus size={14} className="ms-1" />
              إضافة خصم
            </Button>
          </div>
          <div className="space-y-3">
            {form.deductions.map((deduction, i) => (
              <div key={i} className="grid grid-cols-1 md:grid-cols-5 gap-3 items-end border border-[var(--color-outline-variant)] rounded p-3">
                <Select
                  label="النوع"
                  value={String(deduction.deductionType)}
                  onChange={(e) => updateDeduction(i, 'deductionType', Number(e.target.value))}
                  options={Object.entries(deductionTypeLabels).map(([v, l]) => ({ value: v, label: l }))}
                />
                <Input
                  label="الحساب *"
                  type="number"
                  value={deduction.accountId || ''}
                  onChange={(e) => updateDeduction(i, 'accountId', Number(e.target.value))}
                />
                <Input
                  label="المبلغ *"
                  type="number"
                  step="0.01"
                  value={deduction.amount || ''}
                  onChange={(e) => updateDeduction(i, 'amount', Number(e.target.value))}
                />
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={deduction.isMandatory ?? false}
                    onChange={(e) => updateDeduction(i, 'isMandatory', e.target.checked)}
                  />
                  إلزامي
                </label>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeDeduction(i)}
                  aria-label="حذف الخصم"
                >
                  <Trash2 size={14} />
                </Button>
              </div>
            ))}
          </div>
          <div className="mt-3 text-sm text-[var(--color-on-surface-variant)]">
            مجموع الخصومات: <span className="font-mono">{deductionsSum.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
        </section>

        {/* Totals preview */}
        <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <h2 className="text-sm font-semibold mb-3">الملخص</h2>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div>الإجمالي: <span className="font-mono">{form.amountGross.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
            <div>الخصومات: <span className="font-mono">{form.deductionAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
            <div>الصافي: <span className="font-mono">{netAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
          </div>
        </section>

        {errors.submit && <p className="text-sm text-[var(--color-error)]">{errors.submit}</p>}

        <div className="flex justify-end gap-2">
          <Button variant="ghost" type="button" onClick={() => navigate(-1)}>إلغاء</Button>
          <Button variant="primary" type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'جارٍ الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </Page>
  );
}
