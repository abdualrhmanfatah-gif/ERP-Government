// Budgeting Shared Types — Single source of truth for all budgeting frontend specs
// Matches backend DTOs from specs/046-budget-ledger-redesign

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum BudgetControlMethod {
  None = 0,
  Warning = 1,
  Blocking = 2,
}

export enum FundType {
  General = 0,
  Special = 1,
  Project = 2,
}

export enum FundCategory {
  Operating = 0,
  Capital = 1,
}

export enum BudgetStatus {
  Draft = 'Draft',
  Submitted = 'Submitted',
  Approved = 'Approved',
  Active = 'Active',
  Suspended = 'Suspended',
  Closed = 'Closed',
  Cancelled = 'Cancelled',
}

export enum EncumbranceType {
  Commitment = 'Commitment',
  Obligational = 'Obligational',
  Contractual = 'Contractual',
  Advance = 'Advance',
  Adjustment = 'Adjustment',
}

export enum EncumbranceStatus {
  Draft = 'Draft',
  PendingApproval = 'PendingApproval',
  Approved = 'Approved',
  Active = 'Active',
  PartiallyReleased = 'PartiallyReleased',
  PartiallyLiquidated = 'PartiallyLiquidated',
  FullyLiquidated = 'FullyLiquidated',
  Closed = 'Closed',
  Cancelled = 'Cancelled',
  Reversed = 'Reversed',
}

// ─── Arabic Label Maps ────────────────────────────────────────────────────────

export const budgetControlMethodLabels: Record<BudgetControlMethod, string> = {
  [BudgetControlMethod.None]: 'لا يوجد',
  [BudgetControlMethod.Warning]: 'تحذير',
  [BudgetControlMethod.Blocking]: 'حجب',
};

export const fundTypeLabels: Record<string, string> = {
  General: 'عام',
  Special: 'خاص',
  Project: 'مشروع',
};

export const fundCategoryLabels: Record<string, string> = {
  Operating: 'تشغيلي',
  Capital: 'رأسمالي',
};

export const budgetStatusLabels: Record<BudgetStatus, string> = {
  [BudgetStatus.Draft]: 'مسودة',
  [BudgetStatus.Submitted]: 'مرسل للمراجعة',
  [BudgetStatus.Approved]: 'معتمد',
  [BudgetStatus.Active]: 'نشط',
  [BudgetStatus.Suspended]: 'معلق',
  [BudgetStatus.Closed]: 'مغلق',
  [BudgetStatus.Cancelled]: 'ملغي',
};

export const encumbranceTypeLabels: Record<EncumbranceType, string> = {
  [EncumbranceType.Commitment]: 'التزام',
  [EncumbranceType.Obligational]: 'التزامي',
  [EncumbranceType.Contractual]: 'تعاقدي',
  [EncumbranceType.Advance]: 'سلفة',
  [EncumbranceType.Adjustment]: 'تعديل',
};

export const encumbranceStatusLabels: Record<EncumbranceStatus, string> = {
  [EncumbranceStatus.Draft]: 'مسودة',
  [EncumbranceStatus.PendingApproval]: 'بانتظار الاعتماد',
  [EncumbranceStatus.Approved]: 'معتمد',
  [EncumbranceStatus.Active]: 'نشط',
  [EncumbranceStatus.PartiallyReleased]: 'تحرير جزئي',
  [EncumbranceStatus.PartiallyLiquidated]: 'تسوية جزئية',
  [EncumbranceStatus.FullyLiquidated]: 'تسوية كاملة',
  [EncumbranceStatus.Closed]: 'مغلق',
  [EncumbranceStatus.Cancelled]: 'ملغي',
  [EncumbranceStatus.Reversed]: 'معكوس',
};

// Label lookup helpers with fallback for unknown enum values
export function getLabel<T extends number | string>(labels: Record<T, string>, value: T): string {
  return labels[value] ?? String(value);
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

export interface BudgetTypeDto {
  id: number;
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
  isActive: boolean;
  rowVersion: string;
}

export interface FundDto {
  id: number;
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
  isActive: boolean;
  rowVersion: string;
}

export interface BudgetClassificationDto {
  id: number;
  code: string;
  name: string;
  parentId?: number;
  level: number;
  isActive: boolean;
  rowVersion: string;
}

export interface BudgetClassificationTreeDto extends BudgetClassificationDto {
  children?: BudgetClassificationTreeDto[];
}

export interface BudgetDto {
  id: number;
  budgetNumber: string;
  budgetName: string;
  budgetTypeId: number;
  budgetTypeName: string;
  fiscalYearId: number;
  fundId: number;
  fundName: string;
  status: BudgetStatus;
  allowOverrun?: boolean;
  effectiveAllowOverrun: boolean;
  effectiveFrom: string;
  effectiveTo?: string;
  description?: string;
  rowVersion: string;
}

export interface BudgetItemDto {
  id: number;
  itemCode: string;
  itemName: string;
  budgetId: number;
  parentId?: number;
  accountId?: number;
  costCenterId?: number;
  budgetClassificationId?: number;
  level: number;
  allowOverrun?: boolean;
  effectiveAllowOverrun: boolean;
  isActive: boolean;
  rowVersion: string;
}

export interface ApprovalDecisionDto {
  decision: string;
  decisionAt: string;
  approverUserId: string;
  requiredRole?: string;
  reason?: string;
  evaluationSnapshotJson?: string;
}

// ─── Encumbrance DTOs ────────────────────────────────────────────────────────

export interface EncumbranceLineDto {
  id: number;
  budgetItemId: number;
  amount: number;
  liquidatedAmount: number;
  cancelledAmount: number;
  description?: string;
}

export interface EncumbranceDto {
  id: number;
  encumbranceNumber: string;
  encumbranceType: EncumbranceType;
  vendorId?: number;
  purchaseOrderId?: number;
  documentType: string;
  documentId: number;
  description?: string;
  encumbranceDate: string;
  totalAmount: number;
  status: EncumbranceStatus;
  reversalOfId?: number;
  reversalReason?: string;
  rowVersion: string;
  isReversed: boolean;
  budgetId: number;
  budgetNumber: string;
  fundId: number;
  fundName: string;
  latestApproval?: ApprovalDecisionDto;
  createdBy: string;
}

export interface EncumbranceDetailDto extends EncumbranceDto {
  lines: EncumbranceLineDto[];
}

// ─── Availability DTOs ────────────────────────────────────────────────────────

export interface BudgetAvailabilitySummaryDto {
  budgetItemId: number;
  revisedBudget: number;
  actualExpenditure: number;
  outstandingEncumbrance: number;
  available: number;
  effectiveAllowOverrun: boolean;
  controlMethod: BudgetControlMethod;
}

// ─── Command Types ────────────────────────────────────────────────────────────

export interface CreateBudgetTypeCommand {
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
}

export interface UpdateBudgetTypeCommand {
  id: number;
  rowVersion: string;
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
}

export interface ToggleBudgetTypeActiveCommand {
  id: number;
  rowVersion: string;
  isActive: boolean;
}

export interface CreateFundCommand {
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
}

export interface UpdateFundCommand {
  id: number;
  rowVersion: string;
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
}

// ─── Problem Details (Error Contract) ─────────────────────────────────────────

export interface ProblemDetails {
  type?: string;
  title?: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// ─── Filter Types ─────────────────────────────────────────────────────────────

export interface BudgetTypeFilters {
  search?: string;
  controlMethod?: BudgetControlMethod;
  isActive?: boolean;
}

export interface FundFilters {
  search?: string;
  fundType?: FundType;
  fundCategory?: FundCategory;
  isActive?: boolean;
}

export interface BudgetFilters {
  search?: string;
  status?: BudgetStatus;
  fiscalYearId?: number;
  fundId?: number;
}

export interface EncumbranceFilters {
  search?: string;
  encumbranceType?: EncumbranceType;
  status?: EncumbranceStatus;
  budgetId?: number;
}
