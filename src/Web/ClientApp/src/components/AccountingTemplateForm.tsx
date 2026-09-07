import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { JournalEntryTemplateType } from '../web-api-client';
import type { JournalEntryTemplateDto, JournalDto } from '../web-api-client';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';

const templateTypeOptions = [
  { value: JournalEntryTemplateType.Standard, label: 'قياسية' },
  { value: JournalEntryTemplateType.Recurring, label: 'دورية' },
  { value: JournalEntryTemplateType.Adjustment, label: 'تسوية' },
];

const schema = z.object({
  templateName: z.string().min(1, 'اسم القالب مطلوب').max(200, 'الاسم يجب ألا يتجاوز 200 حرفاً'),
  description: z.string().optional(),
  journalId: z.coerce.number().min(1, 'الدفتر مطلوب'),
  templateType: z.string().min(1, 'نوع القالب مطلوب'),
});

type FormData = z.infer<typeof schema>;

interface TemplateFormProps {
  initialData?: JournalEntryTemplateDto;
  journals: JournalDto[];
  onSubmit: (data: FormData) => void;
  loading?: boolean;
}

export function TemplateForm({ initialData, journals, onSubmit, loading }: TemplateFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    values: initialData ? {
      templateName: initialData.templateName ?? '',
      description: initialData.description ?? '',
      journalId: initialData.journalId ?? 0,
      templateType: initialData.templateType ?? '',
    } : undefined,
    defaultValues: {
      templateName: '',
      description: '',
      journalId: 0,
      templateType: '',
    },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
      <Input
        label="اسم القالب"
        {...register('templateName')}
        error={errors.templateName?.message}
        required
      />
      <Input
        label="الوصف"
        {...register('description')}
      />
      <Select
        label="الدفتر"
        {...register('journalId')}
        options={journals.map((j) => ({ value: String(j.id), label: `${j.code} - ${j.name}` }))}
        error={errors.journalId?.message}
      />
      <Select
        label="نوع القالب"
        {...register('templateType')}
        options={templateTypeOptions}
        error={errors.templateType?.message}
      />
      <Button type="submit" variant="primary" loading={loading}>
        {initialData ? 'حفظ التعديلات' : 'إنشاء القالب'}
      </Button>
    </form>
  );
}
