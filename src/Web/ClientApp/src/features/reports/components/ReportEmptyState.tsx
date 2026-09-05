import { FileText } from 'lucide-react';

interface ReportEmptyStateProps {
  title: string;
  description: string;
}

export function ReportEmptyState({ title, description }: ReportEmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16 px-4">
      <div className="relative mb-6">
        {/* Background circle */}
        <div className="absolute inset-0 w-20 h-20 rounded-full bg-[var(--color-surface-container)] scale-110" />
        {/* Icon */}
        <div className="relative w-20 h-20 rounded-full bg-[var(--color-surface-container-low)] border-2 border-dashed border-[var(--color-outline-variant)] flex items-center justify-center">
          <FileText className="w-8 h-8 text-[var(--color-on-surface-variant)]" strokeWidth={1.5} />
        </div>
      </div>

      <h3 className="text-lg font-semibold text-[var(--color-on-surface)] mb-2">{title}</h3>
      <p className="text-sm text-[var(--color-on-surface-variant)] text-center max-w-sm leading-relaxed">
        {description}
      </p>

      {/* Decorative ledger lines */}
      <div className="mt-6 flex flex-col gap-1.5 opacity-30">
        <div className="w-32 h-1 bg-[var(--color-outline-variant)] rounded-full" />
        <div className="w-24 h-1 bg-[var(--color-outline-variant)] rounded-full" />
        <div className="w-28 h-1 bg-[var(--color-outline-variant)] rounded-full" />
      </div>
    </div>
  );
}
