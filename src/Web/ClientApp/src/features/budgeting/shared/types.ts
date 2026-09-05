// Budgeting Shared Types — Single source of truth for all 5 frontend specs
// Matches backend DTO shapes from specs/010-rebuild-budgeting-module/contracts/budgeting-dtos.md

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
  Draft = 0,
  Submitted = 1,
  Approved = 2,
  Active = 3,
  Suspended = 4,
  Closed = 5,
  Cancelled = 6,
}

export enum AppropriationType {
  Original = 0,
  Supplement = 1,
  Reduction = 2,
  Adjustment = 3,
}

export enum AppropriationStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Active = 3,
  Suspended = 4,
  Closed = 5,
  Cancelled = 6,
}

export enum EncumbranceType {
  Commitment = 0,
  Obligational = 1,
  Contractual = 2,
  Advance = 3,
  Adjustment = 4,
}

export enum EncumbranceStatus {
  Draft = 0,
  PendingApproval = 1,
  Approved = 2,
  Active = 3,
  PartialReleased = 4,
  PartialLiquidated = 5,
  FullyLiquidated = 6,
  Closed = 7,
  Cancelled = 8,
  Reversed = 9,
}

// ─── Arabic Label Maps ────────────────────────────────────────────────────────

export const budgetControlMethodLabels: Record<BudgetControlMethod, string> = {
  [BudgetControlMethod.None]: 'لا يوجد',
  [BudgetControlMethod.Warning]: 'تحذير',
  [BudgetControlMethod.Blocking]: 'حجب',
};

export const fundTypeLabels: Record<FundType, string> = {
  [FundType.General]: 'عام',
  [FundType.Special]: 'خاص',
  [FundType.Project]: 'مشروع',
};

export const fundCategoryLabels: Record<FundCategory, string> = {
  [FundCategory.Operating]: 'تشغيلي',
  [FundCategory.Capital]: 'رأسمالي',
};

export const budgetStatusLabels: Record<BudgetStatus, string> = {
  [BudgetStatus.Draft]: 'مسودة',
  [BudgetStatus.Submitted]: 'مقدم',
  [BudgetStatus.Approved]: 'معتمد',
  [BudgetStatus.Active]: 'نشط',
  [BudgetStatus.Suspended]: 'معلق',
  [BudgetStatus.Closed]: 'مغلق',
  [BudgetStatus.Cancelled]: 'ملغي',
};

export const appropriationTypeLabels: Record<AppropriationType, string> = {
  [AppropriationType.Original]: 'أصلي',
  [AppropriationType.Supplement]: 'تكميلي',
  [AppropriationType.Reduction]: 'تخفيض',
  [AppropriationType.Adjustment]: 'تعديل',
};

export const appropriationStatusLabels: Record<AppropriationStatus, string> = {
  [AppropriationStatus.Draft]: 'مسودة',
  [AppropriationStatus.PendingApproval]: 'بانتظار الاعتماد',
  [AppropriationStatus.Approved]: 'معتمد',
  [AppropriationStatus.Active]: 'نشط',
  [AppropriationStatus.Suspended]: 'معلق',
  [AppropriationStatus.Closed]: 'مغلق',
  [AppropriationStatus.Cancelled]: 'ملغي',
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
  [EncumbranceStatus.PartialReleased]: 'تحرير جزئي',
  [EncumbranceStatus.PartialLiquidated]: 'تسوية جزئية',
  [EncumbranceStatus.FullyLiquidated]: 'تسوية كاملة',
  [EncumbranceStatus.Closed]: 'مغلق',
  [EncumbranceStatus.Cancelled]: 'ملغي',
  [EncumbranceStatus.Reversed]: 'معكوس',
};

// Label lookup helpers with fallback for unknown enum values
export function getLabel<T extends number>(labels: Record<T, string>, value: T): string {
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
  fiscalYearId?: number;
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
  totalAmount: number;
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
  fundId?: number;
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

export interface AppropriationDto {
  id: number;
  appropriationNumber: string;
  budgetId: number;
  budgetItemId: number;
  appropriationType: AppropriationType;
  documentType: string;
  documentId: number;
  amount: number;
  status: AppropriationStatus;
  rowVersion: string;
  budgetNumber: string;
  budgetName: string;
  fundId: number;
  fundName: string;
  fiscalYearId: number;
  availableForItem: number;
  latestApproval?: ApprovalDecisionDto;
  createdBy: string;
}

export interface EncumbranceDto {
  id: number;
  encumbranceNumber: string;
  encumbranceType: EncumbranceType;
  appropriationId: number;
  vendorId?: number;
  purchaseOrderId?: number;
  documentType: string;
  documentId: number;
  description?: string;
  encumbranceDate: string;
  amount: number;
  status: EncumbranceStatus;
  reversalOfId?: number;
  reversalReason?: string;
  rowVersion: string;
  isReversed: boolean;
  budgetId: number;
  budgetNumber: string;
  budgetItemId: number;
  itemCode: string;
  fundId: number;
  fundName: string;
  fiscalYearId: number;
  availableForEncumbrance: number;
  latestApproval?: ApprovalDecisionDto;
  createdBy: string;
}

// ─── Availability DTOs ────────────────────────────────────────────────────────

export interface ItemAvailabilityDto {
  budgetItemId: number;
  netAppropriated: number;
  totalSupplement: number;
  totalReduction: number;
  totalAdjustment: number;
  available: number;
  controlMethod: BudgetControlMethod;
  effectiveAllowOverrun: boolean;
  warning?: string;
}

export interface EncumbranceAvailabilityDto {
  appropriationId: number;
  budgetItemId: number;
  netAppropriated: number;
  totalEncumbered: number;
  available: number;
  controlMethod: BudgetControlMethod;
  effectiveAllowOverrun: boolean;
  warning?: string;
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
  fiscalYearId?: number;
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
  fiscalYearId?: number;
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

export interface AppropriationFilters {
  search?: string;
  appropriationType?: AppropriationType;
  status?: AppropriationStatus;
  budgetId?: number;
}

export interface EncumbranceFilters {
  search?: string;
  encumbranceType?: EncumbranceType;
  status?: EncumbranceStatus;
  appropriationId?: number;
}
