import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useRecurringEntry,
  usePauseRecurringEntry,
  useResumeRecurringEntry,
  useCancelRecurringEntry,
} from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS, STATUS_LABELS } from '../shared/types';
import { Button, Textarea } from '@/components/ui';

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

  if (isLoading) return <div className="text-center py-8">جاري التحميل...</div>;
  if (!entry) return <div className="text-center py-8">الجدول غير موجود</div>;

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
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <Button variant="link" onClick={() => navigate(-1)} className="mb-2">
            ← رجوع
          </Button>
          <h1 className="text-2xl font-bold">{entry.name}</h1>
          <p className="text-gray-500">{entry.entryNumber}</p>
        </div>
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
      </div>

      <div className="grid grid-cols-2 gap-6">
        <div className="space-y-4">
          <h2 className="text-lg font-semibold">التفاصيل</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-sm text-gray-500">الدفتر</label>
              <p>{entry.journalName}</p>
            </div>
            <div>
              <label className="text-sm text-gray-500">القالب</label>
              <p>{entry.templateName ?? 'بدون قالب'}</p>
            </div>
            <div>
              <label className="text-sm text-gray-500">الدورية</label>
              <p>{FREQUENCY_LABELS[entry.frequency ?? '']}</p>
            </div>
            <div>
              <label className="text-sm text-gray-500">المبلغ</label>
              <p>{entry.amount?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            <div>
              <label className="text-sm text-gray-500">تاريخ البداية</label>
              <p>{new Date(entry.startDate!).toLocaleDateString('ar-YE')}</p>
            </div>
            <div>
              <label className="text-sm text-gray-500">تاريخ النهاية</label>
              <p>
                {entry.endDate
                  ? new Date(entry.endDate).toLocaleDateString('ar-YE')
                  : 'غير محدد'}
              </p>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <h2 className="text-lg font-semibold">التوليد</h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-sm text-gray-500">التاريخ القادم</label>
              <p>
                {entry.nextExecutionDate
                  ? new Date(entry.nextExecutionDate).toLocaleDateString('ar-YE')
                  : '-'}
              </p>
            </div>
            <div>
              <label className="text-sm text-gray-500">آخر تنفيذ</label>
              <p>
                {entry.lastExecutedAt
                  ? new Date(entry.lastExecutedAt).toLocaleString('ar-YE')
                  : 'لم يتم التوليد بعد'}
              </p>
            </div>
            <div>
              <label className="text-sm text-gray-500">الحالة</label>
              <p>
                <span
                  className={`px-2 py-1 rounded text-sm ${
                    entry.status === 'Active'
                      ? 'bg-green-100 text-green-800'
                      : entry.status === 'Paused'
                      ? 'bg-yellow-100 text-yellow-800'
                      : entry.status === 'Cancelled'
                      ? 'bg-red-100 text-red-800'
                      : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {STATUS_LABELS[entry.status ?? '']}
                </span>
              </p>
            </div>
            <div>
              <label className="text-sm text-gray-500">القيد المولد</label>
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
                  <span className="text-gray-400">لم يتم التوليد بعد</span>
                )}
              </p>
            </div>
          </div>
        </div>
      </div>

      {showPauseDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-96">
            <h3 className="text-lg font-semibold mb-4">إيقاف الجدول</h3>
            <Textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="سبب الإيقاف (اختياري)"
              rows={3}
              className="mb-4"
            />
            <div className="flex gap-2 justify-end">
              <Button variant="ghost" onClick={() => setShowPauseDialog(false)}>
                إلغاء
              </Button>
              <Button variant="outline" onClick={handlePause}>
                إيقاف
              </Button>
            </div>
          </div>
        </div>
      )}

      {showCancelDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-96">
            <h3 className="text-lg font-semibold mb-4">إلغاء الجدول</h3>
            <p className="text-red-600 mb-4">هذا الإجراء لا يمكن التراجع عنه</p>
            <Textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="سبب الإلغاء (اختياري)"
              rows={3}
              className="mb-4"
            />
            <div className="flex gap-2 justify-end">
              <Button variant="ghost" onClick={() => setShowCancelDialog(false)}>
                رجوع
              </Button>
              <Button variant="destructive" onClick={handleCancel}>
                إلغاء الجدول
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
