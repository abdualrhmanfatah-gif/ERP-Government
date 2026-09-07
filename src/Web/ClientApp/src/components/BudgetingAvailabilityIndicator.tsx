import { useQuery } from '@tanstack/react-query';
import { cn } from '@/lib/utils';
import { appropriationsClient, encumbrancesClient, budgetingKeys } from '@/features/budgeting/shared/client';
import { BudgetControlMethod } from '@/features/budgeting/shared/types';

type AvailabilityTone = 'green' | 'amber' | 'red';

const toneClasses: Record<AvailabilityTone, string> = {
  green: 'bg-[var(--status-active-bg)] text-[var(--status-active-fg)]',
  amber: 'bg-[var(--status-pending-bg)] text-[var(--status-pending-fg)]',
  red: 'bg-[var(--color-error-container)] text-[var(--color-on-error-container)]',
};

function resolveTone(available: number, controlMethod: BudgetControlMethod, hasWarning: boolean): AvailabilityTone {
  if (controlMethod === BudgetControlMethod.None) return 'green';
  if (hasWarning) return controlMethod === BudgetControlMethod.Blocking ? 'red' : 'amber';
  if (available < 0) return controlMethod === BudgetControlMethod.Blocking ? 'red' : 'amber';
  return 'green';
}

interface AvailabilityIndicatorBaseProps {
  className?: string;
}

interface FetchProps extends AvailabilityIndicatorBaseProps {
  budgetItemId?: number;
  appropriationId?: number;
}

interface InlineProps extends AvailabilityIndicatorBaseProps {
  available: number;
  controlMethod: BudgetControlMethod;
  warning?: string | null;
}

type AvailabilityIndicatorProps = FetchProps | InlineProps;

function isInlineProps(props: AvailabilityIndicatorProps): props is InlineProps {
  return 'available' in props;
}

export function AvailabilityIndicator(props: AvailabilityIndicatorProps) {
  if (isInlineProps(props)) {
    return <InlineIndicator {...props} />;
  }
  return <FetchIndicator {...props} />;
}

function InlineIndicator({ available, controlMethod, warning, className }: InlineProps) {
  const tone = resolveTone(available, controlMethod, !!warning);
  const label = `المتاح: ${available.toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

  return (
    <span
      role="status"
      aria-label={warning ?? label}
      title={warning ?? undefined}
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-3 py-0.5 text-xs font-medium whitespace-nowrap',
        toneClasses[tone],
        className,
      )}
    >
      <span aria-hidden="true">●</span>
      {label}
      {warning ? <span className="font-normal">({warning})</span> : null}
    </span>
  );
}

function FetchIndicator({ budgetItemId, appropriationId, className }: FetchProps) {
  const isEncumbrance = appropriationId !== undefined;

  const { data, isLoading } = useQuery({
    queryKey: isEncumbrance
      ? budgetingKeys.encumbrances.availability(appropriationId!)
      : budgetingKeys.appropriations.availability(budgetItemId!),
    queryFn: () =>
      isEncumbrance
        ? encumbrancesClient.getAvailability(appropriationId!)
        : appropriationsClient.getAvailability(budgetItemId!),
    enabled: isEncumbrance ? Number.isFinite(appropriationId) : Number.isFinite(budgetItemId),
  });

  if (isLoading) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full px-3 py-0.5 text-xs text-[var(--color-on-surface-variant)] bg-[var(--color-surface-container)] whitespace-nowrap">
        <span aria-hidden="true">●</span>
        جاري التحميل...
      </span>
    );
  }

  if (!data) {
    return (
      <span className="inline-flex items-center gap-1 rounded-full px-3 py-0.5 text-xs text-[var(--color-on-surface-variant)] bg-[var(--color-surface-container)] whitespace-nowrap">
        <span aria-hidden="true">●</span>
        المتاح: —
      </span>
    );
  }

  const tone = resolveTone(data.available, data.controlMethod, !!data.warning);
  const label = `المتاح: ${data.available.toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

  return (
    <span
      role="status"
      aria-label={data.warning ?? label}
      title={data.warning ?? undefined}
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-3 py-0.5 text-xs font-medium whitespace-nowrap',
        toneClasses[tone],
        className,
      )}
    >
      <span aria-hidden="true">●</span>
      {label}
      {data.warning ? <span className="font-normal">({data.warning})</span> : null}
    </span>
  );
}
