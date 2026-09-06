import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  FundsClient,
  CreateFundRequest,
  UpdateFundRequest,
  FundToggleActiveRequest,
} from '../../../web-api-client';

const client = new FundsClient();

export function useFundsList() {
  return useQuery({
    queryKey: ['funds'],
    queryFn: () => client.fundsAll(),
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
      client.fundsPOST(new CreateFundRequest(data)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useUpdateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.fundsPUT(id, new UpdateFundRequest({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useActivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.toggleActivePATCH3(id, new FundToggleActiveRequest({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}

export function useDeactivateFund() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.toggleActivePATCH3(id, new FundToggleActiveRequest({ id, ...data })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['funds'] }),
  });
}
