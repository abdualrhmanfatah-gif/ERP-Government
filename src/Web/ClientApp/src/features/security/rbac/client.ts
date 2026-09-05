import type {
  SecurityRoleDto,
  SecurityPermissionDto,
  RolePermissionDto,
  UserPermissionDto,
  CreateRoleCommand,
  UpdateRoleCommand,
  AssignRolePermissionCommand,
  AssignUserPermissionCommand,
} from './types';
import { authFetch } from '../../../shared/utils/auth-fetch';

const BASE_ROLES = '/api/Roles';
const BASE_USERS = '/api/Users';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

async function handleVoid(response: Response): Promise<void> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
}

// Roles
export const rolesClient = {
  async list(): Promise<SecurityRoleDto[]> {
    return handleResponse<SecurityRoleDto[]>(
      await authFetch(BASE_ROLES, { headers: { Accept: 'application/json' } })
    );
  },

  async getById(id: number): Promise<SecurityRoleDto> {
    return handleResponse<SecurityRoleDto>(
      await authFetch(`${BASE_ROLES}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },

  async create(data: CreateRoleCommand): Promise<void> {
    await handleVoid(
      await authFetch(BASE_ROLES, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async update(data: UpdateRoleCommand): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_ROLES}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async delete(id: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_ROLES}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// RolePermissions
export const rolePermissionsClient = {
  async list(roleId: number): Promise<RolePermissionDto[]> {
    return handleResponse<RolePermissionDto[]>(
      await authFetch(`${BASE_ROLES}/${roleId}/permissions`, { headers: { Accept: 'application/json' } })
    );
  },

  async assign(data: AssignRolePermissionCommand): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_ROLES}/${data.roleId}/permissions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async remove(roleId: number, permissionId: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_ROLES}/${roleId}/permissions/${permissionId}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Permissions (read-only)
export const permissionsClient = {
  async list(): Promise<SecurityPermissionDto[]> {
    return handleResponse<SecurityPermissionDto[]>(
      await authFetch('/api/Permissions', { headers: { Accept: 'application/json' } })
    );
  },
};

// UserPermissions
export const userPermissionsClient = {
  async list(userId: number): Promise<UserPermissionDto[]> {
    return handleResponse<UserPermissionDto[]>(
      await authFetch(`${BASE_USERS}/${userId}/permissions`, { headers: { Accept: 'application/json' } })
    );
  },

  async assign(data: AssignUserPermissionCommand & { userId: number }): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_USERS}/${data.userId}/permissions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ permissionId: data.permissionId, isGranted: data.isGranted, effectiveFrom: data.effectiveFrom, effectiveTo: data.effectiveTo, reason: data.reason }),
      })
    );
  },

  async remove(userId: number, permissionId: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE_USERS}/${userId}/permissions/${permissionId}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};
