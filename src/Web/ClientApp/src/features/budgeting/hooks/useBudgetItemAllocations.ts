import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetItemAllocationsClient,
  CreateBudgetItemAllocationRequest,
  UpdateBudgetItemAllocationRequest,
} from '../../../web-api-client';

const client = new BudgetItemAllocationsClient();

export function useBudgetItemAllocationsList(budgetId: number) {
  return useQuery({
    queryKey: ['budget-item-allocations', budgetId],
    queryFn: () => client.budgetItemAllocationsAll(budgetId),
    enabled: Number.isFinite(budgetId) && budgetId > 0,
  });
}

export function useBudgetItemAllocationDetail(id: number) {
  return useQuery({
    queryKey: ['budget-item-allocations', id],
    queryFn: () => client.budgetItemAllocationsGET(id),
    enabled: Number.isFinite(id) && id > 0,
  });
}

function invalidateAllocations(qc: ReturnType<typeof useQueryClient>, budgetId?: number) {
  qc.invalidateQueries({ queryKey: ['budget-item-allocations'] });
  if (budgetId) {
    qc.invalidateQueries({ queryKey: ['budget-item-allocations', budgetId] });
  }
}

export function useCreateBudgetItemAllocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: { budgetId: number; budgetItemId: number; proposedAmount: number; remarks?: string }) =>
      client.budgetItemAllocationsPOST(
        new CreateBudgetItemAllocationRequest({
          budgetId: data.budgetId,
          budgetItemId: data.budgetItemId,
          proposedAmount: data.proposedAmount,
          remarks: data.remarks,
        }),
      ),
    onSuccess: (_result, variables) => {
      invalidateAllocations(qc, variables.budgetId);
    },
  });
}

export function useUpdateBudgetItemAllocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, budgetId, ...data }: { id: number; budgetId: number; proposedAmount: number; remarks?: string; rowVersion: string }) =>
      client.budgetItemAllocationsPUT(
        id,
        new UpdateBudgetItemAllocationRequest({
          proposedAmount: data.proposedAmount,
          remarks: data.remarks,
          rowVersion: data.rowVersion,
        }),
      ),
    onSuccess: (_result, variables) => {
      invalidateAllocations(qc, variables.budgetId);
    },
  });
}

export function useDeleteBudgetItemAllocation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id }: { id: number }) =>
      client.budgetItemAllocationsDELETE(id),
    onSuccess: () => {
      invalidateAllocations(qc);
    },
  });
}
