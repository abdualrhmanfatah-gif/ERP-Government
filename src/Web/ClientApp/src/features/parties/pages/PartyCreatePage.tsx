import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateParty } from '../hooks/useParties';
import { partiesClient } from '../shared/client';
import { PartyType, PARTY_TYPE_LABELS, type CreatePartyCommand } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Input, Select, Textarea } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function PartyCreatePage() {
  const navigate = useNavigate();
  const createParty = useCreateParty();
  const { hasPermission: canCreate } = usePermission('Parties.Create');

  const [form, setForm] = useState<CreatePartyCommand>({
    partyType: PartyType.Supplier,
    nameAr: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [taxDuplicateWarning, setTaxDuplicateWarning] = useState(false);

  if (!canCreate) {
    return (
      <div className="p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">غير مصرح بالوصول</p>
      </div>
    );
  }

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!form.nameAr.trim()) e.nameAr = 'الاسم بالعربية مطلوب';
    setErrors(e);
    return Object.keys(e).length === 0;
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

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    try {
      const result = await createParty.mutateAsync(form);
      navigate(`/parties/${result}`);
    } catch (err: unknown) {
      const problem = err as { status?: number; detail?: string; errors?: string[] };
      const msg = problem.errors?.[0] ?? problem.detail ?? 'حدث خطأ أثناء الحفظ';
      if (msg.includes('tax number already exists')) {
        setTaxDuplicateWarning(true);
        setErrors({ taxNumber: 'رقم ضريبي مسجل مسبقاً' });
      } else {
        setErrors({ submit: msg });
      }
    }
  }

  function updateField<K extends keyof CreatePartyCommand>(field: K, value: CreatePartyCommand[K]) {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }));
    if (field === 'taxNumber') setTaxDuplicateWarning(false);
  }

  return (
    <Page
      title="طرف جديد"
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate('/parties')} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <form onSubmit={handleSubmit} className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <Select
              label="النوع *"
              value={String(form.partyType)}
              onChange={(e) => updateField('partyType', Number(e.target.value) as PartyType)}
              options={Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({ value, label }))}
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
          <Button variant="ghost" type="button" onClick={() => navigate('/parties')}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" disabled={createParty.isPending}>
            {createParty.isPending ? 'جارٍ الحفظ...' : 'حفظ'}
          </Button>
        </div>
      </form>
    </Page>
  );
}
