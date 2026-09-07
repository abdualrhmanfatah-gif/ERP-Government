import { useMutation, useQueryClient } from '@tanstack/react-query';
import { TemplatesClient, CreateTemplateCommand } from '../../../web-api-client';

const client = new TemplatesClient();

export function useCreateTemplate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) => {
      const cmd = CreateTemplateCommand.fromJS(data);
      return client.templatesPOST(cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['templates'] }),
  });
}
