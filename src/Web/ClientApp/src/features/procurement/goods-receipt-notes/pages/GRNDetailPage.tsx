import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Dialog, Textarea, ErrorState, Skeleton } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { useGRNDetail, useConfirmGRN, useRejectGRN } from '../hooks/useGRNs';
import { grnStatusLabels, grnStatusVariant } from '../shared/types';

export default function GRNDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const grnId = parseInt(id || '0');
  const { data: grn, isLoading, isError } = useGRNDetail(grnId);
  const confirmGRN = useConfirmGRN();
  const rejectGRN = useRejectGRN();
  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectNotes, setRejectNotes] = useState('');

  if (isLoading) return <Skeleton className="h-96" />;
  if (isError || !grn) return <ErrorState message="لم يتم العثور على إشعار الاستلام" />;

  const handleConfirm = async () => {
    try {
      await confirmGRN.mutateAsync(grnId);
      navigate('/procurement/goods-receipt-notes');
    } catch {
      // Error handled by mutation
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
      // Error handled by mutation
    }
  };

  return (
    <Page title={`إشعار الاستلام — ${grn.grnNumber}`}>
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle>تفاصيل إشعار الاستلام</CardTitle>
            <StatusBadge
              status={grn.status}
              labels={grnStatusLabels}
              variants={grnStatusVariant}
            />
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 mb-6">
            <div>
              <label className="text-sm text-muted-foreground">رقم أمر الشراء</label>
              <p className="font-medium">{grn.purchaseOrderNumber || grn.purchaseOrderId}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">المورد</label>
              <p className="font-medium">{grn.supplierName || '-'}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">المستودع</label>
              <p className="font-medium">{grn.warehouseName || '-'}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">التاريخ</label>
              <p className="font-medium">{new Date(grn.grnDate).toLocaleDateString('ar-YE')}</p>
            </div>
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

          {grn.details && grn.details.length > 0 && (
            <div>
              <h3 className="font-medium mb-2">بنود الاستلام</h3>
              <div className="overflow-x-auto border rounded">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="bg-muted">
                      <th className="border p-2 text-start">الصنف</th>
                      <th className="border p-2 text-start">الوحدة</th>
                      <th className="border p-2 text-start">الكمية المطلوبة</th>
                      <th className="border p-2 text-start">الكمية المستلمة</th>
                      <th className="border p-2 text-start">المقبولة</th>
                      <th className="border p-2 text-start">المرفوضة</th>
                      <th className="border p-2 text-start">المتبقي</th>
                      <th className="border p-2 text-start">سعر الوحدة</th>
                      <th className="border p-2 text-start">الإجمالي</th>
                      <th className="border p-2 text-start">الدفعة</th>
                      <th className="border p-2 text-start">تاريخ الانتهاء</th>
                      <th className="border p-2 text-start">ملاحظات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {grn.details.map((line) => (
                      <tr key={line.id} className="border-b last:border-b-0">
                        <td className="border p-2">{line.itemName || line.itemId}</td>
                        <td className="border p-2">{line.unitName || line.unitId}</td>
                        <td className="border p-2 tabular-nums">{line.orderedQuantity}</td>
                        <td className="border p-2 tabular-nums">{line.receivedQuantity}</td>
                        <td className="border p-2 tabular-nums">{line.acceptedQuantity ?? '-'}</td>
                        <td className="border p-2 tabular-nums">{line.rejectedQuantity ?? '-'}</td>
                        <td className="border p-2 tabular-nums">{line.remainingQuantity}</td>
                        <td className="border p-2 tabular-nums font-mono">{line.unitCost?.toLocaleString('ar-YE') ?? '-'}</td>
                        <td className="border p-2 tabular-nums font-mono font-semibold">{line.totalCost?.toLocaleString('ar-YE') ?? '-'}</td>
                        <td className="border p-2">{line.batchNumber ?? '-'}</td>
                        <td className="border p-2">{line.expiryDate ? new Date(line.expiryDate).toLocaleDateString('ar-YE') : '-'}</td>
                        <td className="border p-2">{line.notes ?? '-'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          <div className="flex gap-2 justify-end mt-6">
            <Button variant="outline" onClick={() => navigate('/procurement/goods-receipt-notes')}>
              رجوع
            </Button>
            {grn.status === 'Draft' && (
              <>
                <Button variant="destructive" onClick={() => setRejectOpen(true)} disabled={rejectGRN.isPending}>
                  رفض
                </Button>
                <Button onClick={handleConfirm} disabled={confirmGRN.isPending}>
                  تأكيد
                </Button>
              </>
            )}
          </div>
        </CardContent>
      </Card>

      <Dialog open={rejectOpen} onClose={() => { setRejectOpen(false); setRejectNotes(''); }} title="رفض إشعار الاستلام">
        <div className="space-y-4">
          <p>هل أنت متأكد من رفض هذا الإشعار؟</p>
          <Textarea
            value={rejectNotes}
            onChange={(e) => setRejectNotes(e.target.value)}
            placeholder="سبب الرفض (اختياري)"
            rows={3}
          />
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => { setRejectOpen(false); setRejectNotes(''); }}>إلغاء</Button>
            <Button variant="destructive" onClick={handleReject} loading={rejectGRN.isPending} disabled={rejectGRN.isPending || !rejectNotes.trim()}>
              رفض
            </Button>
          </div>
        </div>
      </Dialog>
    </Page>
  );
}
