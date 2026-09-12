import { Sheet } from '@/components/ui/Sheet';
import { Tabs } from '@/components/ui/Tabs';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Skeleton } from '@/components/ui/Loading';
import { useBudgetExecutionDetail } from '@/features/reporting/budget-execution-report/hooks/useBudgetExecutionReport';

interface ReportingBudgetExecutionDetailProps {
  budgetItemId: number | null;
  onClose: () => void;
}

const encumbranceStatusLabels: Record<string, string> = {
  Active: 'نشط',
  PartiallyReleased: 'مُحرَّر جزئياً',
  PartiallyLiquidated: 'مسدَّد جزئياً',
};

const paymentStatusLabels: Record<string, string> = {
  Approved: 'معتمد',
  SentToTreasury: 'مُرسل للخزينة',
  Paid: 'مدفوع',
  PartiallyPaid: 'مدفوع جزئياً',
};

export function ReportingBudgetExecutionDetail({
  budgetItemId,
  onClose,
}: ReportingBudgetExecutionDetailProps) {
  const { data: detail, isLoading, isError, refetch } = useBudgetExecutionDetail(budgetItemId);

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
        </div>
      ) : (
        <Tabs
          defaultKey="encumbrances"
          tabs={[
            {
              key: 'encumbrances',
              label: `التزامات (${detail.encumbrances.length})`,
              content:
                detail.encumbrances.length === 0 ? (
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
                      {detail.encumbrances.map((e) => (
                        <tr key={e.encumbranceId} className="border-b">
                          <td className="py-2">
                            <span dir="ltr" className="font-mono">{e.encumbranceNumber}</span>
                          </td>
                          <td className="py-2 text-sm">
                            {e.encumbranceDate ? new Date(e.encumbranceDate).toLocaleDateString('ar-YE') : '—'}
                          </td>
                          <td className="py-2"><MoneyDisplay value={e.amount} /></td>
                          <td className="py-2">
                            <Badge variant="outline">{encumbranceStatusLabels[e.status] ?? e.status}</Badge>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ),
            },
            {
              key: 'payments',
              label: `مدفوعات (${detail.payments.length})`,
              content:
                detail.payments.length === 0 ? (
                  <EmptyState message="لا توجد مدفوعات لهذا البند" />
                ) : (
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b">
                        <th scope="col" className="py-2 text-start font-medium">رقم الأمر</th>
                        <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                        <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                        <th scope="col" className="py-2 text-start font-medium">الحالة</th>
                      </tr>
                    </thead>
                    <tbody>
                      {detail.payments.map((p) => (
                        <tr key={p.paymentOrderId} className="border-b">
                          <td className="py-2">
                            <span dir="ltr" className="font-mono">{p.orderNumber}</span>
                          </td>
                          <td className="py-2 text-sm">
                            {p.orderDate ? new Date(p.orderDate).toLocaleDateString('ar-YE') : '—'}
                          </td>
                          <td className="py-2"><MoneyDisplay value={p.amount} /></td>
                          <td className="py-2">
                            <Badge variant="outline">{paymentStatusLabels[p.status] ?? p.status}</Badge>
                          </td>
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
