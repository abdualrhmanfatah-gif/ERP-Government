import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, Card, FormField, Input } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCreateCurrency } from '../../hooks/useCurrencies';
import { Iso4217Picker } from '@/components/FinancialSettingsIso4217Picker';
import type { Iso4217CodeDto } from '../../shared/types';

export default function CurrencyCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateCurrency();
  const [selectedCode, setSelectedCode] = useState<Iso4217CodeDto | null>(null);
  const [name, setName] = useState('');
  const [symbol, setSymbol] = useState('');
  const [roundingPrecision, setRoundingPrecision] = useState(0.01);
  const [isBase, setIsBase] = useState(false);

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!selectedCode) {
      notify({ type: 'error', title: 'اختر عملة من القائمة' });
      return;
    }
    if (!symbol.trim()) {
      notify({ type: 'error', title: 'الرمز مطلوب' });
      return;
    }

    createMutation.mutate(
      {
        code: selectedCode.code,
        name: name || selectedCode.name,
        symbol,
        decimalPlaces: selectedCode.decimalPlaces,
        roundingPrecision,
        isBase,
      },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم إنشاء العملة بنجاح' });
          navigate('/financial-settings/currencies');
        },
        onError: (err) => notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ أثناء الإنشاء' }),
      },
    );
  }

  return (
    <Page
      title="عملة جديدة"
      maxWidth="sm"
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/currencies')} className="cursor-pointer">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <Card className="bg-[var(--color-surface-container-lowest)]">
        <form onSubmit={handleSubmit} className="space-y-4">
          <Iso4217Picker
            onSelect={(code) => {
              setSelectedCode(code);
              setName(code.name);
            }}
            selectedCode={selectedCode}
          />

          {selectedCode && (
            <>
              <FormField label="اسم العملة" htmlFor="name">
                <Input
                  id="name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
              </FormField>
              <FormField label="الرمز" htmlFor="symbol" required>
                <Input
                  id="symbol"
                  value={symbol}
                  onChange={(e) => setSymbol(e.target.value)}
                  placeholder="مثال:﷼"
                />
              </FormField>
              <FormField label="الكسور العشرية" htmlFor="decimalPlaces">
                <Input
                  id="decimalPlaces"
                  type="number"
                  value={selectedCode.decimalPlaces}
                  disabled
                />
              </FormField>
              <FormField label="دقة التقريب" htmlFor="roundingPrecision">
                <Input
                  id="roundingPrecision"
                  type="number"
                  min={0.01}
                  step={0.01}
                  value={roundingPrecision}
                  onChange={(e) => setRoundingPrecision(Number(e.target.value))}
                />
              </FormField>
              <FormField label="العملة الأساسية" htmlFor="isBase">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    id="isBase"
                    type="checkbox"
                    checked={isBase}
                    onChange={(e) => setIsBase(e.target.checked)}
                    className="w-4 h-4 rounded border-[var(--color-border-container)] text-[var(--color-primary)] focus:ring-[var(--color-focus-ring)]"
                  />
                  <span className="text-sm text-[var(--color-on-surface)]">عملة أساسية</span>
                </label>
              </FormField>
            </>
          )}

          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="ghost" onClick={() => navigate('/financial-settings/currencies')} className="cursor-pointer">
              إلغاء
            </Button>
            <Button type="submit" disabled={createMutation.isPending || !selectedCode} className="cursor-pointer">
              {createMutation.isPending ? 'جاري الإنشاء...' : 'إنشاء'}
            </Button>
          </div>
        </form>
      </Card>
    </Page>
  );
}
