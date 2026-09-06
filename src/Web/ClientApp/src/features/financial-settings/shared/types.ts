// Financial Settings Shared Types — DTOs, enums, Arabic label maps, filter/command types

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum FiscalYearStatus {
  Draft = 0,
  Open = 1,
  SoftClosed = 2,
  HardClosed = 3,
}

export enum ExchangeRateType {
  Official = 0,
  Market = 1,
}

export enum ClosingEntryStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Posted = 3,
  Cancelled = 4,
}

export enum ResetPolicy {
  Yearly = 0,
  Never = 1,
}

// ─── Arabic Label Maps ───────────────────────────────────────────────────────

export const fiscalYearStatusLabels: Record<FiscalYearStatus, string> = {
  [FiscalYearStatus.Draft]: 'مسودة',
  [FiscalYearStatus.Open]: 'مفتوح',
  [FiscalYearStatus.SoftClosed]: 'مغلق ( مؤقت )',
  [FiscalYearStatus.HardClosed]: 'مغلق',
};

export const fiscalPeriodLockLabels: Record<string, string> = {
  locked: 'مقفل للتقيد',
  unlocked: 'غير مقفل',
};

export const exchangeRateTypeLabels: Record<ExchangeRateType, string> = {
  [ExchangeRateType.Official]: 'رسمي',
  [ExchangeRateType.Market]: 'سوق',
};

export const closingEntryStatusLabels: Record<ClosingEntryStatus, string> = {
  [ClosingEntryStatus.Draft]: 'مسودة',
  [ClosingEntryStatus.PendingApproval]: 'بانتظار الاعتماد',
  [ClosingEntryStatus.Approved]: 'معتمد',
  [ClosingEntryStatus.Posted]: 'مقيّد',
  [ClosingEntryStatus.Cancelled]: 'ملغي',
};

export const resetPolicyLabels: Record<ResetPolicy, string> = {
  [ResetPolicy.Yearly]: 'سنوي',
  [ResetPolicy.Never]: 'أبداً',
};

// Label lookup helper with fallback
export function getLabel<T extends number>(labels: Record<T, string>, value: T): string {
  return labels[value] ?? String(value);
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

export interface FiscalYearDto {
  id: number;
  name: string;
  yearNumber: number;
  startDate: string;
  endDate: string;
  status: string;
  isClosed: boolean;
  closingJournalEntryId?: number;
  isActive: boolean;
  rowVersion: string;
  createdBy?: string;
  createdAt?: string;
  modifiedBy?: string;
  modifiedAt?: string;
}

export interface FiscalPeriodDto {
  id: number;
  fiscalYearId: number;
  periodNumber: number;
  name: string;
  startDate: string;
  endDate: string;
  isLockedForPosting: boolean;
  isActive: boolean;
  rowVersion: string;
}

export interface DocumentSequenceDto {
  id: number;
  name: string;
  documentType: string;
  fiscalYearId?: number;
  currentNumber: number;
  resetPolicy: string;
  isActive: boolean;
  rowVersion: string;
}

export interface CurrencyDto {
  id: number;
  code: string;
  name: string;
  symbol: string;
  decimalPlaces: number;
  roundingPrecision: number;
  isBase: boolean;
  isActive: boolean;
  rowVersion: string;
  createdAt?: string;
  createdBy?: string;
  modifiedAt?: string;
  modifiedBy?: string;
}

export interface Iso4217CodeDto {
  code: string;
  name: string;
  decimalPlaces: number;
}

export interface ExchangeRateDto {
  id: number;
  baseCurrencyId: number;
  baseCurrencyCode: string;
  currencyId: number;
  currencyCode: string;
  rateDate: string;
  rateType: string;
  rate: number;
  isActive: boolean;
  rowVersion: string;
}

export interface ExchangeRateLookupDto {
  rate: number;
  rateDate: string;
  rateType: string;
}

export interface ClosingEntryDto {
  id: number;
  closingEntryNumber: string;
  fiscalYearId: number;
  fiscalYearName: string;
  closingDate: string;
  description?: string;
  status: string;
  isReversal: boolean;
  reversalOfId?: number;
  reversalOfNumber?: string;
  journalEntryId?: number;
  journalEntryEntryNumber?: string;
  approvedById?: string;
  isActive: boolean;
  rowVersion: string;
}

// ─── Command Types ───────────────────────────────────────────────────────────

export interface CreateFiscalYearCommand {
  name: string;
  startDate: string;
  endDate: string;
}

export interface UpdateFiscalYearCommand {
  id: number;
  rowVersion: string;
  name: string;
  startDate: string;
  endDate: string;
}

export interface CreateFiscalPeriodCommand {
  fiscalYearId: number;
  periodNumber: number;
  name: string;
  startDate: string;
  endDate: string;
}

export interface BulkGeneratePeriodsCommand {
  fiscalYearId: number;
}

export interface CreateDocumentSequenceCommand {
  name: string;
  documentType: string;
  fiscalYearId?: number;
  resetPolicy: string;
}

export interface UpdateDocumentSequenceCommand {
  id: number;
  rowVersion: string;
  name?: string;
  fiscalYearId?: number;
  resetPolicy?: string;
  isActive?: boolean;
}

export interface CreateCurrencyCommand {
  code: string;
  name: string;
  symbol: string;
  decimalPlaces: number;
  roundingPrecision: number;
  isBase: boolean;
}

export interface UpdateCurrencyCommand {
  id: number;
  rowVersion: string;
  name: string;
  symbol: string;
  decimalPlaces: number;
  roundingPrecision: number;
  isBase: boolean;
}

export interface CreateExchangeRateCommand {
  baseCurrencyId: number;
  currencyId: number;
  rateDate: string;
  rateType: ExchangeRateType;
  rate: number;
}

export interface UpdateExchangeRateCommand {
  id: number;
  rowVersion: string;
  rateDate: string;
  rateType: ExchangeRateType;
  rate: number;
}

export interface GenerateYearEndClosingCommand {
  fiscalYearId: number;
  description?: string;
}

export interface ReverseClosingEntryCommand {
  id: number;
  rowVersion: string;
  reason: string;
}

export interface ActivateDeactivateCommand {
  id: number;
  rowVersion: string;
}

// ─── Problem Details (Error Contract) ─────────────────────────────────────────

export interface ProblemDetails {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// ─── Filter Types ────────────────────────────────────────────────────────────

export interface FiscalYearFilters {
  search?: string;
  status?: FiscalYearStatus | null;
  isActive?: boolean;
}

export interface ExchangeRateFilters {
  search?: string;
  currencyId?: number | null;
  rateType?: ExchangeRateType | null;
  fromDate?: string;
  toDate?: string;
  isActive?: boolean;
}

export interface ClosingEntryFilters {
  fiscalYearId?: number | null;
  status?: ClosingEntryStatus | null;
}
