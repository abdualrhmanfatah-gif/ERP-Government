import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateItem } from '../hooks/useItems';
import { useItemCategoriesList } from '../../item-categories/hooks/useItemCategories';
import { useUnitsList } from '../../units/hooks/useUnits';
import { notify } from '@/features/notifications/notify';
import { handleApiError } from '@/shared/api/result-to-ui';
import { ItemForm } from '../components/ItemForm';
import type { CreateItemInput } from '../shared/schemas';

export default function ItemCreatePage() {
  const navigate = useNavigate();
  const createItem = useCreateItem();
  const { data: categories = [], isLoading: catLoading } = useItemCategoriesList();
  const { data: units = [], isLoading: unitsLoading } = useUnitsList();

  const categoryOptions = categories.map((c) => ({ value: String(c.id), label: c.name }));
  const unitOptions = units.map((u) => ({ value: String(u.id), label: u.name }));

  function onSubmit(data: CreateItemInput) {
    createItem.mutate(data, {
      onSuccess: (id) => {
        notify({ type: 'success', title: 'تم إنشاء الصنف بنجاح' });
        navigate(`/inventory/items/${id}`);
      },
      onError: (err) => handleApiError(err, () => {}),
    });
  }

  return (
    <Page title="صنف جديد" description="إضافة صنف جديد للمخزون" maxWidth="lg">
      <ItemForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/inventory/items')}
        isPending={createItem.isPending}
        categoryOptions={categoryOptions}
        unitOptions={unitOptions}
        isLoading={catLoading || unitsLoading}
      />
    </Page>
  );
}
