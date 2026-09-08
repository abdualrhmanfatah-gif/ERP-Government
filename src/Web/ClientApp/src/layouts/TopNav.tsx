import { useState, useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { ChevronDown } from 'lucide-react';
import { moduleGroups } from './navigation';
import { Button } from '@/components/ui';

export function TopNav() {
  const [openGroup, setOpenGroup] = useState<string | null>(null);
  const navRef = useRef<HTMLElement>(null);
  const location = useLocation();

  useEffect(() => { setOpenGroup(null); }, [location.pathname]);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (navRef.current && !navRef.current.contains(e.target as Node)) setOpenGroup(null);
    };
    const handleKey = (e: KeyboardEvent) => { if (e.key === 'Escape') setOpenGroup(null); };
    document.addEventListener('mousedown', handleClick);
    document.addEventListener('keydown', handleKey);
    return () => { document.removeEventListener('mousedown', handleClick); document.removeEventListener('keydown', handleKey); };
  }, []);

  return (
    <nav aria-label="التنقل الرئيسي" ref={navRef} className="hidden lg:flex items-center gap-1">
      {moduleGroups.map((group) => {
        const open = openGroup === group.label;
        const active = group.items.some((i) => location.pathname.startsWith(i.path));
        return (
          <div key={group.label} className="relative">
            <Button
              variant="header"
              aria-expanded={open}
              aria-haspopup="menu"
              aria-label={group.label}
              onClick={() => setOpenGroup(open ? null : group.label)}
              className={open || active ? 'bg-white/15' : ''}
            >
              {group.label}
              <ChevronDown size={14} aria-hidden="true" className={open ? 'rotate-180 transition-transform duration-200' : 'transition-transform duration-200'} />
            </Button>
            {open && (
              <div role="menu" aria-label={group.label} className="absolute top-full mt-2 start-0 min-w-56 bg-[var(--color-surface)] rounded-xl shadow-xl border border-[var(--color-border-container)] z-50 overflow-hidden py-1">
                {group.items.map((item) => {
                  const isCurrent = location.pathname.startsWith(item.path);
                  return (
                    <Link key={item.path} to={item.path} role="menuitem" aria-current={isCurrent ? 'page' : undefined}
                      className={`flex items-center gap-2 px-4 py-2.5 no-underline text-body-md transition-colors duration-150 cursor-pointer focus-visible:outline-none focus-visible:bg-[var(--color-surface-container)] ${isCurrent ? 'bg-[var(--color-surface-container)] font-semibold text-[var(--color-primary)] border-e-2 border-e-[var(--color-primary)]' : 'text-[var(--color-on-surface-variant)] hover:bg-[var(--color-surface-container)] hover:text-[var(--color-on-surface)]'}`}
                    >{item.label}</Link>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </nav>
  );
}
