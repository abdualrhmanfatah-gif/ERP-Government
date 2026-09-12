import { Sheet } from '@/components/ui/Sheet';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Skeleton } from '@/components/ui/Loading';
import { useDisbursementRegisterDetail } from '@/features/reporting/disbursement-register-report/hooks/useDisbursementRegisterReport';

interface ReportingDisbursementRegisterDetailProps {
  paymentOrderId: number | null;
  onClose: () => void;
}

const paymentStatusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Submitted: 'مُقدّم',
  Approved: 'معتمد',
  Paid: 'مدفوع',
  Rejected: 'مرفوض',
};

export function ReportingDisbursementRegisterDetail({
  paymentOrderId,
  onClose,
}: ReportingDisbursementRegisterDetailProps) {
  const { data: detail, isLoading, isError, refetch } = useDisbursementRegisterDetail(paymentOrderId);

  if (paymentOrderId === null) return null;

  return (
    <Sheet
      open={paymentOrderId !== null}
      onClose={onClose}
      title={detail ? `${detail.orderNumber} — ${detail.payeeName}` : 'تفاصيل أمر الصرف'}
    >
      {isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : isLoading || !detail ? (
        <div className="space-y-2">
          <Skeleton className="h-8 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      ) : (
        <>
          <div className="mb-4 space-y-1 text-sm">
            <div><span className="text-muted-foreground">رقم الأمر:</span> <span dir="ltr" className="font-mono">{detail.orderNumber}</span></div>
            <div><span className="text-muted-foreground">التاريخ:</span> {detail.orderDate ? new Date(detail.orderDate).toLocaleDateString('ar-YE') : '—'}</div>
            <div><span className="text-muted-foreground">المبلغ:</span> <MoneyDisplay value={detail.amount ?? 0} /></div>
            <div><span className="text-muted-foreground">الحالة:</span> <Badge variant="outline">{paymentStatusLabels[detail.status ?? ''] ?? detail.status}</Badge></div>
            <div><span className="text-muted-foreground">الصندوق:</span> {detail.fundCode ?? '—'}</div>
            {detail.approverName && <div><span className="text-muted-foreground">المعتمد:</span> {detail.approverName}</div>}
            {detail.paidAt && <div><span className="text-muted-foreground">تاريخ الدفع:</span> {new Date(detail.paidAt).toLocaleDateString('ar-YE')}</div>}
          </div>
          <h4 className="mb-2 text-sm font-medium">المدفوعات</h4>
          {!detail.payments || detail.payments.length === 0 ? (
            <EmptyState message="لا توجد مدفوعات لهذا الأمر" />
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-start">
                  <th scope="col" className="py-2 text-start font-medium">رقم الدفعة</th>
                  <th scope="col" className="py-2 text-start font-medium">التاريخ</th>
                  <th scope="col" className="py-2 text-start font-medium">المبلغ</th>
                  <th scope="col" className="py-2 text-start font-medium">الحالة</th>
                </tr>
              </thead>
              <tbody>
                {detail.payments.map((p, i) => (
                  <tr key={i} className="border-b">
                    <td className="py-2"><span dir="ltr" className="font-mono">{p.paymentNumber ?? '—'}</span></td>
                    <td className="py-2 text-sm">{p.paymentDate ? new Date(p.paymentDate).toLocaleDateString('ar-YE') : '—'}</td>
                    <td className="py-2"><MoneyDisplay value={p.amount ?? 0} /></td>
                    <td className="py-2"><Badge variant="outline">{p.status ?? '—'}</Badge></td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </Sheet>
  );
}
