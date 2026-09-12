import { useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBudgetDetail, useSubmitBudget, useApproveBudget, useActivateBudget, useSuspendBudget, useCloseBudget, useCancelBudget } from '../../hooks/useBudgets';
import { useBudgetItemsTree, useCreateBudgetItem, useUpdateBudgetItem } from '../../hooks/useBudgetItems';
import { useBudgetItemAllocationsList, useCreateBudgetItemAllocation, useUpdateBudgetItemAllocation } from '../../hooks/useBudgetItemAllocations';
import { useBudgetItemFormFields } from '../../hooks/useBudgetItemFormFields';
import { budgetStatusLabels, BudgetStatus } from '../../shared/types';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { BudgetItemTree } from '@/components/BudgetingBudgetItemTree';
import { ApprovalHistoryPanel } from '@/components/BudgetingApprovalHistoryPanel';
import { MonthlyPlanEditor } from '@/components/BudgetingMonthlyPlanEditor';

import { Page, Button, Badge, Card, Grid, Stack, Label, ConfirmDialog, Switch, Input, Select, Textarea } from '@/components/ui';
import { ArrowRight, Plus, Calendar, Pencil, X, Check } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import type { BudgetItemDto } from '@/web-api-client';

interface LifecycleAction {
  key: string;
  label: string;
  permission?: string;
  confirmMessage?: string;
}

function findBudgetItem(nodes: BudgetItemDto[], id: number): BudgetItemDto | undefined {
  for (const n of nodes) {
    if (n.id === id) return n;
    if (n.children) {
      const found = findBudgetItem(n.children, id);
      if (found) return found;
    }
  }
  return undefined;
}

const budgetActions: Record<string, LifecycleAction[]> = {
  [BudgetStatus.Draft]: [
    { key: 'submit', label: 'إرسال للمراجعة', permission: BUDGET_PERMISSIONS.Budgets.Submit, confirmMessage: 'هل أنت متأكد من إرسال هذه الموازنة للمراجعة؟' },
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
  const can = (_p?: string) => true;

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
  const { data: allocations = [] } = useBudgetItemAllocationsList(budgetId);
  const createItem = useCreateBudgetItem();
  const createAllocation = useCreateBudgetItemAllocation();
  const updateAllocation = useUpdateBudgetItemAllocation();
  const { costCenterOptions, accountOptions, classificationOptions } = useBudgetItemFormFields();

  const [pendingAction, setPendingAction] = useState<string | null>(null);
  const [confirmingAction, setConfirmingAction] = useState<LifecycleAction | null>(null);
  const [showAddItem, setShowAddItem] = useState(false);
  const [newItemCode, setNewItemCode] = useState('');
  const [newItemName, setNewItemName] = useState('');
  const [newAccountId, setNewAccountId] = useState<number>(0);
  const [newCostCenterId, setNewCostCenterId] = useState<number>(0);
  const [newClassificationId, setNewClassificationId] = useState<number>(0);
  const [newRemarks, setNewRemarks] = useState('');
  const [newAllowOverrun, setNewAllowOverrun] = useState(false);
  const [newProposedAmount, setNewProposedAmount] = useState<number>(0);
  const [selectedItemId, setSelectedItemId] = useState<number | null>(null);
  const [isEditingItem, setIsEditingItem] = useState(false);
  const [editItemName, setEditItemName] = useState('');
  const [editAccountId, setEditAccountId] = useState<number>(0);
  const [editCostCenterId, setEditCostCenterId] = useState<number>(0);
  const [editClassificationId, setEditClassificationId] = useState<number>(0);
  const [editAllowOverrun, setEditAllowOverrun] = useState(false);
  const [editRemarks, setEditRemarks] = useState('');
  const [editProposedAmount, setEditProposedAmount] = useState<number>(0);

  const updateItem = useUpdateBudgetItem();

  const itemDetail = useMemo(() => {
    if (!selectedItemId || !tree) return undefined;
    return findBudgetItem(tree, selectedItemId);
  }, [selectedItemId, tree]);

  const selectedItemAllocation = useMemo(() => {
    if (!selectedItemId) return undefined;
    return allocations.find((a) => a.budgetItemId === selectedItemId);
  }, [selectedItemId, allocations]);

  const isDraft = budget?.status === BudgetStatus.Draft;
  const canAddItems = isDraft;
  const isTerminal = budget?.status === BudgetStatus.Closed || budget?.status === BudgetStatus.Cancelled;

  async function handleLifecycleAction(actionKey: string) {
    const hook = transitionHooks[actionKey];
    if (!hook || !budget) return;
    setPendingAction(actionKey);
    try {
      await hook.mutateAsync({ id: budgetId, rowVersion: budget.rowVersion });
    } catch (err: unknown) {
      let detail = '';
      if (err instanceof Error && 'response' in err) {
        const body = (err as { response: string }).response;
        try { detail = JSON.parse(body).join(', '); } catch { detail = body || err.message; }
      } else if (err instanceof Error) {
        detail = err.message;
      }
      notify({ type: 'error', title: detail || 'حدث خطأ' });
    } finally {
      setPendingAction(null);
    }
  }

  async function handleAddItem() {
    if (!newItemCode.trim() || !newItemName.trim()) return;
    const newItemId = await createItem.mutateAsync({
      budgetId,
      itemCode: newItemCode,
      itemName: newItemName,
      accountId: newAccountId || undefined,
      costCenterId: newCostCenterId || undefined,
      budgetClassificationId: newClassificationId || undefined,
      remarks: newRemarks || undefined,
      allowOverrun: newAllowOverrun || undefined,
    });
    if (newProposedAmount > 0 && newItemId) {
      await createAllocation.mutateAsync({
        budgetId,
        budgetItemId: Number(newItemId),
        proposedAmount: newProposedAmount,
      });
    }
    setNewItemCode('');
    setNewItemName('');
    setNewAccountId(0);
    setNewCostCenterId(0);
    setNewClassificationId(0);
    setNewRemarks('');
    setNewAllowOverrun(false);
    setNewProposedAmount(0);
    setShowAddItem(false);
  }

  function startEditItem() {
    if (!itemDetail) return;
    setEditItemName(itemDetail.itemName ?? '');
    setEditAccountId(itemDetail.accountId ?? 0);
    setEditCostCenterId(itemDetail.costCenterId ?? 0);
    setEditClassificationId(itemDetail.budgetClassificationId ?? 0);
    setEditAllowOverrun(itemDetail.allowOverrunEffective ?? false);
    setEditRemarks(itemDetail.remarks ?? '');
    setEditProposedAmount(selectedItemAllocation?.proposedAmount ?? 0);
    setIsEditingItem(true);
  }

  function cancelEditItem() {
    setIsEditingItem(false);
  }

  async function handleSaveItem() {
    if (!itemDetail || !editItemName.trim() || !itemDetail.rowVersion) return;
    await updateItem.mutateAsync({
      budgetId,
      itemId: itemDetail.id!,
      itemName: editItemName,
      accountId: editAccountId || undefined,
      costCenterId: editCostCenterId || undefined,
      budgetClassificationId: editClassificationId || undefined,
      allowOverrun: editAllowOverrun,
      remarks: editRemarks || undefined,
      rowVersion: itemDetail.rowVersion ?? '',
    });
    if (selectedItemAllocation && editProposedAmount !== selectedItemAllocation.proposedAmount && selectedItemAllocation.rowVersion) {
      await updateAllocation.mutateAsync({
        id: selectedItemAllocation.id!,
        budgetId,
        proposedAmount: editProposedAmount,
        rowVersion: selectedItemAllocation.rowVersion ?? '',
      });
    } else if (!selectedItemAllocation && editProposedAmount > 0) {
      await createAllocation.mutateAsync({
        budgetId,
        budgetItemId: itemDetail.id!,
        proposedAmount: editProposedAmount,
      });
    }
    setIsEditingItem(false);
  }

  return (
    <Page
      title={budget?.budgetName ?? ''}
      description={
        budget
          ? [
              budget.budgetNumber,
              budget.fundName && `الصندوق: ${budget.fundName}`,
              budget.effectiveFrom && `من تاريخ: ${budget.effectiveFrom.toLocaleDateString('ar-YE')}`,
            ]
              .filter(Boolean)
              .join('  ·  ')
          : undefined
      }
      loading={isLoading}
      error={!isLoading && !budget ? 'لم يتم العثور على الموازنة' : undefined}
      breadcrumbs={[{ label: 'الموازنات', path: '/budgeting/budgets' }, { label: budget?.budgetName ?? '' }]}
      actions={
        <Stack direction="row" gap="sm" align="center">
          {budget && (
            <Badge variant={
              budget.status === BudgetStatus.Active ? 'primary' :
              budget.status === BudgetStatus.Draft ? 'default' :
              budget.status === BudgetStatus.Submitted ? 'warning' :
              budget.status === BudgetStatus.Approved ? 'success' :
              budget.status === BudgetStatus.Suspended ? 'warning' :
              budget.status === BudgetStatus.Closed ? 'secondary' :
              budget.status === BudgetStatus.Cancelled ? 'destructive' :
              'default'
            }>
              {budget.status ? budgetStatusLabels[budget.status] : ''}
            </Badge>
          )}
          {!isTerminal && budget && (
            <>
              {(budgetActions[budget.status] ?? [])
                .filter((a) => can(a.permission))
                .map((action) => (
                  <Button
                    key={action.key}
                    variant="primary"
                    size="sm"
                    disabled={pendingAction === action.key}
                    onClick={() => {
                      if (action.confirmMessage) {
                        setConfirmingAction(action);
                        return;
                      }
                      handleLifecycleAction(action.key);
                    }}
                  >
                    {action.label}
                  </Button>
                ))}
              <ConfirmDialog
                open={confirmingAction !== null}
                onClose={() => setConfirmingAction(null)}
                onConfirm={() => {
                  if (confirmingAction) handleLifecycleAction(confirmingAction.key);
                  setConfirmingAction(null);
                }}
                title="تأكيد الإجراء"
                message={confirmingAction?.confirmMessage ?? ''}
              />
            </>
          )}
          <Button variant="ghost" size="icon" onClick={() => navigate('/budgeting/budgets')} aria-label="العودة">
            <ArrowRight size={18} />
          </Button>
        </Stack>
      }
    >
      <Card variant="flat" padding="sm">
        <Stack direction="row" justify="between" align="center" className="mb-4">
          <Label>بنود الموازنة</Label>
          {canAddItems && (
            <Button variant="ghost" size="sm" onClick={() => setShowAddItem(true)}>
              <Plus size={14} className="ms-1" />
              إضافة بند
            </Button>
          )}
        </Stack>

        {showAddItem && (
          <Card variant="flat" className="mb-4">
            <Stack gap="sm">
              <Stack direction="row" gap="sm">
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
              </Stack>
              <Grid columns={3} gap="sm">
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
              </Grid>
              <Textarea
                placeholder="ملاحظات"
                value={newRemarks}
                onChange={(e) => setNewRemarks(e.target.value)}
                rows={2}
              />
              <Input
                label="المبلغ المقترح"
                type="number"
                value={newProposedAmount || ''}
                onChange={(e) => setNewProposedAmount(Number(e.target.value))}
                min="0"
                step="0.01"
              />
              <Stack direction="row" justify="between" align="center">
                <Switch
                  checked={newAllowOverrun}
                  onChange={setNewAllowOverrun}
                  label="السماح بالتجاوز"
                />
              </Stack>
              <Stack direction="row" gap="sm" justify="end">
                <Button variant="primary" size="sm" onClick={handleAddItem}>
                  <Check size={14} className="ms-1" />
                  حفظ
                </Button>
                <Button variant="ghost" size="sm" onClick={() => setShowAddItem(false)}>
                  <X size={14} className="ms-1" />
                  إلغاء
                </Button>
              </Stack>
            </Stack>
          </Card>
        )}

        <BudgetItemTree
          nodes={tree ?? []}
          selectedId={selectedItemId ?? undefined}
          canEdit={canAddItems}
          onAddChild={() => setShowAddItem(true)}
          onSelect={setSelectedItemId}
          onSelectItem={setSelectedItemId}
          selectedItemDetail={
            selectedItemId && itemDetail ? (
              <>
                <Card variant="outlined">
                  <Stack direction="row" justify="between" align="center" className="mb-3 pb-2 border-b border-[var(--color-outline-variant)]">
                    <Stack direction="row" gap="sm" align="center">
                      <span className="font-bold text-base text-[var(--color-primary)]">
                        {itemDetail.itemCode}
                      </span>
                      <h3 className="font-semibold text-sm text-[var(--color-on-surface)]">
                        {itemDetail.itemName}
                      </h3>
                    </Stack>
                    <Stack direction="row" gap="sm" align="center">
                      <Badge variant={itemDetail.isActive !== false ? 'success' : 'secondary'}>
                        {itemDetail.isActive !== false ? 'نشط' : 'معطل'}
                      </Badge>
                      {!isEditingItem && (
                        <Button variant="ghost" size="icon-xs" onClick={startEditItem} aria-label="تعديل البند">
                          <Pencil className="size-3.5" />
                        </Button>
                      )}
                    </Stack>
                  </Stack>

                  <Stack gap="sm">
                    <Grid columns={4} gap="sm">
                      <Input
                        label="كود البند"
                        value={itemDetail.itemCode ?? ''}
                        readOnly
                        disabled
                      />
                      <div className="md:col-span-3">
                        <Input
                          label="اسم البند"
                          value={isEditingItem ? editItemName : (itemDetail.itemName ?? '')}
                          readOnly={!isEditingItem}
                          disabled={!isEditingItem}
                          onChange={(e) => setEditItemName(e.target.value)}
                        />
                      </div>
                    </Grid>

                    <Grid columns={3} gap="sm">
                      <Select
                        label="الحساب"
                        value={isEditingItem ? String(editAccountId) : String(itemDetail.accountId ?? 0)}
                        disabled={!isEditingItem}
                        onChange={isEditingItem ? (e) => setEditAccountId(Number(e.target.value)) : undefined}
                        options={
                          isEditingItem
                            ? [{ value: '0', label: 'الحساب...' }, ...accountOptions.map((a) => ({ value: String(a.id), label: a.label }))]
                            : [{ value: String(itemDetail.accountId ?? 0), label: itemDetail.accountName || 'غير محدد' }]
                        }
                      />
                      <Select
                        label="مركز التكلفة"
                        value={isEditingItem ? String(editCostCenterId) : String(itemDetail.costCenterId ?? 0)}
                        disabled={!isEditingItem}
                        onChange={isEditingItem ? (e) => setEditCostCenterId(Number(e.target.value)) : undefined}
                        options={
                          isEditingItem
                            ? [{ value: '0', label: 'مركز التكلفة...' }, ...costCenterOptions.map((cc) => ({ value: String(cc.id), label: cc.label }))]
                            : [{ value: String(itemDetail.costCenterId ?? 0), label: itemDetail.costCenterName || 'غير محدد' }]
                        }
                      />
                      <Select
                        label="التصنيف"
                        value={isEditingItem ? String(editClassificationId) : String(itemDetail.budgetClassificationId ?? 0)}
                        disabled={!isEditingItem}
                        onChange={isEditingItem ? (e) => setEditClassificationId(Number(e.target.value)) : undefined}
                        options={
                          isEditingItem
                            ? [{ value: '0', label: 'التصنيف...' }, ...classificationOptions.map((c) => ({ value: String(c.id), label: c.label }))]
                            : [{ value: String(itemDetail.budgetClassificationId ?? 0), label: itemDetail.budgetClassificationName || 'غير محدد' }]
                        }
                      />
                    </Grid>

                    <Grid columns={4} gap="sm">
                      <div className="flex items-center h-11">
                        <Switch
                          checked={isEditingItem ? editAllowOverrun : (itemDetail.allowOverrunEffective ?? false)}
                          disabled={!isEditingItem}
                          onChange={isEditingItem ? setEditAllowOverrun : undefined}
                          label="السماح بالتجاوز"
                        />
                      </div>
                      <div className="md:col-span-3">
                        <Textarea
                          label="الملاحظات"
                          value={isEditingItem ? editRemarks : (itemDetail.remarks ?? '')}
                          readOnly={!isEditingItem}
                          disabled={!isEditingItem}
                          onChange={(e) => setEditRemarks(e.target.value)}
                          rows={2}
                        />
                      </div>
                    </Grid>

                    <Grid columns={2} gap="sm">
                      <Input
                        label="المبلغ المقترح"
                        type="number"
                        value={isEditingItem ? (editProposedAmount || '') : (selectedItemAllocation?.proposedAmount ?? '')}
                        readOnly={!isEditingItem}
                        disabled={!isEditingItem}
                        onChange={isEditingItem ? (e) => setEditProposedAmount(Number(e.target.value)) : undefined}
                        min="0"
                        step="0.01"
                      />
                      {selectedItemAllocation?.approvedAmount != null && (
                        <Input
                          label="المبلغ المعتمد"
                          type="number"
                          value={selectedItemAllocation.approvedAmount}
                          readOnly
                          disabled
                        />
                      )}
                    </Grid>
                    {isEditingItem && (
                      <Stack direction="row" gap="sm" justify="end">
                        <Button variant="primary" size="sm" onClick={handleSaveItem}>
                          <Check size={14} className="ms-1" />
                          حفظ
                        </Button>
                        <Button variant="ghost" size="sm" onClick={cancelEditItem}>
                          <X size={14} className="ms-1" />
                          إلغاء
                        </Button>
                      </Stack>
                    )}
                  </Stack>
                </Card>

                <Stack className="mt-4 pt-4 border-t border-[var(--color-outline-variant)]">
                  <Stack direction="row" gap="sm" align="center" className="mb-3">
                    <Calendar size={16} className="text-[var(--color-primary)]" />
                    <Label>الخطة الشهرية</Label>
                  </Stack>
                  <MonthlyPlanEditor
                    budgetItemId={selectedItemId}
                    appropriatedTotal={selectedItemAllocation?.approvedAmount ?? selectedItemAllocation?.proposedAmount ?? 0}
                  />
                </Stack>
              </>
            ) : undefined
          }
        />
      </Card>

      <ApprovalHistoryPanel
        decisions={[budget?.latestApproval].filter(Boolean)}
        title="سجل اعتمادات الموازنة"
      />
    </Page>
  );
}
