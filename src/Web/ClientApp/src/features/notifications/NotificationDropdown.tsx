import { Check, X } from 'lucide-react';
import { Button } from '@/components/ui';
import { useNotifications, useMarkAsRead, useMarkAllAsRead, useDeleteNotification, useClearAllNotifications } from './hooks';
import { useNotificationStore } from './store';
import type { NotificationEntry, NotificationDto } from './types';
import { useEffect } from 'react';

interface Props {
  onClose: () => void;
}

function relativeTime(dateStr: string): string {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'الآن';
  if (diffMin < 60) return `منذ ${diffMin} دقيقة`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `منذ ${diffHr} ساعة`;
  const diffDay = Math.floor(diffHr / 24);
  return `منذ ${diffDay} يوم`;
}

function notificationTypeIcon(type: string): string {
  switch (type) {
    case 'Success': return '✓';
    case 'Warning': return '⚠';
    case 'Error': return '✗';
    case 'Info':
    default: return 'ℹ';
  }
}

function toEntry(dto: NotificationDto): NotificationEntry {
  return {
    id: String(dto.id),
    notificationType: dto.notificationType as NotificationEntry['notificationType'],
    title: dto.title,
    message: dto.message,
    isRead: dto.isRead,
    created: dto.created,
    source: 'server',
  };
}

function NotificationItem({
  notification,
  onMarkAsRead,
  onDelete,
}: {
  notification: NotificationEntry;
  onMarkAsRead: (id: string) => void;
  onDelete: (id: string) => void;
}) {
  return (
    <li
      className={`px-4 py-3 border-b border-[var(--color-border-container)] last:border-b-0 transition-colors ${
        notification.isRead ? 'bg-[var(--color-surface)]' : 'bg-[var(--color-surface-container-low)]'
      }`}
    >
      <div className="flex items-start gap-3">
        <span className="text-body-sm mt-0.5" aria-hidden="true">
          {notificationTypeIcon(notification.notificationType)}
        </span>
        <div className="flex-1 min-w-0">
          <div className="text-body-sm text-[var(--color-on-surface)] font-medium truncate">
            {notification.title}
          </div>
          <div className="text-label-sm text-[var(--color-on-surface-variant)] mt-0.5 line-clamp-2">
            {notification.message}
          </div>
          <div className="text-label-sm text-[var(--color-on-surface-variant)] mt-1">
            {relativeTime(notification.created)}
          </div>
        </div>
        <div className="flex items-center gap-1">
          {!notification.isRead ? (
            <Button
              variant="ghost"
              size="icon-xs"
              aria-label="تحديد كمقروء"
              onClick={() => onMarkAsRead(notification.id)}
            >
              <Check size={16} />
            </Button>
          ) : null}
          <Button
            variant="ghost"
            size="icon-xs"
            aria-label="حذف"
            onClick={() => onDelete(notification.id)}
          >
            <X size={16} />
          </Button>
        </div>
      </div>
    </li>
  );
}

export function NotificationDropdown({ onClose: _onClose }: Props) {
  const { data } = useNotifications(1, 50);
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();
  const deleteNotification = useDeleteNotification();
  const clearAll = useClearAllNotifications();
  const store = useNotificationStore();

  useEffect(() => {
    if (data?.items) {
      store.setServer(data.items.map(toEntry));
    }
  }, [data, store]);

  const unreadCount = store.entries.filter((n) => !n.isRead).length;

  function handleMarkAllAsRead() {
    markAllAsRead.mutate();
    store.entries.filter((n) => !n.isRead).forEach((n) => store.markRead(n.id));
  }

  function handleClearAll() {
    clearAll.mutate();
    store.clearLocal();
  }

  return (
    <div
      role="listbox"
      aria-label="الإشعارات"
      className="absolute start-0 top-full mt-2 w-80 bg-[var(--color-surface-container-low)] rounded-xl shadow-lg border border-[var(--color-border-container)] z-[300] overflow-hidden"
    >
      <div className="px-4 py-3 border-b border-[var(--color-border-container)] flex items-center justify-between">
        <span className="text-body-sm font-semibold text-[var(--color-on-surface)]">الإشعارات</span>
        <div className="flex items-center gap-2">
          {unreadCount > 0 ? (
            <Button
              variant="ghost"
              size="sm"
              className="text-[var(--color-primary)]"
              onClick={handleMarkAllAsRead}
              disabled={markAllAsRead.isPending}
            >
              تحديد الكل كمقروء
            </Button>
          ) : null}
          {store.entries.length > 0 ? (
            <Button
              variant="ghost"
              size="sm"
              className="text-[var(--color-error)]"
              onClick={handleClearAll}
              disabled={clearAll.isPending}
            >
              مسح الكل
            </Button>
          ) : null}
        </div>
      </div>
      {store.entries.length > 0 ? (
        <ul className="list-none m-0 p-0 max-h-64 overflow-y-auto">
          {store.entries.map((notification) => (
            <NotificationItem
              key={notification.id}
              notification={notification}
              onMarkAsRead={(id) => {
                if (notification.source === 'server') markAsRead.mutate(Number(id));
                store.markRead(id);
              }}
              onDelete={(id) => {
                if (notification.source === 'server') deleteNotification.mutate(Number(id));
                store.remove(id);
              }}
            />
          ))}
        </ul>
      ) : (
        <div className="px-4 py-6 text-center text-body-sm text-[var(--color-on-surface-variant)]">
          لا توجد إشعارات
        </div>
      )}
    </div>
  );
}
