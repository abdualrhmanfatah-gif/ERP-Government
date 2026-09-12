import { useParams, useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useItemById, useUpdateItem } from '../hooks/useItems';
import { useItemCategoriesList } from '../../item-categories/hooks/useItemCategories';
import { useUnitsList } from '../../units/hooks/useUnits';
import { notify } from '@/features/notifications/notify';
import { handleApiError } from '@/shared/api/result-to-ui';
import { ItemForm } from '../components/ItemForm';
import type { CreateItemInput } from '../shared/schemas';

export default function ItemEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const itemId = Number(id);

  const { data: item, isLoading: itemLoading } = useItemById(itemId);
  const updateItem = useUpdateItem();
  const { data: categories = [], isLoading: catLoading } = useItemCategoriesList();
  const { data: units = [], isLoading: unitsLoading } = useUnitsList();

  const categoryOptions = categories.map((c) => ({ value: String(c.id), label: c.name }));
  const unitOptions = units.map((u) => ({ value: String(u.id), label: u.name }));

  function onSubmit(data: CreateItemInput) {
    updateItem.mutate(
      { id: itemId, data },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم تحديث الصنف بنجاح' });
          navigate(`/inventory/items/${itemId}`);
        },
        onError: (err) => handleApiError(err, () => {}),
      }
    );
  }

  if (!item && !itemLoading) {
    return (
      <Page title="الصنف غير موجود" maxWidth="lg">
        <p className="text-[var(--color-error)]">الصنف غير موجود</p>
      </Page>
    );
  }

  return (
    <Page
      title={`تعديل الصنف: ${item?.name ?? ''}`}
      loading={itemLoading}
      maxWidth="lg"
      breadcrumbs={[{ label: 'الأصناف', path: '/inventory/items' }, { label: item?.name ?? '' }]}
    >
      <ItemForm
        initialData={item}
        onSubmit={onSubmit}
        onCancel={() => navigate(`/inventory/items/${itemId}`)}
        isPending={updateItem.isPending}
        categoryOptions={categoryOptions}
        unitOptions={unitOptions}
        isLoading={itemLoading || catLoading || unitsLoading}
      />
    </Page>
  );
}
