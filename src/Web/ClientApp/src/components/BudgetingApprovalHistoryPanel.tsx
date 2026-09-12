import type { ApprovalDecisionDto } from '@/web-api-client';
import { Loading } from '@/components/ui/Loading';
import { formatDateTimeMedium } from '@/shared/utils/formatters';

interface ApprovalHistoryPanelProps {
  decisions: (ApprovalDecisionDto | null | undefined)[];
  isLoading?: boolean;
  title?: string;
}

export function ApprovalHistoryPanel({
  decisions,
  isLoading,
  title = 'سجل الاعتمادات',
}: ApprovalHistoryPanelProps) {
  const items = decisions.filter(
    (d): d is ApprovalDecisionDto => !!d && !!d.decision,
  );

  return (
    <section aria-label={title} className="rounded-lg border border-[var(--color-outline)] p-4">
      <h2 className="mb-3 text-sm font-semibold">{title}</h2>
      {isLoading ? (
        <Loading text="جارٍ التحميل..." />
      ) : items.length === 0 ? (
        <p className="text-sm text-[var(--color-on-surface-variant)]">لا يوجد سجل اعتمادات بعد</p>
      ) : (
        <ol className="space-y-3">
          {items.map((d, index) => (
            <li key={`${d.decision}-${index}`} className="flex items-start gap-3 text-sm">
              <span
                aria-hidden="true"
                className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-[var(--color-primary)]"
              />
              <div>
                <p className="font-medium">{d.decision}</p>
                <p className="text-xs text-[var(--color-on-surface-variant)]">
                  {formatDateTimeMedium(d.decisionAt)} · {d.requiredRole ? `${d.requiredRole} · ` : ''}المستخدم #{d.approverUserId}
                </p>
                {d.reason ? <p className="mt-1 text-xs">{d.reason}</p> : null}
              </div>
            </li>
          ))}
        </ol>
      )}
    </section>
  );
}
