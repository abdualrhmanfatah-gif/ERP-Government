import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Calendar } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { Loading } from '@/components/ui/Loading';
import { ErrorState } from '@/components/ui/ErrorState';
import { InfoCard, InfoRow } from '../components/InfoCard';
import { AuditInfo } from '../components/AuditInfo';
import { StatusBadge } from '../components/StatusBadge';
import { ClosingEntriesList } from '../closing-entries/ClosingEntriesList';
import { ClosingEntryDetail } from '../closing-entries/ClosingEntryDetail';
import { useClosingEntriesByFiscalYear } from '../hooks/useClosingEntriesByFiscalYear';
import { useGenerateClosingEntry } from '../hooks/useGenerateClosingEntry';
import { useApproveClosingEntry } from '../hooks/useApproveClosingEntry';
import { useReverseClosingEntry } from '../hooks/useReverseClosingEntry';
import { FiscalYearsClient } from '../../../../web-api-client';
import { authFetchFn } from '../../../../shared/utils/auth-fetch';
import type { FiscalYearStatus } from '../types';
import type { ClosingEntryDto } from '../types';

const client = new FiscalYearsClient('', authFetchFn);

export function FiscalYearDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const fiscalYearId = id ? Number(id) : 0;

  const [showGenerateDialog, setShowGenerateDialog] = useState(false);
  const [selectedClosingEntry, setSelectedClosingEntry] = useState<ClosingEntryDto | null>(null);

  const {
    data: year,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ['fiscalYear', fiscalYearId],
    queryFn: () => client.fiscalYearsGET(fiscalYearId),
    enabled: fiscalYearId > 0,
  });

  const { data: closingEntries, isLoading: isLoadingEntries } = useClosingEntriesByFiscalYear(fiscalYearId);
  const { mutate: generateClosing, isPending: isGenerating } = useGenerateClosingEntry();
  const { mutate: approveClosing, isPending: isApproving } = useApproveClosingEntry();
  const { mutate: reverseClosing, isPending: isReversing } = useReverseClosingEntry();

  const formatDate = (d: string | Date | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  if (isLoading) return <Loading />;
  if (error || !year) return <ErrorState message="فشل تحميل تفاصيل السنة المالية" onRetry={() => refetch()} />;

  // ─── Closing Entry Detail View ──────────────────────────────────────
  if (selectedClosingEntry) {
    return (
      <div>
        <PageHeader
          title={selectedClosingEntry.closingEntryNumber}
          description="تفاصيل قيد الإغلاق السنوي"
          actions={
            <Button variant="ghost" size="sm" onClick={() => setSelectedClosingEntry(null)}>
              العودة
            </Button>
          }
        />
        <ClosingEntryDetail
          entry={selectedClosingEntry}
          onApprove={(entryId) => approveClosing(entryId, {
            onSuccess: () => {
              setSelectedClosingEntry(null);
              refetch();
            },
          })}
          onReverse={(entryId, reason) => reverseClosing({ id: entryId, reason }, {
            onSuccess: () => {
              setSelectedClosingEntry(null);
              refetch();
            },
          })}
          isApproving={isApproving}
          isReversing={isReversing}
        />
      </div>
    );
  }

  // ─── Fiscal Year Detail View ────────────────────────────────────────
  return (
    <div>
      <PageHeader
        title={year.name!}
        description={`تفاصيل السنة المالية ${year.yearNumber}`}
        actions={
          <div className="flex gap-2">
            <Button
              variant="ghost"
              size="sm"
              icon={<Calendar size={16} />}
              onClick={() => navigate(`/financial/fiscal-years/${fiscalYearId}/periods`)}
            >
              إدارة الفترات
            </Button>
            <Button variant="ghost" size="sm" onClick={() => navigate('/financial/fiscal-years')}>
              العودة
            </Button>
          </div>
        }
      />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <InfoCard title="المعلومات الأساسية">
          <InfoRow label="السنة" value={year.yearNumber} />
          <InfoRow label="الاسم" value={year.name} />
          <InfoRow label="تاريخ البداية" value={formatDate(year.startDate)} />
          <InfoRow label="تاريخ النهاية" value={formatDate(year.endDate)} />
          <InfoRow label="الحالة" value={<StatusBadge status={year.status as FiscalYearStatus} />} />
        </InfoCard>

        <InfoCard title="معلومات التدقيق">
          <AuditInfo
            createdAt={year.createdAt}
            createdBy={year.createdBy}
            updatedAt={year.updatedAt}
            updatedBy={year.updatedBy}
          />
        </InfoCard>
      </div>

      {/* ─── Closing Entries Section ─────────────────────────── */}
      <div className="mt-6">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-[var(--color-on-surface)]">قيود الإغلاق السنوي</h2>
          {(year.status === 'Open' || year.status === 'SoftClosed') && !closingEntries?.length && (
            <Button
              variant="primary"
              size="sm"
              onClick={() => setShowGenerateDialog(true)}
              disabled={isGenerating}
            >
              {isGenerating ? 'جاري التوليد...' : 'توليد قيد الإغلاق السنوي'}
            </Button>
          )}
        </div>
        <div className="rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)]">
          <ClosingEntriesList
            entries={closingEntries ?? []}
            isLoading={isLoadingEntries}
            onSelect={(entry) => setSelectedClosingEntry(entry)}
          />
        </div>
      </div>

      {/* ─── Generate Confirmation Dialog ────────────────────── */}
      <Dialog
        open={showGenerateDialog}
        onClose={() => setShowGenerateDialog(false)}
        title="تأكيد توليد قيد الإغلاق"
      >
        <p className="mb-4 text-sm text-[var(--color-on-surface-variant)]">
          سيتم توليد قيد إغلاق سنوي وإقفال جميع الفترات المالية. لا يمكن التراجع عن هذا الإجراء.
        </p>
        <div className="flex justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowGenerateDialog(false)}
          >
            إلغاء
          </Button>
          <Button
            variant="primary"
            size="sm"
            onClick={() => {
              generateClosing(
                { fiscalYearId },
                {
                  onSuccess: () => setShowGenerateDialog(false),
                }
              );
            }}
          >
            تأكيد التوليد
          </Button>
        </div>
      </Dialog>
    </div>
  );
}
