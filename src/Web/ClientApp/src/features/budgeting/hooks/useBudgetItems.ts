import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetsClient,
  CreateBudgetItemRequest,
  UpdateBudgetItemRequest,
} from '../../../web-api-client';

const client = new BudgetsClient();

export function useBudgetItemsTree(budgetId: number) {
  return useQuery({
    queryKey: ['budget-items-tree', budgetId],
    queryFn: () => client.tree2(budgetId),
    enabled: Number.isFinite(budgetId),
  });
}

function invalidateItems(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: ['budget-items-tree'] });
  qc.invalidateQueries({ queryKey: ['budget-items'] });
  qc.invalidateQueries({ queryKey: ['budgets'] });
}

export function useCreateBudgetItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ budgetId, ...data }: Record<string, unknown> & { budgetId: number }) =>
      client.itemsPOST(budgetId, new CreateBudgetItemRequest({ budgetId, ...data } as any)),
    onSuccess: () => invalidateItems(qc),
  });
}

export function useUpdateBudgetItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ budgetId, itemId, ...data }: Record<string, unknown> & { budgetId: number; itemId: number }) =>
      client.itemsPUT(budgetId, itemId, new UpdateBudgetItemRequest({ ...data } as any)),
    onSuccess: () => invalidateItems(qc),
  });
}
