import { useMemo, useState } from 'react';
import { Button, MoneyDisplay, EmptyState, Input, Select, Label } from '@/components/ui';
import { useEligibleVouchers } from '../hooks/useEligibleVouchers';
import { depositSlipFormTypeLabels } from '../shared/types';
import { FormType } from '../../../web-api-client';

interface DepositSlipFormProps {
  onSubmit: (data: { slipDate: string; formType: FormType; voucherIds: number[] }) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: {
    slipDate: string;
    formType: FormType;
    voucherIds: number[];
  };
  onEdit?: () => void;
}

export function DepositSlipForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
}: DepositSlipFormProps) {
  const [formType, setFormType] = useState<FormType>(initialData?.formType ?? FormType.Form47);
  const [slipDate, setSlipDate] = useState(initialData?.slipDate ?? new Date().toISOString().slice(0, 10));
  const [selectedIds, setSelectedIds] = useState<number[]>(initialData?.voucherIds ?? []);

  const { eligible, isLoading } = useEligibleVouchers(
    formType === FormType.Form47 ? 'Form47' : 'Form48',
  );

  const total = useMemo(
    () =>
      eligible
        .filter((v) => v.id != null && selectedIds.includes(v.id))
        .reduce((sum, v) => sum + (v.totalAmount ?? 0), 0),
    [eligible, selectedIds],
  );

  const toggle = (id: number) =>
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  const selectAll = () => setSelectedIds(eligible.map((v) => v.id!).filter(Boolean));
  const clearAll = () => setSelectedIds([]);

  if (readOnly && initialData) {
    return (
      <div className="space-y-4">
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold">بيانات البطاقة</h3>
            {onEdit && (
              <Button variant="outline" size="sm" onClick={onEdit}>
                تعديل
              </Button>
            )}
          </div>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <span className="text-[var(--color-on-surface-variant)]">التاريخ:</span>{' '}
              <span>{new Date(initialData.slipDate).toLocaleDateString('ar-YE')}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">النوع:</span>{' '}
              <span>{depositSlipFormTypeLabels[initialData.formType] ?? initialData.formType}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">عدد الأعضاء:</span>{' '}
              <span>{initialData.voucherIds.length}</span>
            </div>
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الإجمالي:</span>{' '}
              <MoneyDisplay value={total} />
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex gap-4 items-end">
        <Select
          label="نوع البطاقة"
          value={formType}
          onChange={(e) => {
            setFormType(e.target.value as FormType);
            setSelectedIds([]);
          }}
          options={Object.entries(depositSlipFormTypeLabels).map(([val, label]) => ({ value: val, label }))}
        />

        <Input
          label="تاريخ البطاقة"
          type="date"
          value={slipDate}
          onChange={(e) => setSlipDate(e.target.value)}
          max={new Date().toISOString().slice(0, 10)}
        />
      </div>

      {isLoading ? (
        <p className="text-sm text-[var(--color-on-surface-variant)] text-center py-8">جاري التحميل...</p>
      ) : eligible.length === 0 ? (
        <EmptyState message="لا توجد سندات مؤهلة لهذا النوع" />
      ) : (
        <>
          <div className="flex gap-2 mb-2">
            <Button variant="outline" onClick={selectAll}>
              تحديد الكل
            </Button>
            <Button variant="outline" onClick={clearAll}>
              إلغاء التحديد
            </Button>
            <span className="ms-auto text-sm text-[var(--color-on-surface-variant)]">
              {selectedIds.length} من {eligible.length}
            </span>
          </div>

          <div className="border rounded divide-y max-h-96 overflow-y-auto">
            {eligible.map((v) => (
              <Label
                key={v.id}
                className="flex items-center gap-3 px-4 py-3 hover:bg-[var(--color-surface-container)] cursor-pointer"
              >
                <input
                  type="checkbox"
                  checked={v.id != null && selectedIds.includes(v.id)}
                  onChange={() => toggle(v.id ?? 0)}
                />
                <span className="font-medium tabular-nums">{v.voucherNumber}</span>
                <span className="text-[var(--color-on-surface-variant)] text-sm">{v.receivedFrom}</span>
                <span className="ms-auto">
                  <MoneyDisplay value={v.totalAmount ?? 0} />
                </span>
              </Label>
            ))}
          </div>

          <div className="flex items-center justify-between border-t pt-4">
            <div className="text-lg font-semibold">
              الإجمالي: <MoneyDisplay value={total} />
            </div>
            <div className="flex gap-2">
              <Button variant="outline" onClick={onCancel}>
                إلغاء
              </Button>
              <Button
                onClick={() => onSubmit({ slipDate, formType, voucherIds: selectedIds })}
                disabled={isPending || selectedIds.length === 0}
                loading={isPending}
              >
                إنشاء البطاقة
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
