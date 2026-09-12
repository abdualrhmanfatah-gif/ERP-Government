import { RecurringEntriesClient } from '../../../../web-api-client';

export const recurringEntriesClient = new RecurringEntriesClient();

export interface RecurringEntryListFilters {
  isActive?: boolean;
  journalId?: number;
  frequency?: string;
  status?: string;
}
