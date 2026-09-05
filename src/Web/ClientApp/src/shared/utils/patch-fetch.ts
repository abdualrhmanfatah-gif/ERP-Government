/**
 * Global fetch interceptor — injects Authorization: Bearer header for /api/ requests.
 * Import once in main.tsx.
 */
const TOKEN_KEY = 'erp_jwt_token';
const _originalFetch = window.fetch;

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
    return _originalFetch.call(window, input, { ...init, headers });
  }

  return _originalFetch.call(window, input, init);
} as typeof window.fetch;
