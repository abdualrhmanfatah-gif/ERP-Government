import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Eye, Play } from 'lucide-react';
import { Alert, Button, Card, CardContent, CardHeader, CardTitle, DataGrid, DatePicker, Grid, MoneyDisplay, Page, Select, Stack, StatusBadge, Textarea } from '@/components/ui';
import { useFiscalPeriodsList } from '@/features/financial-settings/hooks/useFiscalPeriods';
import { useFiscalYearsList } from '@/features/financial-settings/hooks/useFiscalYears';
import { getAssetStatusBadge } from '@/features/assets/shared/status';
import { MetaItem } from '@/components/MetaItem';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { usePreviewDepreciation, useRunDepreciation } from '../hooks/useDepreciation';
import { missedPeriodsPolicyOptions, type PreviewDepreciationResult, type PreviewScheduleLine } from '../shared/types';
import { formatPercent } from '@/shared/utils/formatters';

export function DepreciationRunPage() {
  const navigate = useNavigate();
  const [fiscalYearId, setFiscalYearId] = useState(0);
  const [fiscalPeriodId, setFiscalPeriodId] = useState(0);
  const [depreciationDate, setDepreciationDate] = useState('');
  const [missedPeriodsPolicy, setMissedPeriodsPolicy] = useState('CurrentPeriodOnly');
  const [notes, setNotes] = useState('');
  const [preview, setPreview] = useState<PreviewDepreciationResult | null>(null);

  const { data: fiscalYears = [] } = useFiscalYearsList(true);
  const { data: periods = [] } = useFiscalPeriodsList(fiscalYearId);
  const previewDepreciation = usePreviewDepreciation();
  const runDepreciation = useRunDepreciation();

  const canRun = fiscalYearId > 0 && fiscalPeriodId > 0 && !!depreciationDate;
  const fiscalYearName = fiscalYears.find((year) => year.id === fiscalYearId)?.name ?? '-';

  function resetPreview() {
    previewDepreciation.reset();
    setPreview(null);
  }

  async function handlePreview() {
    try {
      const result = await previewDepreciation.mutateAsync({
        fiscalYearId,
        fiscalPeriodId,
        depreciationDate,
        missedPeriodsPolicy,
      });
      setPreview(result);
    } catch {
      setPreview(null);
    }
  }

  async function handleRun() {
    try {
      const result = await runDepreciation.mutateAsync({
        fiscalYearId,
        fiscalPeriodId,
        depreciationDate,
        missedPeriodsPolicy,
        notes: notes || undefined,
      }) as { runId?: number };
      if (result?.runId) navigate(`/assets/depreciation/${result.runId}`);
      else navigate('/assets/depreciation');
    } catch (err) {
      handleLifecycleError(err);
    }
  }

  return (
    <Page
      title="تشغيل إهلاك الأصول"
      description="إنشاء عملية إهلاك دورية لجميع الأصول المستحقة"
      breadcrumbs={[{ label: 'إهلاك الأصول', path: '/assets/depreciation' }, { label: 'تشغيل الإهلاك' }]}
      maxWidth="full"
      onBack={() => navigate('/assets/depreciation')}
      actions={
        <div className="flex gap-2">
          <Button variant="ghost" type="button" onClick={() => navigate('/assets/depreciation')}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            type="button"
            icon={<Play size={16} />}
            loading={runDepreciation.isPending}
            disabled={!canRun}
            onClick={handleRun}
          >
            تشغيل الإهلاك
          </Button>
        </div>
      }
    >
      <Stack gap="md">
      <Card padding="sm">
        <CardHeader className="p-0 pb-2">
          <CardTitle className="text-lg">بيانات العملية</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          <Grid columns={4} gap="sm">
            <Select
              label="السنة المالية"
              required
              value={fiscalYearId || ''}
              onChange={(event) => {
                setFiscalYearId(Number(event.target.value));
                setFiscalPeriodId(0);
                setDepreciationDate('');
                resetPreview();
              }}
              options={[{ value: '', label: 'اختر السنة المالية' }, ...fiscalYears.map((year) => ({ value: String(year.id), label: year.name }))]}
            />
            <Select
              label="الفترة المالية"
              required
              disabled={!fiscalYearId}
              value={fiscalPeriodId || ''}
              onChange={(event) => {
                const id = Number(event.target.value);
                setFiscalPeriodId(id);
                const period = periods.find((item) => item.id === id);
                setDepreciationDate(period?.endDate ?? '');
                resetPreview();
              }}
              options={[
                { value: '', label: 'اختر الفترة المالية' },
                ...periods.map((period) => ({
                  value: String(period.id),
                  label: `${period.periodNumber} - ${period.name}`,
                  disabled: period.isLockedForPosting || !period.isActive,
                })),
              ]}
            />
            <DatePicker
              label="تاريخ الإهلاك"
              required
              value={depreciationDate}
              onChange={(value) => {
                setDepreciationDate(value);
                resetPreview();
              }}
            />
            <Select
              label="سياسة الفترات الفائتة"
              value={missedPeriodsPolicy}
              onChange={(event) => {
                setMissedPeriodsPolicy(event.target.value);
                resetPreview();
              }}
              options={missedPeriodsPolicyOptions}
            />
            <div className="col-span-4">
              <Textarea label="ملاحظات" value={notes} onChange={(event) => setNotes(event.target.value)} maxLength={500} rows={2} />
            </div>
          </Grid>
        </CardContent>
      </Card>

      <Card padding="sm">
        <CardHeader className="flex flex-row items-center justify-between p-0 pb-2">
          <CardTitle className="text-lg">معاينة الإهلاك</CardTitle>
          <Button
            variant="outline"
            size="sm"
            type="button"
            icon={<Eye size={16} />}
            loading={previewDepreciation.isPending}
            disabled={!canRun}
            onClick={handlePreview}
          >
            معاينة
          </Button>
        </CardHeader>
        <CardContent className="p-0">
          {previewDepreciation.isError && (
            <Alert variant="error">{getQueryErrorMessage(previewDepreciation.error)}</Alert>
          )}
          {preview && (
            <div className="mb-4 flex flex-wrap items-center gap-6">
              <MetaItem label="عدد الأصول" value={preview.totalAssets} />
              <MetaItem label="إجمالي الإهلاك المتوقع" value={<MoneyDisplay value={preview.totalDepreciation} />} />
            </div>
          )}
          <DataGrid<PreviewScheduleLine>
            data={preview?.lines ?? []}
            rowKey={(row) => row.assetId}
            emptyMessage="اضغط «معاينة» لعرض أسطر الإهلاك المستحقة"
            columns={[
              { id: 'assetCode', accessorKey: 'assetCode', header: 'كود الأصل', width: 120, cell: (row) => <span dir="ltr" className="font-mono">{row.assetCode}</span> },
              { id: 'assetName', accessorKey: 'assetName', header: 'الأصل' },
              { id: 'days', accessorKey: 'days', header: 'الأيام', width: 80, align: 'right' },
              { id: 'rate', accessorKey: 'rate', header: 'النسبة', width: 90, align: 'right', cell: (row) => formatPercent(row.rate) },
              { id: 'amount', accessorKey: 'amount', header: 'إهلاك الفترة', align: 'right', cell: (row) => <MoneyDisplay value={row.amount} /> },
              { id: 'originalValue', accessorKey: 'originalValue', header: 'القيمة الأصلية', align: 'right', cell: (row) => <MoneyDisplay value={row.originalValue} /> },
              { id: 'accumulatedDepreciation', accessorKey: 'accumulatedDepreciation', header: 'مجمع الإهلاك الافتتاحي', align: 'right', cell: (row) => <MoneyDisplay value={row.accumulatedDepreciation} /> },
              { id: 'openingBookValue', accessorKey: 'openingBookValue', header: 'القيمة الدفترية الافتتاحية', align: 'right', cell: (row) => <MoneyDisplay value={row.openingBookValue} /> },
              {
                id: 'assetStatus',
                accessorKey: 'assetStatus',
                header: 'حالة الأصل',
                width: 110,
                cell: (row) => {
                  const badge = getAssetStatusBadge(row.assetStatus);
                  return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
                },
              },
              { id: 'fiscalYear', header: 'السنة المالية', width: 120, cell: () => fiscalYearName },
            ]}
          />
        </CardContent>
      </Card>
      </Stack>
    </Page>
  );
}
