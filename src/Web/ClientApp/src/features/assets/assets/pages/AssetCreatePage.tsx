import { useNavigate } from 'react-router-dom';
import { Button, Page } from '@/components/ui';
import { useCreateAsset } from '../hooks/useAssets';
import { useAssetGroupsList } from '../../asset-groups/hooks/useAssetGroups';
import { AssetForm } from '../components/AssetForm';
import { useEmployees } from '@/features/organization/hooks';
import { useCurrenciesList } from '@/features/financial-settings/hooks/useCurrencies';
import { useExchangeRatesList } from '@/features/financial-settings/hooks/useExchangeRates';
import { useActiveLocations } from '@/features/inventory/locations/hooks/useLocations';
import { documentsClient } from '@/features/documents/shared/client';
import { notify } from '@/features/notifications/notify';

export function AssetCreatePage() {
  const navigate = useNavigate();

  const { data: assetGroups = [] } = useAssetGroupsList({ isActive: true });
  const { data: employees = [] } = useEmployees();
  const { data: locations = [] } = useActiveLocations();
  const { data: currencies = [] } = useCurrenciesList(true);
  const { data: exchangeRates = [] } = useExchangeRatesList({ isActive: true });
  const createAsset = useCreateAsset();

  const groupOptions = assetGroups.map(g => ({ value: g.id, label: `${g.code} - ${g.name}` }));
  const employeeOptions = employees.map((e: { id: number; name: string }) => ({ value: e.id, label: e.name }));
  const locationOptions = locations.map((l) => ({ value: l.id, label: l.breadcrumb ? `${l.breadcrumb}/${l.code} - ${l.name}` : `${l.code} - ${l.name}` }));
  const currencyOptions = currencies.map((c) => ({ value: c.id, label: `${c.code} - ${c.name}` }));
  const exchangeRateOptions = exchangeRates.map((r) => ({ value: r.id, label: `${r.currencyCode} - ${r.rate}` }));

  return (
    <Page
      title="إنشاء أصل جديد"
      description="إضافة أصل إلى سجل الأصول الثابتة"
      maxWidth="full"
      onBack={() => navigate('/assets')}
      actions={
        <div className="flex gap-2">
          <Button variant="ghost" type="button" onClick={() => navigate('/assets')}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" form="asset-form" loading={createAsset.isPending}>
            إنشاء الأصل
          </Button>
        </div>
      }
    >
      <AssetForm
        mode="create"
        showActions={false}
        categoryOptions={groupOptions}
        locationOptions={locationOptions}
        employeeOptions={employeeOptions}
        currencyOptions={currencyOptions}
        exchangeRateOptions={exchangeRateOptions}
        isPending={createAsset.isPending}
        onCancel={() => navigate('/assets')}
        onSubmit={async (data, attributeValues, files) => {
          const result = await createAsset.mutateAsync({
            ...data,
            description: data.description || undefined,
            locationId: data.locationId || undefined,
            employeeId: data.employeeId || undefined,
            assetTag: data.assetTag || undefined,
            barcode: data.barcode || undefined,
            serialNumber: data.serialNumber || undefined,
            exchangeRateId: data.exchangeRateId || undefined,
            acquisitionCost: data.acquisitionCost || undefined,
            usefulLifeYears: data.usefulLifeYears || undefined,
            notes: data.notes || undefined,
            attributeValues,
          } as unknown as Record<string, unknown>);

          for (const file of files) {
            try {
              await documentsClient.upload('Asset', result, file);
              notify({ type: 'success', title: `تم رفع ${file.name}` });
            } catch {
              notify({ type: 'error', title: `فشل رفع ${file.name}` });
            }
          }

          navigate(`/assets/${result}`);
        }}
      />
    </Page>
  );
}
