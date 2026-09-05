import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ActivateBudgetCommand,
  ApproveBudgetCommand,
  BudgetsClient,
  BudgetStatus,
  CancelBudgetCommand,
  CloseBudgetCommand,
  CreateBudgetCommand,
  SubmitBudgetCommand,
  SuspendBudgetCommand,
  UpdateBudgetCommand,
} from '../../../web-api-client';

const client = new BudgetsClient();

export function useBudgetsList(filters?: {
  fiscalYearId?: number;
  fundId?: number;
  status?: BudgetStatus | null;
}) {
  return useQuery({
    queryKey: ['budgets', filters],
    queryFn: () =>
      client.budgetsAll(filters?.fiscalYearId, filters?.fundId, filters?.status ?? null),
  });
}

export function useBudgetDetail(id: number) {
  return useQuery({
    queryKey: ['budgets', id],
    queryFn: () => client.budgetsGET(id),
    enabled: Number.isFinite(id),
  });
}

function useBudgetTransition(
  fn: (id: number, body: never) => Promise<void>,
) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      fn(id, data as never),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export function useCreateBudget() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.budgetsPOST(CreateBudgetCommand.fromJS(data)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export function useUpdateBudget() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.budgetsPUT(id, UpdateBudgetCommand.fromJS({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export const useSubmitBudget = () =>
  useBudgetTransition((id, body) =>
    client.submit3(id, SubmitBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useApproveBudget = () =>
  useBudgetTransition((id, body) =>
    client.approve7(id, ApproveBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useActivateBudget = () =>
  useBudgetTransition((id, body) =>
    client.activate7(id, ActivateBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useSuspendBudget = () =>
  useBudgetTransition((id, body) =>
    client.suspend2(id, SuspendBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCloseBudget = () =>
  useBudgetTransition((id, body) =>
    client.close3(id, CloseBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCancelBudget = () =>
  useBudgetTransition((id, body) =>
    client.cancel7(id, CancelBudgetCommand.fromJS(body as unknown as Record<string, unknown>)));
