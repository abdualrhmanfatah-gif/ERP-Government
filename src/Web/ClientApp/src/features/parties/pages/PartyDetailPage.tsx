import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useParty, useUpdateParty, useTogglePartyActive } from '../hooks/useParties';
import { PARTY_TYPE_LABELS } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Card, Input, Select, StatusBadge, Textarea } from '@/components/ui';
import { Edit } from 'lucide-react';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { MetaItem } from '@/components/MetaItem';
import { notify } from '@/features/notifications/notify';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { PartyForm } from '../components/PartyForm';

export default function PartyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const partyId = Number(id);
  const { hasPermission: canUpdate } = usePermission('Parties.Update');

  const { data: party, isLoading, error } = useParty(partyId);
  const updateParty = useUpdateParty(partyId);
  const toggleActive = useTogglePartyActive(partyId);

  const [isEditing, setIsEditing] = useState(false);

  function startEdit() {
    setIsEditing(true);
  }

  function cancelEdit() {
    setIsEditing(false);
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

  const fv = {
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

  const headerActions = (
    <div className="flex items-center gap-2 flex-wrap">
      {!isEditing && canUpdate && (
        <Button variant="primary" size="sm" onClick={startEdit}>
          <Edit size={14} className="ms-1" /> تعديل
        </Button>
      )}
      {!isEditing && canUpdate && (
        <Button
          variant={party.isActive ? 'destructive' : 'outline'}
          size="sm"
          onClick={handleToggleActive}
          disabled={toggleActive.isPending}
          loading={toggleActive.isPending}
        >
          {party.isActive ? 'تعطيل' : 'تفعيل'}
        </Button>
      )}
    </div>
  );

  return (
    <Page
      title={party.nameAr}
      actions={headerActions}
      onBack={() => navigate('/parties')}
      maxWidth="lg"
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-[var(--radius-lg)] bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <StatusBadge variant={party.isActive ? 'active' : 'inactive'}>
            {getActiveStatusLabel(party.isActive)}
          </StatusBadge>
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
      {isEditing ? (
        <PartyForm
          initialData={fv}
          onSubmit={(data) => updateParty.mutateAsync(data)}
          onSuccess={() => {
            notify({ type: 'success', title: 'تم حفظ التعديلات' });
            setIsEditing(false);
          }}
          onCancel={cancelEdit}
          isPending={updateParty.isPending}
        />
      ) : (
        <Card className="bg-[var(--color-surface-container-lowest)]">
          <h2 className="text-label-md font-semibold mb-4 text-[var(--color-on-surface)]">بيانات المورد</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Select
                label="النوع"
                value={String(fv.partyType)}
                options={Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({ value, label }))}
                disabled
              />
            </div>
            <div>
              <Input label="الاسم بالعربية" type="text" value={fv.nameAr} required disabled />
            </div>
            <div>
              <Input label="الاسم بالإنجليزية" type="text" dir="ltr" value={fv.nameEn ?? ''} disabled />
            </div>
            <div>
              <Input label="الرقم الضريبي" type="text" dir="ltr" value={fv.taxNumber ?? ''} disabled />
            </div>
            <div>
              <Input label="الهوية الوطنية" type="text" dir="ltr" value={fv.nationalId ?? ''} disabled />
            </div>
            <div>
              <Input label="الهاتف" type="text" dir="ltr" value={fv.phone ?? ''} disabled />
            </div>
            <div>
              <Input label="البريد الإلكتروني" type="email" dir="ltr" value={fv.email ?? ''} disabled />
            </div>
            <div>
              <Input label="العنوان" type="text" value={fv.address ?? ''} disabled />
            </div>
            <div className="md:col-span-2">
              <Textarea label="ملاحظات" value={fv.notes ?? ''} rows={3} disabled />
            </div>
          </div>
        </Card>
      )}
    </Page>
  );
}
