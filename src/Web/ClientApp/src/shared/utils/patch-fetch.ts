import { removeToken } from './auth-token';

/**
 * Global fetch interceptor — injects Authorization: Bearer header for /api/ requests.
 * Handles 401 responses by clearing auth state and redirecting to login.
 * Import once in main.tsx.
 */
const TOKEN_KEY = 'erp_jwt_token';
const _originalFetch = window.fetch;

function handleSessionExpired(): void {
  removeToken();
  const currentPath = window.location.pathname;
  if (currentPath !== '/login') {
    window.location.href = `/login?return=${encodeURIComponent(currentPath)}`;
  }
}

window.fetch = function (input: RequestInfo | URL, init?: RequestInit): Promise<Response> {
  const url = typeof input === 'string' ? input : input instanceof URL ? input.toString() : input.url;

  if (url.startsWith('/api/')) {
    const headers = new Headers(init?.headers);
    if (!headers.has('Authorization')) {
      const token = localStorage.getItem(TOKEN_KEY);
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
    }
    if (!headers.has('Accept')) {
      headers.set('Accept', 'application/json');
    }
    return _originalFetch.call(window, input, { ...init, headers }).then((response) => {
      if (response.status === 401) {
        handleSessionExpired();
      }
      return response;
    });
  }

  return _originalFetch.call(window, input, init);
} as typeof window.fetch;
