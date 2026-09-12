// FEATURE-013 — Organization Module Types (US5-US8)
// DTOs
export interface OrganizationalUnitDto {
  id: number;
  code: string;
  name: string;
  parentId?: number | null;
  parentName?: string | null;
  costCenterCount: number;
  employeeCount: number;
  projectCount: number;
  isActive: boolean;
}

export interface EmployeeDto {
  id: number;
  employeeNumber: string;
  name: string;
  userId?: number | null;
  organizationalUnitId: number;
  organizationalUnitName: string;
  jobTitle: string;
  jobGrade?: string | null;
  hireDate: string;
  employmentStatus: string;
  isActive: boolean;
}

export interface CostCenterDto {
  id: number;
  code: string;
  name: string;
  organizationUnitId?: number | null;
  organizationUnitName?: string | null;
  budgetLimit?: number | null;
  isActive: boolean;
}

export interface ProjectDto {
  id: number;
  code: string;
  name: string;
  fundId?: number | null;
  costCenterId?: number | null;
  costCenterName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  budgetAmount?: number | null;
  status: string;
  isActive: boolean;
}

// Commands
export interface CreateOrgUnitCommand {
  code: string;
  name: string;
  parentId?: number;
}

export interface UpdateOrgUnitCommand {
  id: number;
  code: string;
  name: string;
  parentId?: number;
  isActive: boolean;
}

export interface CreateEmployeeCommand {
  employeeNumber: string;
  name: string;
  organizationalUnitId: number;
  jobTitle: string;
  jobGrade?: string;
  hireDate: string;
}

export interface UpdateEmployeeCommand {
  id: number;
  employeeNumber: string;
  name: string;
  organizationalUnitId: number;
  jobTitle: string;
  jobGrade?: string;
  hireDate: string;
  employmentStatus: string;
  isActive: boolean;
}

export interface CreateCostCenterCommand {
  code: string;
  name: string;
  organizationUnitId?: number;
  budgetLimit?: number;
}

export interface UpdateCostCenterCommand {
  id: number;
  code: string;
  name: string;
  organizationUnitId?: number;
  budgetLimit?: number;
  isActive: boolean;
}

export interface CreateProjectCommand {
  code: string;
  name: string;
  fundId?: number;
  costCenterId?: number;
  startDate?: string;
  endDate?: string;
  budgetAmount?: number;
}

export interface UpdateProjectCommand {
  id: number;
  code: string;
  name: string;
  fundId?: number;
  costCenterId?: number;
  startDate?: string;
  endDate?: string;
  budgetAmount?: number;
  status: string;
  isActive: boolean;
}

// ─── Status Labels ──────────────────────────────────────────────────────────

export const projectStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Active: 'نشط',
  OnHold: 'معلق',
  Completed: 'مكتمل',
  Cancelled: 'ملغي',
};

export const employeeStatusLabels: Record<string, string> = {
  Active: 'نشط',
  Suspended: 'موقوف',
  Terminated: 'منتهي',
};
