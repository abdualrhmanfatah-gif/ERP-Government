import { useState, useRef, useEffect } from 'react';
import { Bell } from 'lucide-react';
import { useUnreadCount } from './hooks';
import { NotificationDropdown } from './NotificationDropdown';

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const { data } = useUnreadCount();

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        className="p-3 min-w-11 min-h-11 rounded-lg hover:bg-[var(--color-surface-container-high)] transition-colors relative"
        aria-label={`الإشعارات${data && data.count > 0 ? ` — ${data.count} إشعار غير مقروء` : ''}`}
        aria-expanded={isOpen}
        onClick={() => setIsOpen(!isOpen)}
      >
        <Bell size={18} />
        {data && data.count > 0 ? (
          <span
            className="absolute -top-0.5 -start-0.5 min-w-[18px] h-[18px] flex items-center justify-center bg-[var(--color-error)] text-[var(--color-on-error)] text-label-sm font-bold rounded-full px-1"
            aria-hidden="true"
          >
            {data.count}
          </span>
        ) : null}
      </button>
      {isOpen ? (
        <NotificationDropdown onClose={() => setIsOpen(false)} />
      ) : null}
    </div>
  );
}
