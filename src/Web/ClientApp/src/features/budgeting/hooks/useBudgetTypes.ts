import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetTypesClient,
  CreateBudgetTypeRequest,
  BudgetTypeToggleActiveRequest,
  UpdateBudgetTypeRequest,
} from '../../../web-api-client';

const client = new BudgetTypesClient();

export function useBudgetTypesList(isActive?: boolean) {
  return useQuery({
    queryKey: ['budget-types', { isActive }],
    queryFn: () => client.budgetTypesAll(isActive),
  });
}

export function useBudgetTypeDetail(id: number) {
  return useQuery({
    queryKey: ['budget-types', id],
    queryFn: () => client.budgetTypesGET(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateBudgetType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.budgetTypesPOST(new CreateBudgetTypeRequest(data)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budget-types'] }),
  });
}

export function useUpdateBudgetType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.budgetTypesPUT(id, new UpdateBudgetTypeRequest({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budget-types'] }),
  });
}

export function useToggleBudgetTypeActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.toggleActivePATCH2(id, new BudgetTypeToggleActiveRequest({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budget-types'] }),
  });
}
