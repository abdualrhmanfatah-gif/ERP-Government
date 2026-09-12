import { useNavigate, useParams } from 'react-router-dom';
import { Page, Skeleton } from '@/components/ui';
import { useQuotationDetail, useUpdateQuotation } from '../hooks/useQuotations';
import { usePartiesList } from '@/features/parties/hooks/useParties';
import { useUnitsList } from '@/features/inventory/units/hooks/useUnits';
import { useItemsList } from '@/features/inventory/items/hooks/useItems';
import { QuotationForm } from '../components/QuotationForm';
import type { CreateQuotationFormData } from '../shared/schemas';

export default function QuotationEditPage() {
  const { id } = useParams<{ id: string }>();
  const numericId = Number(id);
  const navigate = useNavigate();
  const { data: detail, isLoading: detailLoading } = useQuotationDetail(numericId);
  const updateMutation = useUpdateQuotation();

  const { data: suppliersData, isLoading: suppliersLoading } = usePartiesList({ partyType: 'Supplier' });
  const { data: unitsData, isLoading: unitsLoading } = useUnitsList();
  const { data: itemsData, isLoading: itemsLoading } = useItemsList();

  const isLoading = detailLoading || suppliersLoading || unitsLoading || itemsLoading;

  const supplierOptions = (suppliersData ?? []).map((party) => ({
    value: String(party.id),
    label: party.name,
  }));

  const currencyOptions = [
    { value: 'YER', label: 'ريال يمني (YER)' },
    { value: 'USD', label: 'دولار أمريكي (USD)' },
    { value: 'SAR', label: 'ريال سعودي (SAR)' },
  ];

  const itemOptions = (itemsData?.items ?? []).map((item) => ({
    value: String(item.id),
    label: item.name,
  }));

  const unitOptions = (unitsData ?? []).map((unit) => ({
    value: String(unit.id),
    label: unit.name,
  }));

  function onSubmit(data: CreateQuotationFormData) {
    updateMutation.mutate(
      { id: numericId, data },
      {
        onSuccess: () => navigate(`/procurement/quotations/${numericId}`),
        onError: () => {},
      }
    );
  }

  if (isLoading) {
    return (
      <Page title="تعديل عرض السعر">
        <Skeleton className="h-96" />
      </Page>
    );
  }

  if (!detail) {
    return <Page title="لم يتم العثور على العرض" />;
  }

  if (detail.status !== 'Draft') {
    navigate(`/procurement/quotations/${numericId}`);
    return null;
  }

  return (
    <Page
      title="تعديل عرض السعر"
      maxWidth="xl"
      actions={
        <button
          type="button"
          onClick={() => navigate(`/procurement/quotations/${numericId}`)}
          className="text-sm text-[var(--color-primary)] hover:underline"
        >
          رجوع
        </button>
      }
    >
      <QuotationForm
        initialData={detail}
        onSubmit={onSubmit}
        onCancel={() => navigate(`/procurement/quotations/${numericId}`)}
        isPending={updateMutation.isPending}
        supplierOptions={supplierOptions}
        currencyOptions={currencyOptions}
        itemOptions={itemOptions}
        unitOptions={unitOptions}
      />
    </Page>
  );
}
