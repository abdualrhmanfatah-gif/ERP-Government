import { NormalizedError, ProblemDetails } from './types';

export function normalizeError(err: unknown): NormalizedError {
  if (err instanceof DOMException && err.name === 'AbortError') {
    return { kind: 'cancelled', message: '' };
  }

  if (err instanceof TypeError && err.message === 'Failed to fetch') {
    return {
      kind: 'network',
      message: 'تعذر الاتصال بالخادم. تحقق من اتصال الشبكة وأعد المحاولة.',
    };
  }

  if (err && typeof err === 'object' && 'status' in err && 'response' in err) {
    const ex = err as { status: number; response: string | object };
    return parseSwaggerException(ex.status, ex.response);
  }

  if (err && typeof err === 'object' && 'status' in err && 'problemDetails' in err) {
    const ex = err as { status: number; problemDetails: ProblemDetails };
    return fromProblemDetails(ex.status, ex.problemDetails);
  }

  if (err instanceof Error) {
    try {
      const parsed = JSON.parse(err.message) as Partial<ProblemDetails>;
      if (parsed && typeof parsed === 'object' && 'status' in parsed && typeof parsed.status === 'number') {
        return fromProblemDetails(parsed.status, parsed as ProblemDetails);
      }
    } catch {
      // Not JSON — fall through
    }
    if (err.message.includes('<!DOCTYPE') || err.message.includes('Unexpected token') || err.message.includes('Invalid JSON')) {
      return {
        kind: 'invalid-response',
        message: 'تعذر الحصول على بيانات صالحة من الخادم. يرجى التأكد من تشغيل الخدمة الخلفية.',
      };
    }
    return {
      kind: 'invalid-response',
      message: err.message || 'خطأ غير معروف',
    };
  }

  return {
    kind: 'invalid-response',
    message: 'خطأ غير معروف',
  };
}

function parseSwaggerException(status: number, response: string | object): NormalizedError {
  if (typeof response === 'string') {
    try {
      const body = JSON.parse(response) as Partial<ProblemDetails>;
      return fromProblemDetails(status, body as ProblemDetails);
    } catch {
      return { kind: 'http', status, message: response || 'خطأ غير معروف' };
    }
  }
  if (typeof response === 'object' && response !== null) {
    return fromProblemDetails(status, response as ProblemDetails);
  }
  return { kind: 'http', status, message: 'خطأ غير معروف' };
}

function fromProblemDetails(status: number, body: Partial<ProblemDetails>): NormalizedError {
  return {
    kind: 'http',
    status,
    code: body.code,
    traceId: body.traceId,
    message: body.detail || body.title || 'خطأ غير معروف',
    errors: body.errors,
  };
}

export function getQueryErrorMessage(error: unknown): string {
  const normalized = normalizeError(error);
  if (normalized.kind === 'cancelled') return '';
  if (normalized.kind === 'network') return normalized.message;
  if (normalized.errors) {
    const messages = Object.values(normalized.errors).flat();
    if (messages.length > 0) return messages[0];
  }
  if (normalized.status === 403) return 'ليس لديك صلاحية للوصول إلى هذا المورد';
  if (normalized.status === 401) return 'يجب تسجيل الدخول أولاً';
  if (normalized.status === 404) return 'المورد المطلوب غير موجود';
  if (normalized.status && normalized.status >= 500) return 'خطأ في الخادم. يرجى المحاولة لاحقاً';
  return normalized.message || 'فشل تحميل البيانات';
}
