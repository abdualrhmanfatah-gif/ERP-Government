import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Card } from '@/components/ui/Card';

interface DashboardCardProps {
  title: string;
  action?: ReactNode;
  children: ReactNode;
  className?: string;
  noPadding?: boolean;
  /** Make card interactive with hover/focus states */
  interactive?: boolean;
  /** Click handler — automatically enables interactive mode */
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
        // base (preserve DashboardCard look on top of shared Card primitive)
        'border-[var(--color-border-container)] bg-[var(--color-surface)] overflow-hidden shadow-sm',
        'transition-all duration-200 ease-in-out',
        // interactive states
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
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-[var(--color-border-container)]">
        <h2 className="text-headline-sm font-bold text-[var(--color-on-surface)]">
          {title}
        </h2>
        {action && <div className="flex items-center">{action}</div>}
      </div>

      {/* Content */}
      <div className={noPadding ? '' : 'p-6'}>{children}</div>
    </Card>
  );
}
