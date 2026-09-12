import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Page, Skeleton } from '@/components/ui';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { usePurchaseRequestDetail, useUpdatePurchaseRequest } from '../hooks/usePurchaseRequests';
import { useItemsList, useUnitsList, useDepartmentsList, useCostCentersList } from '../shared/catalog-hooks';
import { PurchaseRequestForm } from '../components/PurchaseRequestForm';
import type { CreatePurchaseRequestFormData } from '../shared/schemas';

export default function PurchaseRequestEditPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading: detailLoading, error } = usePurchaseRequestDetail(numericId);
  const updateMutation = useUpdatePurchaseRequest();

  const { data: items, isLoading: itemsLoading } = useItemsList();
  const { data: units, isLoading: unitsLoading } = useUnitsList();
  const { data: departments, isLoading: departmentsLoading } = useDepartmentsList();
  const { data: costCenters, isLoading: costCentersLoading } = useCostCentersList();

  const isLoading = detailLoading || itemsLoading || unitsLoading || departmentsLoading || costCentersLoading;

  useEffect(() => {
    if (detail && detail.status !== 'Draft') {
      navigate(`/procurement/purchase-requests/${numericId}`, { replace: true });
    }
  }, [detail, navigate, numericId]);

  function onSubmit(data: CreatePurchaseRequestFormData) {
    updateMutation.mutate(
      { id: numericId, data },
      {
        onSuccess: () => navigate(`/procurement/purchase-requests/${numericId}`),
        onError: (err) => handleLifecycleError(err),
      }
    );
  }

  if (isLoading) {
    return (
      <Page title="جاري التحميل...">
        <Skeleton className="h-96" />
      </Page>
    );
  }

  if (error || !detail) {
    return (
      <Page title="تعديل طلب شراء">
        <p className="text-[var(--color-error)]">فشل تحميل طلب الشراء</p>
      </Page>
    );
  }

  if (detail.status !== 'Draft') {
    return null;
  }

  return (
    <Page
      title={`تعديل طلب شراء ${detail.requestNumber}`}
      maxWidth="xl"
      actions={
        <button
          type="button"
          onClick={() => navigate(`/procurement/purchase-requests/${numericId}`)}
          className="text-sm text-[var(--color-primary)] hover:underline"
        >
          رجوع
        </button>
      }
    >
      <PurchaseRequestForm
        initialData={detail}
        onSubmit={onSubmit}
        onCancel={() => navigate(`/procurement/purchase-requests/${numericId}`)}
        isPending={updateMutation.isPending}
        itemOptions={items ?? []}
        unitOptions={units ?? []}
        departmentOptions={departments ?? []}
        costCenterOptions={costCenters ?? []}
      />
    </Page>
  );
}
