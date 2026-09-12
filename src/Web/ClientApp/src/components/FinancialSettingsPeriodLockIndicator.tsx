import { Lock, Unlock } from 'lucide-react';
import { fiscalPeriodLockLabels } from '@/features/financial-settings/shared/types';

interface PeriodLockIndicatorProps {
  isLocked: boolean;
}

export function PeriodLockIndicator({ isLocked }: PeriodLockIndicatorProps) {
  return (
    <span className="inline-flex items-center gap-1 text-sm">
      {isLocked ? (
        <Lock size={14} className="text-[var(--color-error)]" aria-hidden="true" />
      ) : (
        <Unlock size={14} className="text-[var(--color-success)]" aria-hidden="true" />
      )}
      <span className={isLocked ? 'text-[var(--color-error)]' : 'text-[var(--color-success)]'}>
        {isLocked ? fiscalPeriodLockLabels.locked : fiscalPeriodLockLabels.unlocked}
      </span>
    </span>
  );
}
