import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { Button, Badge, EmptyState, MoneyDisplay, Page, Dialog } from '@/components/ui';
import { CollectionOrdersClient, ApproveCollectionOrderCommand } from '@/web-api-client';
import { collectionOrderStatusLabels, collectionOrderStatusBadgeVariant } from '../../shared/types';
import { notify } from '@/features/notifications/notify';
import { formatDate } from '@/shared/utils/formatters';

const client = new CollectionOrdersClient();

export default function CollectionOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const orderId = Number(id);
  const [approveOpen, setApproveOpen] = useState(false);

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ['collection-orders', 'detail', orderId],
    queryFn: () => client.collectionOrdersAll(undefined, undefined),
    enabled: orderId > 0,
  });

  const order = orders.find((o) => o.id === orderId);

  const approveMutation = useMutation({
    mutationFn: (cmd: ApproveCollectionOrderCommand) => client.approvePOST4(orderId, cmd),
    onSuccess: () => {
      notify({ type: 'success', title: 'تم اعتماد أمر التحصيل بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['collection-orders'] });
      setApproveOpen(false);
    },
    onError: (err: unknown) => {
      let msg = 'حدث خطأ أثناء الاعتماد';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string };
        try { const b = JSON.parse(ex.response); msg = Array.isArray(b) ? b.join('\n') : (b.detail ?? b.title ?? ex.message); } catch { msg = ex.message; }
      }
      notify({ type: 'error', title: msg });
    },
  });

  const isDraft = order?.status === 'Draft';
  const canCreateVoucher = order?.status === 'Approved' || order?.status === 'PartiallyCollected';

  const headerActions = order ? (
    <div className="flex items-center gap-2">
      <Badge variant={collectionOrderStatusBadgeVariant[order.status ?? 'Draft']}>
        {collectionOrderStatusLabels[order.status ?? 'Draft']}
      </Badge>
      {isDraft && (
        <Button onClick={() => setApproveOpen(true)}>
          اعتماد أمر التحصيل
        </Button>
      )}
      {canCreateVoucher && (
        <Button onClick={() => navigate(`/treasury/receipt-vouchers/create?orderId=${order.id}`)}>
          إنشاء سند قبض
        </Button>
      )}
    </div>
  ) : undefined;

  return (
    <Page
      title={order ? `أمر تحصيل ${order.orderNumber ?? ''}` : 'تفاصيل أمر التحصيل'}
      onBack={() => navigate('/treasury/collection-orders')}
      loading={isLoading}
      actions={headerActions}
    >
      {!order ? (
        <EmptyState message="لم يتم العثور على أمر التحصيل" />
      ) : (
        <div className="space-y-6">
          <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">بيانات أمر التحصيل</h3>
              <Badge variant={collectionOrderStatusBadgeVariant[order.status ?? 'Draft']}>
                {collectionOrderStatusLabels[order.status ?? 'Draft']}
              </Badge>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
              <div>
                <span className="text-[var(--color-on-surface-variant)]">رقم الأمر:</span>{' '}
                <span className="font-medium tabular-nums">{order.orderNumber}</span>
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">التاريخ:</span>{' '}
                <span>{formatDate(order.orderDate)}</span>
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">رقم المطالبة:</span>{' '}
                <span className="text-[var(--color-primary)] cursor-pointer" onClick={() => navigate(`/treasury/revenue-claims/${order.revenueClaimId}`)}>
                  {order.revenueClaimNumber}
                </span>
              </div>
            </div>
          </div>

          <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <h3 className="text-lg font-semibold mb-4">ملخص المبالغ</h3>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ المفوض</p>
                <MoneyDisplay value={order.authorizedAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المحصّل</p>
                <MoneyDisplay value={order.collectedAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">تحت التحصيل</p>
                <MoneyDisplay value={order.underCollectionAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المتاح</p>
                <MoneyDisplay value={order.availableAmount ?? 0} />
              </div>
            </div>
          </div>

          {order.receiptVouchers && order.receiptVouchers.length > 0 && (
            <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
              <h3 className="mb-3 text-sm font-medium">سندات القبض المرتبطة</h3>
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-xs text-[var(--color-on-surface-variant)]">
                    <th className="py-2 text-start">رقم السند</th>
                    <th className="py-2 text-start">التاريخ</th>
                    <th className="py-2 text-start">طريقة الدفع</th>
                    <th className="py-2 text-end">الإجمالي</th>
                    <th className="py-2 text-end">الحالة</th>
                  </tr>
                </thead>
                <tbody>
                  {order.receiptVouchers.map((voucher) => (
                    <tr
                      key={voucher.id}
                      className="border-t border-[var(--color-outline-variant)] cursor-pointer hover:bg-[var(--color-surface-container)]"
                      onClick={() => navigate(`/treasury/receipt-vouchers/${voucher.id}`)}
                    >
                      <td className="py-2 font-medium tabular-nums">{voucher.voucherNumber}</td>
                      <td className="py-2">{formatDate(voucher.voucherDate)}</td>
                      <td className="py-2">{voucher.paymentMethodName}</td>
                      <td className="py-2 text-end"><MoneyDisplay value={voucher.totalAmount ?? 0} /></td>
                      <td className="py-2 text-end">{voucher.statusName ?? voucher.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {order.notes && (
            <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
              <h3 className="mb-2 text-sm font-medium">ملاحظات</h3>
              <p className="text-sm text-[var(--color-on-surface-variant)]">{order.notes}</p>
            </div>
          )}
        </div>
      )}

      <Dialog open={approveOpen} onOpenChange={setApproveOpen} title="اعتماد أمر التحصيل">
        <p className="text-sm text-[var(--color-on-surface-variant)] mb-4">
          هل أنت متأكد من اعتماد أمر التحصيل رقم <strong>{order?.orderNumber}</strong>؟
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={() => setApproveOpen(false)}>إلغاء</Button>
          <Button
            loading={approveMutation.isPending}
            onClick={() => approveMutation.mutate({ id: orderId, rowVersion: order?.rowVersion ?? [] } as ApproveCollectionOrderCommand)}
          >
            اعتماد
          </Button>
        </div>
      </Dialog>
    </Page>
  );
}
