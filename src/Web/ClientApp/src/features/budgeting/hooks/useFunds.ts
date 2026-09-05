import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ActivateFundCommand,
  CreateFundCommand,
  DeactivateFundCommand,
  FundsClient,
  UpdateFundCommand,
} from '../../../web-api-client';

const client = new FundsClient();

export function useFundsList(isActive?: boolean) {
  return useQuery({
    queryKey: ['funds', isActive],
    queryFn: () => client.fundsAll(isActive),
  });
}

export function useFundDetail(id: number) {
  return useQuery({
    queryKey: ['funds', id],
    queryFn: () => client.fundsGET(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.fundsPOST(CreateFundCommand.fromJS(data)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useUpdateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.fundsPUT(id, UpdateFundCommand.fromJS({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useActivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.activate9(id, ActivateFundCommand.fromJS({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useDeactivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.deactivatePOST8(id, DeactivateFundCommand.fromJS({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}
