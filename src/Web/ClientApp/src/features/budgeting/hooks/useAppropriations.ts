import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ActivateAppropriationCommand,
  AppropriationsClient,
  ApproveAppropriationCommand,
  AppropriationStatus,
  CancelAppropriationCommand,
  CloseAppropriationCommand,
  CreateAppropriationCommand,
  DeleteAppropriationCommand,
  SubmitAppropriationCommand,
  SuspendAppropriationCommand,
  UpdateAppropriationCommand,
} from '../../../web-api-client';

const client = new AppropriationsClient();

export function useAppropriationsList(filters?: {
  budgetId?: number;
  budgetItemId?: number;
  status?: AppropriationStatus | null;
}) {
  return useQuery({
    queryKey: ['appropriations', filters],
    queryFn: () =>
      client.appropriationsAll(
        filters?.budgetId,
        filters?.budgetItemId,
        filters?.status ?? null,
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

function useAppropriationTransition(
  fn: (id: number, body: never) => Promise<void>,
) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      fn(id, data as never),
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
      client.appropriationsPOST(CreateAppropriationCommand.fromJS(data)),
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
      client.appropriationsPUT(id, UpdateAppropriationCommand.fromJS({ id, ...data })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useDeleteAppropriation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.appropriationsDELETE(id, DeleteAppropriationCommand.fromJS({ id, ...data })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['appropriations'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export const useSubmitAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.submit2(id, SubmitAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useApproveAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.approve6(id, ApproveAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useActivateAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.activate6(id, ActivateAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useSuspendAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.suspend(id, SuspendAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCloseAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.close2(id, CloseAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCancelAppropriation = () =>
  useAppropriationTransition((id, body) =>
    client.cancel6(id, CancelAppropriationCommand.fromJS(body as unknown as Record<string, unknown>)));
