import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { DocumentSequenceDto } from '../types';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';

const resetPolicyOptions = [
  { value: 'Yearly', label: 'سنوي' },
  { value: 'Never', label: 'أبداً' },
];

const documentTypeOptions = [
  { value: 'Appropriation', label: 'ال Allocation' },
  { value: 'Encumbrance', label: 'التحفظ' },
  { value: 'PaymentOrder', label: 'أمر الدفع' },
  { value: 'PaymentExecution', label: 'تنفيذ الدفع' },
  { value: 'AdvancePayment', label: 'الدفع المقدم' },
  { value: 'JournalEntry', label: ' القيد اليومي' },
  { value: 'VendorBill', label: 'فاتورة المورد' },
  { value: 'PurchaseOrder', label: 'أمر الشراء' },
  { value: 'RevenueReceipt', label: 'إيراد' },
];

const schema = z.object({
  name: z.string().min(1, 'الاسم مطلوب').max(100, 'الاسم يجب ألا يتجاوز 100 حرف'),
  documentType: z.string().min(1, 'نوع المستند مطلوب'),
  fiscalYearId: z.coerce.number().int().positive().nullable().optional(),
  resetPolicy: z.enum(['Yearly', 'Never']),
});

type FormData = z.infer<typeof schema>;

interface DocumentSequenceFormProps {
  initialData?: DocumentSequenceDto;
  onSubmit: (data: FormData) => void;
  serverError?: string;
  loading?: boolean;
  isEdit?: boolean;
}

export function DocumentSequenceForm({ initialData, onSubmit, serverError, loading, isEdit = false }: DocumentSequenceFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: initialData ? {
      name: initialData.name ?? '',
      documentType: initialData.documentType ?? '',
      fiscalYearId: initialData.fiscalYearId ?? null,
      resetPolicy: initialData.resetPolicy ?? 'Yearly',
    } : undefined,
    defaultValues: {
      name: '',
      documentType: '',
      fiscalYearId: null,
      resetPolicy: 'Yearly',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
      <Input
        label="الاسم"
        {...register('name')}
        error={errors.name?.message}
        required
        disabled={isEdit}
      />
      <Select
        label="نوع المستند"
        options={documentTypeOptions}
        {...register('documentType')}
        error={errors.documentType?.message}
        disabled={isEdit}
      />
      <Select
        label="سياسة إعادة التعيين"
        options={resetPolicyOptions}
        {...register('resetPolicy')}
        error={errors.resetPolicy?.message}
      />
      {serverError ? (
        <div
          role="alert"
          className="px-3 py-2 mb-4 rounded-lg text-sm bg-[var(--color-error-container)] text-[var(--color-on-error-container)]"
        >
          {serverError}
        </div>
      ) : null}
      <Button type="submit" variant="primary" loading={loading}>
        {isEdit ? 'حفظ التعديلات' : 'إنشاء التسلسل'}
      </Button>
    </form>
  );
}
