import { authFetch } from '@/shared/utils/auth-fetch';

interface ApiOptions {
  params?: Record<string, unknown>;
}

async function request<T>(method: string, url: string, data?: unknown, options?: ApiOptions): Promise<T> {
  let fullUrl = url;

  if (options?.params) {
    const searchParams = new URLSearchParams();
    for (const [key, value] of Object.entries(options.params)) {
      if (value !== undefined && value !== null && value !== '') {
        searchParams.set(key, String(value));
      }
    }
    const qs = searchParams.toString();
    if (qs) fullUrl += `?${qs}`;
  }

  const init: RequestInit = { method, headers: {} };
  if (data !== undefined) {
    (init.headers as Record<string, string>)['Content-Type'] = 'application/json';
    init.body = JSON.stringify(data);
  }

  const response = await authFetch(fullUrl, init);
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  const text = await response.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

export const api = {
  get: <T>(url: string, options?: ApiOptions) => request<T>('GET', url, undefined, options),
  post: <T>(url: string, data?: unknown) => request<T>('POST', url, data),
  put: <T>(url: string, data?: unknown) => request<T>('PUT', url, data),
  patch: <T>(url: string, data?: unknown) => request<T>('PATCH', url, data),
  delete: <T>(url: string) => request<T>('DELETE', url),
};
