import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useJournalEntry,
  useSubmitJournalEntry,
  useApproveJournalEntry,
  usePostJournalEntry,
  useReverseJournalEntry,
  useCancelJournalEntry,
} from '../hooks/useJournalEntries';
import { usePermission, hasPermission as checkPermission } from '../../../shared/hooks/usePermission';
import { StatusBadge } from '@/components/AccountingStatusBadge';
import { BalanceIndicator } from '@/components/AccountingBalanceIndicator';
import { ReverseDialog } from '@/components/AccountingReverseDialog';
import { ApprovalsPanel } from '@/components/DocumentsApprovalsPanel';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { Button, Card } from '@/components/ui';
function formatDate(value: unknown): string {
  if (!value) return '';
  if (typeof value === 'string') return value;
  if (value instanceof Date) return value.toLocaleDateString('ar-YE');
  return String(value);
}

function getActionsForStatus(status: string) {
  switch (status) {
    case 'Draft': return ['submit', 'cancel'];
    case 'Submitted': return ['approve', 'cancel'];
    case 'Approved': return ['post'];
    case 'Posted': return ['reverse'];
    default: return [];
  }
}

const actionLabels: Record<string, string> = { submit: 'تقديم', approve: 'موافقة', post: 'تسجيل', reverse: 'عكس', cancel: 'إلغاء' };
const permissionMap: Record<string, string> = {
  submit: 'Accounting.JournalEntries.Submit', approve: 'Accounting.JournalEntries.Approve',
  post: 'Accounting.JournalEntries.Post', reverse: 'Accounting.JournalEntries.Reverse', cancel: 'Accounting.JournalEntries.Cancel',
};

export function JournalEntryDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const entryId = Number(id);

  const { data: entry, isLoading, error, refetch } = useJournalEntry(entryId);
  const submitMutation = useSubmitJournalEntry();
  const approveMutation = useApproveJournalEntry();
  const postMutation = usePostJournalEntry();
  const reverseMutation = useReverseJournalEntry();
  const cancelMutation = useCancelJournalEntry();
  const { hasPermission } = usePermission();

  const [showReverseDialog, setShowReverseDialog] = useState(false);
  const [conflictError, setConflictError] = useState<string | null>(null);

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

  if (isLoading) {
    return (
      <div className="max-w-6xl mx-auto py-16 px-6">
        <div className="flex items-center justify-center gap-3 text-[var(--color-on-surface-variant)]">
          <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24" fill="none"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
          <span className="text-sm font-medium">جاري تحميل القيد...</span>
        </div>
      </div>
    );
  }

  if (error || !entry) {
    return (
      <div className="max-w-6xl mx-auto py-16 px-6 text-center">
        <svg className="mx-auto h-12 w-12 mb-4 text-[var(--color-error)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
        </svg>
        <p className="text-sm font-bold text-[var(--color-error)]">خطأ في تحميل القيد</p>
        <Button variant="link" onClick={() => navigate('/accounting/journal-entries')} className="mt-4">العودة للقائمة</Button>
      </div>
    );
  }

  const actions = getActionsForStatus(entry.entryStatus);

  return (
    <div className="max-w-6xl mx-auto py-8 px-6" dir="rtl">
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
        {/* بيانات القيد */}
        <Card variant="default">
          <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-xl font-bold text-[var(--color-on-surface)]">
                  {entry.entryNumber}
                  {entry.isSystemGenerated && (
                    <span className="mr-2 text-[10px] px-2 py-0.5 rounded-full font-bold bg-[var(--color-surface-container-high)] text-[var(--color-on-surface-variant)]">نظام</span>
                  )}
                </h2>
                <p className="text-sm mt-0.5 text-[var(--color-on-surface-variant)]">{formatDate(entry.documentDate)}</p>
              </div>
              <StatusBadge status={entry.entryStatus} />
            </div>
          </div>
          <div className="p-6">
            <div className="grid grid-cols-3 gap-4 text-sm">
              <div><span className="text-[var(--color-on-surface-variant)]">الفترة:</span> <span className="font-bold text-[var(--color-on-surface)]">{entry.periodName || entry.periodId}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">السنة المالية:</span> <span className="font-bold text-[var(--color-on-surface)]">{entry.fiscalYearName || entry.fiscalYearId}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">اليومية:</span> <span className="font-bold text-[var(--color-on-surface)]">{entry.journalName || '-'}</span></div>
            </div>
            {entry.narration && <p className="mt-4 text-sm text-[var(--color-on-surface)]">{entry.narration}</p>}
            {entry.ref && <p className="mt-2 text-sm text-[var(--color-on-surface-variant)]">المرجع: {entry.ref}</p>}
            {entry.postedByName && entry.postedAt && <p className="mt-2 text-sm text-[var(--color-on-surface-variant)]">سجل بواسطة: {entry.postedByName} — {formatDate(entry.postedAt)}</p>}
            {entry.cancelledByName && entry.cancelledAt && <p className="mt-2 text-sm text-[var(--color-on-surface-variant)]">ألغى بواسطة: {entry.cancelledByName} — {formatDate(entry.cancelledAt)}</p>}
          </div>
        </Card>

        {/* الأسطر */}
        <Card variant="default">
          <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
            <h2 className="text-base font-bold text-[var(--color-on-surface)]">الأسطر</h2>
          </div>
          <div className="p-6">
            {entry.lines.length > 0 ? (
              <div className="overflow-x-auto rounded-lg border" style={{ borderColor: 'var(--color-outline-variant)' }}>
                <table className="min-w-full divide-y" style={{ borderColor: 'var(--color-outline-variant)' }}>
                    <thead>
                      <tr style={{ backgroundColor: 'var(--color-surface-container-low)' }}>
                        <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">#</th>
                        <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">الحساب</th>
                        <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">مدين</th>
                        <th className="px-4 py-3 text-end text-xs font-bold text-[var(--color-on-surface-variant)]">دائن</th>
                        <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">الوصف</th>
                        <th className="px-4 py-3 text-start text-xs font-bold text-[var(--color-on-surface-variant)]">الأبعاد</th>
                      </tr>
                    </thead>
                  <tbody className="divide-y" style={{ borderColor: 'var(--color-outline-variant)' }}>
                    {entry.lines.map((line) => (
                      <tr key={line.id} style={{ backgroundColor: 'var(--color-surface)' }}>
                        <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.sequence}</td>
                        <td className="px-4 py-3 text-sm font-bold text-[var(--color-on-surface)]">{line.accountCode} - {line.accountName}</td>
                        <td className="px-4 py-3 text-sm text-end tabular-nums font-bold text-[var(--color-on-surface)]">{line.debit > 0 ? line.debit.toLocaleString('ar-YE') : '-'}</td>
                        <td className="px-4 py-3 text-sm text-end tabular-nums font-bold text-[var(--color-on-surface)]">{line.credit > 0 ? line.credit.toLocaleString('ar-YE') : '-'}</td>
                        <td className="px-4 py-3 text-sm text-[var(--color-on-surface-variant)]">{line.description || '-'}</td>
                        <td className="px-4 py-3 text-xs text-[var(--color-on-surface-variant)]">
                          {[
                            line.fundName && `صندوق: ${line.fundName}`,
                            line.projectName && `مشروع: ${line.projectName}`,
                            line.budgetItemCode && `بند: ${line.budgetItemCode}`,
                            line.encumbranceNumber && `التزام: ${line.encumbranceNumber}`,
                            line.paymentOrderNumber && `دفع: ${line.paymentOrderNumber}`,
                          ].filter(Boolean).join(' · ') || '-'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد أسطر</p>
            )}
            <div className="mt-4"><BalanceIndicator totalDebit={entry.totalDebit} totalCredit={entry.totalCredit} /></div>
          </div>
        </Card>

        {/* الإجراءات */}
        {actions.length > 0 && !entry.isSystemGenerated && (
          <Card variant="default">
            <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
              <h2 className="text-base font-bold text-[var(--color-on-surface)]">الإجراءات</h2>
            </div>
            <div className="p-6">
              <div className="flex flex-wrap gap-2">
                {actions.map((action) => {
                  const isLoading = loadingMap[action];
                  const handler = handlerMap[action];
                  const allowed = hasPermission || checkPermission(permissionMap[action]);
                  return (
                    <Button key={action} type="button" onClick={handler} disabled={isLoading || !allowed}
                      variant={action === 'cancel' || action === 'reverse' ? 'destructive' : 'primary'}
                      loading={isLoading}>
                      {actionLabels[action]}
                    </Button>
                  );
                })}
              </div>
            </div>
          </Card>
        )}

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
    </div>
  );
}
