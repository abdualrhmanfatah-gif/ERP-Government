import type { NotificationDto, PaginatedResult, UnreadCountDto } from './types';

const BASE = '/api/Notifications';

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

export const notificationsClient = {
  async list(page = 1, pageSize = 25): Promise<PaginatedResult<NotificationDto>> {
    return handleResponse<PaginatedResult<NotificationDto>>(
      await fetch(`${BASE}?page=${page}&pageSize=${pageSize}`, {
        headers: { Accept: 'application/json' },
      })
    );
  },

  async unreadCount(): Promise<UnreadCountDto> {
    return handleResponse<UnreadCountDto>(
      await fetch(`${BASE}/unread-count`, {
        headers: { Accept: 'application/json' },
      })
    );
  },

  async markAsRead(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE}/${id}/read`, {
        method: 'PUT',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async markAllAsRead(): Promise<void> {
    await handleVoid(
      await fetch(`${BASE}/read-all`, {
        method: 'PUT',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async delete(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },

  async clearAll(): Promise<void> {
    await handleVoid(
      await fetch(`${BASE}/all`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};
