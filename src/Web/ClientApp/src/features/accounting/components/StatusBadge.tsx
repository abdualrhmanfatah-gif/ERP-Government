import { EntryStatus } from '../types';

const statusConfig: Record<EntryStatus, { label: string; cssVar: string }> = {
  [EntryStatus.Draft]: { label: 'مسودة', cssVar: 'statusDraft' },
  [EntryStatus.Submitted]: { label: 'مقدمة', cssVar: 'statusPending' },
  [EntryStatus.Approved]: { label: 'موافق عليها', cssVar: 'statusApproved' },
  [EntryStatus.Posted]: { label: 'محاسبة', cssVar: 'statusActive' },
  [EntryStatus.Reversed]: { label: 'معكوسة', cssVar: 'statusClosed' },
  [EntryStatus.Cancelled]: { label: 'ملغاة', cssVar: 'errorContainer' },
};

const statusFgConfig: Record<EntryStatus, string> = {
  [EntryStatus.Draft]: 'statusDraftFg',
  [EntryStatus.Submitted]: 'statusPendingFg',
  [EntryStatus.Approved]: 'statusApprovedFg',
  [EntryStatus.Posted]: 'statusActiveFg',
  [EntryStatus.Reversed]: 'statusClosedFg',
  [EntryStatus.Cancelled]: 'onErrorContainer',
};

interface StatusBadgeProps {
  status: EntryStatus;
  className?: string;
}

export function StatusBadge({ status, className = '' }: StatusBadgeProps) {
  const config = statusConfig[status] ?? statusConfig[EntryStatus.Draft];
  const fgVar = statusFgConfig[status] ?? statusFgConfig[EntryStatus.Draft];

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${className}`}
      style={{
        backgroundColor: `var(--color-${config.cssVar})`,
        color: `var(--color-${fgVar})`,
      }}
    >
      {config.label}
    </span>
  );
}
