import { useState } from 'react';
import { Button, Input, Select, Textarea } from '@/components/ui';
import { PartyType, PARTY_TYPE_LABELS, type CreatePartyCommand } from '../shared/types';
import { partiesClient } from '../shared/client';

const partyTypeOptions = Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({
  value,
  label,
}));

interface PartyFormProps {
  initialData?: CreatePartyCommand;
  onSubmit: (data: CreatePartyCommand) => void;
  onCancel: () => void;
  isPending?: boolean;
  readOnly?: boolean;
  onEdit?: () => void;
}

export function PartyForm({
  initialData,
  onSubmit,
  onCancel,
  isPending = false,
  readOnly = false,
  onEdit,
}: PartyFormProps) {
  const [form, setForm] = useState<CreatePartyCommand>(
    initialData ?? {
      partyType: PartyType.Supplier,
      nameAr: '',
    }
  );

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [taxDuplicateWarning, setTaxDuplicateWarning] = useState(false);

  function updateField<K extends keyof CreatePartyCommand>(field: K, value: CreatePartyCommand[K]) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
    if (field === 'taxNumber') setTaxDuplicateWarning(false);
  }

  async function handleTaxNumberBlur() {
    const taxNumber = form.taxNumber?.trim();
    if (!taxNumber) {
      setTaxDuplicateWarning(false);
      return;
    }
    try {
      const exists = await partiesClient.checkDuplicateTaxNumber(taxNumber);
      setTaxDuplicateWarning(exists);
    } catch {
      setTaxDuplicateWarning(false);
    }
  }

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.nameAr.trim()) e.nameAr = 'الاسم بالعربية مطلوب';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    onSubmit(form);
  }

  if (readOnly) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">بيانات الطرف</h3>
          {onEdit && (
            <Button variant="outline" size="sm" onClick={onEdit}>
              تعديل
            </Button>
          )}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div>
            <span className="text-[var(--color-on-surface-variant)]">النوع:</span>{' '}
            <span>{PARTY_TYPE_LABELS[initialData!.partyType]}</span>
          </div>
          <div>
            <span className="text-[var(--color-on-surface-variant)]">الاسم بالعربية:</span>{' '}
            <span>{initialData!.nameAr}</span>
          </div>
          {initialData!.nameEn && (
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الاسم بالإنجليزية:</span>{' '}
              <span>{initialData!.nameEn}</span>
            </div>
          )}
          {initialData!.taxNumber && (
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الرقم الضريبي:</span>{' '}
              <span dir="ltr">{initialData!.taxNumber}</span>
            </div>
          )}
          {initialData!.nationalId && (
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الهوية الوطنية:</span>{' '}
              <span dir="ltr">{initialData!.nationalId}</span>
            </div>
          )}
          {initialData!.phone && (
            <div>
              <span className="text-[var(--color-on-surface-variant)]">الهاتف:</span>{' '}
              <span dir="ltr">{initialData!.phone}</span>
            </div>
          )}
          {initialData!.email && (
            <div>
              <span className="text-[var(--color-on-surface-variant)]">البريد الإلكتروني:</span>{' '}
              <span>{initialData!.email}</span>
            </div>
          )}
          {initialData!.address && (
            <div className="md:col-span-2">
              <span className="text-[var(--color-on-surface-variant)]">العنوان:</span>{' '}
              <span>{initialData!.address}</span>
            </div>
          )}
          {initialData!.notes && (
            <div className="md:col-span-2 pt-2 border-t border-[var(--color-outline-variant)]">
              <span className="text-[var(--color-on-surface-variant)]">ملاحظات:</span>{' '}
              <span>{initialData!.notes}</span>
            </div>
          )}
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6" aria-label="بيانات الطرف">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <Select
            label="النوع *"
            value={String(form.partyType)}
            onChange={(e) => updateField('partyType', Number(e.target.value) as PartyType)}
            options={partyTypeOptions}
          />
        </div>

        <div>
          <Input
            label="الاسم بالعربية *"
            type="text"
            value={form.nameAr}
            onChange={(e) => updateField('nameAr', e.target.value)}
            required
          />
          {errors.nameAr && <p className="text-xs text-[var(--color-error)] mt-1">{errors.nameAr}</p>}
        </div>

        <div>
          <Input
            label="الاسم بالإنجليزية"
            type="text"
            value={form.nameEn ?? ''}
            onChange={(e) => updateField('nameEn', e.target.value || undefined)}
          />
        </div>

        <div>
          <Input
            label="الرقم الضريبي"
            type="text"
            value={form.taxNumber ?? ''}
            onChange={(e) => updateField('taxNumber', e.target.value || undefined)}
            onBlur={handleTaxNumberBlur}
          />
          {errors.taxNumber && <p className="text-xs text-[var(--color-error)] mt-1">{errors.taxNumber}</p>}
          {taxDuplicateWarning && !errors.taxNumber && (
            <p className="text-xs text-[var(--color-warning)] mt-1">تنبيه: رقم ضريبي مسجل مسبقاً</p>
          )}
        </div>

        <div>
          <Input
            label="الهوية الوطنية"
            type="text"
            value={form.nationalId ?? ''}
            onChange={(e) => updateField('nationalId', e.target.value || undefined)}
          />
        </div>

        <div>
          <Input
            label="الهاتف"
            type="text"
            value={form.phone ?? ''}
            onChange={(e) => updateField('phone', e.target.value || undefined)}
          />
        </div>

        <div>
          <Input
            label="البريد الإلكتروني"
            type="email"
            value={form.email ?? ''}
            onChange={(e) => updateField('email', e.target.value || undefined)}
          />
        </div>

        <div>
          <Input
            label="العنوان"
            type="text"
            value={form.address ?? ''}
            onChange={(e) => updateField('address', e.target.value || undefined)}
          />
        </div>

        <div className="md:col-span-2">
          <Textarea
            label="ملاحظات"
            value={form.notes ?? ''}
            onChange={(e) => updateField('notes', e.target.value || undefined)}
            rows={3}
          />
        </div>
      </div>

      {errors.submit && <p className="text-sm text-[var(--color-error)] mt-4">{errors.submit}</p>}

      <div className="flex justify-end gap-2 mt-6">
        <Button variant="ghost" type="button" onClick={onCancel}>
          إلغاء
        </Button>
        <Button variant="primary" type="submit" disabled={isPending}>
          {isPending ? 'جارٍ الحفظ...' : 'حفظ'}
        </Button>
      </div>
    </form>
  );
}
