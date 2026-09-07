import { JournalEntriesClient, JournalsClient, TemplatesClient } from '../../../web-api-client';

export const journalEntriesClient = new JournalEntriesClient();
export const journalsClient = new JournalsClient();
export const templatesClient = new TemplatesClient();

export interface JournalEntryListFilters {
  entryStatus?: string;
  journalId?: number;
  fromDate?: string;
  toDate?: string;
}
