import { useMutation, useQueryClient } from '@tanstack/react-query';
import { TemplatesClient, UpdateTemplateCommand } from '../../../web-api-client';

const client = new TemplatesClient();

export function useUpdateTemplate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: {
      id: number;
      data: {
        id?: number;
        templateName?: string;
        description?: string;
        journalId?: number;
        templateType?: string;
        rowVersion?: string;
      };
    }) => {
      const cmd = UpdateTemplateCommand.fromJS(data);
      return client.templatesPUT(id, cmd);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['templates'] });
      qc.invalidateQueries({ queryKey: ['template'] });
    },
  });
}
