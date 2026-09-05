import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { AccountDto, AccountGroupDto } from '../types';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Switch } from '@/components/ui/Switch';
import { Button } from '@/components/ui/Button';

const schema = z.object({
  code: z.string().min(1, 'الرمز مطلوب'),
  name: z.string().min(1, 'الاسم مطلوب'),
  description: z.string().optional(),
  accountGroupId: z.coerce.number().min(1, 'المجموعة مطلوبة'),
  parentId: z.coerce.number().optional().nullable(),
  normalBalance: z.coerce.number().min(0, 'الرصيد الطبيعي مطلوب'),
  isPostable: z.boolean(),
  isReconcilable: z.boolean(),
  currencyId: z.coerce.number().optional().nullable(),
});

type FormData = z.infer<typeof schema>;

interface AccountFormProps {
  initialData?: AccountDto;
  accountGroups: AccountGroupDto[];
  parentAccounts: AccountDto[];
  onSubmit: (data: FormData) => void;
  loading?: boolean;
}

export function AccountForm({ initialData, accountGroups, parentAccounts, onSubmit, loading }: AccountFormProps) {
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: initialData ? {
      code: initialData.code ?? '',
      name: initialData.name ?? '',
      description: initialData.description ?? '',
      accountGroupId: initialData.accountGroupId ?? 0,
      parentId: initialData.parentId ?? null,
      normalBalance: initialData.normalBalance === 'credit' ? 1 : 0,
      isPostable: initialData.isPostable ?? true,
      isReconcilable: initialData.isReconcilable ?? false,
      currencyId: initialData.currencyId ?? null,
    } : undefined,
    defaultValues: {
      code: '',
      name: '',
      description: '',
      accountGroupId: 0,
      parentId: null,
      normalBalance: 0,
      isPostable: true,
      isReconcilable: false,
      currencyId: null,
    },
  });

  const isPostable = watch('isPostable');
  const isReconcilable = watch('isReconcilable');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
      <Input label="الرمز" {...register('code')} error={errors.code?.message} required disabled={!!initialData} />
      <Input label="الاسم" {...register('name')} error={errors.name?.message} required />
      <Input label="الوصف" {...register('description')} />
      <Select
        label="المجموعة"
        {...register('accountGroupId')}
        options={accountGroups.map((g) => ({ value: String(g.id), label: g.name ?? '' }))}
        error={errors.accountGroupId?.message}
      />
      <Select
        label="الحساب الأب"
        {...register('parentId')}
        options={[
          { value: '', label: '— بدون —' },
          ...parentAccounts.map((a) => ({ value: String(a.id), label: `${a.code} - ${a.name}` })),
        ]}
      />
      <Select
        label="الرصيد الطبيعي"
        {...register('normalBalance')}
        options={[
          { value: '0', label: 'مدين' },
          { value: '1', label: 'دائن' },
        ]}
        error={errors.normalBalance?.message}
      />
      <fieldset className="border border-[var(--color-border-container)] rounded-lg p-4 mb-4">
        <legend className="text-sm font-medium text-[var(--color-on-surface)] px-1">خيارات إضافية</legend>
        <div className="flex flex-col gap-3 mt-2">
          <Switch
            checked={isPostable}
            onChange={(v) => setValue('isPostable', v, { shouldValidate: true })}
            label="قابل للترحيل"
          />
          <Switch
            checked={isReconcilable}
            onChange={(v) => setValue('isReconcilable', v, { shouldValidate: true })}
            label="قابل للموازنة"
          />
        </div>
      </fieldset>
      <Button type="submit" variant="primary" loading={loading}>
        {initialData ? 'حفظ التعديلات' : 'إنشاء الحساب'}
      </Button>
    </form>
  );
}
