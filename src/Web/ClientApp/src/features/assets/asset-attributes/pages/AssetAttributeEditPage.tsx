import { useNavigate, useParams } from 'react-router-dom';
import { Page, EmptyState, StatusBadge } from '@/components/ui';
import { AssetAttributeForm } from '../components/AssetAttributeForm';
import { useAssetAttributeDetail, useUpdateAssetAttribute } from '../hooks/useAssetAttributes';
import type { AttributeDefinitionInput } from '../shared/schemas';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { getActiveBadge } from '../../shared/status';

export default function AssetAttributeEditPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const definitionId = Number(id);

  const { data: definition, isLoading, error, refetch } = useAssetAttributeDetail(definitionId);
  const updateMutation = useUpdateAssetAttribute();

  if (isLoading) {
    return <Page title="" loading>{null}</Page>;
  }

  if (error) {
    return (
      <Page title="" error={getQueryErrorMessage(error)} onRetry={() => refetch()}>
        {null}
      </Page>
    );
  }

  if (!definition) {
    return (
      <Page title="" onBack={() => navigate('/assets/attributes')}>
        <EmptyState message="المواصفة غير موجودة" />
      </Page>
    );
  }

  const badge = getActiveBadge(definition.isActive);

  async function handleSubmit(data: AttributeDefinitionInput) {
    await updateMutation.mutateAsync({
      id: definitionId,
      data: {
        name: data.name,
        description: data.description || undefined,
        unit: data.unit || undefined,
        sortOrder: data.sortOrder,
        attributeDataType: data.attributeDataType,
        isActive: data.isActive,
        rowVersion: definition!.rowVersion,
      },
    });
    navigate('/assets/attributes');
  }

  return (
    <Page
      title={`تعديل: ${definition.name}`}
      description={
        <span className="flex flex-wrap items-center gap-3">
          <span dir="ltr" className="tabular-nums font-mono">{definition.code}</span>
          <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>
        </span>
      }
      maxWidth="lg"
      onBack={() => navigate('/assets/attributes')}
    >
      <AssetAttributeForm
        mode="edit"
        defaultValues={{
          code: definition.code,
          name: definition.name,
          description: definition.description ?? '',
          unit: definition.unit ?? '',
          sortOrder: definition.sortOrder,
          attributeDataType: definition.attributeDataType,
          isActive: definition.isActive,
        }}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/assets/attributes')}
        saving={updateMutation.isPending}
      />
    </Page>
  );
}
