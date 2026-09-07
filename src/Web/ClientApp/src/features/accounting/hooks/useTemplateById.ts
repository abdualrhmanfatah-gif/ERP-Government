import { useQuery } from '@tanstack/react-query';
import { TemplatesClient } from '../../../web-api-client';

const client = new TemplatesClient();

export function useTemplateById(id: number | null) {
  return useQuery({
    queryKey: ['template', id],
    queryFn: () => client.templatesGET(id!),
    enabled: id !== null && id > 0,
  });
}
