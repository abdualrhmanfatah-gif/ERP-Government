import type { AccountDto, AccountGroupDto } from '../../web-api-client';

export type { AccountDto, AccountGroupDto };

export interface AccountTreeNode extends AccountDto {
  children: AccountTreeNode[];
}

// ─── EntryStatus Enum ──────────────────────────────────────────────────
export enum EntryStatus {
  Draft = "Draft",
  Submitted = "Submitted",
  Approved = "Approved",
  Posted = "Posted",
  Reversed = "Reversed",
  Cancelled = "Cancelled",
}

// ─── MoveEntryType Enum ────────────────────────────────────────────────
export enum MoveEntryType {
  Standard = "Standard",
  Reversing = "Reversing",
  Adjusting = "Adjusting",
  Closing = "Closing",
  Opening = "Opening",
  SystemGenerated = "SystemGenerated",
}

// ─── JournalEntry DTOs ─────────────────────────────────────────────────
export interface JournalEntryDto {
  id: number;
  entryNumber: string;
  ref?: string | null;
  documentDate: string;
  postingDate?: string | null;
  entryType?: MoveEntryType | null;
  entryStatus: EntryStatus;
  journalId?: number | null;
  journalName?: string | null;
  periodId: number;
  periodName?: string;
  fiscalYearId: number;
  fiscalYearName?: string;
  narration?: string | null;
  reversalOfId?: number | null;
  reversalReason?: string | null;
  postedById?: number | null;
  postedByName?: string | null;
  postedAt?: string | null;
  cancelledById?: number | null;
  cancelledByName?: string | null;
  cancelledAt?: string | null;
  isSystemGenerated: boolean;
  totalDebit: number;
  totalCredit: number;
  baseCurrencyId?: number;
  totalBaseDebit?: number;
  totalBaseCredit?: number;
  rowVersion: string;
  lines: JournalEntryLineDto[];
}

export interface JournalEntryLineDto {
  id: number;
  journalEntryId: number;
  sequence: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  description?: string | null;
  currencyId: number;
  currencyCode?: string;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
  costCenterName?: string | null;
  fundId?: number | null;
  fundName?: string | null;
  projectId?: number | null;
  projectName?: string | null;
  budgetItemId?: number | null;
  budgetItemCode?: string | null;
  encumbranceId?: number | null;
  encumbranceNumber?: string | null;
  paymentOrderId?: number | null;
  paymentOrderNumber?: string | null;
  baseDebit?: number;
  baseCredit?: number;
  resolvedRate?: number;
  resolvedRateDate?: string;
  rowVersion: string;
}

// ─── Command Types ─────────────────────────────────────────────────────
export interface CreateJournalEntryCommand {
  ref?: string | null;
  documentDate: string;
  entryType?: MoveEntryType | null;
  journalId?: number | null;
  periodId: number;
  fiscalYearId: number;
  baseCurrencyId?: number;
  narration?: string | null;
}

export interface UpdateJournalEntryCommand {
  id: number;
  narration?: string | null;
  ref?: string | null;
  rowVersion: string;
}

export interface CreateJournalEntryLineCommand {
  accountId: number;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
  fundId?: number | null;
  projectId?: number | null;
  budgetItemId?: number | null;
  encumbranceId?: number | null;
  paymentOrderId?: number | null;
}

export interface UpdateJournalEntryLineCommand {
  id: number;
  journalEntryId: number;
  accountId: number;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
  fundId?: number | null;
  projectId?: number | null;
  budgetItemId?: number | null;
  encumbranceId?: number | null;
  paymentOrderId?: number | null;
  rowVersion: string;
}

// ─── Lifecycle Commands ────────────────────────────────────────────────
export interface SubmitJournalEntryCommand {
  reason?: string | null;
}

export interface ApproveJournalEntryCommand {
  reason?: string | null;
}

export interface PostJournalEntryCommand {
  reason?: string | null;
}

export interface ReverseJournalEntryCommand {
  reason: string;
}

export interface CancelJournalEntryCommand {
  reason?: string | null;
}
