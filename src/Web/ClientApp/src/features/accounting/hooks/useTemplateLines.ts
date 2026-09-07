import { useMutation, useQueryClient } from '@tanstack/react-query';

export interface CreateTemplateLinePayload {
  accountId: number;
  description?: string;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
}

export interface UpdateTemplateLinePayload extends CreateTemplateLinePayload {
  rowVersion: string;
}

async function postLine(templateId: number, payload: CreateTemplateLinePayload): Promise<{ lineId: number }> {
  const res = await fetch(`/api/Templates/${templateId}/lines`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ templateId, ...payload }),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

async function putLine(templateId: number, lineId: number, payload: UpdateTemplateLinePayload): Promise<void> {
  const res = await fetch(`/api/Templates/${templateId}/lines/${lineId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id: lineId, templateId, ...payload }),
  });
  if (!res.ok) throw new Error(await res.text());
}

async function deleteLine(templateId: number, lineId: number): Promise<void> {
  const res = await fetch(`/api/Templates/${templateId}/lines/${lineId}`, { method: 'DELETE' });
  if (!res.ok) throw new Error(await res.text());
}

export function useCreateTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, payload }: { templateId: number; payload: CreateTemplateLinePayload }) =>
      postLine(templateId, payload),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}

export function useUpdateTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, lineId, payload }: { templateId: number; lineId: number; payload: UpdateTemplateLinePayload }) =>
      putLine(templateId, lineId, payload),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}

export function useRemoveTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, lineId }: { templateId: number; lineId: number }) =>
      deleteLine(templateId, lineId),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}
