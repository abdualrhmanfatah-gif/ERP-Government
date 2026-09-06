import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  EncumbrancesClient,
  EncumbranceStatus,
  CreateEncumbranceRequest,
  EncumbranceActionRequest,
  ReverseEncumbranceRequest,
} from '../../../web-api-client';

const client = new EncumbrancesClient();

export function useEncumbrancesList(filters?: {
  appropriationId?: number;
  status?: EncumbranceStatus | null;
}) {
  return useQuery({
    queryKey: ['encumbrances', filters],
    queryFn: () =>
      client.encumbrancesAll(filters?.appropriationId ?? undefined, filters?.status ?? undefined),
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

function useEncumbranceTransition(fn: (id: number, body: EncumbranceActionRequest) => Promise<void>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      fn(id, new EncumbranceActionRequest({ notes: data.notes as string | undefined })),
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
      client.encumbrancesPOST(new CreateEncumbranceRequest(data as any)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['encumbrances'] });
      qc.invalidateQueries({ queryKey: ['encumbrance-availability'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
}

export const useSubmitEncumbrance = () =>
  useEncumbranceTransition((id, body) => client.submitPATCH3(id, body));
export const useApproveEncumbrance = () =>
  useEncumbranceTransition((id, body) => client.approvePATCH3(id, body));
export const useActivateEncumbrance = () =>
  useEncumbranceTransition((id, body) => client.activatePATCH3(id, body));
export const useCloseEncumbrance = () =>
  useEncumbranceTransition((id, body) => client.closePATCH3(id, body));
export const useCancelEncumbrance = () =>
  useEncumbranceTransition((id, body) => client.cancelPATCH3(id, body));
export const useReverseEncumbrance = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...data }: Record<string, unknown> & { id: number }) =>
      client.reversePATCH2(id, new ReverseEncumbranceRequest(data as any)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['encumbrances'] });
      qc.invalidateQueries({ queryKey: ['encumbrance-availability'] });
      qc.invalidateQueries({ queryKey: ['item-availability'] });
    },
  });
};
