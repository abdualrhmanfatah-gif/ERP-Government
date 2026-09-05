import type { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { Check, X, Clock, ChevronRight, User } from 'lucide-react';

type ApprovalStatus = 'pending' | 'approved' | 'rejected' | 'current';

interface ApprovalStep {
  id: string | number;
  label: string;
  status: ApprovalStatus;
  assignee?: string;
  assigneeAvatar?: string;
  date?: string;
  comment?: string;
  children?: ReactNode;
}

interface ApprovalTimelineProps {
  steps: ApprovalStep[];
  orientation?: 'vertical' | 'horizontal';
  className?: string;
}

const statusConfig: Record<ApprovalStatus, {
  icon: typeof Check;
  label: string;
  bgClass: string;
  iconClass: string;
  connectorClass: string;
}> = {
  pending: {
    icon: Clock,
    label: 'قيد الانتظار',
    bgClass: 'bg-[var(--color-surface-container-high)]',
    iconClass: 'text-[var(--color-outline)]',
    connectorClass: 'bg-[var(--color-border-container)]',
  },
  current: {
    icon: ChevronRight,
    label: 'قيد المراجعة',
    bgClass: 'bg-[color-mix(in_srgb,var(--color-secondary)_10%,transparent)]',
    iconClass: 'text-[var(--color-secondary)]',
    connectorClass: 'bg-[var(--color-border-container)]',
  },
  approved: {
    icon: Check,
    label: 'تمت الموافقة',
    bgClass: 'bg-status-approved-bg',
    iconClass: 'text-status-approved-fg',
    connectorClass: 'bg-status-approved-fg',
  },
  rejected: {
    icon: X,
    label: 'مرفوض',
    bgClass: 'bg-status-closed-bg',
    iconClass: 'text-status-closed-fg',
    connectorClass: 'bg-status-closed-fg',
  },
};

export function ApprovalTimeline({
  steps,
  orientation = 'vertical',
  className,
}: ApprovalTimelineProps) {
  if (orientation === 'horizontal') {
    return (
      <div className={cn('flex items-center', className)} role="list" aria-label="خط الموافقة">
        {steps.map((step, index) => {
          const config = statusConfig[step.status];
          const StatusIcon = config.icon;
          const isLast = index === steps.length - 1;

          return (
            <div key={step.id} className="flex items-center" role="listitem">
              <div className="flex flex-col items-center">
                <div
                  aria-label={`${config.label}: ${step.label}`}
                  className={cn(
                    'flex items-center justify-center w-10 h-10 rounded-full border-2',
                    config.bgClass,
                    step.status === 'approved' && 'border-status-approved-fg',
                    step.status === 'rejected' && 'border-status-closed-fg',
                    step.status === 'current' && 'border-[var(--color-secondary)]',
                    step.status === 'pending' && 'border-[var(--color-border-container)]'
                  )}
                >
                  <StatusIcon
                    size={16}
                    aria-hidden="true"
                    className={config.iconClass}
                  />
                </div>
                <span className="mt-2 text-xs text-[var(--color-on-surface-variant)] text-center max-w-24">
                  {step.label}
                </span>
                {step.assignee && (
                  <span className="text-[0.6875rem] text-[var(--color-outline)] mt-1 text-center">
                    {step.assignee}
                  </span>
                )}
                {step.date && (
                  <span className="text-[0.625rem] text-[var(--color-outline-variant)] mt-0.5 text-center">
                    {step.date}
                  </span>
                )}
              </div>
              {!isLast && (
                <div
                  aria-hidden="true"
                  className={cn(
                    'w-16 h-0.5 mx-2 mb-6',
                    config.connectorClass,
                    step.status === 'approved' ? 'opacity-100' : 'opacity-50'
                  )}
                />
              )}
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <div className={cn('relative', className)} role="list" aria-label="خط الموافقة">
      {steps.map((step, index) => {
        const config = statusConfig[step.status];
        const StatusIcon = config.icon;
        const isLast = index === steps.length - 1;

        return (
          <div key={step.id} className="relative pb-8" role="listitem">
            {!isLast && (
              <div
                aria-hidden="true"
                className={cn(
                  'absolute top-10 end-4 w-0.5 bottom-0',
                  config.connectorClass,
                  step.status === 'approved' ? 'opacity-100' : 'opacity-50'
                )}
              />
            )}

            <div className="flex items-start gap-4">
              <div
                aria-label={`${config.label}: ${step.label}`}
                className={cn(
                  'flex items-center justify-center w-8 h-8 rounded-full border-2 shrink-0',
                  config.bgClass,
                  step.status === 'approved' && 'border-status-approved-fg',
                  step.status === 'rejected' && 'border-status-closed-fg',
                  step.status === 'current' && 'border-[var(--color-secondary)]',
                  step.status === 'pending' && 'border-[var(--color-border-container)]'
                )}
              >
                <StatusIcon
                  size={14}
                  aria-hidden="true"
                  className={config.iconClass}
                />
              </div>

              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-[var(--color-on-surface)]">
                    {step.label}
                  </span>
                  <span
                    role="status"
                    className={cn(
                      'text-[0.6875rem] px-2 py-0.5 rounded-full',
                      config.bgClass,
                      config.iconClass
                    )}
                  >
                    {config.label}
                  </span>
                </div>

                {step.assignee && (
                  <div className="flex items-center gap-1.5 mt-1">
                    <User size={12} aria-hidden="true" className="text-[var(--color-outline)]" />
                    <span className="text-xs text-[var(--color-on-surface-variant)]">
                      {step.assignee}
                    </span>
                    {step.date && (
                      <>
                        <span aria-hidden="true" className="text-[var(--color-outline-variant)]">·</span>
                        <time className="text-xs text-[var(--color-outline-variant)]">
                          {step.date}
                        </time>
                      </>
                    )}
                  </div>
                )}

                {step.comment && (
                  <p className="mt-2 text-xs text-[var(--color-on-surface-variant)] bg-[var(--color-surface-container-low)] rounded-lg p-3 border border-[var(--color-border-container)]">
                    {step.comment}
                  </p>
                )}

                {step.children && (
                  <div className="mt-2">
                    {step.children}
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
