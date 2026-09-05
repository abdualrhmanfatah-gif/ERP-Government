import type { ReactNode } from 'react';
import { Inbox } from 'lucide-react';

interface EmptyStateProps {
  message?: string;
  icon?: ReactNode;
  action?: ReactNode;
}

export function EmptyState({
  message = 'لا توجد سجلات',
  icon,
  action,
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-12 px-4 text-center">
      <span aria-hidden="true" className="text-[var(--color-on-surface-variant)]">
        {icon ?? <Inbox size={32} strokeWidth={1.5} />}
      </span>
      <span className="text-sm text-[var(--color-on-surface-variant)]">{message}</span>
      {action ? <div>{action}</div> : null}
    </div>
  );
}
