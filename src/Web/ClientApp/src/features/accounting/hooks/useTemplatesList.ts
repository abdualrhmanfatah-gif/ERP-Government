import { useQuery } from '@tanstack/react-query';
import { TemplatesClient, JournalEntryTemplateType } from '../../../web-api-client';

const client = new TemplatesClient();

export function useTemplatesList(filters?: {
  isActive?: boolean;
  journalId?: number;
  templateType?: JournalEntryTemplateType;
}) {
  return useQuery({
    queryKey: ['templates', filters],
    queryFn: () =>
      client.templatesAll(
        filters?.isActive,
        filters?.journalId,
        filters?.templateType ?? null,
      ),
  });
}
