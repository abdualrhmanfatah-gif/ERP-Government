import { Link, useLocation } from 'react-router-dom';
import { ChevronDown } from 'lucide-react';
import { moduleGroups } from './navigation';
import { Button, DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem } from '@/components/ui';

export function TopNav() {
  const location = useLocation();

  return (
    <nav
      aria-label="التنقل الرئيسي"
      className="hidden lg:flex items-center gap-0.5 sm:gap-1 min-w-0 overflow-x-auto [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden py-1"
    >
      {moduleGroups.map((group) => {
        const active = group.items.some((i) => location.pathname.startsWith(i.path));
        return (
          <DropdownMenu key={group.label}>
            <DropdownMenuTrigger asChild>
              <Button
                variant="header"
                size="sm"
                aria-label={group.label}
                className={`px-2.5 py-1 text-xs font-medium gap-1 shrink-0 rounded-lg transition-all outline-none focus-visible:ring-1 focus-visible:ring-white/40 cursor-pointer ${
                  active
                    ? 'bg-white/20 text-white font-semibold shadow-xs'
                    : 'text-white/85 hover:bg-white/10 hover:text-white'
                }`}
              >
                {group.label}
                <ChevronDown size={13} aria-hidden="true" className="transition-transform duration-200" />
              </Button>
            </DropdownMenuTrigger>

            <DropdownMenuContent
              align="start"
              side="bottom"
              sideOffset={6}
              className="min-w-52 bg-[var(--color-surface)] border border-[var(--color-border-container)] shadow-xl rounded-xl py-1 z-50"
            >
              {group.items.map((item) => {
                const isCurrent = location.pathname.startsWith(item.path);
                return (
                  <DropdownMenuItem key={item.path} asChild className="p-0 focus:bg-transparent">
                    <Link
                      to={item.path}
                      className={`flex items-center gap-2 px-3.5 py-2.5 no-underline text-body-sm transition-colors cursor-pointer w-full rounded-md ${
                        isCurrent
                          ? 'bg-[var(--color-surface-container)] font-semibold text-[var(--color-primary)] border-s-2 border-s-[var(--color-primary)]'
                          : 'text-[var(--color-on-surface-variant)] hover:bg-[var(--color-surface-container)] hover:text-[var(--color-on-surface)]'
                      }`}
                    >
                      {item.label}
                    </Link>
                  </DropdownMenuItem>
                );
              })}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      })}
    </nav>
  );
}
