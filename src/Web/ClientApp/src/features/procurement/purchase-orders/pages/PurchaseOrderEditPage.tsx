import { useNavigate, useParams } from 'react-router-dom';
import { Page, Skeleton, ErrorState } from '@/components/ui';
import { usePurchaseOrderDetail, useUpdatePurchaseOrder } from '../hooks/usePurchaseOrders';
import { useItems, useUnits, useSuppliers, useWarehouses, useLocations, useCurrencies } from '../shared/catalog-hooks';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { ProcurementPurchaseOrdersForm } from '@/components/ProcurementPurchaseOrdersForm';
import type { CreatePurchaseOrderFormData } from '../shared/schemas';

export default function PurchaseOrderEditPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading, error, refetch } = usePurchaseOrderDetail(numericId);
  const mutation = useUpdatePurchaseOrder();

  const suppliersQuery = useSuppliers();
  const itemsQuery = useItems();
  const unitsQuery = useUnits();
  const warehousesQuery = useWarehouses();
  const locationsQuery = useLocations();
  const currenciesQuery = useCurrencies();

  if (isLoading || suppliersQuery.isLoading || itemsQuery.isLoading) {
    return <Page title="جاري التحميل..."><Skeleton className="h-96" /></Page>;
  }
  if (error || !detail) {
    return <Page title="خطأ"><ErrorState message="فشل تحميل التفاصيل" onRetry={refetch} /></Page>;
  }
  if (detail.status !== 'Draft') {
    return (
      <Page title="غير قابل للتعديل">
        <div className="p-4 border rounded-lg bg-[var(--color-surface)]">
          <p className="text-[var(--color-on-surface-variant)]">يمكن تعديل أوامر الشراء في حالة المسودة فقط.</p>
          <p className="mt-2 text-sm">الحالة الحالية: {detail.status}</p>
        </div>
      </Page>
    );
  }

  const supplierOptions = (suppliersQuery.data ?? []).map((s) => ({ value: String(s.id), label: s.nameAr }));
  const itemOptions = (itemsQuery.data ?? []).map((i) => ({ value: String(i.id), label: `${i.code} - ${i.name}` }));
  const unitOptions = (unitsQuery.data ?? []).map((u) => ({ value: String(u.id), label: u.name }));
  const warehouseOptions = (warehousesQuery.data ?? []).map((w) => ({ value: String(w.id), label: w.name }));
  const locationOptions = Array.isArray(locationsQuery.data) ? locationsQuery.data.map((l) => ({ value: String(l.id), label: l.name })) : [];
  const currencyOptions = (currenciesQuery.data ?? []).map((c) => ({ value: c.code, label: `${c.code} - ${c.nameAr}` }));

  const initialData: CreatePurchaseOrderFormData = {
    supplierPartyId: detail.supplierPartyId,
    purchaseRequestId: detail.purchaseRequestId ?? null,
    quotationId: detail.quotationId ?? null,
    warehouseId: null,
    deliveryLocationId: null,
    currencyCode: null,
    exchangeRate: null,
    paymentTerms: detail.paymentTerms ?? null,
    deliveryTerms: detail.deliveryTerms ?? null,
    expectedDeliveryDate: detail.expectedDeliveryDate ?? null,
    notes: detail.notes ?? null,
    lines: detail.lines.map((l) => ({
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
      notes: null,
    })),
  };

  function handleSubmit(data: CreatePurchaseOrderFormData) {
    mutation.mutate({
      id: numericId,
      data: {
        paymentTerms: data.paymentTerms || null,
        deliveryTerms: data.deliveryTerms || null,
        expectedDeliveryDate: data.expectedDeliveryDate || null,
        notes: data.notes || null,
        lines: data.lines.map((l) => ({
          ...l,
          purchaseRequestDetailId: l.purchaseRequestDetailId || null,
          quotationDetailId: l.quotationDetailId || null,
          expectedDeliveryDate: l.expectedDeliveryDate || null,
          notes: l.notes || null,
        })),
      },
    }, {
      onSuccess: () => navigate(`/procurement/purchase-orders/${numericId}`),
      onError: (err) => handleLifecycleError(err),
    });
  }

  return (
    <Page title={`تعديل أمر شراء ${detail.poNumber}`}>
      <ProcurementPurchaseOrdersForm
        initialData={initialData}
        onSubmit={handleSubmit}
        onCancel={() => navigate(`/procurement/purchase-orders/${numericId}`)}
        isPending={mutation.isPending}
        supplierOptions={supplierOptions}
        itemOptions={itemOptions}
        unitOptions={unitOptions}
        warehouseOptions={warehouseOptions}
        locationOptions={locationOptions}
        currencyOptions={currencyOptions}
        currencies={currenciesQuery.data ?? []}
      />
    </Page>
  );
}
