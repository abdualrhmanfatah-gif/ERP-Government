import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Dialog, Textarea, Skeleton } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { MetaItem } from '@/components/MetaItem';
import { useGRNDetail, useConfirmGRN, useRejectGRN } from '../hooks/useGRNs';
import { grnStatusLabels, grnStatusVariant } from '../shared/types';
import { usePermission } from '@/shared/hooks/usePermission';
import { PROCUREMENT_PERMISSIONS } from '@/shared/constants/permissions';
import { ArrowRight } from 'lucide-react';

export default function GRNDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const grnId = parseInt(id || '0');
  const { data: grn, isLoading, isError, refetch } = useGRNDetail(grnId);
  const confirmGRN = useConfirmGRN();
  const rejectGRN = useRejectGRN();
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectNotes, setRejectNotes] = useState('');

  const { hasPermission: canConfirm } = usePermission(PROCUREMENT_PERMISSIONS.GoodsReceipts.Confirm);
  const { hasPermission: canReject } = usePermission(PROCUREMENT_PERMISSIONS.GoodsReceipts.Reject);

  if (isLoading) return <Skeleton className="h-96" />;
  if (isError || !grn) return <Page title="إشعار الاستلام" error="لم يتم العثور على إشعار الاستلام" onRetry={() => refetch()} />;

  const handleConfirm = async () => {
    try {
      await confirmGRN.mutateAsync(grnId);
      navigate('/procurement/goods-receipt-notes');
    } catch {
      // error handled by mutation onError
    }
  };

  const handleReject = async () => {
    if (!rejectNotes.trim()) return;
    try {
      await rejectGRN.mutateAsync({ id: grnId, notes: rejectNotes });
      setRejectOpen(false);
      setRejectNotes('');
      navigate('/procurement/goods-receipt-notes');
    } catch {
      // error handled by mutation onError
    }
  };

  const status = grn.status;
  const isDraft = status === 'Draft';

  const headerActions = (
    <div className="flex items-center gap-2 flex-wrap">
      {isDraft && canReject && (
        <Button variant="destructive" size="sm" onClick={() => setRejectOpen(true)} disabled={rejectGRN.isPending} loading={rejectGRN.isPending}>
          رفض
        </Button>
      )}
      {isDraft && canConfirm && (
        <Button variant="primary" size="sm" onClick={handleConfirm} disabled={confirmGRN.isPending} loading={confirmGRN.isPending}>
          تأكيد
        </Button>
      )}
      <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
        <ArrowRight size={18} />
      </Button>
    </div>
  );

  return (
    <Page
      title={`إشعار الاستلام — ${grn.grnNumber}`}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <StatusBadge variant={grnStatusVariant[status]}>
            {grnStatusLabels[status]}
          </StatusBadge>
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="رقم الإشعار" value={grn.grnNumber} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={new Date(grn.grnDate).toLocaleDateString('ar-YE')} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="أمر الشراء" value={grn.purchaseOrderNumber || grn.purchaseOrderId} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="المورد" value={grn.supplierName || '-'} />
        </div>
      }
    >
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* العمود الأيسر: البنود */}
        <div className="lg:col-span-8 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>تفاصيل إشعار الاستلام</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4 mb-6">
                <div>
                  <label className="text-sm text-muted-foreground">المستودع</label>
                  <p className="font-medium">{grn.warehouseName || '-'}</p>
                </div>
                {grn.locationName && (
                  <div>
                    <label className="text-sm text-muted-foreground">الموقع</label>
                    <p className="font-medium">{grn.locationName}</p>
                  </div>
                )}
                {grn.receivedByName && (
                  <div>
                    <label className="text-sm text-muted-foreground">تم الاستلام بواسطة</label>
                    <p className="font-medium">{grn.receivedByName}</p>
                  </div>
                )}
                {grn.notes && (
                  <div className="col-span-2">
                    <label className="text-sm text-muted-foreground">ملاحظات</label>
                    <p className="font-medium">{grn.notes}</p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {grn.details && grn.details.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>بنود الاستلام</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm border-collapse">
                    <thead>
                      <tr className="border-b-2 border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
                        <th className="px-4 py-3 text-start font-semibold">#</th>
                        <th className="px-4 py-3 text-start font-semibold">الصنف</th>
                        <th className="px-4 py-3 text-start font-semibold">الوحدة</th>
                        <th className="px-4 py-3 text-start font-semibold">المطلوب</th>
                        <th className="px-4 py-3 text-start font-semibold">المستلم</th>
                        <th className="px-4 py-3 text-start font-semibold">المقبولة</th>
                        <th className="px-4 py-3 text-start font-semibold">المرفوضة</th>
                        <th className="px-4 py-3 text-start font-semibold">المتبقي</th>
                        <th className="px-4 py-3 text-start font-semibold">سعر الوحدة</th>
                        <th className="px-4 py-3 text-start font-semibold">الإجمالي</th>
                        <th className="px-4 py-3 text-start font-semibold">الدفعة</th>
                        <th className="px-4 py-3 text-start font-semibold">تاريخ الانتهاء</th>
                        <th className="px-4 py-3 text-start font-semibold">ملاحظات</th>
                      </tr>
                    </thead>
                    <tbody>
                      {grn.details.map((line, idx) => (
                        <tr key={line.id} className="border-b border-[var(--color-outline-variant)] last:border-b-0">
                          <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{idx + 1}</td>
                          <td className="px-4 py-3">{line.itemName || line.itemId}</td>
                          <td className="px-4 py-3">{line.unitName || line.unitId}</td>
                          <td className="px-4 py-3 tabular-nums">{line.orderedQuantity}</td>
                          <td className="px-4 py-3 tabular-nums font-semibold">{line.receivedQuantity}</td>
                          <td className="px-4 py-3 tabular-nums">{line.acceptedQuantity ?? '-'}</td>
                          <td className="px-4 py-3 tabular-nums">{line.rejectedQuantity ?? '-'}</td>
                          <td className="px-4 py-3 tabular-nums">{line.remainingQuantity}</td>
                          <td className="px-4 py-3 tabular-nums font-mono">{line.unitCost?.toLocaleString('ar-YE') ?? '-'}</td>
                          <td className="px-4 py-3 tabular-nums font-mono font-semibold">{line.totalCost?.toLocaleString('ar-YE') ?? '-'}</td>
                          <td className="px-4 py-3">{line.batchNumber ?? '-'}</td>
                          <td className="px-4 py-3">{line.expiryDate ? new Date(line.expiryDate).toLocaleDateString('ar-YE') : '-'}</td>
                          <td className="px-4 py-3">{line.notes ?? '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        {/* العمود الأيمن: معلومات إضافية */}
        <div className="lg:col-span-4 space-y-6">
          <Card>
            <h2 className="text-sm font-semibold mb-4 px-4 pt-4">ملخص الاستلام</h2>
            <div className="space-y-2 text-sm px-4 pb-4">
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">عدد البنود</span>
                <span className="font-semibold">{grn.details?.length ?? 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">إجمالي المستلم</span>
                <span className="font-semibold tabular-nums">{grn.details?.reduce((s, l) => s + l.receivedQuantity, 0) ?? 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">إجمالي المقبولة</span>
                <span className="font-semibold tabular-nums">{grn.details?.reduce((s, l) => s + (l.acceptedQuantity ?? 0), 0) ?? 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">إجمالي المرفوضة</span>
                <span className="font-semibold tabular-nums text-[var(--color-error)]">{grn.details?.reduce((s, l) => s + (l.rejectedQuantity ?? 0), 0) ?? 0}</span>
              </div>
            </div>
          </Card>
        </div>
      </div>

      {/* نافذة الرفض */}
      <Dialog
        open={rejectOpen}
        onClose={() => { setRejectOpen(false); setRejectNotes(''); }}
        title="رفض إشعار الاستلام"
        footer={
          <>
            <Button variant="outline" onClick={() => { setRejectOpen(false); setRejectNotes(''); }}>إلغاء</Button>
            <Button variant="destructive" onClick={handleReject} loading={rejectGRN.isPending} disabled={rejectGRN.isPending || !rejectNotes.trim()}>
              رفض
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الرفض"
          value={rejectNotes}
          onChange={(e) => setRejectNotes(e.target.value)}
          placeholder="اذكر سبب الرفض..."
          rows={3}
        />
      </Dialog>
    </Page>
  );
}
