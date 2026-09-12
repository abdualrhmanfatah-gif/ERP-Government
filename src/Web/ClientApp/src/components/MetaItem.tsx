import type { ReactNode } from 'react';

interface MetaItemProps {
  label: string;
  value?: ReactNode;
}

export function MetaItem({ label, value }: MetaItemProps) {
  return (
    <span className="flex items-center gap-1.5 text-sm text-[var(--color-on-surface)]">
      {label}: <strong className="text-sm font-bold text-[var(--color-on-surface)]">{value || '-'}</strong>
    </span>
  );
}
