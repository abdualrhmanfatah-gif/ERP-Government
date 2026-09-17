import { useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Page, Skeleton, ErrorState } from '@/components/ui';
import { useCreatePurchaseOrder } from '../hooks/usePurchaseOrders';
import {
  useItems, useUnits, useSuppliers, useWarehouses, useLocations, useCurrencies,
  usePurchaseRequestDetail, useQuotationDetail,
} from '../shared/catalog-hooks';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { ProcurementPurchaseOrdersForm } from '@/components/ProcurementPurchaseOrdersForm';
import type { CreatePurchaseOrderFormData } from '../shared/schemas';

export default function PurchaseOrderCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const prId = Number(searchParams.get('purchaseRequestId')) || 0;
  const quotationId = Number(searchParams.get('quotationId')) || 0;

  const mutation = useCreatePurchaseOrder();

  const suppliersQuery = useSuppliers();
  const itemsQuery = useItems();
  const unitsQuery = useUnits();
  const warehousesQuery = useWarehouses();
  const locationsQuery = useLocations();
  const currenciesQuery = useCurrencies();
  const prQuery = usePurchaseRequestDetail(prId);
  const quotationQuery = useQuotationDetail(quotationId);

  const supplierOptions = (suppliersQuery.data ?? []).map((s) => ({ value: String(s.id), label: s.nameAr }));
  const itemOptions = (itemsQuery.data ?? []).map((i) => ({ value: String(i.id), label: `${i.code} - ${i.name}` }));
  const unitOptions = (unitsQuery.data ?? []).map((u) => ({ value: String(u.id), label: u.name }));
  const warehouseOptions = (warehousesQuery.data ?? []).map((w) => ({ value: String(w.id), label: w.name }));
  const locationOptions = Array.isArray(locationsQuery.data) ? locationsQuery.data.map((l) => ({ value: String(l.id), label: l.name })) : [];
  const currencyOptions = (currenciesQuery.data ?? []).map((c) => ({ value: c.code, label: `${c.code} - ${c.nameAr}` }));

  const initialData = useMemo<CreatePurchaseOrderFormData | undefined>(() => {
    if (quotationId > 0 && quotationQuery.data) {
      const q = quotationQuery.data;
      return {
        purchaseRequestId: null,
        quotationId,
        supplierPartyId: q.supplierPartyId,
        warehouseId: null,
        deliveryLocationId: null,
        currencyCode: q.currencyCode ?? null,
        exchangeRate: q.exchangeRate ?? null,
        paymentTerms: q.paymentTerms ?? null,
        deliveryTerms: q.deliveryTerms ?? null,
        expectedDeliveryDate: null,
        notes: null,
        lines: q.lines.map((l) => ({
          purchaseRequestDetailId: l.purchaseRequestDetailId,
          quotationDetailId: l.id,
          itemId: l.itemId,
          unitId: l.unitId,
          orderedQuantity: l.quantity,
          unitPrice: l.unitPrice ?? 0,
          discountPercent: l.discountPercent ?? null,
          taxPercent: l.taxPercent ?? null,
          expectedDeliveryDate: null,
          notes: null,
        })),
      };
    }
    if (prId > 0 && prQuery.data) {
      const pr = prQuery.data;
      return {
        purchaseRequestId: prId,
        quotationId: null,
        supplierPartyId: 0,
        warehouseId: null,
        deliveryLocationId: null,
        currencyCode: null,
        exchangeRate: null,
        paymentTerms: null,
        deliveryTerms: null,
        expectedDeliveryDate: null,
        notes: null,
        lines: pr.lines.map((l) => ({
          purchaseRequestDetailId: l.id,
          quotationDetailId: null,
          itemId: l.itemId,
          unitId: l.unitId,
          orderedQuantity: l.approvedQuantity ?? l.requestedQuantity,
          unitPrice: l.unitCostEstimate ?? 0,
          discountPercent: null,
          taxPercent: null,
          expectedDeliveryDate: null,
          notes: null,
        })),
      };
    }
    return undefined;
  }, [prId, prQuery.data, quotationId, quotationQuery.data]);

  const isLoading = suppliersQuery.isLoading || itemsQuery.isLoading || unitsQuery.isLoading
    || (prId > 0 && prQuery.isLoading)
    || (quotationId > 0 && quotationQuery.isLoading);

  if (isLoading) return <Page title="إنشاء أمر شراء"><Skeleton className="h-96" /></Page>;
  if (suppliersQuery.error || itemsQuery.error || unitsQuery.error) {
    const firstError = suppliersQuery.error || itemsQuery.error || unitsQuery.error;
    return <Page title="خطأ"><ErrorState message={getQueryErrorMessage(firstError)} onRetry={() => { suppliersQuery.refetch(); itemsQuery.refetch(); unitsQuery.refetch(); }} /></Page>;
  }

  function handleSubmit(data: CreatePurchaseOrderFormData) {
    const payload = {
      ...data,
      supplierPartyId: data.supplierPartyId,
      purchaseRequestId: data.purchaseRequestId || prId || null,
      quotationId: data.quotationId || quotationId || null,
      warehouseId: data.warehouseId || null,
      deliveryLocationId: data.deliveryLocationId || null,
      currencyCode: data.currencyCode || null,
      exchangeRate: data.exchangeRate || null,
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
    };
    mutation.mutate(payload, {
      onSuccess: (id) => navigate(`/procurement/purchase-orders/${id}`),
      onError: (err) => handleLifecycleError(err),
    });
  }

  const title = quotationId > 0
    ? 'أمر شراء من عرض السعر'
    : prId > 0
      ? 'أمر شراء مباشر من طلب الشراء'
      : 'إنشاء أمر شراء جديد';

  return (
    <Page title={title}>
      <ProcurementPurchaseOrdersForm
        initialData={initialData}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/procurement/purchase-orders')}
        isPending={mutation.isPending}
        isCreate
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
