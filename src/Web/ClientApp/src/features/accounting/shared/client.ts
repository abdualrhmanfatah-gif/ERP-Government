import { JournalEntriesClient, JournalsClient, TemplatesClient, AccountGroupsClient } from '../../../web-api-client';

export const journalEntriesClient = new JournalEntriesClient();
export const journalsClient = new JournalsClient();
export const templatesClient = new TemplatesClient();
export const accountGroupsClient = new AccountGroupsClient();

export interface JournalEntryListFilters {
  entryStatus?: string;
  journalId?: number;
  fromDate?: string;
  toDate?: string;
}
