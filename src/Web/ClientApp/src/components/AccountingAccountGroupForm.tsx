import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { Select } from '@/components/ui/Select';
import { Alert } from '@/components/ui/Alert';
import { accountGroupSchema, type AccountGroupFormData } from '@/features/accounting/shared/schemas';
import { handleApiError } from '@/shared/api/result-to-ui';
import type { AccountGroupDto, CreateAccountGroupRequest } from '@/features/accounting/account-groups/types';
import { useAccountGroupsList } from '@/features/accounting/account-groups/hooks/useAccountGroupsList';

type Props = { open: boolean; onOpenChange:(v:boolean)=>void; initial?: AccountGroupDto | null; onSubmit:(data:CreateAccountGroupRequest & {rowVersion?:string})=>Promise<void>; isPending?: boolean };

export function AccountGroupForm({ open, onOpenChange, initial, onSubmit, isPending }: Props) {
  const [submitting, setSubmitting] = useState(false);
  const { data: groupsData } = useAccountGroupsList({ page: 1, pageSize: 1000 });
  const groups = groupsData?.items ?? [];

  const form = useForm<AccountGroupFormData>({
    resolver: zodResolver(accountGroupSchema),
    defaultValues: {
      code: initial?.code ?? '',
      name: initial?.name ?? '',
      type: (initial?.type as never) ?? 'Asset',
      normalBalance: (initial?.normalBalance as never) ?? 'Debit',
      description: initial?.description ?? '',
      parentId: initial?.parentId ?? null,
    },
  });

  useEffect(() => {
    if (open) {
      form.reset({
        code: initial?.code ?? '',
        name: initial?.name ?? '',
        type: (initial?.type as never) ?? 'Asset',
        normalBalance: (initial?.normalBalance as never) ?? 'Debit',
        description: initial?.description ?? '',
        parentId: initial?.parentId ?? null,
      });
    }
  }, [open, initial, form]);

  const handleSubmit = async (data: AccountGroupFormData) => {
    setSubmitting(true);
    try {
      await onSubmit(data as never);
      onOpenChange(false);
    } catch (err) {
      handleApiError(err, form.setError);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={()=>onOpenChange(false)} title={initial ? 'تعديل المجموعة' : 'إنشاء مجموعة'}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4" aria-label="نموذج مجموعة الحسابات">
        {form.formState.errors.root && (
          <Alert variant="error" role="alert">{form.formState.errors.root.message}</Alert>
        )}
        <Input label="الكود" {...form.register('code')} disabled={!!initial} error={form.formState.errors.code?.message} required />
        <Input label="الاسم" {...form.register('name')} error={form.formState.errors.name?.message} required />
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Select
            label="النوع"
            value={form.watch('type')}
            onChange={(e: React.ChangeEvent<HTMLSelectElement>) => form.setValue('type', e.target.value as 'Asset'|'Liability'|'Equity'|'Revenue'|'Expense', { shouldValidate: true })}
            options={[
              { value: 'Asset', label: 'أصل' },
              { value: 'Liability', label: 'التزام' },
              { value: 'Equity', label: 'حقوق ملكية' },
              { value: 'Revenue', label: 'إيراد' },
              { value: 'Expense', label: 'مصروف' },
            ]}
            error={form.formState.errors.type?.message}
          />
          <Select
            label="نوع الحساب"
            value={form.watch('normalBalance')}
            onChange={(e: React.ChangeEvent<HTMLSelectElement>) => form.setValue('normalBalance', e.target.value as 'Debit'|'Credit', { shouldValidate: true })}
            options={[
              { value: 'Debit', label: 'مدين' },
              { value: 'Credit', label: 'دائن' },
            ]}
            error={form.formState.errors.normalBalance?.message}
          />
        </div>
        <Input label="الوصف" {...form.register('description')} error={form.formState.errors.description?.message} />
        <Select
          label="المجموعة الأب"
          value={form.watch('parentId') ? String(form.watch('parentId')) : ''}
          onChange={(e: React.ChangeEvent<HTMLSelectElement>) => form.setValue('parentId', e.target.value ? Number(e.target.value) : null, { shouldValidate: true })}
          options={[
            { value: '', label: '— جذر (بدون أب) —' },
            ...groups
              .filter((g) => !initial || g.id !== initial.id)
              .map((g) => ({ value: String(g.id), label: `${g.code} — ${g.name} (مستوى ${g.level})` })),
          ]}
          error={form.formState.errors.parentId?.message}
        />
        {initial && <p className="text-xs text-[var(--color-on-surface-variant)]">المستوى: {initial.level} — يُحتسب تلقائياً</p>}
        <div className="flex justify-end gap-2">
          <Button type="button" variant="ghost" onClick={()=>onOpenChange(false)}>إلغاء</Button>
          <Button type="submit" variant="primary" loading={isPending || submitting}>
            {isPending ? 'جاري الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
