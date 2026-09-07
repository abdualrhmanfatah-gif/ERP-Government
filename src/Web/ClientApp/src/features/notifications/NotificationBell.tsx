import { useState, useRef, useEffect } from 'react';
import { Bell } from 'lucide-react';
import { Button } from '@/components/ui';
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
      <Button
        variant="ghost"
        size="icon"
        aria-label={`الإشعارات${data && data.count > 0 ? ` — ${data.count} إشعار غير مقروء` : ''}`}
        aria-expanded={isOpen}
        onClick={() => setIsOpen(!isOpen)}
        className="relative"
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
      </Button>
      {isOpen ? (
        <NotificationDropdown onClose={() => setIsOpen(false)} />
      ) : null}
    </div>
  );
}
