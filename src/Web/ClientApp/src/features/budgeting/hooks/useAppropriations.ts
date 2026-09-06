import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  AppropriationsClient,
  AppropriationStatus,
  AppropriationType,
  CreateAppropriationRequest,
  UpdateAppropriationRequest,
  AppropriationActionRequest,
  CreateTransferRequest,
} from '../../../web-api-client';

const client = new AppropriationsClient();

export function useAppropriationsList(filters?: {
  budgetId?: number;
  budgetItemId?: number;
  status?: AppropriationStatus | null;
  appropriationType?: AppropriationType | null;
}) {
  return useQuery({
    queryKey: ['appropriations', filters],
    queryFn: () =>
      client.appropriationsAll(
        filters?.budgetId ?? undefined,
        filters?.budgetItemId ?? undefined,
        filters?.status ?? undefined,
        filters?.appropriationType ?? undefined,
      ),
  });
}

export function useAppropriationDetail(id: number) {
  return useQuery({
    queryKey: ['appropriations', id],
    queryFn: () => client.appropriationsGET(id),
    enabled: Number.isFinite(id),
  });
}

export function useItemAvailability(budgetItemId: number | undefined) {
  return useQuery({
    queryKey: ['item-availability', budgetItemId],
    queryFn: () => client.availability(budgetItemId!),
    enabled: Number.isFinite(budgetItemId),
  });
}

function useAppropriationTransition(fn: (id: number, body: AppropriationActionRequest) => Promise<void>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      fn(id, new AppropriationActionRequest({ notes: data.notes as string | undefined })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useCreateAppropriation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.appropriationsPOST(new CreateAppropriationRequest(data as any)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useUpdateAppropriation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.appropriationsPUT(id, new UpdateAppropriationRequest({ id, ...data } as any)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useDeleteAppropriation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id }: { id: number }) =>
      client.appropriationsDELETE(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export const useSubmitAppropriation = () =>
  useAppropriationTransition((id, body) => client.submitPATCH(id, body));
export const useApproveAppropriation = () =>
  useAppropriationTransition((id, body) => client.approvePATCH(id, body));
export const useActivateAppropriation = () =>
  useAppropriationTransition((id, body) => client.activatePATCH(id, body));
export const useSuspendAppropriation = () =>
  useAppropriationTransition((id, body) => client.suspend(id, body));
export const useCloseAppropriation = () =>
  useAppropriationTransition((id, body) => client.closePATCH(id, body));
export const useCancelAppropriation = () =>
  useAppropriationTransition((id, body) => client.cancelPATCH(id, body));

export function useReverseAppropriation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.reversePATCH(id, new AppropriationActionRequest({ rowVersion: data.rowVersion as string })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useCreateTransfer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.transfers(new CreateTransferRequest(data as any)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}
