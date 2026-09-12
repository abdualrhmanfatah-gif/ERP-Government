import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BudgetsClient,
  BudgetStatus,
  CreateBudgetRequest,
  UpdateBudgetRequest,
  BudgetActionRequest,
} from '../../../web-api-client';

const client = new BudgetsClient();

export function useBudgetsList(filters?: {
  fiscalYearId?: number;
  fundId?: number;
  budgetTypeId?: number;
  status?: BudgetStatus | null;
}) {
  return useQuery({
    queryKey: ['budgets', filters],
    queryFn: () =>
      client.budgetsAll(filters?.fiscalYearId ?? undefined, filters?.fundId ?? undefined, filters?.budgetTypeId ?? undefined, filters?.status ?? undefined),
  });
}

export function useBudgetDetail(id: number) {
  return useQuery({
    queryKey: ['budgets', id],
    queryFn: () => client.budgetsGET(id),
    enabled: Number.isFinite(id),
  });
}

function useBudgetTransition(fn: (id: number, body: BudgetActionRequest) => Promise<void>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, ...data }: Record<string, unknown> & { id: number; rowVersion: string }) =>
      fn(id, new BudgetActionRequest({ rowVersion, notes: data.notes as string | undefined })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export function useCreateBudget() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.budgetsPOST(new CreateBudgetRequest(data as any)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export function useUpdateBudget() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.budgetsPUT(id, new UpdateBudgetRequest({ id, ...data } as any)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['budgets'] }),
  });
}

export const useSubmitBudget = () =>
  useBudgetTransition((id, body) => client.submitPATCH(id, body));
export const useApproveBudget = () =>
  useBudgetTransition((id, body) => client.approvePATCH(id, body));
export const useActivateBudget = () =>
  useBudgetTransition((id, body) => client.activatePATCH(id, body));
export const useSuspendBudget = () =>
  useBudgetTransition((id, body) => client.suspend(id, body));
export const useCloseBudget = () =>
  useBudgetTransition((id, body) => client.closePATCH(id, body));
export const useCancelBudget = () =>
  useBudgetTransition((id, body) => client.cancelPATCH(id, body));
