import { authFetch } from '@/shared/utils/auth-fetch';

interface ApiOptions {
  params?: Record<string, unknown>;
  signal?: AbortSignal;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly body?: unknown,
  ) {
    super(message);
    this.name = 'ApiError';
  }
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

  const init: RequestInit = { method, headers: {}, signal: options?.signal };
  if (data !== undefined) {
    (init.headers as Record<string, string>)['Content-Type'] = 'application/json';
    init.body = JSON.stringify(data);
  }

  const response = await authFetch(fullUrl, init);
  if (!response.ok) {
    const text = await response.text();
    let body: unknown;
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
    throw new ApiError(response.status, text || `HTTP ${response.status}`, body);
  }
  const text = await response.text();
  if (!text) return undefined as T;
  const contentType = response.headers.get('content-type');
  if (contentType && contentType.includes('text/html')) {
    throw new ApiError(response.status, 'تعذر الحصول على بيانات صالحة من الخادم. يرجى التأكد من تشغيل الخدمة الخلفية.', text);
  }
  try {
    return JSON.parse(text) as T;
  } catch {
    throw new ApiError(response.status, 'تعذر الحصول على بيانات صالحة من الخادم. يرجى التأكد من تشغيل الخدمة الخلفية.', text);
  }
}

export const api = {
  get: <T>(url: string, options?: ApiOptions) => request<T>('GET', url, undefined, options),
  post: <T>(url: string, data?: unknown, options?: ApiOptions) => request<T>('POST', url, data, options),
  put: <T>(url: string, data?: unknown, options?: ApiOptions) => request<T>('PUT', url, data, options),
  patch: <T>(url: string, data?: unknown, options?: ApiOptions) => request<T>('PATCH', url, data, options),
  delete: <T>(url: string, options?: ApiOptions) => request<T>('DELETE', url, undefined, options),
};
