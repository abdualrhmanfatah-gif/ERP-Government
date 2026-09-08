import { CheckCircle, Circle, XCircle } from 'lucide-react';
import { Loading } from '@/components/ui/Loading';

export interface ApprovalStep {
  step: number;
  approverUserId?: number;
  approverName?: string;
  role?: string;
  decision?: string;
  decisionAt?: string | Date;
}

interface PaymentsApprovalStepperProps {
  approvals: ApprovalStep[];
  isLoading?: boolean;
}

function formatDate(value?: string | Date): string {
  if (!value) return '—';
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return new Intl.DateTimeFormat('ar-EG', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

function getDecisionIcon(decision?: string) {
  if (decision === 'Approved') return <CheckCircle size={18} className="text-[var(--color-approved-fg,#16a34a)]" />;
  if (decision === 'Rejected') return <XCircle size={18} className="text-[var(--color-reversed-fg,#dc2626)]" />;
  return <Circle size={18} className="text-[var(--color-on-surface-variant,#71717a)]" />;
}

export function PaymentsApprovalStepper({ approvals, isLoading }: PaymentsApprovalStepperProps) {
  if (isLoading) {
    return <Loading text="جارٍ تحميل سجل الاعتمادات..." />;
  }

  if (approvals.length === 0) {
    return <p className="text-sm text-[var(--color-on-surface-variant)]">لا يوجد سجل اعتمادات بعد</p>;
  }

  return (
    <section aria-label="التوقيعات المزدوجة" className="rounded-lg border border-[var(--color-outline)] p-4">
      <h2 className="mb-4 text-sm font-semibold">التوقيعات المزدوجة</h2>
      <ol className="space-y-0">
        {approvals.map((step, index) => {
          const isLast = index === approvals.length - 1;
          return (
            <li key={`step-${step.step}`} className="flex gap-3">
              <div className="flex flex-col items-center">
                {getDecisionIcon(step.decision)}
                {!isLast && (
                  <div className="mt-1 w-px flex-1 min-h-[24px] bg-[var(--color-outline)]" aria-hidden="true" />
                )}
              </div>
              <div className={`flex-1 ${isLast ? 'pb-0' : 'pb-4'}`}>
                <div className="text-sm font-medium">
                  الخطوة {step.step}
                  {step.role && (
                    <span className="me-2 text-xs text-[var(--color-on-surface-variant)]">
                      ({step.role})
                    </span>
                  )}
                </div>
                <div className="text-xs text-[var(--color-on-surface-variant)]">
                  {step.approverName ?? '—'}
                </div>
                <div className="text-xs text-[var(--color-on-surface-variant)]">
                  {step.decision === 'Approved' && 'اعتمد'}
                  {step.decision === 'Rejected' && 'رفض'}
                  {!step.decision && 'قيد الانتظار'}
                  {' · '}
                  {formatDate(step.decisionAt)}
                </div>
              </div>
            </li>
          );
        })}
      </ol>
    </section>
  );
}
