import { createContext, useState, useCallback, useRef, useEffect, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { CircleCheckIcon, InfoIcon, TriangleAlertIcon, OctagonXIcon, X } from 'lucide-react';
import type { NotificationType } from '../../features/notifications/types';

interface ToastItem {
  id: string;
  type: NotificationType;
  title: string;
  message?: string;
}

interface ToastContextValue {
  addToast: (toast: Omit<ToastItem, 'id'>) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

let _addToast: ((toast: Omit<ToastItem, 'id'>) => void) | null = null;

export function notify(options: { type?: NotificationType; title: string; message?: string }) {
  _addToast?.({ type: options.type ?? 'info', title: options.title, message: options.message });
}

export function showToast(type: NotificationType, message: string) {
  _addToast?.({ type, title: message });
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const timersRef = useRef<Map<string, ReturnType<typeof setTimeout>>>(new Map());

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
    const timer = timersRef.current.get(id);
    if (timer) {
      clearTimeout(timer);
      timersRef.current.delete(id);
    }
  }, []);

  const addToast = useCallback(
    (toast: Omit<ToastItem, 'id'>) => {
      const id = `toast-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
      setToasts((prev) => [...prev, { ...toast, id }]);

      const duration = toast.type === 'error' ? 6000 : 4000;
      const timer = setTimeout(() => removeToast(id), duration);
      timersRef.current.set(id, timer);
    },
    [removeToast],
  );

  useEffect(() => {
    _addToast = addToast;
    return () => { _addToast = null; };
  }, [addToast]);

  return (
    <ToastContext.Provider value={{ addToast, removeToast }}>
      {children}
      {createPortal(<ToastContainer toasts={toasts} onRemove={removeToast} />, document.body)}
    </ToastContext.Provider>
  );
}

function ToastContainer({ toasts, onRemove }: { toasts: ToastItem[]; onRemove: (id: string) => void }) {
  if (toasts.length === 0) return null;

  return (
    <div
      aria-live="polite"
      aria-label="إشعارات"
      className="fixed top-4 start-1/2 -translate-x-1/2 z-[9999] flex flex-col gap-2 w-full max-w-sm pointer-events-none"
    >
      {toasts.map((toast) => (
        <ToastItemComponent key={toast.id} toast={toast} onRemove={onRemove} />
      ))}
    </div>
  );
}

function ToastItemComponent({ toast, onRemove }: { toast: ToastItem; onRemove: (id: string) => void }) {
  const iconMap: Record<NotificationType, ReactNode> = {
    success: <CircleCheckIcon className="w-5 h-5" />,
    error: <OctagonXIcon className="w-5 h-5" />,
    warning: <TriangleAlertIcon className="w-5 h-5" />,
    info: <InfoIcon className="w-5 h-5" />,
  };

  const colorMap: Record<NotificationType, { bg: string; border: string; icon: string; text: string }> = {
    success: {
      bg: 'var(--color-success-container, #dcfce7)',
      border: 'var(--color-success, #16a34a)',
      icon: 'var(--color-success, #16a34a)',
      text: 'var(--color-on-surface, #0d1c2f)',
    },
    error: {
      bg: 'var(--color-error-container, #ffdad6)',
      border: 'var(--color-error, #e53935)',
      icon: 'var(--color-error, #e53935)',
      text: 'var(--color-on-surface, #0d1c2f)',
    },
    warning: {
      bg: 'var(--color-warning-container, #fef9c3)',
      border: 'var(--color-warning, #ca8a04)',
      icon: 'var(--color-warning, #ca8a04)',
      text: 'var(--color-on-surface, #0d1c2f)',
    },
    info: {
      bg: 'var(--color-info-container, #dbeafe)',
      border: 'var(--color-info, #2563eb)',
      icon: 'var(--color-info, #2563eb)',
      text: 'var(--color-on-surface, #0d1c2f)',
    },
  };

  const colors = colorMap[toast.type];

  return (
    <div
      role="alert"
      className="pointer-events-auto flex items-start gap-3 px-4 py-3 rounded-xl shadow-lg border animate-slide-in"
      style={{
        backgroundColor: colors.bg,
        borderColor: colors.border,
        color: colors.text,
        fontFamily: 'inherit',
        direction: 'rtl',
      }}
    >
      <span className="shrink-0 mt-0.5" style={{ color: colors.icon }}>
        {iconMap[toast.type]}
      </span>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold leading-snug">{toast.title}</p>
        {toast.message ? (
          <p className="text-xs mt-0.5 opacity-75 leading-relaxed">{toast.message}</p>
        ) : null}
      </div>
      <button
        type="button"
        onClick={() => onRemove(toast.id)}
        className="shrink-0 mt-0.5 p-0.5 rounded-md opacity-50 hover:opacity-100 transition-opacity cursor-pointer"
        aria-label="إغلاق"
      >
        <X className="w-4 h-4" />
      </button>
    </div>
  );
}
