import { getToken, removeToken } from './auth-token';

/**
 * Fetch wrapper that injects Authorization: Bearer header.
 * Use this for all authenticated API calls.
 */
export async function authFetch(url: string, init?: RequestInit): Promise<Response> {
  const token = getToken();
  const headers = new Headers(init?.headers);
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json');
  }
  const response = await fetch(url, { ...init, headers });

  if (response.status === 401) {
    handleSessionExpired();
  }

  return response;
}

export function handleSessionExpired(): void {
  removeToken();
  const currentPath = window.location.pathname;
  if (currentPath !== '/login') {
    window.location.href = `/login?return=${encodeURIComponent(currentPath)}`;
  }
}

/**
 * Creates an http-compatible object for NSwag clients.
 * Usage: new UsersClient('', { fetch: authFetchFn })
 */
export const authFetchFn: { fetch(url: RequestInfo, init?: RequestInit): Promise<Response> } = {
  fetch: authFetch,
};
