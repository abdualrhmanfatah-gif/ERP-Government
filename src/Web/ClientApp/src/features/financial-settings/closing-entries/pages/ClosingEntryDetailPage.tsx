import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { Button, Loading, ConfirmDialog } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useClosingEntryDetail, useApproveClosingEntry, useReverseClosingEntry } from '../../hooks/useClosingEntries';
import { FiscalYearStatusBadge } from '../../components/FiscalYearStatusBadge';

export default function ClosingEntryDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const entryId = Number(id);

  const canApprove = usePermission('ClosingEntries.Approve' as never);
  const canReverse = usePermission('ClosingEntries.Reverse' as never);

  const { data: entry, isLoading } = useClosingEntryDetail(entryId);
  const approveMutation = useApproveClosingEntry();
  const reverseMutation = useReverseClosingEntry();

  const [confirmAction, setConfirmAction] = useState<'approve' | 'reverse' | null>(null);
  const [reverseReason, setReverseReason] = useState('');

  if (isLoading) return <Loading />;
  if (!entry) return <div className="text-center py-12 text-[var(--color-on-surface-variant)]">قيد الإغلاق غير موجود</div>;

  function handleApprove() {
    approveMutation.mutate(entryId, {
      onSuccess: () => { notify({ type: 'success', title: 'تم اعتماد قيد الإغلاق' }); setConfirmAction(null); },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الاعتماد' }),
    });
  }

  function handleReverse() {
    if (!reverseReason.trim()) {
      notify({ type: 'error', title: 'أدخل سبب التراجع' });
      return;
    }
    reverseMutation.mutate(
      { id: entryId, rowVersion: entry.rowVersion, reason: reverseReason },
      {
        onSuccess: () => { notify({ type: 'success', title: 'تم إنشاء قيد التراجع' }); setConfirmAction(null); setReverseReason(''); },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التراجع' }),
      },
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/closing-entries')} className="cursor-pointer">
            <ArrowRight size={18} />
          </Button>
          <div>
            <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">{entry.closingEntryNumber}</h1>
            <p className="text-body-sm text-[var(--color-on-surface-variant)]">قيد إغلاق — {entry.fiscalYearName}</p>
          </div>
          <FiscalYearStatusBadge status={entry.status} />
        </div>
        <div className="flex items-center gap-2">
          {entry.status === 'Approved' && canApprove && (
            <Button onClick={() => setConfirmAction('approve')} className="cursor-pointer">
              اعتماد وترحيل
            </Button>
          )}
          {entry.status === 'Posted' && canReverse && !entry.isReversal && (
            <Button variant="destructive" onClick={() => setConfirmAction('reverse')} className="cursor-pointer">
              تراجع
            </Button>
          )}
        </div>
      </div>

      {/* Info Card */}
      <div className="rounded-xl border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">رقم القيد</div>
            <div className="text-sm font-mono font-medium text-[var(--color-on-surface)]">{entry.closingEntryNumber}</div>
          </div>
          <div>
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">التاريخ</div>
            <div className="text-sm font-medium text-[var(--color-on-surface)]">
              {new Intl.DateTimeFormat('ar-EG', { dateStyle: 'long' }).format(new Date(entry.closingDate))}
            </div>
          </div>
          <div>
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">الحالة</div>
            <FiscalYearStatusBadge status={entry.status} />
          </div>
        </div>
        {entry.description && (
          <div className="mt-4 pt-4 border-t border-[var(--color-border-container)]">
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">الوصف</div>
            <div className="text-sm text-[var(--color-on-surface)]">{entry.description}</div>
          </div>
        )}
        {entry.isReversal && entry.reversalOfNumber && (
          <div className="mt-4 pt-4 border-t border-[var(--color-border-container)]">
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">逆转 من</div>
            <div className="text-sm text-[var(--color-on-surface)] font-mono">{entry.reversalOfNumber}</div>
          </div>
        )}
        {entry.journalEntryEntryNumber && (
          <div className="mt-4 pt-4 border-t border-[var(--color-border-container)]">
            <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">قيد اليومية المرتبط</div>
            <div className="text-sm text-[var(--color-on-surface)] font-mono">{entry.journalEntryEntryNumber}</div>
          </div>
        )}
      </div>

      <ConfirmDialog
        open={confirmAction === 'approve'}
        onClose={() => setConfirmAction(null)}
        onConfirm={handleApprove}
        title="اعتماد قيد الإغلاق"
        message="هل تريد اعتماد وترحيل قيد الإغلاق؟ سيتم إنشاء قيد يومية."
        loading={approveMutation.isPending}
      />

      <ConfirmDialog
        open={confirmAction === 'reverse'}
        onClose={() => { setConfirmAction(null); setReverseReason(''); }}
        onConfirm={handleReverse}
        title="反转 قيد الإغلاق"
        message={
          <div className="space-y-2">
            <p>هل تريد إنشاء قيد تراجع لهذا القيد؟</p>
            <textarea
              value={reverseReason}
              onChange={(e) => setReverseReason(e.target.value)}
              placeholder="سبب التراجع..."
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)] text-sm"
              rows={3}
            />
          </div>
        }
        loading={reverseMutation.isPending}
      />
    </div>
  );
}
