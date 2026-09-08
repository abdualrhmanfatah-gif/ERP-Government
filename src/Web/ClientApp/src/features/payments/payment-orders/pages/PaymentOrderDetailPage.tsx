import { useParams, useNavigate } from 'react-router-dom';
import { usePaymentOrderDetail, usePaymentOrderTotals } from '../hooks/usePaymentOrders';
import {
  paymentOrderStatusLabels,
  budgetCheckStatusLabels,
} from '../shared/types';
import { PaymentsStatusBadge } from '@/components/PaymentsStatusBadge';
import { Page, Button } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export default function PaymentOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const orderId = Number(id);

  const { data: order, isLoading } = usePaymentOrderDetail(orderId);
  const { data: totals } = usePaymentOrderTotals(orderId);

  if (isLoading) {
    return <Page title="جارٍ التحميل..." loading />;
  }

  if (!order) {
    return <Page title="لم يتم العثور على أمر الدفع" />;
  }

  return (
    <Page
      title={`أمر دفع — ${order.paymentOrderNumber ?? ''}`}
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <div className="space-y-6">
        {/* Status badges */}
        <div className="flex gap-3 items-center">
          <PaymentsStatusBadge status={order.status ?? ''} variant="order" />
          <PaymentsStatusBadge status={order.budgetCheckStatus ?? ''} variant="budgetCheck" />
        </div>

        {/* Header info */}
        <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
          <h2 className="text-sm font-semibold mb-4">بيانات أمر الدفع</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div><span className="text-[var(--color-on-surface-variant)]">رقم الأمر:</span> <span className="font-mono">{order.paymentOrderNumber}</span></div>
            <div><span className="text-[var(--color-on-surface-variant)]">التاريخ:</span> {order.paymentOrderDate ? new Date(order.paymentOrderDate).toLocaleDateString('ar-EG') : '—'}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">المورد:</span> {order.beneficiaryName}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">الصندوق:</span> {order.fundId}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">السنة المالية:</span> {order.fiscalYearId}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">التخصيص:</span> {order.appropriationId}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">طريقة الدفع:</span> {order.paymentMethodName ?? '—'}</div>
            <div><span className="text-[var(--color-on-surface-variant)]">المستفيد:</span> {order.beneficiaryName}</div>
          </div>
        </section>

        {/* Lines */}
        {order.lines && order.lines.length > 0 && (
          <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <h2 className="text-sm font-semibold mb-4">الأسطر ({order.lines.length})</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--color-outline)]">
                    <th className="text-start py-2 px-3">#</th>
                    <th className="text-start py-2 px-3">النوع</th>
                    <th className="text-start py-2 px-3">الحساب</th>
                    <th className="text-start py-2 px-3">المبلغ</th>
                  </tr>
                </thead>
                <tbody>
                  {order.lines.map((line, i) => (
                    <tr key={i} className="border-b border-[var(--color-outline-variant)]">
                      <td className="py-2 px-3 font-mono">{line.lineNumber}</td>
                      <td className="py-2 px-3">{line.lineType}</td>
                      <td className="py-2 px-3 font-mono">{line.accountId}</td>
                      <td className="py-2 px-3 font-mono">{line.amount?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {/* Deductions */}
        {order.deductions && order.deductions.length > 0 && (
          <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <h2 className="text-sm font-semibold mb-4">الخصومات ({order.deductions.length})</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--color-outline)]">
                    <th className="text-start py-2 px-3">#</th>
                    <th className="text-start py-2 px-3">النوع</th>
                    <th className="text-start py-2 px-3">الحساب</th>
                    <th className="text-start py-2 px-3">المبلغ</th>
                    <th className="text-start py-2 px-3">إلزامي</th>
                  </tr>
                </thead>
                <tbody>
                  {order.deductions.map((d, i) => (
                    <tr key={i} className="border-b border-[var(--color-outline-variant)]">
                      <td className="py-2 px-3 font-mono">{d.lineNumber}</td>
                      <td className="py-2 px-3">{d.deductionType}</td>
                      <td className="py-2 px-3 font-mono">{d.accountId}</td>
                      <td className="py-2 px-3 font-mono">{d.amount?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</td>
                      <td className="py-2 px-3">{d.isMandatory ? 'نعم' : 'لا'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {/* Totals card */}
        {totals && (
          <section className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <h2 className="text-sm font-semibold mb-3">الملخص المالي</h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              <div><span className="text-[var(--color-on-surface-variant)]">الإجمالي:</span> <span className="font-mono">{totals.amountGross?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">الخصومات:</span> <span className="font-mono">{totals.totalDeductions?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">الصافي:</span> <span className="font-mono">{totals.netAmount?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">المدفوع:</span> <span className="font-mono">{totals.paidAmount?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">المتبقي:</span> <span className="font-mono">{totals.remainingAmount?.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span></div>
              <div><span className="text-[var(--color-on-surface-variant)]">مدفوع بالكامل:</span> {totals.isFullyPaid ? 'نعم' : 'لا'}</div>
            </div>
          </section>
        )}
      </div>
    </Page>
  );
}
