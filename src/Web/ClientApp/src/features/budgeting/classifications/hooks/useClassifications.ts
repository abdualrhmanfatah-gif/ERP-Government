import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { budgetClassificationsClient, budgetingKeys } from '../../shared/client';

export function useClassificationsTree() {
  return useQuery({
    queryKey: budgetingKeys.budgetClassifications.tree(),
    queryFn: () => budgetClassificationsClient.tree(),
  });
}

export function useCreateClassification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { code: string; name: string; parentId?: number }) =>
      budgetClassificationsClient.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}

export function useUpdateClassification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, code, name, parentId }: { id: number; rowVersion: string; code: string; name: string; parentId?: number }) =>
      budgetClassificationsClient.update(id, { id, rowVersion, code, name, parentId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}

export function useToggleClassificationActive() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, isActive }: { id: number; rowVersion: string; isActive: boolean }) =>
      budgetClassificationsClient.toggleActive(id, { id, rowVersion, isActive }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: budgetingKeys.budgetClassifications.all });
    },
  });
}
