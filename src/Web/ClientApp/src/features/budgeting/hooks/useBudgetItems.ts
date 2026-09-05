import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetsClient,
  CreateBudgetItemCommand,
  DeleteBudgetItemCommand,
  MoveBudgetItemCommand,
  UpdateBudgetItemCommand,
} from '../../../web-api-client';

const client = new BudgetsClient();

export function useBudgetItemsTree(budgetId: number) {
  return useQuery({
    queryKey: ['budget-items-tree', budgetId],
    queryFn: () => client.tree2(budgetId),
    enabled: Number.isFinite(budgetId),
  });
}

export function useBudgetItemDetail(itemId: number) {
  return useQuery({
    queryKey: ['budget-items', itemId],
    queryFn: () => client.itemsGET(itemId),
    enabled: Number.isFinite(itemId),
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
      client.itemsPOST(budgetId, CreateBudgetItemCommand.fromJS({ budgetId, ...data })),
    onSuccess: () => invalidateItems(qc),
  });
}

export function useUpdateBudgetItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.itemsPUT(id, UpdateBudgetItemCommand.fromJS({ id, ...data })),
    onSuccess: () => invalidateItems(qc),
  });
}

export function useDeleteBudgetItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.itemsDELETE(id, DeleteBudgetItemCommand.fromJS({ id, ...data })),
    onSuccess: () => invalidateItems(qc),
  });
}

export function useMoveBudgetItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.move(id, MoveBudgetItemCommand.fromJS({ id, ...data })),
    onSuccess: () => invalidateItems(qc),
  });
}
