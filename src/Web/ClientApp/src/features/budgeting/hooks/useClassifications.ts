import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetClassificationsClient,
  CreateBudgetClassificationCommand,
  ToggleBudgetClassificationActiveCommand,
  UpdateBudgetClassificationCommand,
} from '../../../web-api-client';

const client = new BudgetClassificationsClient();

export function useClassificationsTree(isActive?: boolean) {
  return useQuery({
    queryKey: ['budget-classifications-tree', isActive],
    queryFn: () => client.tree(isActive),
  });
}

export function useClassificationDetail(id: number) {
  return useQuery({
    queryKey: ['budget-classifications', id],
    queryFn: () => client.budgetClassificationsGET(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateClassification() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.budgetClassificationsPOST(CreateBudgetClassificationCommand.fromJS(data)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budget-classifications-tree'] });
      qc.invalidateQueries({ queryKey: ['budget-classifications'] });
    },
  });
}

export function useUpdateClassification() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.budgetClassificationsPUT(id, UpdateBudgetClassificationCommand.fromJS({ id, ...data })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budget-classifications-tree'] });
      qc.invalidateQueries({ queryKey: ['budget-classifications'] });
    },
  });
}

export function useToggleClassificationActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.toggleActive(id, ToggleBudgetClassificationActiveCommand.fromJS({ id, ...data })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['budget-classifications-tree'] });
      qc.invalidateQueries({ queryKey: ['budget-classifications'] });
    },
  });
}
