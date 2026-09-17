import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useAssetsList } from '@/features/assets/assets/hooks/useAssets';
import { useActiveLocations } from '@/features/inventory/locations/hooks/useLocations';
import { useEmployees } from '@/features/organization/hooks';
import { useCreateTransfer } from '../hooks/useTransfers';
import { TransferForm } from '../components/TransferForm';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import type { TransferFormInput } from '../shared/schemas';

export function TransferCreatePage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string>();

  const { data: assetsPage } = useAssetsList({ status: 'Active', pageSize: 200 });
  const { data: locations = [] } = useActiveLocations();
  const { data: employees = [] } = useEmployees();
  const createTransfer = useCreateTransfer();

  const assetOptions = (assetsPage?.items ?? []).map((asset) => ({
    value: String(asset.id),
    label: `${asset.code} - ${asset.name}`,
  }));

  const locationOptions = locations.map((location) => ({
    value: String(location.id),
    label: location.breadcrumb ? `${location.breadcrumb}/${location.code} - ${location.name}` : `${location.code} - ${location.name}`,
  }));

  const employeeOptions = employees.map((employee: { id: number; name: string }) => ({
    value: String(employee.id),
    label: employee.name,
  }));

  const handleSubmit = async (values: TransferFormInput) => {
    setError(undefined);
    try {
      const id = await createTransfer.mutateAsync({
        assetId: values.assetId,
        transactionDate: values.transactionDate,
        toLocationId: values.toLocationId ?? undefined,
        toEmployeeId: values.toEmployeeId ?? undefined,
        notes: values.notes || undefined,
      });
      navigate(`/assets/transfers/${id}`);
    } catch (err) {
      setError(getErrorText(err));
      handleLifecycleError(err);
    }
  };

  return (
    <Page
      title="نقل جديد"
      description="إنشاء مسودة نقل أصل — لن تتغير بطاقة الأصل قبل التنفيذ"
      maxWidth="full"
      onBack={() => navigate('/assets/transfers')}
    >
      <TransferForm
        mode="create"
        assetOptions={assetOptions}
        locationOptions={locationOptions}
        employeeOptions={employeeOptions}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/assets/transfers')}
        isPending={createTransfer.isPending}
        error={error}
      />
    </Page>
  );
}

function getErrorText(err: unknown): string {
  if (err && typeof err === 'object' && 'message' in err) {
    const message = (err as { message?: string }).message;
    if (message) return message;
  }
  return 'تعذر إنشاء النقل';
}
