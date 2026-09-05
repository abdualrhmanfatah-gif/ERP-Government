import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { CurrencyDto } from '../types';
import { Input } from '@/components/ui/Input';
import { Switch } from '@/components/ui/Switch';
import { Button } from '@/components/ui/Button';

const schema = z.object({
  code: z.string().min(1, 'الرمز مطلوب').max(10, 'الرمز يجب ألا يتجاوز 10 أحرف'),
  name: z.string().min(1, 'الاسم مطلوب').max(100, 'الاسم يجب ألا يتجاوز 100 حرف'),
  symbol: z.string().min(1, 'الرمز الرمزي مطلوب').max(5, 'الرمز الرمزي يجب ألا يتجاوز 5 أحرف'),
  decimalPlaces: z.coerce.number().int().min(0).max(6, 'الحالات العشرية يجب أن تكون بين 0 و 6'),
  roundingPrecision: z.coerce.number().positive('دقة التحديد يجب أن تكون أكبر من 0'),
  isBase: z.boolean(),
});

type FormData = z.infer<typeof schema>;

interface CurrencyFormProps {
  initialData?: CurrencyDto;
  onSubmit: (data: FormData) => void;
  serverError?: string;
  loading?: boolean;
}

export function CurrencyForm({ initialData, onSubmit, serverError, loading }: CurrencyFormProps) {
  const isEdit = !!initialData;
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: initialData ? {
      code: initialData.code ?? '',
      name: initialData.name ?? '',
      symbol: initialData.symbol ?? '',
      decimalPlaces: initialData.decimalPlaces ?? 2,
      roundingPrecision: initialData.roundingPrecision ?? 0.01,
      isBase: initialData.isBase ?? false,
    } : undefined,
    defaultValues: {
      code: '',
      name: '',
      symbol: '',
      decimalPlaces: 2,
      roundingPrecision: 0.01,
      isBase: false,
    },
  });

  const isBase = watch('isBase');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
      <Input
        label="الرمز"
        {...register('code')}
        error={errors.code?.message}
        required
        disabled={isEdit}
        dir="ltr"
        className="text-start"
      />
      <Input label="الاسم" {...register('name')} error={errors.name?.message} required />
      <Input
        label="الرمز الرمزي"
        {...register('symbol')}
        error={errors.symbol?.message}
        required
        dir="ltr"
        className="text-start"
      />
      <Input
        label="الحالات العشرية"
        type="number"
        {...register('decimalPlaces')}
        error={errors.decimalPlaces?.message}
        dir="ltr"
        className="text-start"
      />
      <Input
        label="دقة التحديد"
        type="number"
        step="0.01"
        {...register('roundingPrecision')}
        error={errors.roundingPrecision?.message}
        dir="ltr"
        className="text-start"
      />
      {!isEdit ? (
        <div className="mb-4">
          <Switch
            checked={isBase}
            onChange={(v) => setValue('isBase', v, { shouldValidate: true })}
            label="العملة الأساسية"
          />
        </div>
      ) : null}
      {serverError ? (
        <div
          role="alert"
          className="px-3 py-2 mb-4 rounded-lg text-sm bg-[var(--color-error-container)] text-[var(--color-on-error-container)]"
        >
          {serverError}
        </div>
      ) : null}
      <Button type="submit" variant="primary" loading={loading}>
        {isEdit ? 'حفظ التعديلات' : 'إنشاء العملة'}
      </Button>
    </form>
  );
}
