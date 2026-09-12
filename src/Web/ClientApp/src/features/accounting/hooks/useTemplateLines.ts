import { useMutation, useQueryClient } from '@tanstack/react-query';
import { templatesClient } from '../shared/client';
import type { CreateTemplateLineCommand, UpdateTemplateLineCommand } from '../../../web-api-client';

export function useCreateTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, payload }: { templateId: number; payload: CreateTemplateLineCommand }) =>
      templatesClient.linesPOST4(templateId, payload),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}

export function useUpdateTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, lineId, payload }: { templateId: number; lineId: number; payload: UpdateTemplateLineCommand }) =>
      templatesClient.linesPUT2(templateId, lineId, payload),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}

export function useRemoveTemplateLine() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ templateId, lineId }: { templateId: number; lineId: number }) =>
      templatesClient.linesDELETE2(templateId, lineId),
    onSuccess: (_d, v) => {
      queryClient.invalidateQueries({ queryKey: ['template', v.templateId] });
    },
  });
}
