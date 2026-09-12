import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { disbursementRequestsClient, partiesForBeneficiaryClient } from '../shared/client';
import type { PartyFilters } from '@/features/parties/shared/types';

export function useDisbursementRequestsList(filters?: { status?: number; requestedById?: number }) {
  return useQuery({
    queryKey: ['disbursement-requests', filters],
    queryFn: () => disbursementRequestsClient.list(filters),
  });
}

export function useDisbursementRequestDetail(id: number) {
  return useQuery({
    queryKey: ['disbursement-requests', id],
    queryFn: () => disbursementRequestsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: any) => disbursementRequestsClient.create(cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['disbursement-requests'] }),
    onError: handleLifecycleError,
  });
}

export function useUpdateDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => disbursementRequestsClient.update(id, cmd),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id] });
    },
    onError: handleLifecycleError,
  });
}

export function useSubmitDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => disbursementRequestsClient.submit(id),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables] });
    },
    onError: handleLifecycleError,
  });
}

export function useApproveDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => disbursementRequestsClient.approve(id, cmd),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id] });
    },
    onError: handleLifecycleError,
  });
}

export function useRejectDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => disbursementRequestsClient.reject(id, cmd),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id] });
    },
    onError: handleLifecycleError,
  });
}

export function useCancelDisbursementRequest() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => disbursementRequestsClient.cancel(id, cmd),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id] });
    },
    onError: handleLifecycleError,
  });
}

export function useGetAccrualEntry(id: number) {
  return useQuery({
    queryKey: ['disbursement-requests', id, 'accrual-entry'],
    queryFn: () => disbursementRequestsClient.getAccrualEntry(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateAccrualEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => disbursementRequestsClient.createAccrualEntry(id, cmd),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['disbursement-requests'] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id] });
      qc.invalidateQueries({ queryKey: ['disbursement-requests', variables.id, 'accrual-entry'] });
    },
    onError: handleLifecycleError,
  });
}

export function usePartiesForBeneficiary(filters?: PartyFilters) {
  return useQuery({
    queryKey: ['parties', 'beneficiary', filters],
    queryFn: () => partiesForBeneficiaryClient.search({ ...filters, isActive: true }),
    staleTime: 30_000,
  });
}
