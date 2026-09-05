import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Power, PowerOff, Calendar, Eye } from 'lucide-react';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import { FilterBar, FilterSelect } from '@/components/ui';
import { StatusBadge } from '../components/StatusBadge';
import { ConflictDialog } from '../components/ConflictDialog';
import { FiscalYearForm } from '../components/FiscalYearForm';
import { useFiscalYearsList } from '../hooks/useFiscalYearsList';
import { useCreateFiscalYear } from '../hooks/useCreateFiscalYear';
import { useOpenFiscalYear } from '../hooks/useOpenFiscalYear';
import { useCloseFiscalYear } from '../hooks/useCloseFiscalYear';
import { useBulkGeneratePeriods } from '../hooks/useBulkGeneratePeriods';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import type { FiscalYearDto } from '../types';
import type { FiscalYearStatus } from '../types';

type SortField = 'yearNumber' | 'name' | 'startDate' | 'endDate' | 'status';
type SortDir = 'asc' | 'desc';
type FilterStatus = 'All' | FiscalYearStatus;

export function FiscalYearsListPage() {
  const navigate = useNavigate();
  const [sortField, setSortField] = useState<SortField>('yearNumber');
  const [sortDir, setSortDir] = useState<SortDir>('desc');
  const [filterStatus, setFilterStatus] = useState<FilterStatus>('All');
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [confirmAction, setConfirmAction] = useState<{
    type: 'open' | 'close';
    year: FiscalYearDto;
  } | null>(null);
  const [conflictDialog, setConflictDialog] = useState(false);
  const [selectedYear, setSelectedYear] = useState<FiscalYearDto | null>(null);
  const [showBulkGenerateDialog, setShowBulkGenerateDialog] = useState(false);
  const [bulkGenerateCount, setBulkGenerateCount] = useState(12);

  const { hasPermission: canCreate } = usePermission(PERMISSIONS.FiscalYears.Create);
  const { hasPermission: canOpen } = usePermission(PERMISSIONS.FiscalYears.Open);
  const { hasPermission: canClose } = usePermission(PERMISSIONS.FiscalYears.Close);

  const { data: years = [], isLoading, error, refetch } = useFiscalYearsList();
  const createMutation = useCreateFiscalYear();
  const openMutation = useOpenFiscalYear();
  const closeMutation = useCloseFiscalYear();
  const bulkGenerateMutation = useBulkGeneratePeriods();

  const filteredYears = years
    .filter((y) => filterStatus === 'All' || y.status === filterStatus)
    .sort((a, b) => {
      const aVal = a[sortField];
      const bVal = b[sortField];
      if (aVal == null || bVal == null) return 0;
      const cmp = aVal < bVal ? -1 : aVal > bVal ? 1 : 0;
      return sortDir === 'asc' ? cmp : -cmp;
    });

  const handleSort = (field: SortField) => {
    setSortDir(sortField === field && sortDir === 'asc' ? 'desc' : 'asc');
    setSortField(field);
  };
  void handleSort;

  const handleCreate = async (data: Record<string, unknown>) => {
    try {
      await createMutation.mutateAsync(data as never);
      setShowCreateDialog(false);
      toast.success('تم إنشاء السنة المالية بنجاح');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء السنة المالية';
      toast.error(message);
    }
  };

  const handleBulkGenerate = async () => {
    if (!selectedYear) return;
    try {
      await bulkGenerateMutation.mutateAsync({
        fiscalYearId: selectedYear.id!,
      });
      setShowBulkGenerateDialog(false);
      toast.success(`تم إنشاء ${bulkGenerateCount} فترة بنجاح`);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء الفترات';
      toast.error(message);
    }
  };

  const handleConfirmAction = async () => {
    if (!confirmAction) return;
    const { type, year } = confirmAction;
    try {
      if (type === 'open') {
        await openMutation.mutateAsync({
          id: year.id!,
          rowVersion: year.rowVersion!,
        });
        toast.success('تم فتح السنة المالية بنجاح');
      } else {
        await closeMutation.mutateAsync({
          id: year.id!,
          rowVersion: year.rowVersion!,
        });
        toast.success('تم إغلاق السنة المالية بنجاح');
      }
      setConfirmAction(null);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تنفيذ الإجراء';
      if (message.includes('Conflict')) {
        setConflictDialog(true);
      } else {
        toast.error(message);
      }
    }
  };

  const formatDate = (d: string | Date | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  const hasFilter = filterStatus !== 'All';

  return (
    <div>
      <PageHeader
        title="السنوات المالية"
        description="إدارة السنوات المالية وأدوارها"
        actions={
          canCreate ? (
            <Button
              variant="primary"
              size="sm"
              icon={<Plus size={16} />}
              onClick={() => setShowCreateDialog(true)}
            >
              سنة مالية جديدة
            </Button>
          ) : undefined
        }
      />

      <FilterBar
        hasFilters={hasFilter}
        onClear={() => setFilterStatus('All')}
      >
        <FilterSelect
          label="الحالة"
          value={filterStatus === 'All' ? '' : filterStatus}
          onChange={(v) => setFilterStatus(v === '' ? 'All' : v as FilterStatus)}
          options={[
            { value: 'Draft', label: 'مسودة' },
            { value: 'Open', label: 'مفتوحة' },
            { value: 'SoftClosed', label: 'مغلقة مؤقتاً' },
            { value: 'HardClosed', label: 'مغلقة نهائياً' },
          ]}
          placeholder="كل الحالات"
        />
      </FilterBar>

      <DataGrid<FiscalYearDto>
        columns={[
          {
            key: 'yearNumber',
            header: 'السنة',
            width: 100,
            sortable: true,
          },
          {
            key: 'name',
            header: 'الاسم',
            width: 200,
            sortable: true,
          },
          {
            key: 'startDate',
            header: 'تاريخ البداية',
            width: 130,
            sortable: true,
            render: (row) => formatDate(row.startDate),
          },
          {
            key: 'endDate',
            header: 'تاريخ النهاية',
            width: 130,
            sortable: true,
            render: (row) => formatDate(row.endDate),
          },
          {
            key: 'status',
            header: 'الحالة',
            width: 120,
            sortable: true,
            render: (row) => <StatusBadge status={row.status as FiscalYearStatus} />,
          },
          {
            key: '_actions',
            header: 'الإجراءات',
            width: 320,
            render: (row) => (
              <div className="flex gap-1">
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Eye size={14} />}
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/financial/fiscal-years/${row.id}`);
                  }}
                  aria-label={`تفاصيل ${row.name}`}
                >
                  تفاصيل
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Calendar size={14} />}
                  onClick={(e) => {
                    e.stopPropagation();
                    navigate(`/financial/fiscal-years/${row.id}`);
                  }}
                  aria-label={`إدارة فترات ${row.name}`}
                >
                  الفترات
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Plus size={14} />}
                  onClick={(e) => {
                    e.stopPropagation();
                    setSelectedYear(row);
                    setShowBulkGenerateDialog(true);
                  }}
                  aria-label={`إنشاء فترات لـ ${row.name}`}
                >
                  إنشاء فترات
                </Button>
                {row.status === 'Draft' && canOpen && (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Power size={14} />}
                    onClick={(e) => {
                      e.stopPropagation();
                      setConfirmAction({ type: 'open', year: row });
                    }}
                    aria-label={`فتح ${row.name}`}
                  >
                    فتح
                  </Button>
                )}
                {row.status === 'Open' && canClose && (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<PowerOff size={14} />}
                    onClick={(e) => {
                      e.stopPropagation();
                      setConfirmAction({ type: 'close', year: row });
                    }}
                    aria-label={`إغلاق ${row.name}`}
                  >
                    إغلاق
                  </Button>
                )}
              </div>
            ),
          },
        ]}
        data={filteredYears}
        loading={isLoading}
        error={error ? 'فشل تحميل بيانات السنوات المالية' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد سنوات مالية"
        rowKey={(row) => String(row.id ?? 0)}
      />

      <Dialog
        open={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        title="سنة مالية جديدة"
      >
        <FiscalYearForm
          onSubmit={handleCreate}
          serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء السنة المالية' : undefined}
          loading={createMutation.isPending}
        />
      </Dialog>

      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={handleConfirmAction}
        message={
          confirmAction?.type === 'open'
            ? `هل تريد فتح السنة المالية ${confirmAction?.year.name}؟`
            : `هل تريد إغلاق السنة المالية ${confirmAction?.year.name}؟`
        }
        destructive={confirmAction?.type === 'close'}
      />

      <ConflictDialog
        open={conflictDialog}
        onClose={() => setConflictDialog(false)}
        onRefresh={() => {
          setConflictDialog(false);
          refetch();
        }}
      />

      <Dialog
        open={showBulkGenerateDialog}
        onClose={() => setShowBulkGenerateDialog(false)}
        title={`إنشاء فترات لـ ${selectedYear?.name}`}
      >
        <div className="flex flex-col gap-4">
          <div>
            <Input
              label="عدد الفترات"
              type="number"
              min={1}
              max={24}
              value={bulkGenerateCount}
              onChange={(e) => setBulkGenerateCount(Number(e.target.value))}
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button
              variant="ghost"
              onClick={() => setShowBulkGenerateDialog(false)}
            >
              إلغاء
            </Button>
            <Button
              variant="primary"
              loading={bulkGenerateMutation.isPending}
              onClick={handleBulkGenerate}
            >
              إنشاء
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
