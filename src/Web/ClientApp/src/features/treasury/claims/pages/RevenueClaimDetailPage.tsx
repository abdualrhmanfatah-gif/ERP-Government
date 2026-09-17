import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient, useMutation } from '@tanstack/react-query';
import { Button, Badge, EmptyState, MoneyDisplay, Page, Dialog, Input } from '@/components/ui';
import { RevenueClaimsClient, ApproveRevenueClaimCommand, WriteOffRevenueClaimCommand } from '@/web-api-client';
import { claimStatusLabels, claimStatusBadgeVariant } from '../../shared/types';
import { notify } from '@/features/notifications/notify';
import { formatDate } from '@/shared/utils/formatters';

const client = new RevenueClaimsClient();

export default function RevenueClaimDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const claimId = Number(id);
  const [approveOpen, setApproveOpen] = useState(false);
  const [writeOffOpen, setWriteOffOpen] = useState(false);
  const [reason, setReason] = useState('');

  const { data: claims = [], isLoading } = useQuery({
    queryKey: ['revenue-claims', 'detail', claimId],
    queryFn: () => client.revenueClaimsAll(undefined, undefined),
    enabled: claimId > 0,
  });

  const claim = claims.find((c) => c.id === claimId);

  const approveMutation = useMutation({
    mutationFn: (cmd: ApproveRevenueClaimCommand) => client.approvePOST8(claimId, cmd),
    onSuccess: () => {
      notify({ type: 'success', title: 'تم اعتماد المطالبة بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['revenue-claims'] });
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

  const writeOffMutation = useMutation({
    mutationFn: (cmd: WriteOffRevenueClaimCommand) => client.writeOff(claimId, cmd),
    onSuccess: () => {
      notify({ type: 'success', title: 'تم شطب المطالبة بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['revenue-claims'] });
      setWriteOffOpen(false);
      setReason('');
    },
    onError: (err: unknown) => {
      let msg = 'حدث خطأ أثناء الشطب';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string };
        try { const b = JSON.parse(ex.response); msg = Array.isArray(b) ? b.join('\n') : (b.detail ?? b.title ?? ex.message); } catch { msg = ex.message; }
      }
      notify({ type: 'error', title: msg });
    },
  });

  const isDraft = claim?.status === 'Draft';
  const canCreateOrder = claim?.status === 'Open' || claim?.status === 'PartiallySettled';

  const headerActions = claim ? (
    <div className="flex items-center gap-2">
      <Badge variant={claimStatusBadgeVariant[claim.status ?? 'Draft']}>
        {claimStatusLabels[claim.status ?? 'Draft']}
      </Badge>
      {isDraft && (
        <Button onClick={() => setApproveOpen(true)}>
          اعتماد المطالبة
        </Button>
      )}
      {canCreateOrder && (
        <Button onClick={() => navigate(`/treasury/collection-orders/create?claimId=${claim.id}`)}>
          إصدار أمر تحصيل
        </Button>
      )}
      {canCreateOrder && (
        <Button variant="destructive" onClick={() => setWriteOffOpen(true)}>
          شطب المطالبة
        </Button>
      )}
    </div>
  ) : undefined;

  return (
    <Page
      title={claim ? `مطالبة ${claim.claimNumber ?? ''}` : 'تفاصيل المطالبة'}
      onBack={() => navigate('/treasury/revenue-claims')}
      loading={isLoading}
      actions={headerActions}
    >
      {!claim ? (
        <EmptyState message="لم يتم العثور على المطالبة" />
      ) : (
        <div className="space-y-6">
          <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold">بيانات المطالبة</h3>
              <Badge variant={claimStatusBadgeVariant[claim.status ?? 'Draft']}>
                {claimStatusLabels[claim.status ?? 'Draft']}
              </Badge>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
              <div>
                <span className="text-[var(--color-on-surface-variant)]">رقم المطالبة:</span>{' '}
                <span className="font-medium tabular-nums">{claim.claimNumber}</span>
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">التاريخ:</span>{' '}
                <span>{formatDate(claim.claimDate)}</span>
              </div>
              <div>
                <span className="text-[var(--color-on-surface-variant)]">الجهة:</span>{' '}
                <span>{claim.partyName}</span>
              </div>
            </div>
          </div>

          <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6">
            <h3 className="text-lg font-semibold mb-4">ملخص المبالغ</h3>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المبلغ الإجمالي</p>
                <MoneyDisplay value={claim.totalAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المحصّل</p>
                <MoneyDisplay value={claim.collectedAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">تحت التحصيل</p>
                <MoneyDisplay value={claim.underCollectionAmount ?? 0} />
              </div>
              <div className="text-center p-3 rounded bg-[var(--color-surface-container)]">
                <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">المتاح للتحصيل</p>
                <span className="text-[var(--color-success)]">
                  <MoneyDisplay value={claim.availableAmount ?? 0} />
                </span>
              </div>
            </div>
          </div>

          {claim.collectionOrders && claim.collectionOrders.length > 0 && (
            <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
              <h3 className="mb-3 text-sm font-medium">أوامر التحصيل المرتبطة</h3>
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-xs text-[var(--color-on-surface-variant)]">
                    <th className="py-2 text-start">رقم الأمر</th>
                    <th className="py-2 text-start">التاريخ</th>
                    <th className="py-2 text-end">المبلغ المفوض</th>
                    <th className="py-2 text-end">المحصّل</th>
                    <th className="py-2 text-end">المتاح</th>
                    <th className="py-2 text-end">الحالة</th>
                  </tr>
                </thead>
                <tbody>
                  {claim.collectionOrders.map((order) => (
                    <tr key={order.id} className="border-t border-[var(--color-outline-variant)]">
                      <td className="py-2 font-medium tabular-nums">{order.orderNumber}</td>
                      <td className="py-2">{formatDate(order.orderDate)}</td>
                      <td className="py-2 text-end"><MoneyDisplay value={order.authorizedAmount ?? 0} /></td>
                      <td className="py-2 text-end"><MoneyDisplay value={order.collectedAmount ?? 0} /></td>
                      <td className="py-2 text-end"><MoneyDisplay value={order.availableAmount ?? 0} /></td>
                      <td className="py-2 text-end">
                        <Badge variant={claimStatusBadgeVariant[order.status ?? 'Draft'] ?? 'secondary'}>
                          {order.statusName ?? order.status}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {claim.notes && (
            <div className="rounded-lg border border-[var(--color-outline-variant)] p-6">
              <h3 className="mb-2 text-sm font-medium">ملاحظات</h3>
              <p className="text-sm text-[var(--color-on-surface-variant)]">{claim.notes}</p>
            </div>
          )}
        </div>
      )}

      <Dialog open={approveOpen} onOpenChange={setApproveOpen} title="اعتماد المطالبة">
        <p className="text-sm text-[var(--color-on-surface-variant)] mb-4">
          هل أنت متأكد من اعتماد المطالبة رقم <strong>{claim?.claimNumber}</strong>؟
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={() => setApproveOpen(false)}>إلغاء</Button>
          <Button
            loading={approveMutation.isPending}
            onClick={() => approveMutation.mutate({ id: claimId, rowVersion: claim?.rowVersion ?? [] } as ApproveRevenueClaimCommand)}
          >
            اعتماد
          </Button>
        </div>
      </Dialog>

      <Dialog open={writeOffOpen} onOpenChange={setWriteOffOpen} title="شطب المطالبة">
        <p className="text-sm text-[var(--color-on-surface-variant)] mb-4">
          هل أنت متأكد من شطب المطالبة رقم <strong>{claim?.claimNumber}</strong>؟
        </p>
        <Input
          label="سبب الشطب"
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          className="mb-4"
        />
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={() => { setWriteOffOpen(false); setReason(''); }}>إلغاء</Button>
          <Button
            variant="destructive"
            loading={writeOffMutation.isPending}
            onClick={() => writeOffMutation.mutate({ id: claimId, rowVersion: claim?.rowVersion ?? [], reason: reason || undefined } as WriteOffRevenueClaimCommand)}
          >
            شطب
          </Button>
        </div>
      </Dialog>
    </Page>
  );
}
