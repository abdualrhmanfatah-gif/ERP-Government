import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBudgetDetail, useSubmitBudget, useApproveBudget, useActivateBudget, useSuspendBudget, useCloseBudget, useCancelBudget } from '../../hooks/useBudgets';
import { useBudgetItemsTree, useCreateBudgetItem, useUpdateBudgetItem, useDeleteBudgetItem } from '../../hooks/useBudgetItems';
import { budgetStatusLabels, BudgetStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { BudgetItemTree } from '../../components/BudgetItemTree';
import { LifecycleActions, type LifecycleAction } from '../../components/LifecycleActions';
import { ApprovalHistoryPanel } from '../../components/ApprovalHistoryPanel';
import { MonthlyPlanEditor } from '../../monthly-plan/components/MonthlyPlanEditor';
import { ExecutionDrillDown } from '../../execution/components/ExecutionDrillDown';
import { Button, Badge } from '@/components/ui';
import { ArrowRight, Plus, Calendar, BarChart3 } from 'lucide-react';

const budgetActions: Record<number, LifecycleAction[]> = {
  [BudgetStatus.Draft]: [
    { key: 'submit', label: 'تقديم', permission: BUDGET_PERMISSIONS.Budgets.Submit, confirmMessage: 'هل أنت متأكد من تقديم هذه الموازنة؟' },
  ],
  [BudgetStatus.Submitted]: [
    { key: 'approve', label: 'اعتماد', permission: BUDGET_PERMISSIONS.Budgets.Approve, confirmMessage: 'هل أنت متأكد من اعتماد هذه الموازنة؟' },
  ],
  [BudgetStatus.Approved]: [
    { key: 'activate', label: 'تفعيل', permission: BUDGET_PERMISSIONS.Budgets.Activate, confirmMessage: 'هل أنت متأكد من تفعيل هذه الموازنة؟' },
  ],
  [BudgetStatus.Active]: [
    { key: 'suspend', label: 'تعليق', permission: BUDGET_PERMISSIONS.Budgets.Suspend, confirmMessage: 'هل أنت متأكد من تعليق هذه الموازنة؟' },
    { key: 'close', label: 'إغلاق', permission: BUDGET_PERMISSIONS.Budgets.Close, confirmMessage: 'هل أنت متأكد من إغلاق هذه الموازنة؟' },
  ],
  [BudgetStatus.Suspended]: [
    { key: 'activate', label: 'إعادة تفعيل', permission: BUDGET_PERMISSIONS.Budgets.Activate },
    { key: 'cancel', label: 'إلغاء', permission: BUDGET_PERMISSIONS.Budgets.Cancel, confirmMessage: 'هل أنت متأكد من إلغاء هذه الموازنة؟' },
  ],
};

const transitionHooks: Record<string, ReturnType<typeof useSubmitBudget>> = {
  submit: useSubmitBudget(),
  approve: useApproveBudget(),
  activate: useActivateBudget(),
  suspend: useSuspendBudget(),
  close: useCloseBudget(),
  cancel: useCancelBudget(),
};

export default function BudgetDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const budgetId = Number(id);
  const can = usePermission();

  const { data: budget, isLoading } = useBudgetDetail(budgetId);
  const { data: tree } = useBudgetItemsTree(budgetId);
  const createItem = useCreateBudgetItem(budgetId);
  const updateItem = useUpdateBudgetItem();
  const deleteItem = useDeleteBudgetItem();

  const [pendingAction, setPendingAction] = useState<string | null>(null);
  const [showAddItem, setShowAddItem] = useState(false);
  const [newItemCode, setNewItemCode] = useState('');
  const [newItemName, setNewItemName] = useState('');
  const [selectedItemId, setSelectedItemId] = useState<number | null>(null);
  const [activeTab, setActiveTab] = useState<'items' | 'execution'>('items');

  const isDraft = budget?.status === BudgetStatus.Draft;
  const isTerminal = budget?.status === BudgetStatus.Closed || budget?.status === BudgetStatus.Cancelled;

  async function handleLifecycleAction(actionKey: string) {
    const hook = transitionHooks[actionKey];
    if (!hook || !budget) return;
    setPendingAction(actionKey);
    try {
      await hook.mutateAsync({ id: budget.id, rowVersion: budget.rowVersion });
    } catch {
      // error handled by mutation
    } finally {
      setPendingAction(null);
    }
  }

  async function handleAddItem() {
    if (!newItemCode.trim() || !newItemName.trim()) return;
    await createItem.mutateAsync({ itemCode: newItemCode, itemName: newItemName });
    setNewItemCode('');
    setNewItemName('');
    setShowAddItem(false);
  }

  if (isLoading) {
    return (
      <div className="p-6 space-y-4">
        <div className="h-8 w-64 rounded bg-[var(--color-surface-container)] animate-pulse" />
        <div className="h-48 rounded-lg bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  if (!budget) {
    return <div className="p-6 text-center text-[var(--color-error)]">لم يتم العثور على الموازنة</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/budgeting/budgets')} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-semibold text-[var(--color-on-surface)]">{budget.budgetName}</h1>
            <Badge variant={budget.status === BudgetStatus.Active ? 'info' : budget.status === BudgetStatus.Draft ? 'default' : 'success'}>
              {budgetStatusLabels[budget.status]}
            </Badge>
          </div>
          <p className="text-sm text-[var(--color-on-surface-variant)]">{budget.budgetNumber}</p>
        </div>
      </div>

      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">الصندوق</span>
            <span className="block text-sm">{budget.fundName}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ الإجمالي</span>
            <span className="block text-sm font-mono">{budget.totalAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)] mb-1">من تاريخ</span>
            <span className="block text-sm">{budget.effectiveFrom}</span>
          </div>
        </div>
      </div>

      {!isTerminal && (
        <LifecycleActions
          actions={budgetActions[budget.status] ?? []}
          can={can}
          pendingKey={pendingAction}
          onAction={handleLifecycleAction}
        />
      )}

      <div className="flex gap-1 border-b border-[var(--color-outline-variant)]">
        <button
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'items'
              ? 'border-[var(--color-primary)] text-[var(--color-primary)]'
              : 'border-transparent text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)]'
          }`}
          onClick={() => setActiveTab('items')}
        >
          بنود الموازنة
        </button>
        <button
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === 'execution'
              ? 'border-[var(--color-primary)] text-[var(--color-primary)]'
              : 'border-transparent text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)]'
          }`}
          onClick={() => setActiveTab('execution')}
        >
          <BarChart3 size={14} className="ms-1 inline" />
          التنفيذ
        </button>
      </div>

      {activeTab === 'items' && (
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-sm font-semibold">بنود الموازنة</h2>
            {isDraft && (
              <Button variant="ghost" size="sm" onClick={() => setShowAddItem(true)}>
                <Plus size={14} className="ms-1" />
                إضافة بند
              </Button>
            )}
          </div>

          {showAddItem && (
            <div className="flex gap-2 mb-4 p-3 rounded bg-[var(--color-surface-container)]">
              <input
                type="text"
                placeholder="كود البند"
                value={newItemCode}
                onChange={(e) => setNewItemCode(e.target.value)}
                className="rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-2 py-1 text-sm w-24"
              />
              <input
                type="text"
                placeholder="اسم البند"
                value={newItemName}
                onChange={(e) => setNewItemName(e.target.value)}
                className="flex-1 rounded border border-[var(--color-outline)] bg-[var(--color-surface)] px-2 py-1 text-sm"
              />
              <Button variant="primary" size="sm" onClick={handleAddItem}>حفظ</Button>
              <Button variant="ghost" size="sm" onClick={() => setShowAddItem(false)}>إلغاء</Button>
            </div>
          )}

          <BudgetItemTree
            nodes={tree ?? []}
            canEdit={isDraft}
            onAddChild={() => setShowAddItem(true)}
            onSelectItem={setSelectedItemId}
          />

          {selectedItemId && (
            <div className="mt-4 pt-4 border-t border-[var(--color-outline-variant)]">
              <div className="flex items-center gap-2 mb-3">
                <Calendar size={14} />
                <span className="text-xs text-[var(--color-on-surface-variant)]">الخطة الشهرية للبند المحدد</span>
              </div>
              <MonthlyPlanEditor
                budgetItemId={selectedItemId}
                appropriatedTotal={0}
              />
            </div>
          )}
        </div>
      )}

      {activeTab === 'execution' && (
        <ExecutionDrillDown budgetId={budgetId} />
      )}

      <ApprovalHistoryPanel
        decisions={[budget.latestApproval].filter(Boolean)}
        title="سجل اعتمادات الموازنة"
      />
    </div>
  );
}
