import { notify } from '@/components/ui/Toast';
import type { UseFormSetError, FieldValues, Path } from 'react-hook-form';

interface ProblemErrors {
  [field: string]: string[];
}

interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  errors?: ProblemErrors;
}

function parseApiError(err: unknown): ProblemDetails {
  if (err instanceof Error) {
    try {
      const parsed = JSON.parse(err.message);
      if (Array.isArray(parsed)) {
        return { errors: { '': parsed.map(String) } };
      }
      return parsed as ProblemDetails;
    } catch {
      return { detail: err.message };
    }
  }
  return { detail: 'حدث خطأ غير متوقع' };
}

export function handleApiError<T extends FieldValues>(
  err: unknown,
  setError: UseFormSetError<T>,
): void {
  const problem = parseApiError(err);

  if (problem.status && problem.status >= 500) {
    notify({
      type: 'error',
      title: 'خطأ في الخادم',
      message: problem.detail ?? 'حدث خطأ في الخادم. يرجى المحاولة لاحقاً.',
    });
    return;
  }

  if (problem.errors && Object.keys(problem.errors).length > 0) {
    for (const [field, messages] of Object.entries(problem.errors)) {
      const formField = field.split('.').pop() as Path<T>;
      setError(formField, {
        message: messages.join(', '),
      });
    }
    return;
  }

  if (problem.detail) {
    setError('root' as Path<T>, { message: problem.detail });
    return;
  }

  setError('root' as Path<T>, { message: 'حدث خطأ أثناء الحفظ' });
}

export function handleLifecycleError(err: unknown): void {
  const problem = parseApiError(err);

  if (problem.status && problem.status >= 500) {
    notify({
      type: 'error',
      title: 'خطأ في الخادم',
      message: problem.detail ?? 'حدث خطأ في الخادم. يرجى المحاولة لاحقاً.',
    });
    return;
  }

  const messages = problem.errors
    ? Object.values(problem.errors).flat()
    : [problem.detail ?? 'حدث خطأ غير متوقع'];

  notify({
    type: 'error',
    title: 'فشل الإجراء',
    message: messages.join('\n'),
  });
}
