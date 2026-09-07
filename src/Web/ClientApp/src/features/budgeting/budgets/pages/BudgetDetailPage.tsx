import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBudgetDetail, useSubmitBudget, useApproveBudget, useActivateBudget, useSuspendBudget, useCloseBudget, useCancelBudget } from '../../hooks/useBudgets';
import { useBudgetItemsTree, useCreateBudgetItem, useUpdateBudgetItem, useDeleteBudgetItem } from '../../hooks/useBudgetItems';
import { useBudgetItemFormFields } from '../../hooks/useBudgetItemFormFields';
import { budgetStatusLabels, BudgetStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { BudgetItemTree } from '@/components/BudgetingBudgetItemTree';
import { LifecycleActions, type LifecycleAction } from '@/components/BudgetingLifecycleActions';
import { ApprovalHistoryPanel } from '@/components/BudgetingApprovalHistoryPanel';
import { MonthlyPlanEditor } from '@/components/BudgetingMonthlyPlanEditor';
import { ExecutionDrillDown } from '@/components/BudgetingExecutionDrillDown';
import { Button, Badge, Card, Switch, Input, Select, Textarea } from '@/components/ui';
import { ArrowRight, Plus, Calendar, BarChart3 } from 'lucide-react';

const budgetActions: Record<string, LifecycleAction[]> = {
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

export default function BudgetDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const budgetId = Number(id);
  const { hasPermission } = usePermission();
  const can = (_permission?: string) => hasPermission;

  const transitionHooks: Record<string, ReturnType<typeof useSubmitBudget>> = {
    submit: useSubmitBudget(),
    approve: useApproveBudget(),
    activate: useActivateBudget(),
    suspend: useSuspendBudget(),
    close: useCloseBudget(),
    cancel: useCancelBudget(),
  };

  const { data: budget, isLoading } = useBudgetDetail(budgetId);
  const { data: tree } = useBudgetItemsTree(budgetId);
  const createItem = useCreateBudgetItem(budgetId);
  const updateItem = useUpdateBudgetItem();
  const deleteItem = useDeleteBudgetItem();
  const { fundOptions, costCenterOptions, accountOptions, classificationOptions } = useBudgetItemFormFields();

  const [pendingAction, setPendingAction] = useState<string | null>(null);
  const [showAddItem, setShowAddItem] = useState(false);
  const [newItemCode, setNewItemCode] = useState('');
  const [newItemName, setNewItemName] = useState('');
  const [newFundId, setNewFundId] = useState<number>(0);
  const [newAccountId, setNewAccountId] = useState<number>(0);
  const [newCostCenterId, setNewCostCenterId] = useState<number>(0);
  const [newClassificationId, setNewClassificationId] = useState<number>(0);
  const [newRemarks, setNewRemarks] = useState('');
  const [newAllowOverrun, setNewAllowOverrun] = useState(false);
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
    await createItem.mutateAsync({
      budgetId,
      itemCode: newItemCode,
      itemName: newItemName,
      fundId: newFundId || undefined,
      accountId: newAccountId || undefined,
      costCenterId: newCostCenterId || undefined,
      budgetClassificationId: newClassificationId || undefined,
      remarks: newRemarks || undefined,
      allowOverrun: newAllowOverrun || undefined,
    });
    setNewItemCode('');
    setNewItemName('');
    setNewFundId(0);
    setNewAccountId(0);
    setNewCostCenterId(0);
    setNewClassificationId(0);
    setNewRemarks('');
    setNewAllowOverrun(false);
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

      <Card className="bg-[var(--color-surface-container-lowest)]">
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
            <span className="block text-sm">{budget.effectiveFrom?.toLocaleDateString('ar-EG')}</span>
          </div>
        </div>
      </Card>

      {!isTerminal && (
        <LifecycleActions
          actions={budgetActions[budget.status] ?? []}
          can={can}
          pendingKey={pendingAction}
          onAction={handleLifecycleAction}
        />
      )}

      <div className="flex gap-1 border-b border-[var(--color-outline-variant)]">
        <Button
          variant="ghost"
          onClick={() => setActiveTab('items')}
          className={`border-b-2 rounded-none ${
            activeTab === 'items'
              ? 'border-[var(--color-primary)] text-[var(--color-primary)]'
              : 'border-transparent text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)]'
          }`}
        >
          بنود الموازنة
        </Button>
        <Button
          variant="ghost"
          onClick={() => setActiveTab('execution')}
          className={`border-b-2 rounded-none ${
            activeTab === 'execution'
              ? 'border-[var(--color-primary)] text-[var(--color-primary)]'
              : 'border-transparent text-[var(--color-on-surface-variant)] hover:text-[var(--color-on-surface)]'
          }`}
        >
          <BarChart3 size={14} className="ms-1 inline" />
          التنفيذ
        </Button>
      </div>

      {activeTab === 'items' && (
        <Card padding="sm" className="bg-[var(--color-surface-container-lowest)]">
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
            <div className="mb-4 p-3 rounded bg-[var(--color-surface-container)] space-y-3">
              <div className="flex gap-2">
                <Input
                  type="text"
                  placeholder="كود البند"
                  value={newItemCode}
                  onChange={(e) => setNewItemCode(e.target.value)}
                  className="w-24"
                />
                <Input
                  type="text"
                  placeholder="اسم البند"
                  value={newItemName}
                  onChange={(e) => setNewItemName(e.target.value)}
                  className="flex-1"
                />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <Select
                  value={String(newFundId)}
                  onChange={(e) => setNewFundId(Number(e.target.value))}
                  label="الصندوق"
                  options={[
                    { value: '0', label: 'الصندوق...' },
                    ...fundOptions.map((f) => ({ value: String(f.id), label: f.label })),
                  ]}
                />
                <Select
                  value={String(newAccountId)}
                  onChange={(e) => setNewAccountId(Number(e.target.value))}
                  label="الحساب"
                  options={[
                    { value: '0', label: 'الحساب...' },
                    ...accountOptions.map((a) => ({ value: String(a.id), label: a.label })),
                  ]}
                />
                <Select
                  value={String(newCostCenterId)}
                  onChange={(e) => setNewCostCenterId(Number(e.target.value))}
                  label="مركز التكلفة"
                  options={[
                    { value: '0', label: 'مركز التكلفة...' },
                    ...costCenterOptions.map((cc) => ({ value: String(cc.id), label: cc.label })),
                  ]}
                />
                <Select
                  value={String(newClassificationId)}
                  onChange={(e) => setNewClassificationId(Number(e.target.value))}
                  label="التصنيف"
                  options={[
                    { value: '0', label: 'التصنيف...' },
                    ...classificationOptions.map((c) => ({ value: String(c.id), label: c.label })),
                  ]}
                />
              </div>
              <Textarea
                placeholder="ملاحظات"
                value={newRemarks}
                onChange={(e) => setNewRemarks(e.target.value)}
                rows={2}
              />
              <div className="flex items-center gap-4">
                <Switch
                  checked={newAllowOverrun}
                  onChange={setNewAllowOverrun}
                  label="السماح بالتجاوز"
                />
                <div className="flex-1" />
                <Button variant="primary" size="sm" onClick={handleAddItem}>حفظ</Button>
                <Button variant="ghost" size="sm" onClick={() => setShowAddItem(false)}>إلغاء</Button>
              </div>
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
        </Card>
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
