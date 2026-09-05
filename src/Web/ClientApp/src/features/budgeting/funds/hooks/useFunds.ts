import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { fundsClient, budgetingKeys } from '../../shared/client';
import type { CreateFundCommand, UpdateFundCommand } from '../../shared/types';

export function useFundsList(isActive?: boolean) {
  return useQuery({
    queryKey: budgetingKeys.funds.list({ isActive }),
    queryFn: () => fundsClient.list(isActive),
  });
}

export function useFundDetail(id: number) {
  return useQuery({
    queryKey: budgetingKeys.funds.detail(id),
    queryFn: () => fundsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateFundCommand) => fundsClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.funds.all }),
  });
}

export function useUpdateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: UpdateFundCommand) => fundsClient.update(id, { id, ...data }),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.funds.all }),
  });
}

export function useActivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      fundsClient.activate(id, rowVersion),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.funds.all }),
  });
}

export function useDeactivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      fundsClient.deactivate(id, rowVersion),
    onSuccess: () => qc.invalidateQueries({ queryKey: budgetingKeys.funds.all }),
  });
}
