import { Sheet } from '@/components/ui/Sheet';
import { Tabs } from '@/components/ui/Tabs';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Skeleton } from '@/components/ui/Loading';
import { useRevenueCollectionsDetail } from '@/features/reporting/revenue-collections-report/hooks/useRevenueCollectionsReport';

interface ReportingRevenueCollectionsDetailProps {
  receiptVoucherId: number | null;
  onClose: () => void;
}

const depositStatusLabels: Record<string, string> = {
  Pending: 'قيد الانتظار',
  Deposited: 'تم الإيداع',
  Rejected: 'مرفوض',
};

export function ReportingRevenueCollectionsDetail({
  receiptVoucherId,
  onClose,
}: ReportingRevenueCollectionsDetailProps) {
  const { data: detail, isLoading, isError, refetch } = useRevenueCollectionsDetail(receiptVoucherId);

  if (receiptVoucherId === null) return null;

  return (
    <Sheet
      open={receiptVoucherId !== null}
      onClose={onClose}
      title={detail ? `${detail.voucherNumber} — ${detail.partyName}` : 'تفاصيل سند القبض'}
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
        <>
          <div className="mb-4 space-y-1 text-sm">
            <div><span className="text-muted-foreground">التاريخ:</span> {detail.voucherDate ? new Date(detail.voucherDate).toLocaleDateString('ar-YE') : '—'}</div>
            <div><span className="text-muted-foreground">المبلغ:</span> <MoneyDisplay value={detail.totalAmount ?? 0} /></div>
            <div><span className="text-muted-foreground">طريقة الدفع:</span> {detail.paymentMethod ?? '—'}</div>
            {detail.depositSlipNumber && (
              <div>
                <span className="text-muted-foreground">بطاقة الإيداع:</span> {detail.depositSlipNumber}
                {detail.depositSlipStatus && (
                  <Badge variant="outline" className="ms-2">{depositStatusLabels[detail.depositSlipStatus] ?? detail.depositSlipStatus}</Badge>
                )}
              </div>
            )}
          </div>
          <Tabs
            defaultKey="lines"
            tabs={[
              {
                key: 'lines',
                label: `البنود (${detail.lines?.length ?? 0})`,
                content:
                  !detail.lines || detail.lines.length === 0 ? (
                    <EmptyState message="لا توجد بنود لهذا السند" />
                  ) : (
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b text-start">
                          <th scope="col" className="py-2 text-start font-medium">كود الحساب</th>
                          <th scope="col" className="py-2 text-start font-medium">اسم الحساب</th>
                          <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                        </tr>
                      </thead>
                      <tbody>
                        {detail.lines.map((l, i) => (
                          <tr key={i} className="border-b">
                            <td className="py-2"><span dir="ltr" className="font-mono">{l.accountCode}</span></td>
                            <td className="py-2 text-sm">{l.accountName}</td>
                            <td className="py-2"><MoneyDisplay value={l.amount ?? 0} /></td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  ),
              },
              {
                key: 'checks',
                label: `الشيكات (${detail.checks?.length ?? 0})`,
                content:
                  !detail.checks || detail.checks.length === 0 ? (
                    <EmptyState message="لا توجد شيكات لهذا السند" />
                  ) : (
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b text-start">
                          <th scope="col" className="py-2 text-start font-medium">رقم الشيك</th>
                          <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                          <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                          <th scope="col" className="py-2 text-start font-medium">البنك</th>
                        </tr>
                      </thead>
                      <tbody>
                        {detail.checks.map((c, i) => (
                          <tr key={i} className="border-b">
                            <td className="py-2"><span dir="ltr" className="font-mono">{c.checkNumber}</span></td>
                            <td className="py-2 text-sm">{c.checkDate ? new Date(c.checkDate).toLocaleDateString('ar-YE') : '—'}</td>
                            <td className="py-2"><MoneyDisplay value={c.amount ?? 0} /></td>
                            <td className="py-2 text-sm">{c.bankName ?? '—'}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  ),
              },
            ]}
          />
        </>
      )}
    </Sheet>
  );
}
