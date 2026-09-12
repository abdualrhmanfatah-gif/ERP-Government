import { Sheet } from '@/components/ui/Sheet';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Skeleton } from '@/components/ui/Loading';
import { useLedgerMovement } from '@/features/reporting/trial-balance-report/hooks/useTrialBalanceReport';

interface ReportingTrialBalanceDetailProps {
  accountId: number | null;
  fiscalYearId: number;
  onClose: () => void;
}

export function ReportingTrialBalanceDetail({
  accountId,
  fiscalYearId,
  onClose,
}: ReportingTrialBalanceDetailProps) {
  const { data: movement, isLoading, isError, refetch } = useLedgerMovement(accountId, fiscalYearId);

  if (accountId === null) return null;

  return (
    <Sheet
      open={accountId !== null}
      onClose={onClose}
      title={movement ? `${movement.accountCode} — ${movement.accountName}` : 'حركة الحساب'}
    >
      {isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : isLoading || !movement ? (
        <div className="space-y-2">
          <Skeleton className="h-8 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : (
        <>
          {(!movement.entries || movement.entries.length === 0) ? (
            <EmptyState message="لا توجد حركات لهذا الحساب" />
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-start">
                  <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                  <th scope="col" className="py-2 text-start font-medium">رقم القيد</th>
                  <th scope="col" className="py-2 text-start font-medium">المرجع</th>
                  <th scope="col" className="py-2 text-start font-medium">البيان</th>
                  <th scope="col" className="py-2 text-start font-medium">مدين</th>
                  <th scope="col" className="py-2 text-start font-medium">دائن</th>
                </tr>
              </thead>
              <tbody>
                {movement.entries.map((e, i) => (
                  <tr key={i} className="border-b">
                    <td className="py-2 text-sm">{e.documentDate ? new Date(e.documentDate).toLocaleDateString('ar-YE') : '—'}</td>
                    <td className="py-2"><span dir="ltr" className="font-mono">{e.entryNumber ?? '—'}</span></td>
                    <td className="py-2 text-sm">{e.reference ?? '—'}</td>
                    <td className="py-2 text-sm">{e.narration ?? '—'}</td>
                    <td className="py-2"><MoneyDisplay value={e.debit ?? 0} /></td>
                    <td className="py-2"><MoneyDisplay value={e.credit ?? 0} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          {movement.totals && (
            <div className="mt-3 border-t pt-2">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>الإجماليات</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">مدين:</span>
                    <MoneyDisplay value={movement.totals.totalDebit ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">دائن:</span>
                    <MoneyDisplay value={movement.totals.totalCredit ?? 0} />
                  </span>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </Sheet>
  );
}
