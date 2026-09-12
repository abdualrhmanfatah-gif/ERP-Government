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
  budgetId?: number;
  status?: EncumbranceStatus | null;
}) {
  return useQuery({
    queryKey: ['encumbrances', filters],
    queryFn: () =>
      client.encumbrancesAll(filters?.budgetId ?? undefined, filters?.status ?? undefined),
  });
}

export function useEncumbranceDetail(id: number) {
  return useQuery({
    queryKey: ['encumbrances', id],
    queryFn: () => client.encumbrancesGET(id),
    enabled: Number.isFinite(id),
  });
}

function useEncumbranceTransition(fn: (id: number, body: EncumbranceActionRequest) => Promise<void>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion, ...data }: Record<string, unknown> & { id: number; rowVersion: string }) =>
      fn(id, new EncumbranceActionRequest({ rowVersion, notes: data.notes as string | undefined })),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['encumbrances'] });
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
    },
  });
};
