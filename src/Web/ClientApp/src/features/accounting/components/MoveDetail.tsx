import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import type { MoveDto } from '../types';
import { MoveLinesEditor } from './MoveLinesEditor';
import { useSubmitMove, useApproveMove, usePostMove, useCancelMove } from '../hooks/useMoves';
import { ReverseDialog } from './ReverseDialog';
import { showToast } from '@/components/ui/Toast';

interface MoveDetailProps {
  move: MoveDto;
}

const statusVariantMap: Record<string, 'draft' | 'pending' | 'approved' | 'active' | 'closed'> = {
  Draft: 'draft',
  Submitted: 'pending',
  Approved: 'approved',
  Posted: 'active',
  Reversed: 'closed',
  Cancelled: 'closed',
};

const statusLabelsAr: Record<string, string> = {
  Draft: 'مسودة',
  Submitted: 'مرسل',
  Approved: 'موافق عليه',
  Posted: 'مرحل',
  Reversed: 'ملغى عكسي',
  Cancelled: 'ملغى',
};

export function MoveDetail({ move }: MoveDetailProps) {
  const submit = useSubmitMove();
  const approve = useApproveMove();
  const post = usePostMove();
  const cancel = useCancelMove();
  const [reverseOpen, setReverseOpen] = useState(false);

  const isDraft = move.entryStatus === 'Draft';
  const isSubmitted = move.entryStatus === 'Submitted';
  const isApproved = move.entryStatus === 'Approved';
  const isPosted = move.entryStatus === 'Posted';

  const handleAction = async (
    mutate: (vars: { id: number; rowVersion: string }) => Promise<unknown>,
    successMessage?: string
  ) => {
    try {
      await mutate({ id: move.id, rowVersion: move.rowVersion });
      if (successMessage) showToast('success', successMessage);
    } catch (e) {
      const msg = (e as Error).message;
      if (msg.includes('409') || msg.includes('conflict') || msg.includes('تم تعديل')) {
        showToast('warning', 'تم تعديل القيد من مستخدم آخر');
      } else {
        showToast('error', msg);
      }
    }
  };

  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [postDialogOpen, setPostDialogOpen] = useState(false);

  return (
    <div className="space-y-4">
      {/* Header card — Protocol §23 Page Composition: header region */}
      <div className="bg-[var(--color-surface-container-lowest)] border border-[var(--color-border-container)] rounded-lg p-4 space-y-3">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-lg font-semibold text-on-surface">{move.entryNumber}</h2>
            <p className="text-xs text-on-surface-variant">تاريخ المستند: <span className="tabular-nums">{move.documentDate}</span> {move.postingDate ? `• تاريخ الترحيل: ${move.postingDate}` : ''}</p>
          </div>
          <StatusBadge variant={statusVariantMap[move.entryStatus] ?? 'draft'}>
              {statusLabelsAr[move.entryStatus] ?? move.entryStatus}
            </StatusBadge>
        </div>

        <div className="grid grid-cols-2 md:grid-cols-5 gap-3 text-xs">
          <div className="flex items-center gap-2">
            <span className="font-medium text-on-surface-variant">اليومية:</span>
            <span className="font-medium text-on-surface">{move.journalName ?? '—'}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="font-medium text-on-surface-variant">المرجع:</span>
            <span className="font-medium">{move.ref ?? '—'}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="font-medium text-on-surface-variant">الفترة / السنة:</span>
            <span className="tabular-nums font-medium">{move.periodId} / {move.fiscalYearId}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="font-medium text-on-surface-variant">العملة الأساسية:</span>
            <span className="tabular-nums font-medium">{move.lines[0]?.currencyId ?? '—'}</span>
          </div>
          {move.entryType ? (
            <div className="flex items-center gap-2">
              <span className="font-medium text-on-surface-variant">نوع القيد:</span>
              <span className="font-medium text-on-surface">{move.entryType}</span>
            </div>
          ) : null}
        </div>

        {move.narration ? (
          <div className="text-sm">
            <span className="block text-xs text-on-surface-variant mb-1">البيان</span>
            <p className="text-on-surface leading-relaxed">{move.narration}</p>
          </div>
        ) : null}

        {/* Audit metadata — US2: Posted/Cancelled entries */}
        {move.entryStatus === 'Posted' && move.postedByName ? (
          <div className="text-xs text-on-surface-variant">
            رحّل بواسطة: <span className="font-medium text-on-surface">{move.postedByName}</span>
            {move.postedAt ? <span className="tabular-nums"> • {move.postedAt}</span> : null}
          </div>
        ) : null}
        {move.entryStatus === 'Cancelled' && move.cancelledByName ? (
          <div className="text-xs text-on-surface-variant">
            ألغى بواسطة: <span className="font-medium text-on-surface">{move.cancelledByName}</span>
            {move.cancelledAt ? <span className="tabular-nums"> • {move.cancelledAt}</span> : null}
          </div>
        ) : null}

        {/* Operational fields — US3 */}
        <div className="flex flex-wrap gap-3 text-xs">
          {move.isSystemGenerated ? (
            <StatusBadge variant="draft">تلقائي</StatusBadge>
          ) : null}
        </div>

        <div className="flex gap-3 text-xs">
          {move.reversalOfMoveId ? (
            <span className="text-on-surface-variant">عكس للقيد #{move.reversalOfMoveId}</span>
          ) : null}
          {move.reversalReason ? (
            <span className="text-on-surface-variant">سبب العكس: {move.reversalReason}</span>
          ) : null}
        </div>
      </div>

      {/* Workflow action bar */}
      <div className="flex flex-wrap gap-2" role="toolbar" aria-label="إجراءات القيد">
        {isDraft ? (
          <>
            <Button variant="primary" size="sm" onClick={() => handleAction(submit.mutateAsync, 'تم الإرسال للاعتماد')} disabled={submit.isPending} loading={submit.isPending}>
              إرسال للاعتماد
            </Button>
            <Button variant="outline" size="sm" onClick={() => setCancelDialogOpen(true)} disabled={cancel.isPending}>
              إلغاء
            </Button>
          </>
        ) : null}
        {isSubmitted ? (
          <>
            <Button variant="primary" size="sm" onClick={() => handleAction(approve.mutateAsync, 'تم الاعتماد')} disabled={approve.isPending} loading={approve.isPending}>
              اعتماد
            </Button>
            <Button variant="outline" size="sm" onClick={() => setCancelDialogOpen(true)} disabled={cancel.isPending}>
              إلغاء
            </Button>
          </>
        ) : null}
        {isApproved ? (
          <Button variant="primary" size="sm" onClick={() => setPostDialogOpen(true)} disabled={post.isPending} loading={post.isPending}>
            ترحيل
          </Button>
        ) : null}
        {isPosted ? (
          <Button variant="destructive" size="sm" onClick={() => setReverseOpen(true)}>
            عكس القيد
          </Button>
        ) : null}
      </div>

      {/* Lines editor — Protocol §12 Tables + §26 Monetary */}
      <MoveLinesEditor
        moveId={move.id}
        lines={move.lines}
        entryStatus={move.entryStatus}
        documentDate={move.documentDate}
      />

      {reverseOpen ? (
        <ReverseDialog moveId={move.id} rowVersion={move.rowVersion} onClose={() => setReverseOpen(false)} />
      ) : null}

      <ConfirmDialog
        open={cancelDialogOpen}
        onClose={() => setCancelDialogOpen(false)}
        onConfirm={() => {
          setCancelDialogOpen(false);
          handleAction(cancel.mutateAsync, 'تم الإلغاء');
        }}
        message="هل أنت متأكد من إلغاء القيد؟"
        confirmLabel="إلغاء القيد"
        destructive
        loading={cancel.isPending}
      />

      <ConfirmDialog
        open={postDialogOpen}
        onClose={() => setPostDialogOpen(false)}
        onConfirm={() => {
          setPostDialogOpen(false);
          handleAction(post.mutateAsync, 'تم الترحيل');
        }}
        message="هل أنت متأكد من ترحيل القيد؟"
        confirmLabel="ترحيل"
        loading={post.isPending}
      />
    </div>
  );
}
