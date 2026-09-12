import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Page, Button, Card, CardContent, CardHeader, CardTitle, Dialog, Textarea, Skeleton } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { MetaItem } from '@/components/MetaItem';
import { ArrowRight } from 'lucide-react';
import { useSupplierInvoiceDetail, useSubmitSupplierInvoice, useMatchSupplierInvoice, useCancelSupplierInvoice } from '../hooks/useSupplierInvoices';
import { supplierInvoiceStatusLabels, supplierInvoiceStatusVariant, type SupplierInvoiceStatus } from '../shared/types';

export default function SupplierInvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const invoiceId = parseInt(id || '0');
  const { data: invoice, isLoading, isError, refetch } = useSupplierInvoiceDetail(invoiceId);
  const submitMutation = useSubmitSupplierInvoice();
  const matchMutation = useMatchSupplierInvoice();
  const cancelMutation = useCancelSupplierInvoice();
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelNotes, setCancelNotes] = useState('');

  if (isLoading) return <Skeleton className="h-96" />;
  if (isError || !invoice) return <Page title="فاتورة مورد" error="لم يتم العثور على الفاتورة" onRetry={() => refetch()} />;

  const status = invoice.status as SupplierInvoiceStatus;

  const handleSubmit = async () => {
    try {
      await submitMutation.mutateAsync(invoiceId);
      navigate('/procurement/supplier-invoices');
    } catch {
      // Error handled by mutation
    }
  };

  const handleMatch = async () => {
    try {
      await matchMutation.mutateAsync(invoiceId);
      navigate('/procurement/supplier-invoices');
    } catch {
      // Error handled by mutation
    }
  };

  const handleCancel = async () => {
    try {
      await cancelMutation.mutateAsync({ id: invoiceId, notes: cancelNotes || undefined });
      setCancelOpen(false);
      setCancelNotes('');
      navigate('/procurement/supplier-invoices');
    } catch {
      // Error handled by mutation
    }
  };

  const canSubmit = status === 'Draft';
  const canMatch = status === 'Submitted';
  const canCancel = status === 'Draft' || status === 'Submitted' || status === 'Matched' || status === 'Disputed';

  return (
    <Page
      title={`فاتورة مورد — ${invoice.invoiceNumber}`}
      actions={
        <div className="flex items-center gap-2">
          {canCancel && (
            <Button variant="destructive" size="sm" onClick={() => setCancelOpen(true)} disabled={cancelMutation.isPending} loading={cancelMutation.isPending}>
              إلغاء
            </Button>
          )}
          {canMatch && (
            <Button size="sm" onClick={handleMatch} disabled={matchMutation.isPending} loading={matchMutation.isPending}>
              مطابقة
            </Button>
          )}
          {canSubmit && (
            <Button size="sm" onClick={handleSubmit} disabled={submitMutation.isPending} loading={submitMutation.isPending}>
              تقديم
            </Button>
          )}
          <Button variant="ghost" size="icon" onClick={() => navigate('/procurement/supplier-invoices')} aria-label="العودة">
            <ArrowRight size={18} />
          </Button>
        </div>
      }
      toolbar={
        <div className="flex flex-wrap items-center gap-3 text-sm">
          <StatusBadge variant={supplierInvoiceStatusVariant[status]}>
            {supplierInvoiceStatusLabels[status]}
          </StatusBadge>
          <span className="text-muted-foreground">|</span>
          <MetaItem label="رقم أمر الشراء" value={invoice.purchaseOrderNumber || `#${invoice.purchaseOrderId}`} />
          <span className="text-muted-foreground">|</span>
          <MetaItem label="المورد" value={invoice.supplierName || `#${invoice.supplierPartyId}`} />
          <span className="text-muted-foreground">|</span>
          <MetaItem label="الإجمالي" value={invoice.grandTotal?.toLocaleString('ar-YE')} />
        </div>
      }
    >

      <Card>
        <CardHeader>
          <CardTitle>تفاصيل الفاتورة</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4 mb-6">
            <div>
              <label className="text-sm text-muted-foreground">رقم الفاتورة</label>
              <p className="font-mono" dir="ltr">{invoice.invoiceNumber}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">رقم فاتورة المورد</label>
              <p className="font-mono" dir="ltr">{invoice.supplierInvoiceNumber}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">التاريخ</label>
              <p className="font-medium">{new Date(invoice.invoiceDate).toLocaleDateString('ar-YE')}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">تاريخ الاستحقاق</label>
              <p className="font-medium">{invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString('ar-YE') : '-'}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">رقم أمر الشراء</label>
              <p>
                <Link
                  to={`/procurement/purchase-orders/${invoice.purchaseOrderId}`}
                  className="text-primary underline hover:no-underline"
                >
                  {invoice.purchaseOrderNumber || `#${invoice.purchaseOrderId}`}
                </Link>
              </p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">المورد</label>
              <p className="font-medium">{invoice.supplierName || `#${invoice.supplierPartyId}`}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">العملة</label>
              <p className="font-medium">{invoice.currencyCode || '-'}</p>
            </div>
            <div>
              <label className="text-sm text-muted-foreground">سعر الصرف</label>
              <p className="font-medium tabular-nums">{invoice.exchangeRate?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            {invoice.notes && (
              <div className="col-span-2">
                <label className="text-sm text-muted-foreground">ملاحظات</label>
                <p className="font-medium">{invoice.notes}</p>
              </div>
            )}
          </div>

          <div className="grid grid-cols-5 gap-4 mb-6 p-4 bg-muted rounded-lg">
            <div>
              <label className="text-xs text-muted-foreground">المجموع الفرعي</label>
              <p className="font-mono tabular-nums">{invoice.subTotal?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            <div>
              <label className="text-xs text-muted-foreground">الخصم</label>
              <p className="font-mono tabular-nums">{invoice.discountAmount?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            <div>
              <label className="text-xs text-muted-foreground">الضريبة</label>
              <p className="font-mono tabular-nums">{invoice.taxAmount?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            <div>
              <label className="text-xs text-muted-foreground">الشحن</label>
              <p className="font-mono tabular-nums">{invoice.shippingCost?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
            <div>
              <label className="text-xs text-muted-foreground">الإجمالي</label>
              <p className="font-mono tabular-nums font-semibold text-lg">{invoice.grandTotal?.toLocaleString('ar-YE') ?? '-'}</p>
            </div>
          </div>

          {invoice.details && invoice.details.length > 0 && (
            <div>
              <h3 className="font-medium mb-2">بنود الفاتورة</h3>
              <div className="overflow-x-auto border rounded">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="bg-muted">
                      <th className="border p-2 text-start">الصنف</th>
                      <th className="border p-2 text-start">الكمية</th>
                      <th className="border p-2 text-start">سعر الوحدة</th>
                      <th className="border p-2 text-start">الخصم</th>
                      <th className="border p-2 text-start">الضريبة</th>
                      <th className="border p-2 text-start">الإجمالي</th>
                      <th className="border p-2 text-start">ملاحظات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoice.details.map((line) => (
                      <tr key={line.id} className="border-b last:border-b-0">
                        <td className="border p-2">{line.itemNameAr || line.itemName || line.itemId}</td>
                        <td className="border p-2 tabular-nums">{line.quantity}</td>
                        <td className="border p-2 tabular-nums font-mono">{line.unitPrice.toLocaleString('ar-YE')}</td>
                        <td className="border p-2 tabular-nums font-mono">{line.discountAmount?.toLocaleString('ar-YE') ?? '-'}</td>
                        <td className="border p-2 tabular-nums font-mono">{line.taxAmount?.toLocaleString('ar-YE') ?? '-'}</td>
                        <td className="border p-2 tabular-nums font-mono font-semibold">{line.lineTotal?.toLocaleString('ar-YE') ?? '-'}</td>
                        <td className="border p-2">{line.notes ?? '-'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          <div className="flex gap-2 justify-end mt-6">
            <Button variant="outline" onClick={() => navigate('/procurement/supplier-invoices')}>
              رجوع للقائمة
            </Button>
            {invoice.purchaseOrderId > 0 && (
              <Button variant="ghost" onClick={() => navigate(`/procurement/purchase-orders/${invoice.purchaseOrderId}`)}>
                رجوع لأمر الشراء
              </Button>
            )}
            {canCancel && (
              <Button variant="destructive" onClick={() => setCancelOpen(true)} disabled={cancelMutation.isPending} loading={cancelMutation.isPending}>
                إلغاء
              </Button>
            )}
            {canMatch && (
              <Button onClick={handleMatch} disabled={matchMutation.isPending} loading={matchMutation.isPending}>
                مطابقة
              </Button>
            )}
            {canSubmit && (
              <Button onClick={handleSubmit} disabled={submitMutation.isPending} loading={submitMutation.isPending}>
                تقديم
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <Dialog open={cancelOpen} onClose={() => { setCancelOpen(false); setCancelNotes(''); }} title="إلغاء الفاتورة"
        footer={
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => { setCancelOpen(false); setCancelNotes(''); }}>إلغاء</Button>
            <Button variant="destructive" onClick={handleCancel} loading={cancelMutation.isPending} disabled={cancelMutation.isPending}>
              تأكيد الإلغاء
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <p>هل أنت متأكد من إلغاء هذه الفاتورة؟</p>
          <Textarea
            value={cancelNotes}
            onChange={(e) => setCancelNotes(e.target.value)}
            placeholder="ملاحظات الإلغاء (اختياري)"
            rows={3}
          />
        </div>
      </Dialog>
    </Page>
  );
}
