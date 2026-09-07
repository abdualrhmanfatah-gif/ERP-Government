import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useParty, useUpdateParty, useTogglePartyActive, usePartyDocuments } from '../hooks/useParties';
import { PartyType, PARTY_TYPE_LABELS, type UpdatePartyCommand } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Badge, Card, Input, Select, Textarea } from '@/components/ui';
import { ArrowRight, Edit, Save, X } from 'lucide-react';
import { ApprovalsPanel } from '@/components/DocumentsApprovalsPanel';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';

export default function PartyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const partyId = Number(id);
  const { hasPermission: canUpdate } = usePermission('Parties.Update');

  const { data: party, isLoading } = useParty(partyId);
  const updateParty = useUpdateParty(partyId);
  const toggleActive = useTogglePartyActive(partyId);
  const { data: documents } = usePartyDocuments(partyId);

  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<UpdatePartyCommand | null>(null);
  const [showDeactivationWarning, setShowDeactivationWarning] = useState(false);

  function startEdit() {
    if (!party) return;
    setEditForm({
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
    setEditForm(null);
  }

  async function handleSave() {
    if (!editForm) return;
    await updateParty.mutateAsync(editForm);
    setIsEditing(false);
    setEditForm(null);
  }

  async function handleToggleActive() {
    const openDocs = documents?.filter((d) => ['Draft', 'Submitted'].includes(d.status)) ?? [];
    if (openDocs.length > 0) {
      setShowDeactivationWarning(true);
      return;
    }
    await toggleActive.mutateAsync();
  }

  async function confirmDeactivation() {
    setShowDeactivationWarning(false);
    await toggleActive.mutateAsync();
  }

  if (isLoading) {
    return (
      <div className="p-6 space-y-4">
        <div className="h-8 w-64 rounded bg-[var(--color-surface-container)] animate-pulse" />
        <div className="h-48 rounded-lg bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  if (!party) {
    return <div className="p-6 text-center text-[var(--color-error)]">لم يتم العثور على الطرف</div>;
  }

  return (
    <Page
      title={party.nameAr}
      description={party.partyCode}
      actions={
        <div className="flex gap-2">
          <Button variant="ghost" size="icon" onClick={() => navigate('/parties')} aria-label="العودة">
            <ArrowRight size={18} />
          </Button>
          {!isEditing && canUpdate && (
            <Button variant="ghost" size="sm" onClick={startEdit}>
              <Edit size={14} className="ms-1" />
              تعديل
            </Button>
          )}
          {isEditing && (
            <>
              <Button variant="primary" size="sm" onClick={handleSave} disabled={updateParty.isPending}>
                <Save size={14} className="ms-1" />
                حفظ
              </Button>
              <Button variant="ghost" size="sm" onClick={cancelEdit}>
                <X size={14} className="ms-1" />
                إلغاء
              </Button>
            </>
          )}
          {canUpdate && (
            <Button
              variant={party.isActive ? 'destructive' : 'primary'}
              size="sm"
              onClick={handleToggleActive}
              disabled={toggleActive.isPending}
            >
              {party.isActive ? 'تعطيل' : 'تفعيل'}
            </Button>
          )}
        </div>
      }
    >
      <div className="flex items-center gap-3 mb-2">
        <Badge variant={party.isActive ? 'success' : 'default'}>
          {party.isActive ? 'نشط' : 'غير نشط'}
        </Badge>
      </div>

      {showDeactivationWarning && (
        <div className="rounded-lg border border-[var(--color-error)] bg-[var(--color-error-container)] p-4">
          <p className="text-sm font-medium text-[var(--color-on-error-container)]">
            تحذير: هذا الطرف لديه مستندات مفتوحة (مسودة/مقدمة). هل أنت متأكد من التعطيل؟
          </p>
          <div className="mt-3 flex gap-2">
            <Button variant="destructive" size="sm" onClick={confirmDeactivation}>تأكيد التعطيل</Button>
            <Button variant="ghost" size="sm" onClick={() => setShowDeactivationWarning(false)}>إلغاء</Button>
          </div>
        </div>
      )}

      <Card className="bg-[var(--color-surface-container-lowest)]">
        <h2 className="text-sm font-semibold mb-4">بيانات الطرف</h2>
        {isEditing && editForm ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Select
                label="النوع"
                value={String(editForm.partyType)}
                onChange={(e) => setEditForm({ ...editForm, partyType: Number(e.target.value) as PartyType })}
                options={Object.entries(PARTY_TYPE_LABELS).map(([value, label]) => ({ value, label }))}
              />
            </div>
            <div>
              <Input
                label="الاسم بالعربية *"
                type="text"
                value={editForm.nameAr}
                onChange={(e) => setEditForm({ ...editForm, nameAr: e.target.value })}
                required
              />
            </div>
            <div>
              <Input
                label="الاسم بالإنجليزية"
                type="text"
                value={editForm.nameEn ?? ''}
                onChange={(e) => setEditForm({ ...editForm, nameEn: e.target.value || undefined })}
              />
            </div>
            <div>
              <Input
                label="الرقم الضريبي"
                type="text"
                value={editForm.taxNumber ?? ''}
                onChange={(e) => setEditForm({ ...editForm, taxNumber: e.target.value || undefined })}
              />
            </div>
            <div>
              <Input
                label="الهوية الوطنية"
                type="text"
                value={editForm.nationalId ?? ''}
                onChange={(e) => setEditForm({ ...editForm, nationalId: e.target.value || undefined })}
              />
            </div>
            <div>
              <Input
                label="الهاتف"
                type="text"
                value={editForm.phone ?? ''}
                onChange={(e) => setEditForm({ ...editForm, phone: e.target.value || undefined })}
              />
            </div>
            <div>
              <Input
                label="البريد الإلكتروني"
                type="email"
                value={editForm.email ?? ''}
                onChange={(e) => setEditForm({ ...editForm, email: e.target.value || undefined })}
              />
            </div>
            <div>
              <Input
                label="العنوان"
                type="text"
                value={editForm.address ?? ''}
                onChange={(e) => setEditForm({ ...editForm, address: e.target.value || undefined })}
              />
            </div>
            <div className="md:col-span-2">
              <Textarea
                label="ملاحظات"
                value={editForm.notes ?? ''}
                onChange={(e) => setEditForm({ ...editForm, notes: e.target.value || undefined })}
                rows={3}
              />
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">النوع</span>
              <span className="block text-sm">{PARTY_TYPE_LABELS[party.partyType]}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الاسم بالإنجليزية</span>
              <span className="block text-sm">{party.nameEn ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الرقم الضريبي</span>
              <span className="block text-sm font-mono">{party.taxNumber ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الهوية الوطنية</span>
              <span className="block text-sm">{party.nationalId ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الهاتف</span>
              <span className="block text-sm">{party.phone ?? '—'}</span>
            </div>
            <div>
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">البريد الإلكتروني</span>
              <span className="block text-sm">{party.email ?? '—'}</span>
            </div>
            <div className="md:col-span-2">
              <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">العنوان</span>
              <span className="block text-sm">{party.address ?? '—'}</span>
            </div>
            {party.notes && (
              <div className="md:col-span-3">
                <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">ملاحظات</span>
                <span className="block text-sm">{party.notes}</span>
              </div>
            )}
          </div>
        )}
      </Card>

      <ApprovalsPanel documentType="Party" documentId={partyId} />
      <StatusLogPanel documentType="Party" documentId={partyId} />
      <AttachmentsPanel documentType="Party" documentId={partyId} showGate={false} />

      <Card className="bg-[var(--color-surface-container-lowest)]">
        <h2 className="text-sm font-semibold mb-4">المستندات ذات الصلة</h2>
        {!documents || documents.length === 0 ? (
          <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد مستندات ذات صلة</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-[var(--color-outline-variant)]">
                  <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">النوع</th>
                  <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">الرقم</th>
                  <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">الحالة</th>
                  <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">التاريخ</th>
                  <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">المبلغ</th>
                </tr>
              </thead>
              <tbody>
                {documents.map((doc) => (
                  <tr
                    key={`${doc.documentType}-${doc.documentId}`}
                    className="border-b border-[var(--color-outline-variant)] last:border-0 hover:bg-[var(--color-surface-variant)] cursor-pointer"
                  >
                    <td className="px-3 py-3">{doc.documentType}</td>
                    <td className="px-3 py-3 font-mono">{doc.documentNumber}</td>
                    <td className="px-3 py-3">
                      <Badge variant="default">{doc.status}</Badge>
                    </td>
                    <td className="px-3 py-3">{doc.date}</td>
                    <td className="px-3 py-3 font-mono">
                      {doc.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </Page>
  );
}
