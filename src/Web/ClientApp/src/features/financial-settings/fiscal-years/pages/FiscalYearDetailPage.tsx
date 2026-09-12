import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Card, ConfirmDialog } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { ArrowRight, Unlock, Lock, CalendarPlus, FolderOpen, X } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { FiscalYearStatusBadge } from '@/components/FinancialSettingsFiscalYearStatusBadge';
import { PeriodLockIndicator } from '@/components/FinancialSettingsPeriodLockIndicator';
import { useFiscalYearDetail, useOpenFiscalYear, useCloseFiscalYear } from '../../hooks/useFiscalYears';
import { useFiscalPeriodsList, useBulkGeneratePeriods, useLockFiscalPeriod, useUnlockFiscalPeriod } from '../../hooks/useFiscalPeriods';

export default function FiscalYearDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const fiscalYearId = Number(id);

  const canOpen = usePermission(PERMISSIONS.FiscalYears.Open);
  const canClose = usePermission(PERMISSIONS.FiscalYears.Close);
  const canLock = usePermission(PERMISSIONS.FiscalPeriods.Lock);

  const { data: fy, isLoading: fyLoading } = useFiscalYearDetail(fiscalYearId);
  const { data: periods = [], isLoading: periodsLoading } = useFiscalPeriodsList(fiscalYearId);

  const openMutation = useOpenFiscalYear();
  const closeMutation = useCloseFiscalYear();
  const bulkGenerate = useBulkGeneratePeriods();
  const lockMutation = useLockFiscalPeriod();
  const unlockMutation = useUnlockFiscalPeriod();

  const [confirmAction, setConfirmAction] = useState<'open' | 'close' | null>(null);

  function handleOpen() {
    openMutation.mutate(fiscalYearId, {
      onSuccess: () => { notify({ type: 'success', title: 'تم فتح السنة المالية' }); setConfirmAction(null); },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الفتح' }),
    });
  }

  function handleClose() {
    closeMutation.mutate(fiscalYearId, {
      onSuccess: () => { notify({ type: 'success', title: 'تم إغلاق السنة المالية' }); setConfirmAction(null); },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإغلاق' }),
    });
  }

  function handleBulkGenerate() {
    bulkGenerate.mutate({ fiscalYearId }, {
      onSuccess: () => notify({ type: 'success', title: 'تم إنشاء الفترات بنجاح' }),
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء إنشاء الفترات' }),
    });
  }

  function handleLockPeriod(periodId: number) {
    lockMutation.mutate(periodId, {
      onSuccess: () => notify({ type: 'success', title: 'تم قفل الفترة' }),
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء القفل' }),
    });
  }

  function handleUnlockPeriod(periodId: number) {
    unlockMutation.mutate(periodId, {
      onSuccess: () => notify({ type: 'success', title: 'تم فتح الفترة' }),
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الفتح' }),
    });
  }

  return (
    <Page
      title={fy?.name ?? ''}
      description={fy ? `سنة مالية — ${fy.yearNumber}` : undefined}
      loading={fyLoading || periodsLoading}
      error={!fy ? 'السنة المالية غير موجودة' : undefined}
      actions={
        fy && (
          <>
            <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/fiscal-years')} className="cursor-pointer">
              <ArrowRight size={18} />
            </Button>
            <FiscalYearStatusBadge status={fy.status} />
            {fy.status === 'Draft' && canOpen && (
              <Button onClick={() => setConfirmAction('open')} icon={<FolderOpen size={16} />} className="cursor-pointer">
                فتح السنة
              </Button>
            )}
            {fy.status === 'Open' && canClose && (
              <Button variant="destructive" onClick={() => setConfirmAction('close')} icon={<X size={16} />} className="cursor-pointer">
                إغلاق السنة
              </Button>
            )}
          </>
        )
      }
    >
      {fy && (
        <>
          {/* Info Card */}
          <Card className="bg-[var(--color-surface-container-lowest)]">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ البداية</div>
                <div className="text-sm font-medium text-[var(--color-on-surface)]">
                  {new Intl.DateTimeFormat('ar-YE', { dateStyle: 'long' }).format(new Date(fy.startDate))}
                </div>
              </div>
              <div>
                <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ النهاية</div>
                <div className="text-sm font-medium text-[var(--color-on-surface)]">
                  {new Intl.DateTimeFormat('ar-YE', { dateStyle: 'long' }).format(new Date(fy.endDate))}
                </div>
              </div>
              <div>
                <div className="text-xs text-[var(--color-on-surface-variant)] mb-1">الحالة</div>
                <FiscalYearStatusBadge status={fy.status} />
              </div>
            </div>
            {fy.createdBy && (
              <div className="mt-4 pt-4 border-t border-[var(--color-border-container)] text-xs text-[var(--color-on-surface-variant)]">
                أنشأ: {fy.createdBy} — {fy.createdAt ? new Intl.DateTimeFormat('ar-YE', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(fy.createdAt)) : ''}
              </div>
            )}
          </Card>

          {/* Periods */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-title-md font-semibold text-[var(--color-on-surface)]">الفترات المحاسبية</h2>
              {fy.status !== 'HardClosed' && periods.length === 0 && (
                <Button onClick={handleBulkGenerate} icon={<CalendarPlus size={16} />} disabled={bulkGenerate.isPending} className="cursor-pointer">
                  إنشاء فترات شهرية
                </Button>
              )}
            </div>

            {periods.length === 0 ? (
              <div className="text-center py-8 border-2 border-dashed border-[var(--color-border-container)] rounded-xl">
                <p className="text-[var(--color-on-surface-variant)]">لا توجد فترات بعد</p>
              </div>
            ) : (
              <DataGrid
                columns={[
                  { header: 'رقم الفترة', cell: (row) => <span className="font-mono">{row.periodNumber}</span> },
                  { header: 'الاسم', cell: (row) => row.name },
                  { header: 'تاريخ البداية', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-YE', { dateStyle: 'medium' }).format(new Date(row.startDate))}</span> },
                  { header: 'تاريخ النهاية', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-YE', { dateStyle: 'medium' }).format(new Date(row.endDate))}</span> },
                  { header: 'الحالة', cell: (row) => <PeriodLockIndicator isLocked={row.isLockedForPosting} /> },
                  ...(fy.status !== 'HardClosed' ? [{
                    header: 'إجراءات',
                    cell: (row: typeof periods[number]) => canLock ? (
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => row.isLockedForPosting ? handleUnlockPeriod(row.id) : handleLockPeriod(row.id)}
                        disabled={lockMutation.isPending || unlockMutation.isPending}
                        className="cursor-pointer"
                      >
                        {row.isLockedForPosting ? <Unlock size={16} /> : <Lock size={16} />}
                      </Button>
                    ) : null,
                  }] : []),
                ]}
                data={periods}
                rowKey={(row) => row.id}
                emptyMessage="لا توجد فترات بعد"
              />
            )}
          </div>

          <ConfirmDialog
            open={!!confirmAction}
            onClose={() => setConfirmAction(null)}
            onConfirm={confirmAction === 'open' ? handleOpen : handleClose}
            title={confirmAction === 'open' ? 'فتح السنة المالية' : 'إغلاق السنة المالية'}
            message={confirmAction === 'open' ? 'هل أنت متأكد من فتح هذه السنة المالية؟' : 'هل أنت متأكد من إغلاق هذه السنة المالية؟'}
            loading={openMutation.isPending || closeMutation.isPending}
          />
        </>
      )}
    </Page>
  );
}
