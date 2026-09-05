import { Button } from '@/components/ui/Button';
import type { CurrencyDto } from '../types';

interface CurrencyDetailProps {
  currency: CurrencyDto;
  onEdit?: (currency: CurrencyDto) => void;
}

export function CurrencyDetail({ currency, onEdit }: CurrencyDetailProps) {
  const fields = [
    { label: 'الرمز', value: currency.code },
    { label: 'الاسم', value: currency.name },
    { label: 'الرمز', value: currency.symbol },
    { label: 'الحالات العشرية', value: currency.decimalPlaces },
    { label: 'دقة التحديد', value: currency.roundingPrecision },
    { label: 'العملة الأساسية', value: currency.isBase ? 'نعم' : 'لا' },
    { label: 'الحالة', value: currency.isActive ? 'نشط' : 'غير نشط' },
  ];

  return (
    <div className="bg-[var(--color-surface-container-lowest)] border border-[var(--color-border-container)] rounded-lg p-5">
      <div className="grid grid-cols-[repeat(auto-fill,minmax(14rem,1fr))] gap-5">
        {fields.map((f) => (
          <div key={f.label}>
            <small className="text-[var(--color-on-surface-variant)] font-medium text-xs">{f.label}</small>
            <div className="text-[var(--color-on-surface)] font-medium mt-1">{f.value ?? '—'}</div>
          </div>
        ))}
      </div>
      {onEdit ? (
        <div className="mt-4">
          <Button variant="outline" size="sm" onClick={() => onEdit(currency)}>
            تعديل
          </Button>
        </div>
      ) : null}
    </div>
  );
}
