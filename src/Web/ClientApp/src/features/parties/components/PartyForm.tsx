import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Button, Card, Input, Select, Textarea } from '@/components/ui';
import { handleApiError } from '@/shared/api/result-to-ui';
import { PARTY_TYPE_LABELS, type CreatePartyCommand } from '../shared/types';
import { partiesClient } from '../shared/client';
import {
  EMPTY_PARTY_FORM,
  partyFormSchema,
  toCreatePartyCommand,
  toPartyFormValues,
  type PartyFormValues,
} from '../shared/schemas';

const partyTypeOptions = Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({
  value,
  label,
}));

interface PartyFormProps {
  initialData?: CreatePartyCommand;
  onSubmit: (data: CreatePartyCommand) => Promise<unknown>;
  onSuccess?: (result: unknown) => void;
  onCancel: () => void;
  isPending?: boolean;
}

export function PartyForm({
  initialData,
  onSubmit,
  onSuccess,
  onCancel,
  isPending = false,
}: PartyFormProps) {
  const {
    register,
    handleSubmit,
    setError,
    getValues,
    formState: { errors, isSubmitting },
  } = useForm<PartyFormValues>({
    resolver: zodResolver(partyFormSchema),
    defaultValues: initialData ? toPartyFormValues(initialData) : EMPTY_PARTY_FORM,
  });

  const [taxDuplicateWarning, setTaxDuplicateWarning] = useState(false);
  const pending = isPending || isSubmitting;

  async function checkTaxDuplicate() {
    const taxNumber = getValues('taxNumber')?.trim();
    if (!taxNumber) {
      setTaxDuplicateWarning(false);
      return;
    }
    try {
      setTaxDuplicateWarning(await partiesClient.checkDuplicateTaxNumber(taxNumber));
    } catch {
      setTaxDuplicateWarning(false);
    }
  }

  async function submit(values: PartyFormValues) {
    try {
      const result = await onSubmit(toCreatePartyCommand(values));
      onSuccess?.(result);
    } catch (err) {
      handleApiError(err, setError);
    }
  }

  return (
    <form
      onSubmit={handleSubmit(submit)}
      aria-label="بيانات الطرف"
      className="flex flex-col gap-6"
    >
      <Card className="flex flex-col gap-8">
        <section className="flex flex-col gap-4">
          <h3 className="text-label-md font-semibold text-[var(--color-on-surface)]">البيانات الأساسية</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Select
              label="النوع"
              required
              options={partyTypeOptions}
              error={errors.partyType?.message}
              disabled={pending}
              {...register('partyType', { valueAsNumber: true })}
            />
            <Input
              label="الاسم بالعربية"
              type="text"
              required
              error={errors.nameAr?.message}
              disabled={pending}
              {...register('nameAr')}
            />
            <Input
              label="الاسم بالإنجليزية"
              type="text"
              dir="ltr"
              error={errors.nameEn?.message}
              disabled={pending}
              {...register('nameEn')}
            />
          </div>
        </section>

        <section className="flex flex-col gap-4">
          <h3 className="text-label-md font-semibold text-[var(--color-on-surface)]">بيانات التواصل</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <Input
                label="الرقم الضريبي"
                type="text"
                dir="ltr"
                error={errors.taxNumber?.message}
                disabled={pending}
                {...register('taxNumber', { onBlur: checkTaxDuplicate })}
              />
              {taxDuplicateWarning && !errors.taxNumber ? (
                <p className="text-xs text-[var(--color-warning)] mt-1">تنبيه: رقم ضريبي مسجل مسبقاً</p>
              ) : null}
            </div>
            <Input
              label="الهوية الوطنية"
              type="text"
              dir="ltr"
              error={errors.nationalId?.message}
              disabled={pending}
              {...register('nationalId')}
            />
            <Input
              label="الهاتف"
              type="text"
              dir="ltr"
              error={errors.phone?.message}
              disabled={pending}
              {...register('phone')}
            />
            <Input
              label="البريد الإلكتروني"
              type="email"
              dir="ltr"
              error={errors.email?.message}
              disabled={pending}
              {...register('email')}
            />
          </div>
        </section>

        <section className="flex flex-col gap-4">
          <h3 className="text-label-md font-semibold text-[var(--color-on-surface)]">العنوان والملاحظات</h3>
          <div className="grid grid-cols-1 gap-6">
            <Input
              label="العنوان"
              type="text"
              error={errors.address?.message}
              disabled={pending}
              {...register('address')}
            />
            <Textarea
              label="ملاحظات"
              rows={3}
              error={errors.notes?.message}
              disabled={pending}
              {...register('notes')}
            />
          </div>
        </section>
      </Card>

      {errors.root?.message ? (
        <Alert variant="error" role="alert">
          {errors.root.message}
        </Alert>
      ) : null}

      <div className="flex justify-end gap-2">
        <Button variant="ghost" type="button" onClick={onCancel} disabled={pending}>
          إلغاء
        </Button>
        <Button variant="primary" type="submit" disabled={pending} loading={pending}>
          حفظ
        </Button>
      </div>
    </form>
  );
}
