import { toast } from 'sonner';
import { Toaster } from './sonner';

type ToastType = 'success' | 'error' | 'info' | 'warning';

const messages: Record<ToastType, string> = {
  success: 'تم بنجاح',
  error: 'حدث خطأ',
  info: 'تم',
  warning: 'تنبيه',
};

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

export function showToast(type: ToastType, message?: string) {
  const msg = message ?? messages[type];
  switch (type) {
    case 'success':
      toast.success(msg);
      break;
    case 'error':
      toast.error(msg);
      break;
    case 'warning':
      toast.warning(msg);
      break;
    default:
      toast(msg);
      break;
  }
}
