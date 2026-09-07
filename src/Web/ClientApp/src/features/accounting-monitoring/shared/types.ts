export interface AccountBalanceDto {
  id: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  fiscalYearId: number;
  fiscalYearName: string;
  fiscalPeriodId: number;
  periodName: string;
  periodStartDate: string;
  periodEndDate: string;
  currencyId: number;
  currencyCode: string;
  openingDebit: number;
  openingCredit: number;
  debit: number;
  credit: number;
  closingDebit: number;
  closingCredit: number;
  balanceDirection: string;
  isFinalized: boolean;
  finalizedAt: string | null;
}

export interface AccountingEventDto {
  id: number;
  eventType: string;
  sourceTable: string;
  sourceId: number;
  status: string;
  journalEntryId: number | null;
  errorMessage: string | null;
  processedAt: string | null;
  retryCount: number;
}

export interface PostingRuleDto {
  id: number;
  name: string;
  eventType: string;
  journalId: number;
  journalName: string;
  priority: number;
  isActive: boolean;
  lines: PostingRuleLineDto[];
}

export interface PostingRuleLineDto {
  id: number;
  postingRuleId: number;
  sequence: number;
  accountSource: string;
  fixedAccountId: number | null;
  fixedAccountCode: string | null;
  debitOrCredit: string;
  amountSource: string;
  fundDimensionRequired: boolean;
  costCenterDimensionRequired: boolean;
  projectDimensionRequired: boolean;
  isActive: boolean;
}
