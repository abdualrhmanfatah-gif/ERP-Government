import { useNavigate } from 'react-router-dom';
import { Page, Skeleton } from '@/components/ui';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { useCreatePurchaseRequest } from '../hooks/usePurchaseRequests';
import { useItemsList, useUnitsList, useDepartmentsList, useCostCentersList } from '../shared/catalog-hooks';
import { PurchaseRequestForm } from '../components/PurchaseRequestForm';
import type { CreatePurchaseRequestFormData } from '../shared/schemas';

export default function PurchaseRequestCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreatePurchaseRequest();

  const { data: items, isLoading: itemsLoading } = useItemsList();
  const { data: units, isLoading: unitsLoading } = useUnitsList();
  const { data: departments, isLoading: departmentsLoading } = useDepartmentsList();
  const { data: costCenters, isLoading: costCentersLoading } = useCostCentersList();

  const isLoading = itemsLoading || unitsLoading || departmentsLoading || costCentersLoading;

  function onSubmit(data: CreatePurchaseRequestFormData) {
    createMutation.mutate(data, {
      onSuccess: (id) => navigate(`/procurement/purchase-requests/${id}`),
      onError: (err) => handleLifecycleError(err),
    });
  }

  if (isLoading) {
    return (
      <Page title="طلب شراء جديد">
        <Skeleton className="h-96" />
      </Page>
    );
  }

  return (
    <Page
      title="طلب شراء جديد"
      maxWidth="xl"
      actions={
        <button
          type="button"
          onClick={() => navigate('/procurement/purchase-requests')}
          className="text-sm text-[var(--color-primary)] hover:underline"
        >
          رجوع
        </button>
      }
    >
      <PurchaseRequestForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/procurement/purchase-requests')}
        isPending={createMutation.isPending}
        isCreate
        itemOptions={items ?? []}
        unitOptions={units ?? []}
        departmentOptions={departments ?? []}
        costCenterOptions={costCenters ?? []}
      />
    </Page>
  );
}
