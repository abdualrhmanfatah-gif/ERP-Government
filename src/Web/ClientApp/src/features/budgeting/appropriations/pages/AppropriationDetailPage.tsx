import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useAppropriationDetail,
  useSubmitAppropriation,
  useApproveAppropriation,
  useActivateAppropriation,
  useSuspendAppropriation,
  useCloseAppropriation,
  useCancelAppropriation,
  useReverseAppropriation,
  useDeleteAppropriation,
} from '../../hooks/useAppropriations';
import { appropriationTypeLabels, appropriationStatusLabels, AppropriationStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { LifecycleActions, type LifecycleAction } from '@/components/BudgetingLifecycleActions';
import { ApprovalHistoryPanel } from '@/components/BudgetingApprovalHistoryPanel';
import { AvailabilityIndicator } from '@/components/BudgetingAvailabilityIndicator';
import { Page, Button, Badge, Card } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

const appropriationActions: Record<string, LifecycleAction[]> = {
  [AppropriationStatus.Draft]: [
    { key: 'submit', label: 'تقديم', permission: BUDGET_PERMISSIONS.Appropriations.Submit, confirmMessage: 'هل أنت متأكد من تقديم هذا التخصيص؟' },
    { key: 'delete', label: 'حذف', permission: BUDGET_PERMISSIONS.Appropriations.Delete, confirmMessage: 'هل أنت متأكد من حذف هذا التخصيص؟' },
  ],
  [AppropriationStatus.PendingApproval]: [
    { key: 'approve', label: 'اعتماد', permission: BUDGET_PERMISSIONS.Appropriations.Approve, confirmMessage: 'هل أنت متأكد من اعتماد هذا التخصيص؟' },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Appropriations.Cancel, confirmMessage: 'هل أنت متأكد من إلغاء هذا التخصيص؟' },
  ],
  [AppropriationStatus.Approved]: [
    { key: 'activate', label: 'تفعيل', permission: BUDGET_PERMISSIONS.Appropriations.Activate, confirmMessage: 'هل أنت متأكد من تفعيل هذا التخصيص؟' },
  ],
  [AppropriationStatus.Active]: [
    { key: 'suspend', label: 'تعليق', permission: BUDGET_PERMISSIONS.Appropriations.Suspend, confirmMessage: 'هل أنت متأكد من تعليق هذا التخصيص؟' },
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Appropriations.Close, confirmMessage: 'هل أنت متأكد من إغلاق هذا التخصيص؟' },
    { key: 'reverse', label: 'عكس', permission: BUDGET_PERMISSIONS.Appropriations.Reverse, confirmMessage: 'هل أنت متأكد من عكس هذا التخصيص؟ سيُنشأ تخصيص تعديل بمبلغ سالب.' },
  ],
  [AppropriationStatus.Suspended]: [
    { key: 'activate', label: 'إعادة تفعيل', permission: BUDGET_PERMISSIONS.Appropriations.Activate },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Appropriations.Cancel, confirmMessage: 'هل أنت متأكد من إلغاء هذا التخصيص؟' },
  ],
};

export default function AppropriationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const appropriationId = Number(id);
  const { hasPermission } = usePermission();
  const can = (_permission?: string) => hasPermission;

  const { data: appropriation, isLoading } = useAppropriationDetail(appropriationId);
  const submit = useSubmitAppropriation();
  const approve = useApproveAppropriation();
  const activate = useActivateAppropriation();
  const suspend = useSuspendAppropriation();
  const close = useCloseAppropriation();
  const cancel = useCancelAppropriation();
  const reverse = useReverseAppropriation();
  const deleteAppropriation = useDeleteAppropriation();

  const [pendingAction, setPendingAction] = useState<string | null>(null);

  const hooks: Record<string, ReturnType<typeof useSubmitAppropriation>> = {
    submit, approve, activate, suspend, close, cancel,
  };

  async function handleAction(actionKey: string) {
    if (actionKey === 'delete') {
      if (!appropriation) return;
      setPendingAction(actionKey);
      try {
        await deleteAppropriation.mutateAsync({ id: appropriation.id });
        navigate('/budgeting/appropriations');
      } catch {
        // error handled by mutation
      } finally {
        setPendingAction(null);
      }
      return;
    }

    if (actionKey === 'reverse') {
      if (!appropriation) return;
      setPendingAction(actionKey);
      try {
        await reverse.mutateAsync({ id: appropriation.id, rowVersion: appropriation.rowVersion });
      } catch {
        // error handled by mutation
      } finally {
        setPendingAction(null);
      }
      return;
    }

    const hook = hooks[actionKey];
    if (!hook || !appropriation) return;
    setPendingAction(actionKey);
    try {
      await hook.mutateAsync({ id: appropriation.id, rowVersion: appropriation.rowVersion });
    } catch {
      // error handled by mutation
    } finally {
      setPendingAction(null);
    }
  }

  return (
    <Page
      title={appropriation?.appropriationNumber ?? ''}
      description={appropriation ? appropriationTypeLabels[appropriation.appropriationType] : undefined}
      loading={isLoading}
      error={!isLoading && !appropriation ? 'لم يتم العثور على التخصيص' : undefined}
      breadcrumbs={[{ label: 'التخصيصات', path: '/budgeting/appropriations' }, { label: appropriation?.appropriationNumber ?? '' }]}
      actions={
        <div className="flex items-center gap-2">
          {appropriation && <Badge>{appropriationStatusLabels[appropriation.status]}</Badge>}
          <Button variant="ghost" size="icon" onClick={() => navigate('/budgeting/appropriations')} aria-label="العودة">
            <ArrowRight size={18} />
          </Button>
        </div>
      }
    >
      {!appropriation ? null : (<>
      <Card className="bg-[var(--color-surface-container-lowest)]">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ</span>
            <span className="block text-sm font-mono">{appropriation.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الصندوق</span>
            <span className="block text-sm">{appropriation.fundNumber} — {appropriation.fundName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">السنة المالية</span>
            <span className="block text-sm">{appropriation.fiscalYearName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">البند</span>
            <span className="block text-sm">{appropriation.itemCode} — {appropriation.itemName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الموازنة</span>
            <span className="block text-sm">{appropriation.budgetNumber} — {appropriation.budgetName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">نوع المستند</span>
            <span className="block text-sm">{appropriation.documentType}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">رقم المستند</span>
            <span className="block text-sm font-mono">{appropriation.documentId}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">أنشأه</span>
            <span className="block text-sm">{appropriation.createdBy}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">تاريخ الإنشاء</span>
            <span className="block text-sm">{new Date(appropriation.created).toLocaleDateString('ar-EG')}</span>
          </div>
        </div>
      </Card>

      <AvailabilityIndicator budgetItemId={appropriation.budgetItemId} />

      <LifecycleActions
        actions={appropriationActions[appropriation.status] ?? []}
        can={can}
        pendingKey={pendingAction}
        onAction={handleAction}
      />

      <ApprovalHistoryPanel
        decisions={[appropriation.latestApproval].filter(Boolean)}
        title="سجل اعتمادات التخصيص"
      />
    </>)}
    </Page>
  );
}
