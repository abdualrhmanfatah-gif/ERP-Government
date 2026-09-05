export interface ReportLine {
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface ReportSection {
  title: string;
  titleEn: string;
  lines: ReportLine[];
  total: number;
}

export interface BalanceSheetGroup {
  sections: ReportSection[];
  total: number;
}

export interface BalanceSheetDto {
  asOfDate: string;
  currency: string;
  assets: BalanceSheetGroup;
  liabilities: BalanceSheetGroup;
  equity: BalanceSheetGroup;
  liabilitiesAndEquity: number;
  balanced: boolean;
  generatedAt: string;
}

export interface IncomeStatementGroup {
  sections: ReportSection[];
  total: number;
}

export interface IncomeStatementDto {
  startDate: string;
  endDate: string;
  currency: string;
  revenue: IncomeStatementGroup;
  expenses: IncomeStatementGroup;
  netIncome: number;
  generatedAt: string;
}

export interface GeneralLedgerLine {
  documentDate: string;
  entryNumber: string;
  reference: string;
  narration: string;
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface GeneralLedgerTotals {
  debit: number;
  credit: number;
}

export interface GeneralLedgerDto {
  currency: string;
  totalLines: number;
  page: number;
  pageSize: number;
  lines: GeneralLedgerLine[];
  totals: GeneralLedgerTotals;
  generatedAt: string;
}

export interface CashFlowLineItem {
  description: string;
  amount: number;
}

export interface CashFlowSection {
  title: string;
  titleAr: string;
  items: CashFlowLineItem[];
  total: number;
}

export interface CashFlowStatementDto {
  startDate: string;
  endDate: string;
  currency: string;
  operating: CashFlowSection;
  investing: CashFlowSection;
  financing: CashFlowSection;
  netChange: number;
  openingCash: number;
  closingCash: number;
  reconciled: boolean;
  warning: string | null;
  generatedAt: string;
}

export interface TrialBalanceDto {
  fiscalYearId: number;
  fiscalYearName: string;
  fiscalPeriodId: number;
  periodName: string;
  sections: ReportSection[];
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
  currency: string;
  generatedAt: string;
}

export interface TrialBalanceQuery {
  fiscalYearId: number;
  fiscalPeriodId: number;
}
