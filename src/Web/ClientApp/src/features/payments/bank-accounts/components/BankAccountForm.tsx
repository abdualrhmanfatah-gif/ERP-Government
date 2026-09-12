import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button, Card, Input, Switch } from '@/components/ui';

const schema = z.object({
  name: z.string().min(1, 'الاسم مطلوب').max(200),
  bankName: z.string().min(1, 'اسم البنك مطلوب').max(200),
  accountNumber: z.string().min(1, 'رقم الحساب مطلوب').max(100),
  iban: z.string().optional(),
  swiftCode: z.string().optional(),
  branchName: z.string().optional(),
  branchCode: z.string().optional(),
  currencyId: z.number().min(1, 'العملة مطلوبة'),
  fundId: z.number().optional(),
  glAccountId: z.number().optional(),
  isDefault: z.boolean(),
  maxDailyLimit: z.number().optional(),
  maxTransactionLimit: z.number().optional(),
  requiresDualApproval: z.boolean(),
  openingBalance: z.number().optional(),
});

type FormData = z.infer<typeof schema>;

interface BankAccountFormProps {
  initialData?: Partial<FormData>;
  onSubmit: (data: FormData) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  onEdit?: () => void;
}

export function BankAccountForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  onEdit,
}: BankAccountFormProps) {
  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: initialData ?? { isDefault: false, requiresDualApproval: false },
  });

  if (readOnly && initialData) {
    return (
      <div className="space-y-6">
        <Card>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold">بيانات الحساب</h2>
            {onEdit && <Button variant="outline" size="sm" onClick={onEdit}>تعديل</Button>}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">اسم الحساب:</span> <span>{initialData.name}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">اسم البنك:</span> <span>{initialData.bankName}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">رقم الحساب:</span> <span dir="ltr">{initialData.accountNumber}</span></div>
            {initialData.iban && <div><span className="text-[var(--color-on-surface-variant)]">IBAN:</span> <span dir="ltr">{initialData.iban}</span></div>}
            {initialData.swiftCode && <div><span className="text-[var(--color-on-surface-variant)]">Swift Code:</span> <span dir="ltr">{initialData.swiftCode}</span></div>}
            {initialData.branchName && <div><span className="text-[var(--color-on-surface-variant)]">اسم الفرع:</span> <span>{initialData.branchName}</span></div>}
            {initialData.branchCode && <div><span className="text-[var(--color-on-surface-variant)]">كود الفرع:</span> <span dir="ltr">{initialData.branchCode}</span></div>}
          </div>
        </Card>
        <Card>
          <h2 className="text-sm font-semibold mb-4">البيانات المالية</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">العملة:</span> <span>{initialData.currencyId}</span></div>
            {initialData.fundId && <div><span className="text-[var(--color-on-surface-variant)]">الصندوق:</span> <span>{initialData.fundId}</span></div>}
            {initialData.glAccountId && <div><span className="text-[var(--color-on-surface-variant)]">حساب الدفتر الأستاذ:</span> <span>{initialData.glAccountId}</span></div>}
            {initialData.openingBalance != null && <div><span className="text-[var(--color-on-surface-variant)]">الرصيد الافتتاحي:</span> <span>{initialData.openingBalance}</span></div>}
          </div>
        </Card>
        <Card>
          <h2 className="text-sm font-semibold mb-4">الضوابط</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm">
            {initialData.maxDailyLimit != null && <div><span className="text-[var(--color-on-surface-variant)]">الحد اليومي الأقصى:</span> <span>{initialData.maxDailyLimit}</span></div>}
            {initialData.maxTransactionLimit != null && <div><span className="text-[var(--color-on-surface-variant)]">الحد الأقصى للمعاملة:</span> <span>{initialData.maxTransactionLimit}</span></div>}
            <div><span className="text-[var(--color-on-surface-variant)]">يتطلب توقيع مزدوج:</span> <span>{initialData.requiresDualApproval ? 'نعم' : 'لا'}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">حساب افتراضي:</span> <span>{initialData.isDefault ? 'نعم' : 'لا'}</span></div>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <Card>
        <h2 className="text-sm font-semibold mb-4">بيانات الحساب</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label="اسم الحساب" {...register('name')} error={errors.name?.message} required />
          <Input label="اسم البنك" {...register('bankName')} error={errors.bankName?.message} required />
          <Input label="رقم الحساب" {...register('accountNumber')} error={errors.accountNumber?.message} required />
          <Input label="IBAN" {...register('iban')} />
          <Input label="Swift Code" {...register('swiftCode')} />
          <Input label="اسم الفرع" {...register('branchName')} />
          <Input label="كود الفرع" {...register('branchCode')} />
        </div>
      </Card>
      <Card>
        <h2 className="text-sm font-semibold mb-4">البيانات المالية</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input label="العملة" type="number" {...register('currencyId', { valueAsNumber: true })} error={errors.currencyId?.message} />
          <Input label="الصندوق" type="number" {...register('fundId', { valueAsNumber: true })} />
          <Input label="حساب الدفتر الأستاذ العام" type="number" {...register('glAccountId', { valueAsNumber: true })} />
          <Input label="الرصيد الافتتاحي" type="number" step="0.01" {...register('openingBalance', { valueAsNumber: true })} />
        </div>
      </Card>
      <Card>
        <h2 className="text-sm font-semibold mb-4">الضوابط</h2>
        <div className="space-y-4">
          <Input label="الحد اليومي الأقصى" type="number" step="0.01" {...register('maxDailyLimit', { valueAsNumber: true })} />
          <Input label="الحد الأقصى للمعاملة" type="number" step="0.01" {...register('maxTransactionLimit', { valueAsNumber: true })} />
          <Switch label="يتطلب توقيع مزدوج" checked={watch('requiresDualApproval')} onChange={(v) => setValue('requiresDualApproval', v)} />
          <Switch label="حساب افتراضي" checked={watch('isDefault')} onChange={(v) => setValue('isDefault', v)} />
        </div>
      </Card>
      <div className="flex gap-4 justify-end">
        <Button type="button" variant="outline" onClick={onCancel}>إلغاء</Button>
        <Button type="submit" disabled={isPending} loading={isPending}>حفظ</Button>
      </div>
    </form>
  );
}
