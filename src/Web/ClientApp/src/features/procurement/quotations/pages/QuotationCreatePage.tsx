import { useNavigate } from 'react-router-dom';
import { Page, Skeleton } from '@/components/ui';
import { handleApiError } from '@/shared/api/result-to-ui';
import { useCreateQuotation } from '../hooks/useQuotations';
import { usePartiesList } from '@/features/parties/hooks/useParties';
import { useUnitsList } from '@/features/inventory/units/hooks/useUnits';
import { useItemsList } from '@/features/inventory/items/hooks/useItems';
import { QuotationForm } from '../components/QuotationForm';
import type { CreateQuotationFormData } from '../shared/schemas';

export default function QuotationCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateQuotation();

  const { data: suppliersData, isLoading: suppliersLoading } = usePartiesList({ partyType: 'Supplier' });
  const { data: unitsData, isLoading: unitsLoading } = useUnitsList();
  const { data: itemsData, isLoading: itemsLoading } = useItemsList();

  const isLoading = suppliersLoading || unitsLoading || itemsLoading;

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
    createMutation.mutate(data, {
      onSuccess: (id) => navigate(`/procurement/quotations/${id}`),
      onError: (err) => handleApiError(err, () => {}),
    });
  }

  if (isLoading) {
    return (
      <Page title="إنشاء عرض سعر">
        <Skeleton className="h-96" />
      </Page>
    );
  }

  return (
    <Page
      title="إنشاء عرض سعر"
      maxWidth="xl"
      actions={
        <button
          type="button"
          onClick={() => navigate('/procurement/quotations')}
          className="text-sm text-[var(--color-primary)] hover:underline"
        >
          رجوع
        </button>
      }
    >
      <QuotationForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/procurement/quotations')}
        isPending={createMutation.isPending}
        supplierOptions={supplierOptions}
        currencyOptions={currencyOptions}
        itemOptions={itemOptions}
        unitOptions={unitOptions}
      />
    </Page>
  );
}
