// FEATURE-008 — RBAC Types
export interface SecurityRoleDto {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  roleLevel: string;
  isMutuallyExclusive: boolean;
  exclusiveWithRoleId?: number | null;
  requiresMfa: boolean;
  maxSessionDuration?: number | null;
  isSystem: boolean;
  isActive: boolean;
}

export interface SecurityPermissionDto {
  id: number;
  module: string;
  action: string;
  code: string;
  name: string;
  description?: string | null;
  permissionLevel: string;
  isSensitive: boolean;
  dataScope: string;
  isActive: boolean;
}

export interface RolePermissionDto {
  roleId: number;
  permissionId: number;
  permissionCode: string;
  permissionName: string;
}

export interface UserPermissionDto {
  id: number;
  userId: number;
  permissionId: number;
  isGranted: boolean;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  reason?: string | null;
  approvedById?: number | null;
  permissionCode: string;
  permissionName: string;
}

export interface CreateRoleCommand {
  code: string;
  name: string;
  description?: string;
  roleLevel: string;
  isMutuallyExclusive?: boolean;
  exclusiveWithRoleId?: number;
  requiresMfa?: boolean;
  maxSessionDuration?: number;
}

export interface UpdateRoleCommand {
  id: number;
  code?: string;
  name?: string;
  description?: string;
  roleLevel?: string;
  isMutuallyExclusive?: boolean;
  exclusiveWithRoleId?: number;
  requiresMfa?: boolean;
  maxSessionDuration?: number;
  isActive?: boolean;
}

export interface AssignRolePermissionCommand {
  roleId: number;
  permissionId: number;
}

export interface AssignUserPermissionCommand {
  permissionId: number;
  isGranted: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  reason?: string;
}
