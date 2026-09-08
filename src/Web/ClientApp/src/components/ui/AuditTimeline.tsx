import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import {
  Plus,
  Pencil,
  Trash2,
  Eye,
  CheckCircle2,
  XCircle,
  ArrowRight,
  User,
} from 'lucide-react';

type AuditAction = 'create' | 'update' | 'delete' | 'view' | 'approve' | 'reject' | 'submit';

interface AuditEntry {
  id: string | number;
  action: AuditAction;
  user: string;
  userAvatar?: string;
  timestamp: string;
  description: string;
  changes?: Array<{
    field: string;
    oldValue?: string;
    newValue?: string;
  }>;
  children?: ReactNode;
}

interface AuditTimelineProps {
  entries: AuditEntry[];
  className?: string;
}

const actionConfig: Record<AuditAction, {
  icon: typeof Plus;
  label: string;
  colorClass: string;
  bgClass: string;
}> = {
  create: {
    icon: Plus,
    label: 'إنشاء',
    colorClass: 'text-status-active-fg',
    bgClass: 'bg-status-active-bg',
  },
  update: {
    icon: Pencil,
    label: 'تعديل',
    colorClass: 'text-[var(--color-secondary)]',
    bgClass: 'bg-[color-mix(in_srgb,var(--color-secondary)_10%,transparent)]',
  },
  delete: {
    icon: Trash2,
    label: 'حذف',
    colorClass: 'text-status-closed-fg',
    bgClass: 'bg-status-closed-bg',
  },
  view: {
    icon: Eye,
    label: 'عرض',
    colorClass: 'text-[var(--color-outline)]',
    bgClass: 'bg-[var(--color-surface-container-high)]',
  },
  approve: {
    icon: CheckCircle2,
    label: 'موافقة',
    colorClass: 'text-status-approved-fg',
    bgClass: 'bg-status-approved-bg',
  },
  reject: {
    icon: XCircle,
    label: 'رفض',
    colorClass: 'text-status-closed-fg',
    bgClass: 'bg-status-closed-bg',
  },
  submit: {
    icon: ArrowRight,
    label: 'إرسال للمراجعة',
    colorClass: 'text-status-pending-fg',
    bgClass: 'bg-status-pending-bg',
  },
};

export function AuditTimeline({
  entries,
  className,
}: AuditTimelineProps) {
  return (
    <div className={cn('relative', className)} role="list" aria-label="سجل التدقيق">
      {entries.map((entry, index) => {
        const config = actionConfig[entry.action];
        const ActionIcon = config.icon;
        const isLast = index === entries.length - 1;

        return (
          <div key={entry.id} className="relative pb-6" role="listitem">
            {!isLast && (
              <div
                aria-hidden="true"
                className="absolute top-8 end-4 w-0.5 bottom-0 bg-[var(--color-border-container)] opacity-50"
              />
            )}

            <div className="flex items-start gap-3">
              <div
                aria-hidden="true"
                className={cn(
                  'flex items-center justify-center w-8 h-8 rounded-full shrink-0',
                  config.bgClass
                )}
              >
                <ActionIcon size={14} className={config.colorClass} />
              </div>

              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                  <span
                    className={cn(
                      'text-[0.6875rem] px-2 py-0.5 rounded-full font-medium',
                      config.bgClass,
                      config.colorClass
                    )}
                    role="status"
                  >
                    {config.label}
                  </span>
                  <span className="text-sm font-medium text-[var(--color-on-surface)]">
                    {entry.description}
                  </span>
                </div>

                <div className="flex items-center gap-1.5 mt-1">
                  <User size={12} aria-hidden="true" className="text-[var(--color-outline)]" />
                  <span className="text-xs text-[var(--color-on-surface-variant)]">
                    {entry.user}
                  </span>
                  <span aria-hidden="true" className="text-[var(--color-outline-variant)]">·</span>
                  <time className="text-xs text-[var(--color-outline-variant)]">
                    {entry.timestamp}
                  </time>
                </div>

                {entry.changes && entry.changes.length > 0 && (
                  <div className="mt-2 border border-[var(--color-border-container)] rounded-lg overflow-hidden">
                    <table className="w-full text-xs">
                      <thead>
                        <tr className="bg-[var(--color-surface-container-high)]">
                          <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">
                            الحقل
                          </th>
                          <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">
                            من
                          </th>
                          <th className="px-3 py-2 text-start font-medium text-[var(--color-on-surface-variant)]">
                            إلى
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {entry.changes.map((change, changeIndex) => (
                          <tr
                            key={changeIndex}
                            className={cn(
                              'border-t border-[var(--color-border-container)]',
                              changeIndex % 2 === 0 ? 'bg-[var(--color-surface)]' : 'bg-[var(--color-surface-container-lowest)]'
                            )}
                          >
                            <td className="px-3 py-1.5 font-medium text-[var(--color-on-surface)]">
                              {change.field}
                            </td>
                            <td className="px-3 py-1.5 text-[var(--color-on-surface-variant)]">
                              {change.oldValue || '—'}
                            </td>
                            <td className="px-3 py-1.5 text-[var(--color-on-surface-variant)]">
                              {change.newValue || '—'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}

                {entry.children && (
                  <div className="mt-2">
                    {entry.children}
                  </div>
                )}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
