import { notify as toastNotify } from '@/components/ui/Toast';
import { useNotificationStore } from './store';
import type { NotificationType, Source } from './types';

let _store: ReturnType<typeof useNotificationStore> | null = null;

export function bindStore(store: ReturnType<typeof useNotificationStore>) {
  _store = store;
}

export interface NotifyOptions {
  type?: NotificationType;
  title: string;
  message?: string;
  source?: Source;
}

export function notify({ type = 'info', title, message, source = 'local' }: NotifyOptions) {
  toastNotify({ type, title, message });

  if (_store) {
    _store.addLocal({
      id: `local-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      notificationType: type,
      title,
      message: message ?? '',
      isRead: false,
      created: new Date().toISOString(),
      source,
    });
  }
}

export function useNotify() {
  const store = useNotificationStore();
  if (!_store) bindStore(store);
  return notify;
}
