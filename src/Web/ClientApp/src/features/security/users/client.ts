import type {
  UserDto,
  UserDetailDto,
  EffectivePermissionDto,
  UserSessionDto,
  AuditEntryDto,
  CreateUserCommand,
  UpdateUserCommand,
  SetUserRoleCommand,
  AssignUserPermissionCommand,
} from './types';
import { authFetch } from '../../../shared/utils/auth-fetch';

const BASE = '/api/Users';

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

function buildQueryString(params: Record<string, string | number | boolean | undefined>): string {
  const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== '');
  if (entries.length === 0) return '';
  return '?' + entries.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`).join('&');
}

// Users — CRUD
export const usersClient = {
  async list(params?: { search?: string; status?: string; departmentId?: number; page?: number; pageSize?: number }): Promise<UserDto[]> {
    const qs = buildQueryString({
      search: params?.search,
      status: params?.status,
      departmentId: params?.departmentId,
      page: params?.page,
      pageSize: params?.pageSize,
    });
    return handleResponse<UserDto[]>(
      await authFetch(`${BASE}${qs}`, { headers: { Accept: 'application/json' } })
    );
  },

  async getById(id: number): Promise<UserDetailDto> {
    return handleResponse<UserDetailDto>(
      await authFetch(`${BASE}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },

  async create(data: CreateUserCommand): Promise<number> {
    return handleResponse<number>(
      await authFetch(BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async update(data: UpdateUserCommand): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
};

// Users — Lifecycle
export const usersLifecycleClient = {
  async deactivate(id: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${id}/deactivate`, {
        method: 'POST',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async reactivate(id: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${id}/reactivate`, {
        method: 'POST',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async resetFailedLoginAttempts(id: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${id}/reset-failed-login-attempts`, {
        method: 'POST',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Users — Role (single)
export const usersRoleClient = {
  async set(userId: number, data: SetUserRoleCommand): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${userId}/role`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ ...data, userId }),
      })
    );
  },
};

// Users — Permissions
export const usersPermissionsClient = {
  async list(userId: number): Promise<EffectivePermissionDto[]> {
    return handleResponse<EffectivePermissionDto[]>(
      await authFetch(`${BASE}/${userId}/permissions`, { headers: { Accept: 'application/json' } })
    );
  },

  async assign(userId: number, data: AssignUserPermissionCommand): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${userId}/permissions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ ...data, userId }),
      })
    );
  },

  async remove(userId: number, permissionId: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${userId}/permissions/${permissionId}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Users — Sessions
export const usersSessionsClient = {
  async list(userId: number): Promise<UserSessionDto[]> {
    return handleResponse<UserSessionDto[]>(
      await authFetch(`${BASE}/${userId}/sessions`, { headers: { Accept: 'application/json' } })
    );
  },

  async create(userId: number, deviceFingerprint?: string): Promise<number> {
    return handleResponse<number>(
      await authFetch(`${BASE}/${userId}/sessions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ deviceFingerprint }),
      })
    );
  },

  async createCurrent(deviceFingerprint?: string): Promise<number> {
    return handleResponse<number>(
      await authFetch(`${BASE}/sessions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ deviceFingerprint }),
      })
    );
  },

  async revoke(userId: number, sessionId: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${userId}/sessions/${sessionId}/revoke`, {
        method: 'POST',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async revokeAll(userId: number): Promise<void> {
    await handleVoid(
      await authFetch(`${BASE}/${userId}/sessions/revoke-all`, {
        method: 'POST',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Users — Audit (entity-change history)
export const usersAuditClient = {
  async list(userId: number): Promise<AuditEntryDto[]> {
    return handleResponse<AuditEntryDto[]>(
      await authFetch(`${BASE}/${userId}/audit`, { headers: { Accept: 'application/json' } })
    );
  },
};
