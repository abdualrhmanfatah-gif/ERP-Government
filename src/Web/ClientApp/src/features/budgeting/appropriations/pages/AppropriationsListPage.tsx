import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  useAppropriationsList,
  useSubmitAppropriation,
  useApproveAppropriation,
  useActivateAppropriation,
  useSuspendAppropriation,
  useCloseAppropriation,
  useCancelAppropriation,
  useReverseAppropriation,
  useDeleteAppropriation,
} from '../../hooks/useAppropriations';
import {
  appropriationTypeLabels,
  appropriationStatusLabels,
  AppropriationStatus,
  AppropriationType,
} from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { LifecycleActions, type LifecycleAction } from '../../components/LifecycleActions';
import { Button, Badge } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { Plus } from 'lucide-react';

const appropriationActions: Record<number, LifecycleAction[]> = {
  [AppropriationStatus.Draft]: [
    { key: 'submit', label: 'تقديم', permission: BUDGET_PERMISSIONS.Appropriations.Submit },
    { key: 'delete', label: 'حذف', permission: BUDGET_PERMISSIONS.Appropriations.Delete, confirmMessage: 'هل أنت متأكد من حذف هذا التخصيص؟' },
  ],
  [AppropriationStatus.PendingApproval]: [
    { key: 'approve', label: 'اعتماد', permission: BUDGET_PERMISSIONS.Appropriations.Approve },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Appropriations.Cancel, confirmMessage: 'هل أنت متأكد من إلغاء هذا التخصيص؟' },
  ],
  [AppropriationStatus.Approved]: [
    { key: 'activate', label: 'تفعيل', permission: BUDGET_PERMISSIONS.Appropriations.Activate },
  ],
  [AppropriationStatus.Active]: [
    { key: 'suspend', label: 'تعليق', permission: BUDGET_PERMISSIONS.Appropriations.Suspend },
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Appropriations.Close },
    { key: 'reverse', label: 'عكس', permission: BUDGET_PERMISSIONS.Appropriations.Reverse, confirmMessage: 'هل أنت متأكد من عكس هذا التخصيص؟' },
  ],
  [AppropriationStatus.Suspended]: [
    { key: 'activate', label: 'إعادة تفعيل', permission: BUDGET_PERMISSIONS.Appropriations.Activate },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Appropriations.Cancel, confirmMessage: 'هل أنت متأكد من إلغاء هذا التخصيص؟' },
  ],
};

const statusOptions = Object.entries(appropriationStatusLabels).map(([value, label]) => ({ value, label }));
const typeOptions = Object.entries(appropriationTypeLabels).map(([value, label]) => ({ value, label }));

export default function AppropriationsListPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const budgetItemId = searchParams.get('budgetItemId') ? Number(searchParams.get('budgetItemId')) : undefined;
  const { hasPermission } = usePermission();
  const can = (_permission?: string) => hasPermission;

  const [statusFilter, setStatusFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');

  const { data: appropriations, isLoading } = useAppropriationsList({
    budgetItemId,
    status: statusFilter ? (statusFilter as AppropriationStatus) : undefined,
    appropriationType: typeFilter ? (typeFilter as AppropriationType) : undefined,
  });

  const submit = useSubmitAppropriation();
  const approve = useApproveAppropriation();
  const activate = useActivateAppropriation();
  const suspend = useSuspendAppropriation();
  const close = useCloseAppropriation();
  const cancel = useCancelAppropriation();
  const reverse = useReverseAppropriation();
  const deleteAppropriation = useDeleteAppropriation();

  const hooks: Record<string, ReturnType<typeof useSubmitAppropriation>> = {
    submit, approve, activate, suspend, close, cancel,
  };

  async function handleAction(actionKey: string, id: number, rowVersion: string) {
    if (actionKey === 'delete') {
      await deleteAppropriation.mutateAsync({ id });
      return;
    }
    if (actionKey === 'reverse') {
      await reverse.mutateAsync({ id, rowVersion });
      return;
    }
    const hook = hooks[actionKey];
    if (!hook) return;
    await hook.mutateAsync({ id, rowVersion });
  }

  const columns: DataGridColumn<any>[] = [
    {
      header: 'رقم التخصيص',
      cell: (row) => (
        <button
          type="button"
          className="font-mono text-[var(--color-primary)] hover:underline"
          onClick={() => navigate(`/budgeting/appropriations/${row.id}`)}
        >
          {row.appropriationNumber}
        </button>
      ),
    },
    { header: 'النوع', cell: (row) => <Badge variant="outline">{appropriationTypeLabels[row.appropriationType]}</Badge> },
    { header: 'المبلغ', cell: (row) => <span className="font-mono">{row.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span> },
    { header: 'البند', cell: (row) => <span className="text-sm">{row.itemCode} — {row.itemName}</span> },
    { header: 'الصندوق', cell: (row) => <span className="text-sm">{row.fundName}</span> },
    { header: 'السنة المالية', cell: (row) => <span className="text-sm">{row.fiscalYearName}</span> },
    { header: 'المستند', cell: (row) => <span className="text-sm">{row.documentType} — {row.documentId}</span> },
    { header: 'أنشأه', cell: (row) => <span className="text-sm">{row.createdBy}</span> },
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{new Date(row.created).toLocaleDateString('ar-EG')}</span> },
    { header: 'الحالة', cell: (row) => <Badge>{appropriationStatusLabels[row.status]}</Badge> },
    {
      header: 'إجراءات',
      cell: (row) => (
        <LifecycleActions
          actions={appropriationActions[row.status] ?? []}
          can={can}
          onAction={(key) => handleAction(key, row.id, row.rowVersion)}
        />
      ),
    },
  ];

  const hasFilters = !!statusFilter || !!typeFilter;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">التخصيصات</h1>
        <Button variant="primary" size="sm" onClick={() => navigate('/budgeting/appropriations/create')}>
          <Plus size={16} className="ms-1" />
          تخصيص جديد
        </Button>
      </div>

      <div className="flex gap-2 items-center">
        <FilterSelect
          value={statusFilter}
          onChange={setStatusFilter}
          options={statusOptions}
          placeholder="الحالة"
          label="الحالة"
        />
        <FilterSelect
          value={typeFilter}
          onChange={setTypeFilter}
          options={typeOptions}
          placeholder="النوع"
          label="النوع"
        />
        {hasFilters && (
          <Button variant="ghost" size="sm" onClick={() => { setStatusFilter(''); setTypeFilter(''); }}>
            مسح الفلاتر
          </Button>
        )}
      </div>

      <DataGrid
        columns={columns}
        data={appropriations ?? []}
        loading={isLoading}
        emptyMessage="لا توجد تخصيصات بعد"
        rowKey={(row) => row.id}
      />
    </div>
  );
}
