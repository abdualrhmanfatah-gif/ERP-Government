import { Link } from 'react-router-dom';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface BreadcrumbItem {
  label: string;
  path?: string;
}

interface BreadcrumbProps {
  items: BreadcrumbItem[];
  className?: string;
}

export function Breadcrumb({ items, className }: BreadcrumbProps) {
  const Chevron = document.documentElement.dir === 'rtl' ? ChevronLeft : ChevronRight;

  return (
    <nav aria-label="مسار التصفح" className={cn('flex items-center gap-1 text-sm', className)}>
      {items.map((item, index) => {
        const isLast = index === items.length - 1;
        return (
          <span key={index} className="flex items-center gap-1">
            {index > 0 && (
              <Chevron
                size={14}
                className="text-[var(--color-on-surface-variant)] shrink-0"
                aria-hidden="true"
              />
            )}
            {isLast || !item.path ? (
              <span
                aria-current={isLast ? 'page' : undefined}
                className={cn(
                  'text-[var(--color-on-surface)]',
                  isLast ? 'font-medium' : 'text-[var(--color-on-surface-variant)]'
                )}
              >
                {item.label}
              </span>
            ) : (
              <Link
                to={item.path}
                className="text-[var(--color-on-surface-variant)] no-underline hover:text-[var(--color-primary)] transition-colors duration-150"
              >
                {item.label}
              </Link>
            )}
          </span>
        );
      })}
    </nav>
  );
}
