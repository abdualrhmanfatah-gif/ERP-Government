import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { paymentsClient } from '../shared/client';

export function usePaymentsList(filters?: { status?: string; method?: string; from?: string; to?: string }) {
  return useQuery({
    queryKey: ['payments', filters],
    queryFn: () => paymentsClient.list(filters),
  });
}

export function usePaymentDetail(id: number) {
  return useQuery({
    queryKey: ['payments', id],
    queryFn: () => paymentsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useRecordPayment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: any) => paymentsClient.record(cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['payments'] }),
  });
}
