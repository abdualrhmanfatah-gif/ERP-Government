import type { AccountDto, AccountGroupDto } from '../../web-api-client';

export type { AccountDto, AccountGroupDto };

export interface AccountTreeNode extends AccountDto {
  children: AccountTreeNode[];
}

// ─── Moves (FEATURE-026) ────────────────────────────────────────────────
export interface MoveDto {
  id: number;
  entryNumber: string;
  ref?: string | null;
  documentDate: string;
  postingDate?: string | null;
  entryType?: number | null;
  entryStatus: string;
  journalId?: number | null;
  journalName?: string | null;
  periodId: number;
  fiscalYearId: number;
  narration?: string | null;
  reversalOfMoveId?: number | null;
  reversalReason?: string | null;
  postedById?: number | null;
  postedByName?: string | null;
  postedAt?: string | null;
  cancelledById?: number | null;
  cancelledByName?: string | null;
  cancelledAt?: string | null;
  isSystemGenerated: boolean;
  rowVersion: string;
  lines: MoveLineDto[];
}

export interface MoveLineDto {
  id: number;
  moveId: number;
  sequence: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
  costCenterName?: string | null;
  rowVersion: string;
}

export interface CreateMoveCommand {
  ref?: string | null;
  documentDate: string;
  entryType?: number | null;
  journalId?: number | null;
  periodId: number;
  fiscalYearId: number;
  narration?: string | null;
}

export interface UpdateMoveCommand {
  id: number;
  narration?: string | null;
  ref?: string | null;
  rowVersion: string;
}

export interface CreateMoveLineCommand {
  accountId: number;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
}

export interface UpdateMoveLineCommand {
  id: number;
  moveId: number;
  accountId: number;
  description?: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId?: number | null;
  rowVersion: string;
}
