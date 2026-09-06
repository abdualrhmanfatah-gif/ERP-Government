import { useNavigate, useSearchParams } from 'react-router-dom';
import { useEncumbrancesList, useSubmitEncumbrance, useApproveEncumbrance, useActivateEncumbrance, useCloseEncumbrance, useCancelEncumbrance, useReverseEncumbrance } from '../../hooks/useEncumbrances';
import { encumbranceTypeLabels, encumbranceStatusLabels, EncumbranceStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { LifecycleActions, type LifecycleAction } from '../../components/LifecycleActions';
import { Button, Badge } from '@/components/ui';
import { ArrowRight, Plus, RotateCcw } from 'lucide-react';

const encumbranceActions: Record<number, LifecycleAction[]> = {
  [EncumbranceStatus.Draft]: [
    { key: 'submit', label: 'تقديم', permission: BUDGET_PERMISSIONS.Encumbrances.Submit },
  ],
  [EncumbranceStatus.PendingApproval]: [
    { key: 'approve', label: 'اعتماد', permission: BUDGET_PERMISSIONS.Encumbrances.Approve },
  ],
  [EncumbranceStatus.Approved]: [
    { key: 'activate', label: 'تفعيل', permission: BUDGET_PERMISSIONS.Encumbrances.Activate },
  ],
  [EncumbranceStatus.Active]: [
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Encumbrances.Close },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Encumbrances.Cancel },
    { key: 'reverse', label: 'عكس', permission: BUDGET_PERMISSIONS.Encumbrances.Reverse, confirmMessage: 'هل أنت متأكد من عكس هذا الالتزام؟' },
  ],
  [EncumbranceStatus.PartialReleased]: [
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Encumbrances.Close },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Encumbrances.Cancel },
  ],
};

export default function EncumbrancesListPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const appropriationId = searchParams.get('appropriationId') ? Number(searchParams.get('appropriationId')) : undefined;
  const can = usePermission();

  const { data: encumbrances, isLoading } = useEncumbrancesList({ appropriationId });
  const submit = useSubmitEncumbrance();
  const approve = useApproveEncumbrance();
  const activate = useActivateEncumbrance();
  const close = useCloseEncumbrance();
  const cancel = useCancelEncumbrance();
  const reverse = useReverseEncumbrance();

  const hooks: Record<string, ReturnType<typeof useSubmitEncumbrance>> = {
    submit, approve, activate, close, cancel, reverse,
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
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">الالتزامات</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/encumbrances/create')}>
          <Plus size={16} className="ms-1" />
          التزام جديد
        </Button>
      </div>

      {!encumbrances || encumbrances.length === 0 ? (
        <div className="rounded-lg border border-dashed border-[var(--color-outline)] p-12 text-center">
          <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد التزامات بعد</p>
        </div>
      ) : (
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container)]">
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">رقم الالتزام</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">النوع</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">المبلغ</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">التاريخ</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">الحالة</th>
                <th className="px-4 py-3 text-start font-medium text-[var(--color-on-surface-variant)]">إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {encumbrances.map((enc) => (
                <tr key={enc.id} className="border-b border-[var(--color-outline-variant)] last:border-0">
                  <td className="px-4 py-3 font-mono">{enc.encumbranceNumber}</td>
                  <td className="px-4 py-3">
                    <Badge variant="outline">{encumbranceTypeLabels[enc.encumbranceType]}</Badge>
                  </td>
                  <td className="px-4 py-3 font-mono">{enc.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</td>
                  <td className="px-4 py-3">{enc.encumbranceDate}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Badge>{encumbranceStatusLabels[enc.status]}</Badge>
                      {enc.isReversed && (
                        <Badge variant="danger">
                          <RotateCcw size={12} className="ms-1" />
                          معكوس
                        </Badge>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <LifecycleActions
                      actions={encumbranceActions[enc.status] ?? []}
                      can={can}
                      onAction={(key) => handleAction(key, enc.id, enc.rowVersion)}
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
