import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { JournalType } from '../web-api-client';
import type { JournalDto } from '../web-api-client';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Switch } from '@/components/ui/Switch';
import { Button } from '@/components/ui/Button';

const journalTypeOptions = [
  { value: JournalType.General, label: 'عامة' },
  { value: JournalType.Purchase, label: 'مشتريات' },
  { value: JournalType.Sale, label: 'مبيعات' },
  { value: JournalType.Cash, label: 'نقدية' },
  { value: JournalType.Bank, label: 'بنكية' },
  { value: JournalType.Adjustment, label: 'تسوية' },
  { value: JournalType.Closing, label: 'إقفال' },
];

const schema = z.object({
  code: z.string().min(1, 'الرمز مطلوب').max(20, 'الرمز يجب ألا يتجاوز 20 حرفاً'),
  name: z.string().min(1, 'الاسم مطلوب').max(200, 'الاسم يجب ألا يتجاوز 200 حرفاً'),
  type: z.string().min(1, 'النوع مطلوب'),
  accountId: z.coerce.number().optional().nullable(),
  suspenseAccountId: z.coerce.number().optional().nullable(),
  allowForeignCurrency: z.boolean(),
  sequenceId: z.coerce.number().optional().nullable(),
  requireApprovalBeforePosting: z.boolean(),
});

type FormData = z.infer<typeof schema>;

interface JournalFormProps {
  initialData?: JournalDto;
  onSubmit: (data: FormData) => void;
  loading?: boolean;
  lockedFields?: string[];
}

export function JournalForm({ initialData, onSubmit, loading, lockedFields = [] }: JournalFormProps) {
  const isLocked = (field: string) => lockedFields.includes(field);

  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: initialData ? {
      code: initialData.code ?? '',
      name: initialData.name ?? '',
      type: initialData.type ?? '',
      accountId: initialData.accountId ?? null,
      suspenseAccountId: initialData.suspenseAccountId ?? null,
      allowForeignCurrency: initialData.allowForeignCurrency ?? false,
      sequenceId: initialData.sequenceId ?? null,
      requireApprovalBeforePosting: initialData.requireApprovalBeforePosting ?? false,
    } : undefined,
    defaultValues: {
      code: '',
      name: '',
      type: '',
      accountId: null,
      suspenseAccountId: null,
      allowForeignCurrency: false,
      sequenceId: null,
      requireApprovalBeforePosting: false,
    },
  });

  const allowForeignCurrency = watch('allowForeignCurrency');
  const requireApprovalBeforePosting = watch('requireApprovalBeforePosting');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
      <Input
        label="الرمز"
        {...register('code')}
        error={errors.code?.message}
        required
        disabled={!!initialData || isLocked('code')}
      />
      <Input
        label="الاسم"
        {...register('name')}
        error={errors.name?.message}
        required
        disabled={isLocked('name')}
      />
      <Select
        label="النوع"
        {...register('type')}
        options={journalTypeOptions}
        error={errors.type?.message}
        disabled={isLocked('type')}
      />
      <Input
        label="رقم الحساب الافتراضي"
        type="number"
        {...register('accountId')}
        error={errors.accountId?.message}
        disabled={isLocked('accountId')}
      />
      <Input
        label="رقم حساب التعليق"
        type="number"
        {...register('suspenseAccountId')}
        error={errors.suspenseAccountId?.message}
        disabled={isLocked('suspenseAccountId')}
      />
      <Input
        label="رقم السلسلة"
        type="number"
        {...register('sequenceId')}
        error={errors.sequenceId?.message}
        disabled={isLocked('sequenceId')}
      />
      <fieldset className="border border-[var(--color-border-container)] rounded-lg p-4 mb-4">
        <legend className="text-sm font-medium text-[var(--color-on-surface)] px-1">خيارات</legend>
        <div className="flex flex-col gap-3 mt-2">
          <Switch
            checked={allowForeignCurrency}
            onChange={(v) => setValue('allowForeignCurrency', v, { shouldValidate: true })}
            label="السماح بالعملة الأجنبية"
            disabled={isLocked('allowForeignCurrency')}
          />
          <Switch
            checked={requireApprovalBeforePosting}
            onChange={(v) => setValue('requireApprovalBeforePosting', v, { shouldValidate: true })}
            label="اشتراط الاعتماد قبل الترحيل"
          />
        </div>
      </fieldset>
      <Button type="submit" variant="primary" loading={loading}>
        {initialData ? 'حفظ التعديلات' : 'إنشاء الدفتر'}
      </Button>
    </form>
  );
}
