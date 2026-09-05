import { useState } from 'react';
import { Lock, Unlock, Plus, Edit } from 'lucide-react';
import { toast } from 'sonner';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { ConflictDialog } from '../components/ConflictDialog';
import { FiscalPeriodForm } from '../components/FiscalPeriodForm';
import { useFiscalPeriodsList } from '../hooks/useFiscalPeriodsList';
import { useLockFiscalPeriod } from '../hooks/useLockFiscalPeriod';
import { useUnlockFiscalPeriod } from '../hooks/useUnlockFiscalPeriod';
import { useUpdateFiscalPeriod } from '../hooks/useUpdateFiscalPeriod';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { CreateFiscalPeriodCommand, FiscalPeriodsClient } from '../../../../web-api-client';
import { authFetchFn } from '../../../../shared/utils/auth-fetch';
import type { FiscalPeriodDto } from '../types';

const client = new FiscalPeriodsClient('', authFetchFn);

interface FiscalPeriodsListPageProps {
  fiscalYearId: number;
  fiscalYearName: string;
  fiscalYearStart?: string;
  fiscalYearEnd?: string;
  onBack: () => void;
}

export function FiscalPeriodsListPage({
  fiscalYearId,
  fiscalYearName,
  fiscalYearStart,
  fiscalYearEnd,
  onBack,
}: FiscalPeriodsListPageProps) {
  const [confirmAction, setConfirmAction] = useState<{
    type: 'lock' | 'unlock';
    period: FiscalPeriodDto;
  } | null>(null);
  const [conflictDialog, setConflictDialog] = useState(false);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [editingPeriod, setEditingPeriod] = useState<FiscalPeriodDto | null>(null);

  // Permissions
  const { hasPermission: canCreate } = usePermission(PERMISSIONS.FiscalPeriods.Create);
  const { hasPermission: canUpdate } = usePermission(PERMISSIONS.FiscalPeriods.Update);
  const { hasPermission: canLock } = usePermission(PERMISSIONS.FiscalPeriods.Lock);
  const { hasPermission: canUnlock } = usePermission(PERMISSIONS.FiscalPeriods.Unlock);

  const queryClient = useQueryClient();
  const { data: periods = [], isLoading, error, refetch } = useFiscalPeriodsList(fiscalYearId);
  const lockMutation = useLockFiscalPeriod();
  const unlockMutation = useUnlockFiscalPeriod();
  const updateMutation = useUpdateFiscalPeriod();

  const createMutation = useMutation({
    mutationFn: (data: { fiscalYearId: number; name: string; periodNumber: number; startDate: string | Date; endDate: string | Date }) =>
      client.fiscalPeriodsPOST(
        new CreateFiscalPeriodCommand({
          ...data,
          startDate: new Date(data.startDate as string),
          endDate: new Date(data.endDate as string),
        }),
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalPeriods', fiscalYearId] });
      setShowCreateDialog(false);
      toast.success('تم إنشاء الفترة بنجاح');
    },
    onError: (err: Error) => {
      toast.error(err.message || 'حدث خطأ أثناء إنشاء الفترة');
    },
  });

  const formatDate = (d: string | Date | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  const handleConfirmAction = async () => {
    if (!confirmAction) return;
    const { type, period } = confirmAction;
    try {
      if (type === 'lock') {
        await lockMutation.mutateAsync({
          id: period.id!,
          rowVersion: period.rowVersion!,
        });
        toast.success('تم قفل الفترة بنجاح');
      } else {
        await unlockMutation.mutateAsync({
          id: period.id!,
          rowVersion: period.rowVersion!,
        });
        toast.success('تم فتح الفترة بنجاح');
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

  return (
    <div>
      <PageHeader
        title={`فترة ${fiscalYearName}`}
        description="إدارة الفترات المحاسبية"
        actions={
          <div className="flex gap-2">
            {canCreate ? (
              <Button
                variant="primary"
                size="sm"
                icon={<Plus size={16} />}
                onClick={() => setShowCreateDialog(true)}
              >
                فترة جديدة
              </Button>
            ) : undefined}
            <Button variant="ghost" size="sm" onClick={onBack}>
              العودة
            </Button>
          </div>
        }
      />

      <DataGrid<FiscalPeriodDto>
        columns={[
          { key: 'name', header: 'اسم الفترة', width: 180 },
          { key: 'periodNumber', header: 'الرقم', width: 80 },
          {
            key: 'startDate',
            header: 'من',
            width: 120,
            render: (row) => formatDate(row.startDate),
          },
          {
            key: 'endDate',
            header: 'إلى',
            width: 120,
            render: (row) => formatDate(row.endDate),
          },
          {
            key: 'status',
            header: 'الحالة',
            width: 100,
            render: (row) => (
              <StatusBadge variant={row.isLockedForPosting ? 'closed' : 'active'}>
                {row.isLockedForPosting ? 'مقفل' : 'مفتوح'}
              </StatusBadge>
            ),
          },
          {
            key: '_actions',
            header: 'الإجراءات',
            width: 200,
            cell: (row) => (
              <div className="flex gap-1">
                {canUpdate ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Edit size={14} />}
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditingPeriod(row);
                    }}
                    aria-label={`تعديل ${row.name}`}
                  >
                    تعديل
                  </Button>
                ) : null}
                {!row.isLockedForPosting && canLock ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Lock size={14} />}
                    onClick={(e) => {
                      e.stopPropagation();
                      setConfirmAction({ type: 'lock', period: row });
                    }}
                    aria-label={`قفل ${row.name}`}
                  >
                    قفل
                  </Button>
                ) : null}
                {row.isLockedForPosting && canUnlock ? (
                  <Button
                    variant="ghost"
                    size="sm"
                    icon={<Unlock size={14} />}
                    onClick={(e) => {
                      e.stopPropagation();
                      setConfirmAction({ type: 'unlock', period: row });
                    }}
                    aria-label={`فتح ${row.name}`}
                  >
                    فتح
                  </Button>
                ) : null}
              </div>
            ),
          },
        ]}
        data={periods}
        loading={isLoading}
        error={error ? 'فشل تحميل بيانات الفترات' : undefined}
        onRetry={() => refetch()}
        emptyMessage="لا توجد فترات"
        rowKey={(row) => String(row.id ?? 0)}
      />

      {/* Confirm Action Dialog */}
      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={handleConfirmAction}
        message={
          confirmAction?.type === 'lock'
            ? `هل تريد قفل الفترة ${confirmAction?.period.name}؟`
            : `هل تريد فتح الفترة ${confirmAction?.period.name}؟`
        }
        destructive={confirmAction?.type === 'lock'}
      />

      {/* Conflict Dialog */}
      <ConflictDialog
        open={conflictDialog}
        onClose={() => setConflictDialog(false)}
        onRefresh={() => {
          setConflictDialog(false);
          refetch();
        }}
      />

      {/* Create Period Dialog */}
      <Dialog
        open={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        title="فترة جديدة"
      >
        <FiscalPeriodForm
          fiscalYearId={fiscalYearId}
          fiscalYearStart={fiscalYearStart ?? ''}
          fiscalYearEnd={fiscalYearEnd ?? ''}
          onSubmit={(data) => createMutation.mutate({ ...data, fiscalYearId })}
          loading={createMutation.isPending}
          serverError={createMutation.error?.message}
        />
      </Dialog>

      {/* Edit Period Dialog */}
      <Dialog
        open={!!editingPeriod}
        onClose={() => setEditingPeriod(null)}
        title={`تعديل ${editingPeriod?.name ?? ''}`}
      >
        <FiscalPeriodForm
          fiscalYearId={fiscalYearId}
          fiscalYearStart={fiscalYearStart ?? ''}
          fiscalYearEnd={fiscalYearEnd ?? ''}
          initialData={
            editingPeriod
              ? {
                  periodNumber: editingPeriod.periodNumber,
                  name: editingPeriod.name ?? '',
                  startDate: editingPeriod.startDate
                    ? new Date(editingPeriod.startDate).toISOString().split('T')[0]
                    : '',
                  endDate: editingPeriod.endDate
                    ? new Date(editingPeriod.endDate).toISOString().split('T')[0]
                    : '',
                }
              : undefined
          }
          onSubmit={(data) => {
            if (!editingPeriod) return;
            updateMutation.mutate(
              {
                id: editingPeriod.id!,
                data: {
                  name: data.name,
                  startDate: data.startDate,
                  endDate: data.endDate,
                },
              },
              {
                onSuccess: () => {
                  setEditingPeriod(null);
                  toast.success('تم تحديث الفترة بنجاح');
                },
                onError: (err: Error) => {
                  toast.error(err.message || 'حدث خطأ أثناء تحديث الفترة');
                },
              }
            );
          }}
          loading={updateMutation.isPending}
          serverError={updateMutation.error?.message}
        />
      </Dialog>
    </div>
  );
}
