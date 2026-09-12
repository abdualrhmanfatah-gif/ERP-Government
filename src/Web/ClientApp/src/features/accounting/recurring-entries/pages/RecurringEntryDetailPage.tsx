import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useRecurringEntry,
  usePauseRecurringEntry,
  useResumeRecurringEntry,
  useCancelRecurringEntry,
} from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS, STATUS_LABELS } from '../shared/types';
import { Page, Button, Textarea, Badge, Dialog, EmptyState, Label, Card } from '@/components/ui';

export default function RecurringEntryDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const entryId = Number(id);
  const { data: entry, isLoading } = useRecurringEntry(entryId);
  const pauseMutation = usePauseRecurringEntry();
  const resumeMutation = useResumeRecurringEntry();
  const cancelMutation = useCancelRecurringEntry();

  const [reason, setReason] = useState('');
  const [showPauseDialog, setShowPauseDialog] = useState(false);
  const [showCancelDialog, setShowCancelDialog] = useState(false);

  if (!entry) return <EmptyState message="الجدول غير موجود" />;

  const canPause = entry.status === 'Active';
  const canResume = entry.status === 'Paused';
  const canCancel = entry.status === 'Active' || entry.status === 'Paused';

  const handlePause = async () => {
    await pauseMutation.mutateAsync({
      id: entryId,
      data: { id: entryId, reason: reason || undefined, rowVersion: (entry as Record<string, unknown>).rowVersion as string },
    });
    setShowPauseDialog(false);
    setReason('');
  };

  const handleResume = async () => {
    await resumeMutation.mutateAsync({
      id: entryId,
      data: { id: entryId, rowVersion: (entry as Record<string, unknown>).rowVersion as string },
    });
  };

  const handleCancel = async () => {
    await cancelMutation.mutateAsync({
      id: entryId,
      data: { id: entryId, reason: reason || undefined, rowVersion: (entry as Record<string, unknown>).rowVersion as string },
    });
    setShowCancelDialog(false);
    setReason('');
  };

  return (
    <Page
      title={entry.name}
      description={entry.entryNumber}
      loading={isLoading}
      actions={
        <div className="flex gap-2">
          {canPause && (
            <Button variant="outline" onClick={() => setShowPauseDialog(true)}>
              إيقاف
            </Button>
          )}
          {canResume && (
            <Button variant="primary" onClick={handleResume}>
              استئناف
            </Button>
          )}
          {canCancel && (
            <Button variant="destructive" onClick={() => setShowCancelDialog(true)}>
              إلغاء
            </Button>
          )}
        </div>
      }
    >

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <div className="space-y-4">
            <h2 className="text-lg font-semibold">التفاصيل</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-sm text-muted-foreground">الدفتر</Label>
                <p>{entry.journalName}</p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">القالب</Label>
                <p>{entry.templateName ?? 'بدون قالب'}</p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">الدورية</Label>
                <p>{FREQUENCY_LABELS[entry.frequency ?? '']}</p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">المبلغ</Label>
                <p>{entry.amount?.toLocaleString('ar-YE') ?? '-'}</p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">تاريخ البداية</Label>
                <p>{new Date(entry.startDate!).toLocaleDateString('ar-YE')}</p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">تاريخ النهاية</Label>
                <p>
                  {entry.endDate
                    ? new Date(entry.endDate).toLocaleDateString('ar-YE')
                    : 'غير محدد'}
                </p>
              </div>
            </div>
          </div>
        </Card>

        <Card>
          <div className="space-y-4">
            <h2 className="text-lg font-semibold">التوليد</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label className="text-sm text-muted-foreground">التاريخ القادم</Label>
                <p>
                  {entry.nextExecutionDate
                    ? new Date(entry.nextExecutionDate).toLocaleDateString('ar-YE')
                    : '-'}
                </p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">آخر تنفيذ</Label>
                <p>
                  {entry.lastExecutedAt
                    ? new Date(entry.lastExecutedAt).toLocaleString('ar-YE')
                    : 'لم يتم التوليد بعد'}
                </p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">الحالة</Label>
                <p>
                  <Badge
                    variant={
                      entry.status === 'Active'
                        ? 'success'
                        : entry.status === 'Paused'
                        ? 'warning'
                        : entry.status === 'Cancelled'
                        ? 'danger'
                        : 'default'
                    }
                  >
                    {STATUS_LABELS[entry.status ?? '']}
                  </Badge>
                </p>
              </div>
              <div>
                <Label className="text-sm text-muted-foreground">القيد المولد</Label>
                <p>
                  {entry.generatedJournalEntryId ? (
                    <Button
                      variant="link"
                      onClick={() =>
                        navigate(`/accounting/journal-entries/${entry.generatedJournalEntryId}`)
                      }
                    >
                      عرض القيد
                    </Button>
                  ) : (
                    <span className="text-muted-foreground">لم يتم التوليد بعد</span>
                  )}
                </p>
              </div>
            </div>
          </div>
        </Card>
      </div>

      <Dialog
        open={showPauseDialog}
        onClose={() => setShowPauseDialog(false)}
        title="إيقاف الجدول"
        footer={
          <>
            <Button variant="ghost" onClick={() => setShowPauseDialog(false)}>
              إلغاء
            </Button>
            <Button variant="outline" onClick={handlePause} loading={pauseMutation.isPending}>
              إيقاف
            </Button>
          </>
        }
      >
        <Textarea
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="سبب الإيقاف (اختياري)"
          label="سبب الإيقاف"
          rows={3}
        />
      </Dialog>

      <Dialog
        open={showCancelDialog}
        onClose={() => setShowCancelDialog(false)}
        title="إلغاء الجدول"
        footer={
          <>
            <Button variant="ghost" onClick={() => setShowCancelDialog(false)}>
              رجوع
            </Button>
            <Button variant="destructive" onClick={handleCancel} loading={cancelMutation.isPending}>
              إلغاء الجدول
            </Button>
          </>
        }
      >
        <p className="text-[var(--color-error)] mb-4">هذا الإجراء لا يمكن التراجع عنه</p>
        <Textarea
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="سبب الإلغاء (اختياري)"
          label="سبب الإلغاء"
          rows={3}
        />
      </Dialog>
    </Page>
  );
}
