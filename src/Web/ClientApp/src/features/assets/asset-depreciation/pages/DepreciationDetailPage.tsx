import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeftRight, Eye } from 'lucide-react';
import { Button, Card, CardContent, CardHeader, CardTitle, ConfirmDialog, DataGrid, Dialog, EmptyState, ErrorState, Loading, MoneyDisplay, Page, Stack, StatusBadge } from '@/components/ui';
import { MetaItem } from '@/components/MetaItem';
import { useJournalEntry } from '@/features/accounting/hooks/useJournalEntries';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { formatDate, formatPercent } from '@/shared/utils/formatters';
import type { JournalEntryLineDto } from '@/web-api-client';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { useDepreciationRunDetail, usePostDepreciation } from '../hooks/useDepreciation';
import { getDepreciationMethodLabel } from '../../shared/depreciation-method';
import { getDepreciationRunStatusBadge } from '../shared/status';
import type { DepreciationScheduleDetail } from '../shared/types';

export function DepreciationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const runId = Number(id);
  const { data: run, isLoading, error, refetch } = useDepreciationRunDetail(runId);
  const postDepreciation = usePostDepreciation();
  const [confirmPost, setConfirmPost] = useState(false);
  const [previewJournalEntryId, setPreviewJournalEntryId] = useState<number | null>(null);
  const { hasPermission: canReadJournalEntry } = usePermission(PERMISSIONS.Accounting.JournalEntries.Read);
  const journalPreviewOpen = canReadJournalEntry && previewJournalEntryId != null && previewJournalEntryId === run?.journalEntryId;
  const journalEntry = useJournalEntry(journalPreviewOpen ? previewJournalEntryId : 0);

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (error) return <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>{null}</Page>;
  if (!run) return <Page title="" onBack={() => navigate('/assets/depreciation')}><EmptyState message="عملية الإهلاك غير موجودة" /></Page>;

  const badge = getDepreciationRunStatusBadge(run.status);
  const journalTotals = new Map<number | undefined, { debit: number; credit: number }>();
  for (const line of journalEntry.data?.lines ?? []) {
    const totals = journalTotals.get(line.currencyId) ?? { debit: 0, credit: 0 };
    journalTotals.set(line.currencyId, { debit: totals.debit + (line.debit ?? 0), credit: totals.credit + (line.credit ?? 0) });
  }

  return (
    <Page
      title={`عملية الإهلاك ${run.runNumber}`}
      description={<StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>}
      onBack={() => navigate('/assets/depreciation')}
      maxWidth="full"
      actions={<div className="flex gap-2">
        {run.journalEntryId != null && canReadJournalEntry && <Button variant="outline" icon={<Eye size={16} />} onClick={() => setPreviewJournalEntryId(run.journalEntryId!)}>استعراض القيد</Button>}
        {run.status === 'Draft' && <Button variant="primary" icon={<ArrowLeftRight size={16} />} onClick={() => setConfirmPost(true)}>ترحيل</Button>}
      </div>}
    >
      <Stack gap="md">
        <Card padding="sm">
          <CardHeader className="p-0 pb-2">
            <CardTitle className="text-lg">بيانات العملية</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
              <MetaItem label="السنة المالية" value={run.fiscalYear} />
              <MetaItem label="الفترة" value={run.periodNumber} />
              <MetaItem label="التاريخ" value={formatDate(run.depreciationDate)} />
              <MetaItem label="عدد الأصول" value={run.scheduleLines?.length ?? run.assetsCount} />
              <MetaItem label="إجمالي الإهلاك" value={<MoneyDisplay value={run.totalDepreciation} />} />
              {run.journalEntryId != null && <MetaItem label="قيد الترحيل" value={run.journalEntryId} />}
            </div>
          </CardContent>
        </Card>

        <DataGrid<DepreciationScheduleDetail>
        data={run.scheduleLines ?? []}
        rowKey={(row) => row.id}
        emptyMessage="لا توجد تفاصيل إهلاك"
        columns={[
          { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120, cell: (row) => <span dir="ltr" className="font-mono">{row.assetCode}</span> },
          { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
          { id: 'method', accessorKey: 'method', header: 'الطريقة', width: 120, cell: (row) => getDepreciationMethodLabel(row.method) },
          { id: 'rate', accessorKey: 'rate', header: 'النسبة', align: 'right', width: 90, cell: (row) => formatPercent(row.rate) },
          { id: 'openingBookValue', accessorKey: 'openingBookValue', header: 'القيمة الدفترية الافتتاحية', align: 'right', cell: (row) => <MoneyDisplay value={row.openingBookValue} /> },
          { id: 'amount', accessorKey: 'amount', header: 'إهلاك الفترة', align: 'right', cell: (row) => <MoneyDisplay value={row.amount} /> },
          { id: 'closingAccumulatedDepreciation', accessorKey: 'closingAccumulatedDepreciation', header: 'مجمع الإهلاك الختامي', align: 'right', cell: (row) => <MoneyDisplay value={row.closingAccumulatedDepreciation} /> },
          { id: 'closingBookValue', accessorKey: 'closingBookValue', header: 'القيمة الدفترية الختامية', align: 'right', cell: (row) => <MoneyDisplay value={row.closingBookValue} /> },
        ]}
      />
      </Stack>

      <Dialog
        open={journalPreviewOpen}
        onClose={() => setPreviewJournalEntryId(null)}
        title="القيد الناتج عن الإهلاك"
        className="!max-w-[64rem]"
        footer={<Button variant="outline" onClick={() => setPreviewJournalEntryId(null)}>إغلاق</Button>}
      >
        {journalPreviewOpen && (
          journalEntry.isLoading ? <Loading text="جاري تحميل القيد…" /> :
          journalEntry.error ? <ErrorState message={getQueryErrorMessage(journalEntry.error)} onRetry={() => void journalEntry.refetch()} /> :
          !journalEntry.data ? <EmptyState message="القيد غير موجود" /> :
          <div className="space-y-4">
            <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
              <MetaItem label="رقم القيد" value={journalEntry.data.entryNumber} />
              <MetaItem label="التاريخ" value={formatDate(journalEntry.data.documentDate)} />
              <MetaItem label="اليومية" value={journalEntry.data.journalName} />
              <MetaItem label="المرجع" value={journalEntry.data.ref} />
            </div>
            <div><span className="text-[var(--color-on-surface-variant)]">البيان: </span><span className="whitespace-pre-wrap break-words">{journalEntry.data.narration || '—'}</span></div>
            <DataGrid<JournalEntryLineDto>
              data={journalEntry.data.lines ?? []}
              rowKey={(line) => line.id!}
              emptyMessage="لا توجد سطور للقيد"
              columns={[
                { id: 'accountCode', accessorKey: 'accountCode', header: 'كود الحساب', cell: (line) => <span dir="ltr" className="font-mono">{line.accountCode}</span> },
                { id: 'accountName', accessorKey: 'accountName', header: 'الحساب' },
                { id: 'description', accessorKey: 'description', header: 'البيان' },
                { id: 'currencyId', accessorKey: 'currencyId', header: 'معرّف العملة' },
                { id: 'debit', accessorKey: 'debit', header: 'مدين', align: 'right', cell: (line) => <MoneyDisplay value={line.debit ?? 0} /> },
                { id: 'credit', accessorKey: 'credit', header: 'دائن', align: 'right', cell: (line) => <MoneyDisplay value={line.credit ?? 0} /> },
              ]}
            />
            {Array.from(journalTotals, ([currencyId, totals]) => (
              <div key={currencyId ?? 'unknown'} className="space-y-2">
                <strong>الإجمالي — معرّف العملة: {currencyId ?? '—'}</strong>
                <div className="flex flex-wrap items-center gap-6">
                  <MetaItem label="مدين" value={<MoneyDisplay value={totals.debit} />} />
                  <MetaItem label="دائن" value={<MoneyDisplay value={totals.credit} />} />
                </div>
              </div>
            ))}
          </div>
        )}
      </Dialog>

      <ConfirmDialog open={confirmPost} onClose={() => setConfirmPost(false)} title="ترحيل عملية الإهلاك" message="سيتم إنشاء قيد محاسبي واحد وتحديث أرصدة جميع الأصول." confirmLabel="ترحيل" loading={postDepreciation.isPending} onConfirm={async () => {
        try { await postDepreciation.mutateAsync(run.id); setConfirmPost(false); refetch(); }
        catch (err) { handleLifecycleError(err); }
      }} />
    </Page>
  );
}
