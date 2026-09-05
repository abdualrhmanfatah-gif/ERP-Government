import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';

const fiscalPeriodSchema = z.object({
  name: z.string().min(1, 'اسم الفترة مطلوب'),
  periodNumber: z.number().int().min(1).max(12),
  startDate: z.string().min(1, 'تاريخ البداية مطلوب'),
  endDate: z.string().min(1, 'تاريخ النهاية مطلوب'),
}).refine((data) => new Date(data.endDate) > new Date(data.startDate), {
  message: 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية',
  path: ['endDate'],
});

type FiscalPeriodFormData = z.infer<typeof fiscalPeriodSchema>;

interface FiscalPeriodFormProps {
  fiscalYearId: number;
  fiscalYearStart: string;
  fiscalYearEnd: string;
  initialData?: {
    periodNumber?: number;
    name?: string;
    startDate?: string;
    endDate?: string;
  };
  onSubmit: (data: FiscalPeriodFormData) => void;
  loading?: boolean;
  serverError?: string;
}

export function FiscalPeriodForm({
  fiscalYearId: _fiscalYearId,
  fiscalYearStart,
  fiscalYearEnd,
  initialData,
  onSubmit,
  loading = false,
  serverError,
}: FiscalPeriodFormProps) {
  const isEditMode = !!initialData;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FiscalPeriodFormData>({
    resolver: zodResolver(fiscalPeriodSchema),
    defaultValues: {
      name: initialData?.name ?? '',
      periodNumber: initialData?.periodNumber ?? 1,
      startDate: initialData?.startDate ?? fiscalYearStart,
      endDate: initialData?.endDate ?? fiscalYearEnd,
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      {serverError && (
        <div className="rounded-md bg-[var(--color-error-container)] p-3 text-sm text-[var(--color-error)]">
          {serverError}
        </div>
      )}

      <Input
        label="اسم الفترة"
        {...register('name')}
        error={errors.name?.message}
        placeholder="مثال: يناير 2026"
      />

      {isEditMode ? (
        <div className="flex flex-col gap-1">
          <label className="text-sm font-medium text-[var(--color-on-surface-variant)]">رقم الفترة</label>
          <div className="rounded-md border border-[var(--color-border-container)] bg-[var(--color-surface-container)] px-3 py-2 text-sm text-[var(--color-on-surface-variant)]">
            {initialData?.periodNumber}
          </div>
        </div>
      ) : (
        <Input
          label="رقم الفترة"
          type="number"
          {...register('periodNumber', { valueAsNumber: true })}
          error={errors.periodNumber?.message}
        />
      )}

      <Input
        label="تاريخ البداية"
        type="date"
        {...register('startDate')}
        error={errors.startDate?.message}
      />

      <Input
        label="تاريخ النهاية"
        type="date"
        {...register('endDate')}
        error={errors.endDate?.message}
      />

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" loading={loading}>
          حفظ
        </Button>
      </div>
    </form>
  );
}
