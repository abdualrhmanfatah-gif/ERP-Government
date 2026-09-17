import { notify } from '@/components/ui/Toast';
import type { UseFormSetError, FieldValues, Path } from 'react-hook-form';
import { normalizeError } from './query-error';

export function handleApiError<T extends FieldValues>(
  err: unknown,
  setError: UseFormSetError<T>,
): void {
  const normalized = normalizeError(err);

  if (normalized.kind === 'cancelled') return;

  if (normalized.kind === 'network') {
    notify({
      type: 'error',
      title: 'خطأ في الاتصال',
      message: normalized.message,
    });
    return;
  }

  if (normalized.status && normalized.status >= 500) {
    notify({
      type: 'error',
      title: 'خطأ في الخادم',
      message: normalized.message || 'حدث خطأ في الخادم. يرجى المحاولة لاحقاً.',
    });
    return;
  }

  if (normalized.errors && Object.keys(normalized.errors).length > 0) {
    for (const [field, messages] of Object.entries(normalized.errors)) {
      const formField = field as Path<T>;
      setError(formField, {
        message: messages.join(', '),
      });
    }
    return;
  }

  if (normalized.message) {
    setError('root' as Path<T>, { message: normalized.message });
    return;
  }

  setError('root' as Path<T>, { message: 'حدث خطأ أثناء الحفظ' });
}

export function handleLifecycleError(err: unknown): void {
  const normalized = normalizeError(err);

  if (normalized.kind === 'cancelled') return;

  if (normalized.kind === 'network') {
    notify({
      type: 'error',
      title: 'خطأ في الاتصال',
      message: normalized.message,
    });
    return;
  }

  if (normalized.status && normalized.status >= 500) {
    notify({
      type: 'error',
      title: 'خطأ في الخادم',
      message: normalized.message || 'حدث خطأ في الخادم. يرجى المحاولة لاحقاً.',
    });
    return;
  }

  const messages = normalized.errors
    ? Object.values(normalized.errors).flat()
    : [normalized.message || 'حدث خطأ غير متوقع'];

  notify({
    type: 'error',
    title: 'فشل الإجراء',
    message: messages.join('\n'),
  });
}
