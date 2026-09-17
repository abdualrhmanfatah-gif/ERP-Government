import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { AccountDto, AccountGroupDto } from '@/features/accounting/types';
import { accountSchema, type AccountFormData } from '@/features/accounting/shared/schemas';
import { handleApiError } from '@/shared/api/result-to-ui';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Switch } from '@/components/ui/Switch';
import { Button } from '@/components/ui/Button';
import { Alert } from '@/components/ui/Alert';

interface AccountFormProps {
  initialData?: AccountDto;
  accountGroups: AccountGroupDto[];
  parentAccounts: AccountDto[];
  onSubmit: (data: AccountFormData) => Promise<unknown>;
  onSuccess?: () => void;
  onCancel?: () => void;
  loading?: boolean;
}

export function AccountForm({ initialData, accountGroups, parentAccounts, onSubmit, onSuccess, onCancel, loading }: AccountFormProps) {
  const [submitting, setSubmitting] = useState(false);
  const { register, handleSubmit, watch, setValue, setError, formState: { errors } } = useForm<AccountFormData>({
    resolver: zodResolver(accountSchema),
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

  const handleFormSubmit = async (data: AccountFormData) => {
    setSubmitting(true);
    try {
      await onSubmit(data);
      onSuccess?.();
    } catch (err) {
      handleApiError(err, setError);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="max-w-2xl" aria-label="نموذج الحساب">
      {errors.root && (
        <Alert variant="error" role="alert">{errors.root.message}</Alert>
      )}

      {/* Basic info */}
      <fieldset className="border border-[var(--color-container-border)] rounded-lg p-4 mb-4">
        <legend className="text-sm font-medium text-[var(--color-on-surface)] px-1">البيانات الأساسية</legend>
        <div className="grid gap-4 mt-2">
          <Input label="الرمز" {...register('code')} error={errors.code?.message} required disabled={!!initialData} />
          <Input label="الاسم" {...register('name')} error={errors.name?.message} required />
          <Input label="الوصف" {...register('description')} />
        </div>
      </fieldset>

      {/* Classification */}
      <fieldset className="border border-[var(--color-container-border)] rounded-lg p-4 mb-4">
        <legend className="text-sm font-medium text-[var(--color-on-surface)] px-1">التصنيف</legend>
        <div className="grid gap-4 mt-2">
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
            label="نوع الحساب"
            {...register('normalBalance')}
            options={[
              { value: '0', label: 'مدين' },
              { value: '1', label: 'دائن' },
            ]}
            error={errors.normalBalance?.message}
          />
        </div>
      </fieldset>

      {/* Options */}
      <fieldset className="border border-[var(--color-container-border)] rounded-lg p-4 mb-4">
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

      <div className="flex justify-end gap-2">
        {onCancel && (
          <Button type="button" variant="ghost" onClick={onCancel}>إلغاء</Button>
        )}
        <Button type="submit" variant="primary" loading={loading || submitting}>
          {initialData ? 'حفظ التعديلات' : 'إنشاء الحساب'}
        </Button>
      </div>
    </form>
  );
}
