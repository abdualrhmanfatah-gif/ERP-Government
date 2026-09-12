import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Card } from '@/components/ui/Card';

interface DashboardCardProps {
  title: string;
  action?: ReactNode;
  children: ReactNode;
  className?: string;
  noPadding?: boolean;
  interactive?: boolean;
  onClick?: () => void;
}

export function DashboardCard({
  title,
  action,
  children,
  className,
  noPadding = false,
  interactive = false,
  onClick,
}: DashboardCardProps) {
  const isInteractive = interactive || !!onClick;

  return (
    <Card
      padding="none"
      className={cn(
        'border-[var(--color-border-container)] bg-[var(--color-surface)] overflow-hidden shadow-sm h-full flex flex-col',
        'transition-all duration-200 ease-in-out',
        isInteractive && [
          'cursor-pointer',
          'hover:shadow-md hover:border-[var(--color-outline)]',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)] focus-visible:ring-offset-2',
          'active:scale-[0.99]',
        ],
        className
      )}
      onClick={onClick}
      role={isInteractive ? 'button' : undefined}
      tabIndex={isInteractive ? 0 : undefined}
      onKeyDown={
        isInteractive
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                onClick?.();
              }
            }
          : undefined
      }
    >
      <div className="flex items-center justify-between px-3 py-2">
        <h2 className="text-label-md font-semibold text-[var(--color-on-surface)]">
          {title}
        </h2>
        {action && <div className="flex items-center">{action}</div>}
      </div>
      <div className={noPadding ? 'flex-1' : 'flex-1 p-3'}>{children}</div>
    </Card>
  );
}
