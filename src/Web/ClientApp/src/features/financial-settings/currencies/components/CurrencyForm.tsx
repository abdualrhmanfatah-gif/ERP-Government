import { useState } from 'react';
import { Button, Input, Switch, FormField } from '@/components/ui';
import { Iso4217Picker } from '@/components/FinancialSettingsIso4217Picker';
import type { Iso4217CodeDto } from '../../shared/types';

interface CurrencyFormProps {
  onSubmit: (data: { code: string; name: string; symbol: string; decimalPlaces: number; roundingPrecision: number; isBase: boolean }) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  initialData?: { code: string; name: string; symbol: string; decimalPlaces: number; roundingPrecision: number; isBase: boolean };
  onEdit?: () => void;
}

export function CurrencyForm({
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  initialData,
  onEdit,
}: CurrencyFormProps) {
  const [selectedCode, setSelectedCode] = useState<Iso4217CodeDto | null>(
    initialData ? { code: initialData.code, name: initialData.name, decimalPlaces: initialData.decimalPlaces } as Iso4217CodeDto : null
  );
  const [name, setName] = useState(initialData?.name ?? '');
  const [symbol, setSymbol] = useState(initialData?.symbol ?? '');
  const [roundingPrecision, setRoundingPrecision] = useState(initialData?.roundingPrecision ?? 0.01);
  const [isBase, setIsBase] = useState(initialData?.isBase ?? false);

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!selectedCode) return;
    if (!symbol.trim()) return;
    onSubmit({
      code: selectedCode.code,
      name: name || selectedCode.name,
      symbol,
      decimalPlaces: selectedCode.decimalPlaces,
      roundingPrecision,
      isBase,
    });
  }

  if (readOnly && initialData) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات العملة</h3>
          {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div><span className="text-[var(--color-on-surface-variant)]">الكود:</span> <span dir="ltr">{initialData.code}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الاسم:</span> <span>{initialData.name}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الرمز:</span> <span>{initialData.symbol}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">الكسور العشرية:</span> <span>{initialData.decimalPlaces}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">دقة التقريب:</span> <span>{initialData.roundingPrecision}</span></div>
          <div><span className="text-[var(--color-on-surface-variant)]">عملة أساسية:</span> <span>{initialData.isBase ? 'نعم' : 'لا'}</span></div>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="عملة">
      <div className="space-y-4">
        <Iso4217Picker
          onSelect={(code) => { setSelectedCode(code); setName(code.name); }}
          selectedCode={selectedCode}
        />
        {selectedCode && (
          <>
            <FormField label="اسم العملة" htmlFor="currency-name">
              <Input id="currency-name" value={name} onChange={(e) => setName(e.target.value)} />
            </FormField>
            <FormField label="الرمز" htmlFor="currency-symbol" required>
              <Input id="currency-symbol" value={symbol} onChange={(e) => setSymbol(e.target.value)} placeholder="مثال: ﷼" />
            </FormField>
            <FormField label="الكسور العشرية" htmlFor="currency-decimal">
              <Input id="currency-decimal" type="number" value={selectedCode.decimalPlaces} disabled />
            </FormField>
            <FormField label="دقة التقريب" htmlFor="currency-rounding">
              <Input id="currency-rounding" type="number" min={0.01} step={0.01} value={roundingPrecision} onChange={(e) => setRoundingPrecision(Number(e.target.value))} />
            </FormField>
            <Switch id="currency-isBase" label="عملة أساسية" checked={isBase} onChange={setIsBase} />
          </>
        )}
      </div>
      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel}>إلغاء</Button>
        <Button type="submit" disabled={isPending || !selectedCode} loading={isPending}>حفظ</Button>
      </div>
    </form>
  );
}
