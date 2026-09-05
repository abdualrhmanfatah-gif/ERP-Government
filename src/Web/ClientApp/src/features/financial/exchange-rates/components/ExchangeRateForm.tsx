import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { DatePicker } from '@/components/ui/DatePicker';
import { exchangeRateSchema, type ExchangeRateFormData } from '@/shared/utils/validators/financial';

interface ExchangeRateFormProps {
  currencies: Array<{ id?: number; code?: string; name?: string }>;
  initialData?: {
    baseCurrencyId?: number;
    currencyId?: number;
    rateDate?: Date;
    rateType?: number;
    rate?: number;
  };
  onSubmit: (data: Record<string, unknown>) => void;
  serverError?: string;
  loading?: boolean;
}

export function ExchangeRateForm({
  currencies,
  initialData,
  onSubmit,
  serverError,
  loading,
}: ExchangeRateFormProps) {
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<ExchangeRateFormData>({
    resolver: zodResolver(exchangeRateSchema),
    defaultValues: {
      baseCurrencyId: initialData?.baseCurrencyId ?? 0,
      currencyId: initialData?.currencyId ?? 0,
      rateDate: initialData?.rateDate ? new Date(initialData.rateDate) : undefined,
      rateType: initialData?.rateType != null
        ? (initialData.rateType === 1 ? 'Market' : 'Official')
        : undefined,
      rate: initialData?.rate ?? 0,
    },
  });

  const baseCurrencyId = watch('baseCurrencyId');
  const currencyId = watch('currencyId');
  const rateDate = watch('rateDate');
  const rateType = watch('rateType');

  const currencyOptions = [
    { value: '', label: '— اختر العملة —' },
    ...currencies.map((c) => ({
      value: String(c.id),
      label: `${c.code} — ${c.name}`,
    })),
  ];

  const onFormSubmit = (data: ExchangeRateFormData) => {
    onSubmit({
      baseCurrencyId: data.baseCurrencyId,
      currencyId: data.currencyId,
      rateDate: data.rateDate,
      rateType: data.rateType,
      rate: data.rate,
    });
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="flex flex-col gap-1">
      {serverError ? (
        <div role="alert" className="p-3 mb-2 text-sm text-[var(--color-error)] bg-[var(--color-error-container)] border border-[var(--color-error)] rounded">
          {serverError}
        </div>
      ) : null}

      <Select
        label="العملة الأساسية"
        options={currencyOptions}
        value={String(baseCurrencyId || '')}
        onChange={(e) => setValue('baseCurrencyId', e.target.value ? Number(e.target.value) : 0)}
        error={errors.baseCurrencyId?.message}
        required
      />

      <Select
        label="العملة الهدف"
        options={currencyOptions}
        value={String(currencyId || '')}
        onChange={(e) => setValue('currencyId', e.target.value ? Number(e.target.value) : 0)}
        error={errors.currencyId?.message}
        required
      />

      <DatePicker
        label="التاريخ"
        value={rateDate ? new Date(rateDate).toISOString().split('T')[0] : ''}
        onChange={(val) => setValue('rateDate', val ? new Date(val) : new Date())}
        error={errors.rateDate?.message}
        required
      />

      <Select
        label="نوع السعر"
        options={[
          { value: 'Official', label: 'رسمي' },
          { value: 'Market', label: 'سوق' },
        ]}
        value={rateType || ''}
        onChange={(e) => setValue('rateType', e.target.value as 'Official' | 'Market')}
        error={errors.rateType?.message}
      />

      <div className="mb-4">
        <label htmlFor="rate-value" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">
          السعر <span aria-hidden="true" className="text-[var(--color-error)] ms-0.5">*</span>
        </label>
        <input
          type="number"
          id="rate-value"
          step="0.000001"
          min="0"
          {...register('rate', { valueAsNumber: true })}
          aria-invalid={!!errors.rate}
          aria-describedby={errors.rate ? 'rate-value-error' : undefined}
          dir="ltr"
          className="w-full px-3 py-2 text-sm border border-[var(--color-border-input)] rounded-lg bg-[var(--color-surface-container-lowest)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-focus-ring)] focus:border-[var(--color-focus-ring)]"
        />
        {errors.rate?.message ? (
          <p id="rate-value-error" role="alert" className="mt-1 text-xs text-[var(--color-error)]">{errors.rate.message}</p>
        ) : null}
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" size="sm" loading={loading}>
          {initialData ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
