import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Page, EmptyState } from '@/components/ui';
import { useActiveLocations } from '@/features/inventory/locations/hooks/useLocations';
import { useEmployees } from '@/features/organization/hooks';
import { useTransferDetail, useUpdateTransfer } from '../hooks/useTransfers';
import { TransferForm } from '../components/TransferForm';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import type { TransferFormInput } from '../shared/schemas';

export function TransferEditPage() {
  const { id } = useParams<{ id: string }>();
  const transferId = Number(id);
  const navigate = useNavigate();
  const [error, setError] = useState<string>();

  const { data: transfer, isLoading, error: loadError, refetch } = useTransferDetail(transferId);
  const { data: locations = [] } = useActiveLocations();
  const { data: employees = [] } = useEmployees();
  const updateTransfer = useUpdateTransfer();

  if (isLoading) return <Page title="" loading>{null}</Page>;
  if (loadError) {
    return (
      <Page title="" error={getQueryErrorMessage(loadError)} onRetry={() => refetch()}>
        {null}
      </Page>
    );
  }
  if (!transfer) {
    return (
      <Page title="" onBack={() => navigate('/assets/transfers')}>
        <EmptyState message="النقل غير موجود" />
      </Page>
    );
  }
  if (transfer.status !== 'Draft') {
    return (
      <Page title="" onBack={() => navigate(`/assets/transfers/${transferId}`)}>
        <EmptyState message="لا يمكن تعديل نقل ليس في حالة مسودة" />
      </Page>
    );
  }

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
      await updateTransfer.mutateAsync({
        id: transferId,
        data: {
          transactionDate: values.transactionDate,
          toLocationId: values.toLocationId ?? undefined,
          toEmployeeId: values.toEmployeeId ?? undefined,
          notes: values.notes || undefined,
          rowVersion: transfer.rowVersion,
        },
      });
      navigate(`/assets/transfers/${transferId}`);
    } catch (err) {
      setError(getErrorText(err));
      handleLifecycleError(err);
    }
  };

  return (
    <Page
      title={`تعديل النقل ${transfer.documentNumber}`}
      description="عند الحفظ تُحدَّث قيم «من» من بطاقة الأصل الحالية"
      maxWidth="full"
      onBack={() => navigate(`/assets/transfers/${transferId}`)}
    >
      <TransferForm
        mode="edit"
        assetOptions={[]}
        assetLabel={`${transfer.assetCode} - ${transfer.assetName}`}
        locationOptions={locationOptions}
        employeeOptions={employeeOptions}
        defaultValues={{
          assetId: transfer.assetId,
          transactionDate: transfer.transactionDate,
          toLocationId: transfer.toLocationId ?? null,
          toEmployeeId: transfer.toEmployeeId ?? null,
          notes: transfer.notes ?? '',
        }}
        fromSummary={{
          locationName: transfer.fromLocationName,
          employeeName: transfer.fromEmployeeName,
          departmentName: transfer.fromDepartmentName,
        }}
        onSubmit={handleSubmit}
        onCancel={() => navigate(`/assets/transfers/${transferId}`)}
        isPending={updateTransfer.isPending}
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
  return 'تعذر حفظ التعديلات';
}
