import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, FilterBar, FilterSelect, ConfirmDialog, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useClosingEntriesByFiscalYear, useGenerateClosingEntry } from '../../hooks/useClosingEntries';
import { useFiscalYearsList } from '../../hooks/useFiscalYears';

const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Approved: 'معتمد',
  Posted: 'مقيّد',
  Cancelled: 'ملغي',
};

const statusVariants: Record<string, 'default' | 'success' | 'warning' | 'danger'> = {
  Draft: 'default',
  Approved: 'success',
  Posted: 'success',
  Cancelled: 'danger',
};

export default function ClosingEntriesListPage() {
  const navigate = useNavigate();
  const { fiscalYearId: paramFyId } = useParams<{ fiscalYearId: string }>();
  const canGenerate = usePermission(PERMISSIONS.FiscalYears.Close);
  const [fyFilter, setFyFilter] = useState(paramFyId || '');
  const [confirmGenerate, setConfirmGenerate] = useState(false);

  const { data: fiscalYears = [] } = useFiscalYearsList();
  const { data: entries = [], isLoading } = useClosingEntriesByFiscalYear(Number(fyFilter) || 0);
  const generateMutation = useGenerateClosingEntry();

  const fyOptions = fiscalYears.map((fy) => ({ value: String(fy.id), label: `${fy.name} (${fy.yearNumber})` }));

  function handleGenerate() {
    if (!fyFilter) return;
    generateMutation.mutate(
      { fiscalYearId: Number(fyFilter) },
      {
        onSuccess: () => { notify({ type: 'success', title: 'تم إنشاء قيد الإغلاق' }); setConfirmGenerate(false); },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      },
    );
  }

  const columns: DataGridColumn<typeof entries[0]>[] = [
    { header: 'رقم القيد', cell: (row) => <span className="font-mono font-medium">{row.closingEntryNumber}</span> },
    { header: 'التاريخ', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-EG', { dateStyle: 'medium' }).format(new Date(row.closingDate))}</span> },
    { header: 'الحالة', cell: (row) => <Badge variant={statusVariants[row.status] ?? 'default'}>{statusLabels[row.status] ?? row.status}</Badge> },
    { header: '逆转', cell: (row) => row.isReversal && <span className="text-xs">逆转: {row.reversalOfNumber}</span> },
    {
      header: 'إجراءات',
      cell: (row) => <Button variant="ghost" size="icon" onClick={() => navigate(`/financial-settings/closing-entries/${row.id}`)} aria-label="عرض" className="cursor-pointer"><Eye size={16} /></Button>,
    },
  ];

  return (
    <Page
      title="قيود الإغلاق"
      description="إدارة قيود إغلاق السنة المالية"
      actions={
        canGenerate && fyFilter && (
          <Button onClick={() => setConfirmGenerate(true)} icon={<Plus size={16} />} className="cursor-pointer shadow-sm hover:shadow transition-shadow">
            إنشاء قيد إغلاق
          </Button>
        )
      }
      toolbar={
        <FilterBar hasFilters={!!fyFilter} onClear={() => setFyFilter('')}>
          <FilterSelect value={fyFilter} onChange={setFyFilter} options={fyOptions} placeholder="اختر السنة المالية" label="السنة المالية" />
        </FilterBar>
      }
    >
      {!fyFilter ? (
        <div className="text-center py-12 border-2 border-dashed border-[var(--color-border-container)] rounded-xl">
          <p className="text-[var(--color-on-surface-variant)]">اختر سنة مالية لعرض قيود الإغلاق</p>
        </div>
      ) : (
        <DataGrid
          columns={columns}
          data={entries}
          loading={isLoading}
          emptyMessage="لا توجد قيود إغلاق بعد"
          rowKey={(row) => row.id}
        />
      )}

      <ConfirmDialog
        open={confirmGenerate}
        onClose={() => setConfirmGenerate(false)}
        onConfirm={handleGenerate}
        title="إنشاء قيد الإغلاق"
        message="هل تريد إنشاء قيد إغلاق لهذه السنة المالية؟ سيتم إنشاء مقترح محاسبي متوازن."
        loading={generateMutation.isPending}
      />
    </Page>
  );
}
