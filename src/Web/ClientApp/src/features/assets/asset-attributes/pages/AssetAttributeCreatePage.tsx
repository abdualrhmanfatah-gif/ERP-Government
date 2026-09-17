import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { AssetAttributeForm } from '../components/AssetAttributeForm';
import { useCreateAssetAttribute } from '../hooks/useAssetAttributes';
import type { AttributeDefinitionInput } from '../shared/schemas';

export default function AssetAttributeCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateAssetAttribute();

  async function handleSubmit(data: AttributeDefinitionInput) {
    await createMutation.mutateAsync({
      code: data.code,
      name: data.name,
      description: data.description || undefined,
      unit: data.unit || undefined,
      sortOrder: data.sortOrder,
      attributeDataType: data.attributeDataType,
    });
    navigate('/assets/attributes');
  }

  return (
    <Page
      title="تعريف مواصفة جديدة"
      description="إضافة تعريف مواصفة جديد لمواصفات الأصول"
      maxWidth="lg"
      onBack={() => navigate('/assets/attributes')}
    >
      <AssetAttributeForm
        mode="create"
        onSubmit={handleSubmit}
        onCancel={() => navigate('/assets/attributes')}
        saving={createMutation.isPending}
      />
    </Page>
  );
}
