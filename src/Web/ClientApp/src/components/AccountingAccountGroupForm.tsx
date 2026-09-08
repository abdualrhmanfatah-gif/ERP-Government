import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { Select } from '@/components/ui/Select';
import type { AccountGroupDto, CreateAccountGroupRequest } from '@/features/accounting/account-groups/types';
import { useAccountGroupsList } from '@/features/accounting/account-groups/hooks/useAccountGroupsList';

const schema = z.object({
  code: z.string().min(1, 'الكود مطلوب').max(20, 'الكود 20 حرف كحد أقصى'),
  name: z.string().min(1, 'الاسم مطلوب').max(200),
  type: z.enum(['Asset','Liability','Equity','Revenue','Expense']),
  normalBalance: z.enum(['Debit','Credit']),
  description: z.string().max(500).optional().nullable(),
  parentId: z.number().nullable().optional(),
}).refine((d) => {
  const map: Record<string,string> = { Asset:'Debit', Expense:'Debit', Liability:'Credit', Equity:'Credit', Revenue:'Credit' };
  return map[d.type] === d.normalBalance;
}, { message: 'الرصيد الطبيعي غير متوافق مع النوع', path: ['normalBalance'] });

type Props = { open: boolean; onOpenChange:(v:boolean)=>void; initial?: AccountGroupDto | null; onSubmit:(data:CreateAccountGroupRequest & {rowVersion?:string})=>Promise<void>; isPending?: boolean };

export function AccountGroupForm({ open, onOpenChange, initial, onSubmit, isPending }: Props) {
  const { data: groupsData } = useAccountGroupsList({ page: 1, pageSize: 1000 });
  const groups = groupsData?.items ?? [];

  const form = useForm<z.infer<typeof schema>>({
    resolver: zodResolver(schema),
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

  return (
    <Dialog open={open} onClose={()=>onOpenChange(false)} title={initial ? 'تعديل المجموعة' : 'إنشاء مجموعة'}>
      <form onSubmit={form.handleSubmit(async (v) => { await onSubmit(v as never); onOpenChange(false); })} className="space-y-4 p-4">
        <div className="grid gap-4">
          <Input label="الكود (20)" {...form.register('code')} disabled={!!initial} error={form.formState.errors.code?.message} />
          <Input label="الاسم (200)" {...form.register('name')} error={form.formState.errors.name?.message} />
          <div className="grid grid-cols-2 gap-4">
            <Select
              label="النوع"
              value={form.watch('type')}
              onChange={(e: React.ChangeEvent<HTMLSelectElement>) => form.setValue('type', e.target.value as 'Asset'|'Liability'|'Equity'|'Revenue'|'Expense', { shouldValidate: true })}
              options={[
                { value: 'Asset', label: 'أصل - Asset' },
                { value: 'Liability', label: 'التزام - Liability' },
                { value: 'Equity', label: 'حقوق ملكية - Equity' },
                { value: 'Revenue', label: 'إيراد - Revenue' },
                { value: 'Expense', label: 'مصروف - Expense' },
              ]}
              error={form.formState.errors.type?.message}
            />
            <Select
              label="الرصيد الطبيعي"
              value={form.watch('normalBalance')}
              onChange={(e: React.ChangeEvent<HTMLSelectElement>) => form.setValue('normalBalance', e.target.value as 'Debit'|'Credit', { shouldValidate: true })}
              options={[
                { value: 'Debit', label: 'مدين - Debit' },
                { value: 'Credit', label: 'دائن - Credit' },
              ]}
              error={form.formState.errors.normalBalance?.message}
            />
          </div>
          <Input label="الوصف (500 اختياري)" {...form.register('description')} error={form.formState.errors.description?.message} />
          <Select
            label="المجموعة الأب (اختياري)"
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
          {initial && <p className="text-xs text-[var(--color-on-surface-variant)]">المستوى: {initial.level} — يُحتسب تلقائياً (1 للجذر، أب+1)</p>}
        </div>
        <div className="flex justify-end gap-2">
          <Button type="button" variant="outline" onClick={()=>onOpenChange(false)}>إلغاء</Button>
          <Button type="submit" disabled={isPending}>{isPending?'جاري الحفظ...':'حفظ'}</Button>
        </div>
      </form>
    </Dialog>
  );
}
