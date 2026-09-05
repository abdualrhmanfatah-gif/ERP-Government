import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/Button';
import type { CreateMoveCommand } from '../types';

const schema = z.object({
  documentDate: z.string().min(1, 'تاريخ المستند مطلوب'),
  journalId: z.coerce.number().optional(),
  periodId: z.coerce.number().min(1, 'الفترة مطلوبة'),
  fiscalYearId: z.coerce.number().min(1, 'السنة المالية مطلوبة'),
  baseCurrencyId: z.coerce.number().min(1, 'العملة الأساسية مطلوبة'),
  narration: z.string().max(1000).optional(),
  ref: z.string().max(100).optional(),
  entryType: z.string().max(20).optional(),
});

export type MoveFormValues = z.infer<typeof schema>;

interface MoveFormProps {
  defaultValues?: Partial<MoveFormValues>;
  onSubmit: (data: CreateMoveCommand) => Promise<void> | void;
  submitting?: boolean;
  isEdit?: boolean;
}

export function MoveForm({ defaultValues, onSubmit, submitting, isEdit }: MoveFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<MoveFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      documentDate: defaultValues?.documentDate ?? new Date().toISOString().slice(0, 10),
      journalId: defaultValues?.journalId,
      periodId: defaultValues?.periodId,
      fiscalYearId: defaultValues?.fiscalYearId,
      baseCurrencyId: defaultValues?.baseCurrencyId ?? 1,
      narration: defaultValues?.narration ?? '',
      ref: defaultValues?.ref ?? '',
      entryType: defaultValues?.entryType ?? '',
    },
  });

  return (
    <form
      onSubmit={handleSubmit((v) => onSubmit(v as CreateMoveCommand))}
      className="space-y-4 bg-[var(--color-surface-container-lowest)] border border-[var(--color-border-container)] rounded-lg p-4"
      aria-label={isEdit ? 'تعديل القيد' : 'إنشاء قيد'}
    >
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label htmlFor="documentDate" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            تاريخ المستند *
          </label>
          <input
            id="documentDate"
            type="date"
            {...register('documentDate')}
            aria-invalid={!!errors.documentDate}
            aria-describedby={errors.documentDate ? 'documentDate-error' : undefined}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] text-[var(--color-primary)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
          />
          {errors.documentDate && <p id="documentDate-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.documentDate.message}</p>}
        </div>

        <div>
          <label htmlFor="journalId" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            اليومية
          </label>
          <input
            id="journalId"
            type="number"
            placeholder="معرف اليومية"
            {...register('journalId')}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] tabular-nums"
          />
        </div>

        <div>
          <label htmlFor="fiscalYearId" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            السنة المالية *
          </label>
          <input
            id="fiscalYearId"
            type="number"
            {...register('fiscalYearId')}
            aria-invalid={!!errors.fiscalYearId}
            aria-describedby={errors.fiscalYearId ? 'fiscalYearId-error' : undefined}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] tabular-nums focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
          />
          {errors.fiscalYearId && <p id="fiscalYearId-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.fiscalYearId.message}</p>}
        </div>

        <div>
          <label htmlFor="periodId" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            الفترة *
          </label>
          <input
            id="periodId"
            type="number"
            {...register('periodId')}
            aria-invalid={!!errors.periodId}
            aria-describedby={errors.periodId ? 'periodId-error' : undefined}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] tabular-nums focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
          />
          {errors.periodId && <p id="periodId-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.periodId.message}</p>}
        </div>

        <div>
          <label htmlFor="baseCurrencyId" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            العملة الأساسية *
          </label>
          <input
            id="baseCurrencyId"
            type="number"
            {...register('baseCurrencyId')}
            aria-invalid={!!errors.baseCurrencyId}
            aria-describedby={errors.baseCurrencyId ? 'baseCurrencyId-error' : undefined}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] tabular-nums focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
          />
          {errors.baseCurrencyId && <p id="baseCurrencyId-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.baseCurrencyId.message}</p>}
        </div>

        <div>
          <label htmlFor="entryType" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
            نوع القيد
          </label>
          <input
            id="entryType"
            type="text"
            placeholder="مثال: عادي، تسوية، عكس"
            {...register('entryType')}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)] focus-visible:outline-2 focus-visible:outline-[var(--color-secondary)]"
          />
        </div>
      </div>

      <div>
          <label htmlFor="ref" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
          المرجع
        </label>
        <input
          id="ref"
          type="text"
          {...register('ref')}
          aria-invalid={!!errors.ref}
          aria-describedby={errors.ref ? 'ref-error' : undefined}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)]"
        />
        {errors.ref && <p id="ref-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.ref.message}</p>}
      </div>

      <div>
          <label htmlFor="narration" className="block text-xs font-semibold text-[var(--color-primary)] mb-1">
          البيان
        </label>
        <textarea
          id="narration"
          rows={3}
          {...register('narration')}
          aria-invalid={!!errors.narration}
          aria-describedby={errors.narration ? 'narration-error' : undefined}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-lowest)]"
        />
        {errors.narration && <p id="narration-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.narration.message}</p>}
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={submitting} loading={submitting}>
          {submitting ? 'جاري الحفظ...' : isEdit ? 'حفظ التعديلات' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
