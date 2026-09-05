import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ActivateEncumbranceCommand,
  ApproveEncumbranceCommand,
  CancelEncumbranceCommand,
  CloseEncumbranceCommand,
  CreateEncumbranceCommand,
  EncumbrancesClient,
  EncumbranceStatus,
  ReverseEncumbranceCommand,
  SubmitEncumbranceCommand,
} from '../../../web-api-client';

const client = new EncumbrancesClient();

export function useEncumbrancesList(filters?: {
  appropriationId?: number;
  status?: EncumbranceStatus | null;
}) {
  return useQuery({
    queryKey: ['encumbrances', filters],
    queryFn: () =>
      client.encumbrancesAll(filters?.appropriationId, filters?.status ?? null),
  });
}

export function useEncumbranceDetail(id: number) {
  return useQuery({
    queryKey: ['encumbrances', id],
    queryFn: () => client.encumbrancesGET(id),
    enabled: Number.isFinite(id),
  });
}

export function useEncumbranceAvailability(appropriationId: number | undefined) {
  return useQuery({
    queryKey: ['encumbrance-availability', appropriationId],
    queryFn: () => client.availability2(appropriationId!),
    enabled: Number.isFinite(appropriationId),
  });
}

function useEncumbranceTransition(
  fn: (id: number, body: never) => Promise<unknown>,
) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      fn(id, data as never),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['encumbrances'] });
      qc.invalidateQueries({ queryKey: ['encumbrance-availability'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export function useCreateEncumbrance() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) =>
      client.encumbrancesPOST(CreateEncumbranceCommand.fromJS(data)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['encumbrances'] });
      qc.invalidateQueries({ queryKey: ['encumbrance-availability'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export const useSubmitEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.submit4(id, SubmitEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useApproveEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.approve8(id, ApproveEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useActivateEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.activate8(id, ActivateEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCloseEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.close4(id, CloseEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useCancelEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.cancel8(id, CancelEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
export const useReverseEncumbrance = () =>
  useEncumbranceTransition((id, body) =>
    client.reverse2(id, ReverseEncumbranceCommand.fromJS(body as unknown as Record<string, unknown>)));
