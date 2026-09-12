import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useParty, useUpdateParty, useTogglePartyActive } from '../hooks/useParties';
import { PARTY_TYPE_LABELS, type UpdatePartyCommand } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Badge, Card, Input, Select, Textarea } from '@/components/ui';
import { ArrowRight, Edit, Save, X } from 'lucide-react';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { MetaItem } from '@/components/MetaItem';
import { notify } from '@/features/notifications/notify';
import { handleLifecycleError } from '@/shared/api/result-to-ui';

export default function PartyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const partyId = Number(id);
  const { hasPermission: canUpdate } = usePermission('Parties.Update');

  const { data: party, isLoading, error } = useParty(partyId);
  const updateParty = useUpdateParty(partyId);
  const toggleActive = useTogglePartyActive(partyId);

  const [isEditing, setIsEditing] = useState(false);
  const [form, setForm] = useState<UpdatePartyCommand | null>(null);

  const locked = !isEditing;

  function startEdit() {
    if (!party) return;
    setForm({
      partyType: party.partyType,
      nameAr: party.nameAr,
      nameEn: party.nameEn ?? undefined,
      taxNumber: party.taxNumber ?? undefined,
      nationalId: party.nationalId ?? undefined,
      phone: party.phone ?? undefined,
      email: party.email ?? undefined,
      address: party.address ?? undefined,
      notes: party.notes ?? undefined,
    });
    setIsEditing(true);
  }

  function cancelEdit() {
    setIsEditing(false);
    setForm(null);
  }

  async function handleSave() {
    if (!form) return;
    try {
      await updateParty.mutateAsync(form);
      notify({ type: 'success', title: 'تم حفظ التعديلات' });
      setIsEditing(false);
      setForm(null);
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  async function handleToggleActive() {
    try {
      await toggleActive.mutateAsync();
      notify({ type: 'success', title: party!.isActive ? 'تم تعطيل المورد' : 'تم تفعيل المورد' });
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  if (isLoading) return <Page title="..." loading>{null}</Page>;
  if (error || !party) return <Page title="تفاصيل المورد"><p className="text-[var(--color-error)]">لم يتم العثور على المورد</p></Page>;

  const fv = isEditing ? form! : {
    partyType: party.partyType,
    nameAr: party.nameAr,
    nameEn: party.nameEn ?? undefined,
    taxNumber: party.taxNumber ?? undefined,
    nationalId: party.nationalId ?? undefined,
    phone: party.phone ?? undefined,
    email: party.email ?? undefined,
    address: party.address ?? undefined,
    notes: party.notes ?? undefined,
  };

  const set = (patch: Partial<UpdatePartyCommand>) => setForm({ ...fv, ...patch });

  const headerActions = (
    <div className="flex items-center gap-2 flex-wrap">
      {locked && canUpdate && (
        <Button variant="secondary" size="sm" onClick={startEdit}>
          <Edit size={14} className="ms-1" /> تعديل
        </Button>
      )}
      {isEditing && (
        <>
          <Button variant="primary" size="sm" onClick={handleSave} disabled={updateParty.isPending} loading={updateParty.isPending}>
            <Save size={14} className="ms-1" /> حفظ
          </Button>
          <Button variant="ghost" size="sm" onClick={cancelEdit}>
            <X size={14} className="ms-1" /> إلغاء
          </Button>
        </>
      )}
      {canUpdate && (
        <Button
          variant={party.isActive ? 'destructive' : 'primary'}
          size="sm"
          onClick={handleToggleActive}
          disabled={toggleActive.isPending}
          loading={toggleActive.isPending}
        >
          {party.isActive ? 'تعطيل' : 'تفعيل'}
        </Button>
      )}
      <Button variant="ghost" size="icon" onClick={() => navigate('/parties')} aria-label="العودة">
        <ArrowRight size={18} />
      </Button>
    </div>
  );

  return (
    <Page
      title={party.nameAr}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-[var(--radius-lg)] bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <Badge variant={party.isActive ? 'success' : 'default'}>
            {getActiveStatusLabel(party.isActive)}
          </Badge>
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="الكود" value={party.partyCode} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="النوع" value={PARTY_TYPE_LABELS[party.partyType]} />
          {party.taxNumber && (
            <>
              <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
              <MetaItem label="الرقم الضريبي" value={party.taxNumber} />
            </>
          )}
        </div>
      }
    >
      <Card className="bg-[var(--color-surface-container-lowest)]">
        <h2 className="text-[var(--typography-label-md-size)] font-semibold mb-4 text-[var(--color-on-surface)]">بيانات المورد</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Select
              label="النوع"
              value={String(fv.partyType)}
              onChange={(e) => set({ partyType: Number(e.target.value) as never })}
              options={Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({ value, label }))}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="الاسم بالعربية *"
              type="text"
              value={fv.nameAr}
              onChange={(e) => set({ nameAr: e.target.value })}
              required
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="الاسم بالإنجليزية"
              type="text"
              value={fv.nameEn ?? ''}
              onChange={(e) => set({ nameEn: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="الرقم الضريبي"
              type="text"
              value={fv.taxNumber ?? ''}
              onChange={(e) => set({ taxNumber: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="الهوية الوطنية"
              type="text"
              value={fv.nationalId ?? ''}
              onChange={(e) => set({ nationalId: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="الهاتف"
              type="text"
              value={fv.phone ?? ''}
              onChange={(e) => set({ phone: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="البريد الإلكتروني"
              type="email"
              value={fv.email ?? ''}
              onChange={(e) => set({ email: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div>
            <Input
              label="العنوان"
              type="text"
              value={fv.address ?? ''}
              onChange={(e) => set({ address: e.target.value || undefined })}
              disabled={locked}
            />
          </div>
          <div className="md:col-span-2">
            <Textarea
              label="ملاحظات"
              value={fv.notes ?? ''}
              onChange={(e) => set({ notes: e.target.value || undefined })}
              rows={3}
              disabled={locked}
            />
          </div>
        </div>
      </Card>
    </Page>
  );
}
