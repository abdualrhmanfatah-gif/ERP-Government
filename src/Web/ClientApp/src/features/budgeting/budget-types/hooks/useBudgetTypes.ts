import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { budgetTypesClient, budgetingKeys } from '../../shared/client';
import type { CreateBudgetTypeCommand, UpdateBudgetTypeCommand, ToggleBudgetTypeActiveCommand } from '../../shared/types';

export function useBudgetTypesList(isActive?: boolean) {
  return useQuery({
    queryKey: budgetingKeys.budgetTypes.list({ isActive }),
    queryFn: () => budgetTypesClient.list(isActive),
  });
}

export function useBudgetTypeDetail(id: number) {
  return useQuery({
    queryKey: budgetingKeys.budgetTypes.detail(id),
    queryFn: () => budgetTypesClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateBudgetType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateBudgetTypeCommand) => budgetTypesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.budgetTypes.all }),
  });
}

export function useUpdateBudgetType() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateBudgetTypeCommand) => budgetTypesClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.budgetTypes.all }),
  });
}

export function useToggleBudgetTypeActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, isActive }: ToggleBudgetTypeActiveCommand) =>
      budgetTypesClient.toggleActive(id, { id, rowVersion, isActive }),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.budgetTypes.all }),
  });
}
