import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  usePurchaseOrderDetail,
  useSubmitPurchaseOrder,
  useApprovePurchaseOrder,
  useIssuePurchaseOrder,
  useCancelPurchaseOrder,
  useClosePurchaseOrder,
  useUpdatePurchaseOrder,
} from '../hooks/usePurchaseOrders';
import {
  purchaseOrderStatusLabels,
  purchaseOrderStatusVariant,
  type PurchaseOrderStatus,
} from '../shared/types';
import { useItems, useUnits, useSuppliers, useWarehouses, useLocations, useCurrencies } from '../shared/catalog-hooks';
import { StatusBadge, Card, Dialog, Button, Textarea, Page, MoneyDisplay } from '@/components/ui';
import { MetaItem } from '@/components/MetaItem';
import { StatusLogPanel } from '@/components/DocumentsStatusLogPanel';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';
import { ProcurementPurchaseOrdersForm } from '@/components/ProcurementPurchaseOrdersForm';
import type { CreatePurchaseOrderFormData } from '../shared/schemas';
import { ArrowRight, Check } from 'lucide-react';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { notify } from '@/features/notifications/notify';
import { usePermission } from '@/shared/hooks/usePermission';
import { PROCUREMENT_PERMISSIONS } from '@/shared/constants/permissions';

export default function PurchaseOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading, isError, refetch } = usePurchaseOrderDetail(numericId);

  const submitMutation = useSubmitPurchaseOrder();
  const approveMutation = useApprovePurchaseOrder();
  const issueMutation = useIssuePurchaseOrder();
  const cancelMutation = useCancelPurchaseOrder();
  const closeMutation = useClosePurchaseOrder();
  const updateMutation = useUpdatePurchaseOrder();

  const [isEditing, setIsEditing] = useState(false);
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [closeOpen, setCloseOpen] = useState(false);
  const [closeReason, setCloseReason] = useState('');

  const { data: items = [] } = useItems();
  const { data: units = [] } = useUnits();
  const { data: suppliers = [] } = useSuppliers();
  const { data: warehouses = [] } = useWarehouses();
  const { data: locations = [] } = useLocations();
  const { data: currencies = [] } = useCurrencies();

  const { hasPermission: canCreateInvoice } = usePermission(PROCUREMENT_PERMISSIONS.SupplierInvoices.Create);

  if (isLoading) return <Page title="أمر شراء" loading />;
  if (isError || !detail?.id)
    return (
      <Page title="أمر شراء" error="أمر الشراء غير موجود." onRetry={() => navigate('/procurement/purchase-orders')}>
        <Button variant="outline" onClick={() => navigate('/procurement/purchase-orders')}>العودة للقائمة</Button>
      </Page>
    );

  const status = detail.status as PurchaseOrderStatus;
  const lines = detail.lines ?? [];
  const totalOrdered = lines.reduce((s, l) => s + l.orderedQuantity, 0);
  const totalReceived = lines.reduce((s, l) => s + l.receivedQuantity, 0);
  const deliveryProgress = totalOrdered > 0 ? Math.round((totalReceived / totalOrdered) * 100) : 0;

  const canSubmit = status === 'Draft';
  const canApprove = status === 'Submitted';
  const canIssue = status === 'Approved';
  const canCancel = status !== 'Cancelled' && status !== 'Closed';
  const canClose = status === 'PartiallyReceived' || status === 'Received';
  const canEdit = status === 'Draft' || status === 'Submitted';
  const canCreateSupplierInvoice = canCreateInvoice && (status === 'Issued' || status === 'PartiallyReceived');

  function handleSubmit() {
    submitMutation.mutate(numericId, {
      onError: handleLifecycleError,
      onSuccess: () => { notify({ type: 'success', title: 'تم تقديم أمر الشراء' }); refetch(); },
    });
  }

  function handleApprove() {
    approveMutation.mutate(numericId, {
      onError: handleLifecycleError,
      onSuccess: () => { notify({ type: 'success', title: 'تم اعتماد أمر الشراء' }); refetch(); },
    });
  }

  function handleIssue() {
    issueMutation.mutate(numericId, {
      onError: handleLifecycleError,
      onSuccess: () => { notify({ type: 'success', title: 'تم إصدار أمر الشراء' }); refetch(); },
    });
  }

  function handleCancel() {
    cancelMutation.mutate(
      { id: numericId, reason: cancelReason || undefined },
      {
        onError: handleLifecycleError,
        onSuccess: () => { notify({ type: 'success', title: 'تم إلغاء أمر الشراء' }); setCancelOpen(false); setCancelReason(''); refetch(); },
      },
    );
  }

  function handleClose() {
    closeMutation.mutate(
      { id: numericId, reason: closeReason || undefined },
      {
        onError: handleLifecycleError,
        onSuccess: () => { notify({ type: 'success', title: 'تم إغلاق أمر الشراء' }); setCloseOpen(false); setCloseReason(''); refetch(); },
      },
    );
  }

  function handleSaveEdit(formData: CreatePurchaseOrderFormData) {
    updateMutation.mutate(
      { id: numericId, data: formData as unknown as Record<string, unknown> },
      {
        onError: handleLifecycleError,
        onSuccess: () => { notify({ type: 'success', title: 'تم حفظ التعديلات' }); setIsEditing(false); refetch(); },
      },
    );
  }

  function toFormData(): CreatePurchaseOrderFormData {
    return {
      supplierPartyId: detail.supplierPartyId,
      warehouseId: null,
      deliveryLocationId: null,
      currencyCode: null,
      exchangeRate: null,
      paymentTerms: detail.paymentTerms ?? null,
      deliveryTerms: detail.deliveryTerms ?? null,
      expectedDeliveryDate: detail.expectedDeliveryDate ?? null,
      notes: detail.notes ?? null,
      lines: lines.map((l) => ({
        id: l.id,
        purchaseRequestDetailId: l.purchaseRequestDetailId,
        quotationDetailId: l.quotationDetailId ?? null,
        itemId: l.itemId,
        unitId: l.unitId,
        orderedQuantity: l.orderedQuantity,
        unitPrice: l.unitPrice,
        discountPercent: l.discountPercent ?? null,
        taxPercent: l.taxPercent ?? null,
        expectedDeliveryDate: null,
        notes: l.notes ?? null,
      })),
    };
  }

  const headerActions = isEditing ? (
    <div className="flex items-center gap-2 flex-wrap">
      <Button
        type="submit"
        form="purchase-order-detail-form"
        variant="primary"
        size="sm"
        disabled={updateMutation.isPending}
        loading={updateMutation.isPending}
      >
        حفظ التعديلات
      </Button>
      <Button variant="ghost" size="sm" onClick={() => setIsEditing(false)} disabled={updateMutation.isPending}>
        إلغاء
      </Button>
    </div>
  ) : (
    <div className="flex items-center gap-2 flex-wrap">
      {canEdit && (
        <Button variant="secondary" size="sm" onClick={() => setIsEditing(true)}>
          تعديل
        </Button>
      )}
      {canSubmit && (
        <Button variant="primary" size="sm" onClick={handleSubmit} disabled={submitMutation.isPending} loading={submitMutation.isPending}>
          تقديم
        </Button>
      )}
      {canApprove && (
        <Button variant="primary" size="sm" onClick={handleApprove} disabled={approveMutation.isPending} loading={approveMutation.isPending}>
          اعتماد
        </Button>
      )}
      {canIssue && (
        <Button variant="primary" size="sm" onClick={handleIssue} disabled={issueMutation.isPending} loading={issueMutation.isPending}>
          إصدار
        </Button>
      )}
      {canClose && (
        <Button variant="secondary" size="sm" onClick={() => setCloseOpen(true)} disabled={closeMutation.isPending} loading={closeMutation.isPending}>
          إغلاق
        </Button>
      )}
      {canCancel && (
        <Button variant="destructive" size="sm" onClick={() => setCancelOpen(true)} disabled={cancelMutation.isPending} loading={cancelMutation.isPending}>
          إلغاء
        </Button>
      )}
      {canCreateSupplierInvoice && (
        <Button variant="outline" size="sm" onClick={() => navigate(`/procurement/supplier-invoices/create?purchaseOrderId=${numericId}`)}>
          فاتورة مورد جديدة
        </Button>
      )}
      <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
        <ArrowRight size={18} />
      </Button>
    </div>
  );

  return (
    <Page
      title={`أمر شراء — ${detail.purchaseOrderNumber || detail.poNumber}`}
      actions={headerActions}
      toolbar={
        <div className="flex flex-wrap items-center gap-3 rounded-lg bg-[var(--color-surface-container-low)] px-4 py-2.5">
          <StatusBadge variant={purchaseOrderStatusVariant[status]}>
            {purchaseOrderStatusLabels[status]}
          </StatusBadge>
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="رقم الأمر" value={detail.purchaseOrderNumber || detail.poNumber} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="التاريخ" value={detail.poDate} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="المورد" value={detail.supplierName ?? `#${detail.supplierPartyId}`} />
          <span className="h-4 w-px bg-[var(--color-outline-variant)]" aria-hidden="true" />
          <MetaItem label="الإجمالي" value={<MoneyDisplay value={detail.grandTotal ?? 0} />} />
        </div>
      }
    >
      {/* التخطيط الرئيسي: عمودان على سطح المكتب */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* العمود الأيسر: النموذج + تقدم الاستلام */}
        <div className="lg:col-span-8 space-y-6">
          <ProcurementPurchaseOrdersForm
            initialData={toFormData()}
            readOnly={!isEditing}
            onEdit={() => setIsEditing(true)}
            onSubmit={handleSaveEdit}
            onCancel={() => setIsEditing(false)}
            isPending={updateMutation.isPending}
            supplierOptions={suppliers.map((s) => ({ value: String(s.id), label: s.nameAr }))}
            itemOptions={items.map((i) => ({ value: String(i.id), label: `${i.code} - ${i.name}` }))}
            unitOptions={units.map((u) => ({ value: String(u.id), label: u.name }))}
            warehouseOptions={warehouses.map((w) => ({ value: String(w.id), label: `${w.code} - ${w.name}` }))}
            locationOptions={locations.map((l) => ({ value: String(l.id), label: l.name }))}
            currencyOptions={currencies.map((c) => ({ value: c.code, label: `${c.code} - ${c.nameAr}` }))}
            currencies={currencies}
          />

          {/* تقدم الاستلام */}
          {totalOrdered > 0 && (
            <Card>
              <h2 className="text-sm font-semibold mb-4">تقدم الاستلام</h2>
              <div className="flex items-center gap-2 mb-4">
                {[1].map((step) => {
                  const isDone = deliveryProgress === 100;
                  const isCurrent = deliveryProgress > 0 && deliveryProgress < 100;
                  return (
                    <div key={step} className="flex items-center gap-2">
                      <span
                        className={`inline-flex items-center justify-center w-8 h-8 rounded-full text-sm font-bold transition-all ${
                          isDone
                            ? 'bg-[var(--color-primary)] text-[var(--color-on-primary)]'
                            : isCurrent
                              ? 'bg-[var(--color-secondary)] text-[var(--color-on-secondary)] ring-2 ring-[var(--color-secondary)] ring-offset-2'
                              : 'bg-[var(--color-surface-container)] text-[var(--color-on-surface-variant)] border border-[var(--color-outline)]'
                        }`}
                      >
                        {isDone ? <Check size={16} /> : `${deliveryProgress}%`}
                      </span>
                    </div>
                  );
                })}
              </div>
              <div className="flex-1 h-3 bg-[var(--color-surface-container)] rounded-full overflow-hidden mb-3">
                <div
                  className="h-full bg-[var(--color-primary)] rounded-full transition-all"
                  style={{ width: `${deliveryProgress}%` }}
                />
              </div>
              <div className="flex justify-between text-sm text-[var(--color-on-surface-variant)]">
                <span>المستلم: {totalReceived}</span>
                <span>المتبقي: {totalOrdered - totalReceived}</span>
                <span>الإجمالي: {totalOrdered}</span>
              </div>
            </Card>
          )}

          {/* سجل الحالات */}
          <Card>
            <div className="p-6">
              <StatusLogPanel documentType="PurchaseOrder" documentId={numericId} />
            </div>
          </Card>
        </div>

        {/* العمود الأيمن: المرفقات + الملخص المالي */}
        <div className="lg:col-span-4 space-y-6">
          <AttachmentsPanel documentType="PurchaseOrder" documentId={numericId} showGate={canSubmit || canApprove} />

          {/* الملخص المالي */}
          <Card>
            <h2 className="text-sm font-semibold mb-4">الملخص المالي</h2>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">المجموع الفرعي</span>
                <span className="tabular-nums font-mono"><MoneyDisplay value={detail.subTotal ?? 0} /></span>
              </div>
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">الخصم</span>
                <span className="tabular-nums font-mono text-[var(--color-error)]">-<MoneyDisplay value={detail.discountAmount ?? 0} /></span>
              </div>
              <div className="flex justify-between">
                <span className="text-[var(--color-on-surface-variant)]">الضريبة</span>
                <span className="tabular-nums font-mono">+<MoneyDisplay value={detail.taxAmount ?? 0} /></span>
              </div>
              {detail.shippingCost ? (
                <div className="flex justify-between">
                  <span className="text-[var(--color-on-surface-variant)]">الشحن</span>
                  <span className="tabular-nums font-mono">+<MoneyDisplay value={detail.shippingCost} /></span>
                </div>
              ) : null}
              {detail.otherCharges ? (
                <div className="flex justify-between">
                  <span className="text-[var(--color-on-surface-variant)]">رسوم أخرى</span>
                  <span className="tabular-nums font-mono">+<MoneyDisplay value={detail.otherCharges} /></span>
                </div>
              ) : null}
              <div className="flex justify-between border-t border-[var(--color-outline-variant)] pt-2 font-bold text-base">
                <span>الإجمالي</span>
                <MoneyDisplay value={detail.grandTotal ?? 0} />
              </div>
            </div>
          </Card>

          {/* روابط مصدرية */}
          {(detail.purchaseRequestId || detail.quotationId) && (
            <Card>
              <h2 className="text-sm font-semibold mb-4">المستندات المصدرية</h2>
              <div className="space-y-2 text-sm">
                {detail.purchaseRequestId && (
                  <div>
                    <span className="text-[var(--color-on-surface-variant)]">طلب الشراء: </span>
                    <Link
                      to={`/procurement/purchase-requests/${detail.purchaseRequestId}`}
                      className="text-[var(--color-primary)] hover:underline font-mono"
                    >
                      #{detail.purchaseRequestId}
                    </Link>
                  </div>
                )}
                {detail.quotationId && (
                  <div>
                    <span className="text-[var(--color-on-surface-variant)]">عرض السعر: </span>
                    <Link
                      to={`/procurement/quotations/${detail.quotationId}`}
                      className="text-[var(--color-primary)] hover:underline font-mono"
                    >
                      #{detail.quotationId}
                    </Link>
                  </div>
                )}
              </div>
            </Card>
          )}
        </div>
      </div>

      {/* نافذة الإلغاء */}
      <Dialog
        open={cancelOpen}
        onClose={() => setCancelOpen(false)}
        title="إلغاء أمر الشراء — السبب مطلوب"
        footer={
          <>
            <Button variant="outline" onClick={() => setCancelOpen(false)}>تراجع</Button>
            <Button variant="destructive" onClick={handleCancel} disabled={!cancelReason.trim() || cancelMutation.isPending} loading={cancelMutation.isPending}>
              تأكيد الإلغاء
            </Button>
          </>
        }
      >
        <Textarea
          label="سبب الإلغاء"
          value={cancelReason}
          onChange={(e) => setCancelReason(e.target.value)}
          rows={3}
          placeholder="اذكر سبب الإلغاء..."
        />
      </Dialog>

      {/* نافذة الإغلاق */}
      <Dialog
        open={closeOpen}
        onClose={() => setCloseOpen(false)}
        title="إغلاق أمر الشراء"
        footer={
          <>
            <Button variant="outline" onClick={() => setCloseOpen(false)}>تراجع</Button>
            <Button onClick={handleClose} disabled={closeMutation.isPending} loading={closeMutation.isPending}>
              تأكيد الإغلاق
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <p className="text-sm text-[var(--color-on-surface-variant)]">سيتم تحرير الكميات والالتزامات المتبقية.</p>
          <Textarea
            label="سبب الإغلاق"
            value={closeReason}
            onChange={(e) => setCloseReason(e.target.value)}
            rows={3}
            placeholder="سبب الإغلاق (اختياري)..."
          />
        </div>
      </Dialog>
    </Page>
  );
}
