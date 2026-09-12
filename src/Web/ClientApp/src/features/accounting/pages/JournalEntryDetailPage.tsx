import { useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  useJournalEntry,
  useSubmitJournalEntry,
  useApproveJournalEntry,
  usePostJournalEntry,
  useReverseJournalEntry,
  useCancelJournalEntry,
} from '../hooks/useJournalEntries';
import { usePermission } from '../../../shared/hooks/usePermission';
import { useFiscalYearByDate } from '../hooks/useFiscalYearByDate';
import { ReverseDialog } from '@/components/AccountingReverseDialog';
import { ApprovalsPanel } from '@/components/DocumentsApprovalsPanel';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { AccountingJournalEntryDetail } from '@/components/AccountingJournalEntryDetail';
import { StatusBadge } from '@/components/AccountingStatusBadge';
import { Page, Button, Card, Badge } from '@/components/ui';
import { MetaItem } from '@/components/MetaItem';
import { formatDate, toDateInput } from '@/shared/utils/formatters';

function getActionsForStatus(status: string) {
  switch (status) {
    case 'Draft': return ['submit', 'cancel'];
    case 'Submitted': return ['approve', 'cancel'];
    case 'Approved': return ['post'];
    case 'Posted': return ['reverse'];
    default: return [];
  }
}

const actionLabels: Record<string, string> = { submit: 'إرسال للمراجعة', approve: 'موافقة', post: 'تسجيل', reverse: 'عكس', cancel: 'إلغاء' };
const permissionMap: Record<string, string> = {
  submit: 'Accounting.JournalEntries.Submit', approve: 'Accounting.JournalEntries.Approve',
  post: 'Accounting.JournalEntries.Post', reverse: 'Accounting.JournalEntries.Reverse', cancel: 'Accounting.JournalEntries.Cancel',
};

export function JournalEntryDetailPage() {
  const { id } = useParams<{ id: string }>();
  const entryId = Number(id);

  const { data: entry, isLoading, error, refetch } = useJournalEntry(entryId);
  const submitMutation = useSubmitJournalEntry();
  const approveMutation = useApproveJournalEntry();
  const postMutation = usePostJournalEntry();
  const reverseMutation = useReverseJournalEntry();
  const cancelMutation = useCancelJournalEntry();
  const { hasPermission } = usePermission();
  const { hasPermission: canSubmit } = usePermission('Accounting.JournalEntries.Submit');
  const { hasPermission: canApprove } = usePermission('Accounting.JournalEntries.Approve');
  const { hasPermission: canPost } = usePermission('Accounting.JournalEntries.Post');
  const { hasPermission: canReverse } = usePermission('Accounting.JournalEntries.Reverse');
  const { hasPermission: canCancel } = usePermission('Accounting.JournalEntries.Cancel');
  const fiscal = useFiscalYearByDate(toDateInput(entry?.documentDate));

  const [showReverseDialog, setShowReverseDialog] = useState(false);
  const [conflictError, setConflictError] = useState<string | null>(null);
  const [editState, setEditState] = useState({ isEditing: false, canSave: false, isSaving: false });

  const handleAction = async (action: string) => {
    setConflictError(null);
    try {
      switch (action) {
        case 'submit': await submitMutation.mutateAsync({ id: entryId, command: { rowVersion: entry?.rowVersion ?? '' } }); break;
        case 'approve': await approveMutation.mutateAsync({ id: entryId, command: { rowVersion: entry?.rowVersion ?? '' } }); break;
        case 'post': await postMutation.mutateAsync({ id: entryId, command: { rowVersion: entry?.rowVersion ?? '' } }); break;
        case 'reverse': setShowReverseDialog(true); break;
        case 'cancel': await cancelMutation.mutateAsync({ id: entryId, command: { rowVersion: entry?.rowVersion ?? '' } }); break;
      }
    } catch (err: unknown) {
      const e = err as { status?: number };
      if (e?.status === 409) { setConflictError('تم تعديل القيد بواسطة مستخدم آخر. سيتم إعادة تحميل البيانات.'); refetch(); }
    }
  };

  const handleReverseConfirm = async (reason: string) => {
    setConflictError(null);
    try {
      await reverseMutation.mutateAsync({ id: entryId, command: { reason, rowVersion: entry?.rowVersion ?? '' } });
      setShowReverseDialog(false);
    } catch (err: unknown) {
      const e = err as { status?: number };
      if (e?.status === 409) { setConflictError('تم تعديل القيد بواسطة مستخدم آخر.'); refetch(); setShowReverseDialog(false); }
    }
  };

  const loadingMap: Record<string, boolean | undefined> = {
    submit: submitMutation.isPending, approve: approveMutation.isPending,
    post: postMutation.isPending, reverse: reverseMutation.isPending, cancel: cancelMutation.isPending,
  };
  const handlerMap: Record<string, (() => void) | undefined> = {
    submit: () => handleAction('submit'), approve: () => handleAction('approve'),
    post: () => handleAction('post'), reverse: () => handleAction('reverse'), cancel: () => handleAction('cancel'),
  };

  if (!entry) {
    return <Page title="" loading={isLoading} error={error ? 'خطأ في تحميل القيد' : undefined} />;
  }

  const actions = getActionsForStatus(entry.entryStatus);

  const headerActions = editState.isEditing ? (
    <>
      <Button
        type="submit"
        form="journal-entry-detail-form"
        variant="primary"
        disabled={!editState.canSave || editState.isSaving}
        loading={editState.isSaving}
      >
        حفظ التغييرات
      </Button>
      <Button variant="ghost" disabled={editState.isSaving} onClick={() => setEditState({ ...editState, isEditing: false })}>
        إلغاء
      </Button>
    </>
  ) : (
    <>
      {!entry.isSystemGenerated && actions.map((action) => {
        const isLoading = loadingMap[action];
        const handler = handlerMap[action];
        const actionPermissions: Record<string, boolean> = {
          submit: canSubmit, approve: canApprove, post: canPost, reverse: canReverse, cancel: canCancel,
        };
        const allowed = hasPermission || actionPermissions[action];
        return (
          <Button key={action} type="button" onClick={handler} disabled={isLoading || !allowed}
            variant={action === 'cancel' || action === 'reverse' ? 'destructive' : 'primary'}
            loading={isLoading}>
            {actionLabels[action]}
          </Button>
        );
      })}
      {!entry.isSystemGenerated && actions.length > 0 && (
        <Button type="button" variant="secondary" onClick={() => setEditState({ ...editState, isEditing: true })}>
          تعديل
        </Button>
      )}
    </>
  );

  return (
    <Page
      title={entry.entryNumber}
      loading={isLoading}
      error={error ? 'خطأ في تحميل القيد' : undefined}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <StatusBadge status={entry.entryStatus} />
          {entry.isSystemGenerated && <Badge variant="default">نظام</Badge>}
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="السنة المالية" value={fiscal.data?.fiscalYearCode} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="الفترة" value={fiscal.data?.fiscalPeriodName} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={formatDate(entry.documentDate)} />
        </div>
      }
    >
      {conflictError && (
        <Card variant="default" padding="sm" className="mb-6 text-sm flex items-center justify-between bg-[var(--color-error-container)] text-[var(--color-error)]">
          <div className="flex items-center gap-2">
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            <span>{conflictError}</span>
          </div>
          <Button variant="ghost" size="icon-xs" onClick={() => setConflictError(null)} aria-label="إغلاق">
            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
          </Button>
        </Card>
      )}

      <div className="space-y-6">
        <AccountingJournalEntryDetail
          key={`${entry.rowVersion ?? entry.id}-${editState.isEditing ? 'edit' : 'view'}`}
          entry={entry}
          editing={editState.isEditing}
          onToggleEditing={(editing) => setEditState({ ...editState, isEditing: editing })}
          onStateChange={(state) => setEditState((prev) => ({ ...prev, ...state }))}
          onSaved={() => refetch()}
        />

        {/* ربط العكس */}
        {entry.reversalOfId && (
          <Card variant="default">
            <div className="p-6">
              <p className="text-sm text-[var(--color-on-surface)]">
                هذا القيد عكس لـ <a href={`/accounting/journal-entries/${entry.reversalOfId}`} className="font-bold underline cursor-pointer text-[var(--color-link)]">القيد رقم {entry.reversalOfId}</a>
              </p>
              {entry.reversalReason && <p className="mt-2 text-sm text-[var(--color-on-surface-variant)]">السبب: {entry.reversalReason}</p>}
            </div>
          </Card>
        )}

        {/* سجل الحالة */}
        <Card variant="default">
          <div className="p-6"><StatusLogPanel documentType="JournalEntry" documentId={entry.id} /></div>
        </Card>

        {/* الموافقات */}
        {(entry.entryStatus === 'Submitted' || entry.entryStatus === 'Approved') && (
          <Card variant="default">
            <div className="p-6"><ApprovalsPanel documentType="JournalEntry" documentId={entry.id} /></div>
          </Card>
        )}
      </div>

      <ReverseDialog open={showReverseDialog} entryId={entry.id} entryNumber={entry.entryNumber} lines={entry.lines}
        onConfirm={handleReverseConfirm} onClose={() => setShowReverseDialog(false)}
        isReversing={reverseMutation.isPending} error={reverseMutation.isError ? (reverseMutation.error as Error)?.message : undefined} />
    </Page>
  );
}
