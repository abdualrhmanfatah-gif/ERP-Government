import { Sheet } from '@/components/ui/Sheet';
import { Tabs } from '@/components/ui/Tabs';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Skeleton } from '@/components/ui/Loading';
import { useAvailabilitySnapshotDetail } from '@/features/reporting/availability-snapshot-report/hooks/useAvailabilitySnapshotReport';

interface ReportingAvailabilitySnapshotDetailProps {
  budgetItemId: number | null;
  fiscalYearId: number;
  onClose: () => void;
}

export function ReportingAvailabilitySnapshotDetail({
  budgetItemId,
  fiscalYearId,
  onClose,
}: ReportingAvailabilitySnapshotDetailProps) {
  const { data: detail, isLoading, isError, refetch } = useAvailabilitySnapshotDetail(budgetItemId, fiscalYearId);

  if (budgetItemId === null) return null;

  return (
    <Sheet
      open={budgetItemId !== null}
      onClose={onClose}
      title={detail ? `${detail.itemCode} — ${detail.itemName}` : 'تفاصيل البند'}
    >
      {isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : isLoading || !detail ? (
        <div className="space-y-2">
          <Skeleton className="h-8 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : (
        <Tabs
          defaultKey="appropriations"
          tabs={[
            {
              key: 'appropriations',
              label: `التخصيصات (${detail.appropriations?.length ?? 0})`,
              content:
                !detail.appropriations || detail.appropriations.length === 0 ? (
                  <EmptyState message="لا توجد تخصيصات لهذا البند" />
                ) : (
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b text-start">
                        <th scope="col" className="py-2 text-start font-medium">رقم التخصيص</th>
                        <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                        <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                      </tr>
                    </thead>
                    <tbody>
                      {detail.appropriations.map((a, i) => (
                        <tr key={i} className="border-b">
                          <td className="py-2"><span dir="ltr" className="font-mono">{a.appropriationNumber ?? '—'}</span></td>
                          <td className="py-2 text-sm">{a.appropriationDate ? new Date(a.appropriationDate).toLocaleDateString('ar-EG') : '—'}</td>
                          <td className="py-2"><MoneyDisplay value={a.amount ?? 0} /></td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ),
            },
            {
              key: 'encumbrances',
              label: `الالتزامات (${detail.encumbrances?.length ?? 0})`,
              content:
                !detail.encumbrances || detail.encumbrances.length === 0 ? (
                  <EmptyState message="لا توجد التزامات لهذا البند" />
                ) : (
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b text-start">
                        <th scope="col" className="py-2 text-start font-medium">رقم الالتزام</th>
                        <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                        <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                        <th scope="col" className="py-2 text-start font-medium">الحالة</th>
                      </tr>
                    </thead>
                    <tbody>
                      {detail.encumbrances.map((e, i) => (
                        <tr key={i} className="border-b">
                          <td className="py-2"><span dir="ltr" className="font-mono">{e.encumbranceNumber}</span></td>
                          <td className="py-2 text-sm">{e.encumbranceDate ? new Date(e.encumbranceDate).toLocaleDateString('ar-EG') : '—'}</td>
                          <td className="py-2"><MoneyDisplay value={e.amount ?? 0} /></td>
                          <td className="py-2 text-sm">{e.status ?? '—'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ),
            },
            {
              key: 'payments',
              label: `المدفوعات (${detail.payments?.length ?? 0})`,
              content:
                !detail.payments || detail.payments.length === 0 ? (
                  <EmptyState message="لا توجد مدفوعات لهذا البند" />
                ) : (
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b text-start">
                        <th scope="col" className="py-2 text-start font-medium">رقم الأمر</th>
                        <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                        <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                        <th scope="col" className="py-2 text-start font-medium">الحالة</th>
                      </tr>
                    </thead>
                    <tbody>
                      {detail.payments.map((p, i) => (
                        <tr key={i} className="border-b">
                          <td className="py-2"><span dir="ltr" className="font-mono">{p.orderNumber ?? '—'}</span></td>
                          <td className="py-2 text-sm">{p.orderDate ? new Date(p.orderDate).toLocaleDateString('ar-EG') : '—'}</td>
                          <td className="py-2"><MoneyDisplay value={p.amount ?? 0} /></td>
                          <td className="py-2 text-sm">{p.status ?? '—'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ),
            },
          ]}
        />
      )}
    </Sheet>
  );
}
