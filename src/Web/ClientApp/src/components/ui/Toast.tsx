import { Toaster } from './sonner';
import type { NotificationType } from '../../features/notifications/types';
import { notify } from '../../features/notifications/notify';

export function ToastProvider() {
  return (
    <Toaster
      position="top-center"
      toastOptions={{
        style: {
          fontFamily: 'inherit',
          direction: 'rtl',
        },
        className: 'text-sm',
      }}
    />
  );
}

type ToastType = NotificationType;

export function showToast(type: ToastType, message?: string) {
  notify({ type, title: message ?? type });
}
