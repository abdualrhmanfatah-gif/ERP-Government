import { useParams, useNavigate } from 'react-router-dom';
import { Button, Page, Badge, EmptyState } from '@/components/ui';
import { useAssetById, useUpdateAsset } from '../hooks/useAssets';
import { useAssetGroupsList } from '../../asset-groups/hooks/useAssetGroups';
import { AssetForm } from '../components/AssetForm';
import { useEmployees } from '@/features/organization/hooks';
import { useCurrenciesList } from '@/features/financial-settings/hooks/useCurrencies';
import { useExchangeRatesList } from '@/features/financial-settings/hooks/useExchangeRates';
import { useActiveLocations } from '@/features/inventory/locations/hooks/useLocations';
import { getQueryErrorMessage } from '@/shared/api/query-error';

export function AssetEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const assetId = Number(id);

  const { data: asset, isLoading: assetLoading, error, refetch } = useAssetById(assetId);
  const updateAsset = useUpdateAsset();
  const { data: assetGroups = [] } = useAssetGroupsList({ isActive: true });
  const { data: employees = [] } = useEmployees();
  const { data: locations = [] } = useActiveLocations();
  const { data: currencies = [] } = useCurrenciesList(true);
  const { data: exchangeRates = [] } = useExchangeRatesList({ isActive: true });

  if (assetLoading) {
    return <Page title="" loading>{null}</Page>;
  }

  if (error) {
    return (
      <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>
        {null}
      </Page>
    );
  }

  if (!asset) {
    return (
      <Page title="" onBack={() => navigate('/assets')}>
        <EmptyState message="الأصل غير موجود" />
      </Page>
    );
  }

  const groupOptions = assetGroups.map(g => ({ value: g.id, label: `${g.code} - ${g.name}` }));
  const employeeOptions = employees.map((e: { id: number; name: string }) => ({ value: e.id, label: e.name }));
  const locationOptions = locations.map((l) => ({ value: l.id, label: l.breadcrumb ? `${l.breadcrumb}/${l.code} - ${l.name}` : `${l.code} - ${l.name}` }));
  const currencyOptions = currencies.map((c) => ({ value: c.id, label: `${c.code} - ${c.name}` }));
  const exchangeRateOptions = exchangeRates.map((r) => ({ value: r.id, label: `${r.currencyCode} - ${r.rate}` }));

  return (
    <Page
      title={`تعديل الأصل: ${asset.name}`}
      description={
        <span className="flex flex-wrap items-center gap-3">
          <span dir="ltr" className="tabular-nums">{asset.code}</span>
          <Badge variant={asset.isActive ? 'success' : 'default'}>
            {asset.isActive ? 'مُفعّل: نعم' : 'مُفعّل: لا'}
          </Badge>
        </span>
      }
      maxWidth="full"
      onBack={() => navigate(`/assets/${assetId}`)}
      actions={
        <div className="flex gap-2">
          <Button variant="ghost" type="button" onClick={() => navigate(`/assets/${assetId}`)}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" form="asset-form" loading={updateAsset.isPending}>
            حفظ التعديلات
          </Button>
        </div>
      }
    >
      <AssetForm
        mode="edit"
        showActions={false}
        initialData={asset}
        categoryOptions={groupOptions}
        locationOptions={locationOptions}
        employeeOptions={employeeOptions}
        currencyOptions={currencyOptions}
        exchangeRateOptions={exchangeRateOptions}
        isPending={updateAsset.isPending}
        onCancel={() => navigate(`/assets/${assetId}`)}
        onSubmit={async (data, attributeValues) => {
          await updateAsset.mutateAsync({
            id: assetId,
            data: {
              ...data,
              id: assetId,
              rowVersion: asset.rowVersion,
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
            } as unknown as Record<string, unknown>,
          });
          navigate(`/assets/${assetId}`);
        }}
      />
    </Page>
  );
}
