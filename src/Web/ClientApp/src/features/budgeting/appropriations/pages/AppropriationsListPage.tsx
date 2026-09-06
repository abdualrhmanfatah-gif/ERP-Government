import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppropriationsList, useSubmitAppropriation, useApproveAppropriation, useActivateAppropriation, useCancelAppropriation } from '../../hooks/useAppropriations';
import { appropriationTypeLabels, appropriationStatusLabels, AppropriationStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { LifecycleActions, type LifecycleAction } from '../../components/LifecycleActions';
import { Button, Badge } from '@/components/ui';
import { ArrowRight, Plus } from 'lucide-react';

const appropriationActions: Record<number, LifecycleAction[]> = {
  [AppropriationStatus.Draft]: [
    { key: 'submit', label: 'تقديم', permission: BUDGET_PERMISSIONS.Appropriations.Submit },
  ],
  [AppropriationStatus.PendingApproval]: [
    { key: 'approve', label: 'اعتماد', permission: BUDGET_PERMISSIONS.Appropriations.Approve },
  ],
  [AppropriationStatus.Approved]: [
    { key: 'activate', label: 'تفعيل', permission: BUDGET_PERMISSIONS.Appropriations.Activate },
  ],
};

export default function AppropriationsListPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const budgetItemId = searchParams.get('budgetItemId') ? Number(searchParams.get('budgetItemId')) : undefined;
  const can = usePermission();

  const { data: appropriations, isLoading } = useAppropriationsList({ budgetItemId });
  const submit = useSubmitAppropriation();
  const approve = useApproveAppropriation();
  const activate = useActivateAppropriation();
  const cancel = useCancelAppropriation();

  const hooks: Record<string, ReturnType<typeof useSubmitAppropriation>> = {
    submit, approve, activate, cancel,
  };

  async function handleAction(actionKey: string, id: number, rowVersion: string) {
    const hook = hooks[actionKey];
    if (!hook) return;
    await hook.mutateAsync({ id, rowVersion });
  }

  if (isLoading) {
    return <div className="p-6 text-center text-[var(--color-on-surface-variant)]">جارٍ التحميل...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">التخصيصات</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/appropriations/create')}>
          <Plus size={16} className="ms-1" />
          تخصيص جديد
        </Button>
      </div>

      {!appropriations || appropriations.length === 0 ? (
        <div className="rounded-lg border border-dashed border-[var(--color-outline)] p-12 text-center">
          <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد تخصيصات بعد</p>
        </div>
      ) : (
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container)]">
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">رقم التخصيص</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">النوع</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">المبلغ</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">الحالة</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {appropriations.map((app) => (
                <tr key={app.id} className="border-b border-[var(--color-outline-variant)] last:border-0">
                  <td className="px-4 py-3 font-mono">{app.appropriationNumber}</td>
                  <td className="px-4 py-3">
                    <Badge variant="outline">{appropriationTypeLabels[app.appropriationType]}</Badge>
                  </td>
                  <td className="px-4 py-3 font-mono">{app.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</td>
                  <td className="px-4 py-3">
                    <Badge>{appropriationStatusLabels[app.status]}</Badge>
                  </td>
                  <td className="px-4 py-3">
                    <LifecycleActions
                      actions={appropriationActions[app.status] ?? []}
                      can={can}
                      onAction={(key) => handleAction(key, app.id, app.rowVersion)}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
