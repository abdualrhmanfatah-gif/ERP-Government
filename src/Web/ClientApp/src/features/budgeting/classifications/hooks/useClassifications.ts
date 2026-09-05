import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { budgetClassificationsClient, budgetingKeys } from '../../shared/client';
import type { BudgetClassificationTreeDto } from '../../shared/types';

export function useClassificationsTree() {
  return useQuery<BudgetClassificationTreeDto[]>({
    queryKey: budgetingKeys.budgetClassifications.tree(),
    queryFn: () => budgetClassificationsClient.tree(),
  });
}

export function useCreateClassification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { code: string; name: string; parentId?: number; isActive?: boolean }) =>
      budgetClassificationsClient.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}

export function useUpdateClassification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: { id: number; rowVersion: string; code: string; name: string; parentId?: number; isActive?: boolean } }) =>
      budgetClassificationsClient.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}

export function useToggleClassificationActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: { id: number; rowVersion: string; isActive: boolean } }) =>
      budgetClassificationsClient.toggleActive(id, { id, rowVersion: data.rowVersion, isActive: data.isActive }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}
