// User Management Types — mirrors backend DTOs from Security module

export interface UserDto {
  id: number;
  login: string;
  accountType: string;
  isActive: boolean;
  departmentId?: number | null;
  departmentName?: string | null;
  mfaEnabled: boolean;
  lastLoginAt?: string | null;
  failedLoginAttempts: number;
  isLocked: boolean;
  createdAt: string;
}

export interface UserDetailDto {
  id: number;
  login: string;
  externalAuthId?: string | null;
  accountType: string;
  isActive: boolean;
  departmentId?: number | null;
  departmentName?: string | null;
  mfaEnabled: boolean;
  mfaMethod?: string | null;
  mustChangePassword: boolean;
  lastLoginAt?: string | null;
  failedLoginAttempts: number;
  isLocked: boolean;
  lockedUntil?: string | null;
  passwordChangedAt?: string | null;
  activeSessionCount: number;
  role: UserRoleDto | null;
  createdAt: string;
  createdBy?: string | null;
  updatedAt: string;
  updatedBy?: string | null;
}

export interface UserRoleDto {
  roleId: number;
  code: string;
  name: string;
}

export interface EffectivePermissionDto {
  permissionId: number;
  code: string;
  name: string;
  source: 'role' | 'override';
  isGranted: boolean;
  reason?: string | null;
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

export interface UserSessionDto {
  id: number;
  ipAddress: string;
  userAgent?: string | null;
  deviceFingerprint?: string | null;
  createdAt: string;
  lastActivityAt?: string | null;
  expiresAt: string;
  isRevoked: boolean;
  logoutReason?: string | null;
}

export interface AuditEntryDto {
  id: number;
  entityName: string;
  entityId: number;
  action: string;
  actor: string;
  timestamp: string;
  oldValues?: string | null;
  newValues?: string | null;
  requestPath?: string | null;
}

export interface CreateUserCommand {
  login: string;
  name: string;
  departmentId?: number;
  accountType: string;
  password: string;
}

export interface UpdateUserCommand {
  id: number;
  name: string;
  departmentId?: number;
  accountType: string;
  rowVersion: string;
}

export interface SetUserRoleCommand {
  userId: number;
  roleId: number;
}

export interface AssignUserPermissionCommand {
  userId: number;
  permissionId: number;
  isGranted: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  reason: string;
}

export type { SecurityRoleDto } from '../rbac/types';
