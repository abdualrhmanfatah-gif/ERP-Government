import { useNavigate, useSearchParams } from 'react-router-dom';
import { useState } from 'react';
import { useEncumbrancesList, useSubmitEncumbrance, useApproveEncumbrance, useActivateEncumbrance, useCloseEncumbrance, useCancelEncumbrance, useReverseEncumbrance } from '../../hooks/useEncumbrances';
import { encumbranceTypeLabels, encumbranceStatusLabels, EncumbranceStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { Page, Button, Badge, ConfirmDialog } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, RotateCcw } from 'lucide-react';

interface LifecycleAction {
  key: string;
  label: string;
  permission?: string;
  confirmMessage?: string;
}

const encumbranceActions: Record<number, LifecycleAction[]> = {
  [EncumbranceStatus.Draft]: [
    { key: 'submit', label: 'إرسال للمراجعة', permission: BUDGET_PERMISSIONS.Encumbrances.Submit },
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
  [EncumbranceStatus.PartiallyReleased]: [
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Encumbrances.Close },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Encumbrances.Cancel },
  ],
};

export default function EncumbrancesListPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const budgetId = searchParams.get('budgetId') ? Number(searchParams.get('budgetId')) : undefined;
  const { hasPermission } = usePermission();
  const can = (_permission?: string) => hasPermission;

  const { data: encumbrances, isLoading } = useEncumbrancesList({ budgetId });
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

  const columns: DataGridColumn<any>[] = [
    { header: 'رقم الالتزام', cell: (row) => <span className="font-mono">{row.encumbranceNumber}</span> },
    { header: 'النوع', cell: (row) => <Badge variant="outline">{encumbranceTypeLabels[row.encumbranceType]}</Badge> },
    { header: 'المبلغ', cell: (row) => <span className="font-mono">{row.amount.toLocaleString('ar-YE', { minimumFractionDigits: 2 })}</span> },
    { header: 'التاريخ', cell: (row) => row.encumbranceDate },
    {
      header: 'الحالة',
      cell: (row) => (
        <div className="flex items-center gap-2">
          <Badge>{encumbranceStatusLabels[row.status]}</Badge>
          {row.isReversed && (
            <Badge variant="danger">
              <RotateCcw size={12} className="ms-1" />
              معكوس
            </Badge>
          )}
        </div>
      ),
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <RowActionButtons
          actions={encumbranceActions[row.status] ?? []}
          can={can}
          onAction={(key) => handleAction(key, row.id, row.rowVersion)}
        />
      ),
    },
  ];

  return (
    <Page
      title="الالتزامات"
      actions={
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/encumbrances/create')}>
          <Plus size={16} className="ms-1" />
          التزام جديد
        </Button>
      }
    >
      <DataGrid
        columns={columns}
        data={encumbrances ?? []}
        loading={isLoading}
        emptyMessage="لا توجد التزامات بعد"
        rowKey={(row) => row.id}
      />
    </Page>
  );
}

interface RowActionButtonsProps {
  actions: LifecycleAction[];
  can: (permission?: string) => boolean;
  onAction: (key: string) => void;
}

function RowActionButtons({ actions, can, onAction }: RowActionButtonsProps) {
  const [confirming, setConfirming] = useState<LifecycleAction | null>(null);
  const visible = actions.filter((a) => can(a.permission));
  if (visible.length === 0) return null;

  return (
    <>
      <div className="flex flex-wrap gap-2" role="group" aria-label="إجراءات دورة الحياة">
        {visible.map((action) => (
          <Button
            key={action.key}
            variant="primary"
            size="sm"
            onClick={() => {
              if (action.confirmMessage) {
                setConfirming(action);
                return;
              }
              onAction(action.key);
            }}
          >
            {action.label}
          </Button>
        ))}
      </div>
      <ConfirmDialog
        open={confirming !== null}
        onClose={() => setConfirming(null)}
        onConfirm={() => {
          if (confirming) onAction(confirming.key);
          setConfirming(null);
        }}
        title="تأكيد الإجراء"
        message={confirming?.confirmMessage ?? ''}
      />
    </>
  );
}
