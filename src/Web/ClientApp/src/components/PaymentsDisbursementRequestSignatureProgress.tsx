import { Check } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ApprovalStep } from '@/features/payments/disbursement-requests/shared/types';
import { getSignatureProgress } from '@/features/payments/disbursement-requests/shared/types';

interface PaymentsDisbursementRequestSignatureProgressProps {
  approvals: ApprovalStep[];
  className?: string;
}

export function PaymentsDisbursementRequestSignatureProgress({
  approvals,
  className,
}: PaymentsDisbursementRequestSignatureProgressProps) {
  const validApprovals = approvals.filter(a => a.decision === 'Approved');
  const progress = getSignatureProgress(approvals);

  return (
    <div className={cn('flex items-center gap-3', className)} role="status" aria-label={progress}>
      <div className="flex items-center gap-1.5">
        {[1, 2].map(step => {
          const approval = validApprovals.find(a => a.step === step);
          const isCompleted = !!approval;
          return (
            <div key={step} className="flex items-center gap-1">
              <span
                className={cn(
                  'inline-flex items-center justify-center w-6 h-6 rounded-full text-xs font-medium',
                  isCompleted
                    ? 'bg-[var(--color-primary)] text-[var(--color-on-primary)]'
                    : 'bg-[var(--color-surface-container)] text-[var(--color-on-surface-variant)] border border-[var(--color-outline)]'
                )}
                aria-hidden="true"
              >
                {isCompleted ? <Check size={14} /> : step}
              </span>
              {step === 1 && (
                <span className="text-[var(--color-outline)] mx-0.5" aria-hidden="true">—</span>
              )}
            </div>
          );
        })}
      </div>
      <span className="text-xs text-[var(--color-on-surface-variant)]">{progress}</span>
    </div>
  );
}
