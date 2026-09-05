import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';

const fiscalYearSchema = z.object({
  name: z.string().min(1, 'اسم السنة المالية مطلوب'),
  yearNumber: z.number().int().min(2000).max(2100),
  startDate: z.string().min(1, 'تاريخ البداية مطلوب'),
  endDate: z.string().min(1, 'تاريخ النهاية مطلوب'),
}).refine((data) => new Date(data.endDate) > new Date(data.startDate), {
  message: 'تاريخ النهاية يجب أن يكون بعد تاريخ البداية',
  path: ['endDate'],
});

type FiscalYearFormData = z.infer<typeof fiscalYearSchema>;

interface FiscalYearFormProps {
  initialData?: {
    name?: string;
    yearNumber?: number;
    startDate?: string;
    endDate?: string;
  };
  onSubmit: (data: FiscalYearFormData) => void;
  loading?: boolean;
  serverError?: string;
}

export function FiscalYearForm({
  initialData,
  onSubmit,
  loading = false,
  serverError,
}: FiscalYearFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FiscalYearFormData>({
    resolver: zodResolver(fiscalYearSchema),
    defaultValues: {
      name: initialData?.name ?? '',
      yearNumber: initialData?.yearNumber ?? new Date().getFullYear(),
      startDate: initialData?.startDate ?? '',
      endDate: initialData?.endDate ?? '',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      {serverError && (
        <div className="rounded-md bg-[var(--color-error)]/10 p-3 text-sm text-[var(--color-error)]">
          {serverError}
        </div>
      )}

      <Input
        label="اسم السنة المالية"
        {...register('name')}
        error={errors.name?.message}
        placeholder="مثال: السنة المالية 2026"
      />

      <Input
        label="السنة"
        type="number"
        {...register('yearNumber', { valueAsNumber: true })}
        error={errors.yearNumber?.message}
      />

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
