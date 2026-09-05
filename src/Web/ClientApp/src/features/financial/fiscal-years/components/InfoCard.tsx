import type { ReactNode } from 'react';

interface InfoCardProps {
  title: string;
  children: ReactNode;
}

export function InfoCard({ title, children }: InfoCardProps) {
  return (
    <div className="rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-4">
      <h3 className="mb-3 text-sm font-semibold text-[var(--color-on-surface-variant)]">{title}</h3>
      {children}
    </div>
  );
}

interface InfoRowProps {
  label: string;
  value: ReactNode;
}

export function InfoRow({ label, value }: InfoRowProps) {
  return (
    <div className="flex items-center justify-between py-1.5">
      <span className="text-sm text-[var(--color-on-surface-variant)]">{label}</span>
      <span className="text-sm font-medium text-[var(--color-on-surface)]">{value}</span>
    </div>
  );
}
