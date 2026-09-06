import { JournalEntriesClient } from '../../../web-api-client';

export const journalEntriesClient = new JournalEntriesClient();

export interface JournalEntryListFilters {
  entryStatus?: string;
  journalId?: number;
  fromDate?: string;
  toDate?: string;
}
