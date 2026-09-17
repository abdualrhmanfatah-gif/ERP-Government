import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller, type FieldErrors } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Page, Button, Input, Select, Card, Combobox, Alert, Switch, Tabs } from '@/components/ui';
import { Coins, BookOpen } from 'lucide-react';
import { useCreateAssetGroup, useAssetGroupsList } from '../hooks/useAssetGroups';
import { createAssetGroupSchema, type CreateAssetGroupInput } from '../shared/schemas';
import { assetCategoryOptions } from '../shared/types';
import { depreciationMethodOptions } from '../../shared/depreciation-method';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { handleApiError } from '@/shared/api/result-to-ui';

const toOptionalNumber = (value: unknown) =>
  value === '' || value === null || value === undefined ? undefined : Number(value);

const ACCOUNT_FIELD_NAMES: readonly string[] = [
  'assetAccountId',
  'accumulatedDepreciationAccountId',
  'depreciationExpenseAccountId',
  'disposalAccountId',
];

export default function AssetGroupCreatePage() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('params');

  const { data: parentGroups } = useAssetGroupsList({ isActive: true });
  const { data: accounts = [] } = useAccountsList({ isActive: true, isPostable: true });
  const createMutation = useCreateAssetGroup();

  const { register, handleSubmit, control, setError, formState: { errors, isSubmitting } } = useForm<CreateAssetGroupInput>({
    resolver: zodResolver(createAssetGroupSchema),
    defaultValues: {
      isDepreciable: true,
      assetCategory: 'Tangible',
      depreciationMethod: 'StraightLine',
    },
  });

  const parentOptions = (parentGroups ?? []).map(g => ({ value: g.id.toString(), label: `${g.code} - ${g.name}` }));
  const accountOptions = (accounts ?? []).map(a => ({ value: String(a.id ?? ''), label: `${a.code} - ${a.name}` }));

  async function onSubmit(data: CreateAssetGroupInput) {
    try {
      const result = await createMutation.mutateAsync({
        ...data,
        parentAssetGroupId: data.parentAssetGroupId || undefined,
        description: data.description || undefined,
        depreciationRate: data.depreciationRate || undefined,
        defaultUsefulLifeYears: data.defaultUsefulLifeYears || undefined,
        residualValuePercentage: data.residualValuePercentage || undefined,
        assetAccountId: data.assetAccountId || undefined,
        accumulatedDepreciationAccountId: data.accumulatedDepreciationAccountId || undefined,
        depreciationExpenseAccountId: data.depreciationExpenseAccountId || undefined,
        disposalAccountId: data.disposalAccountId || undefined,
      });
      navigate(`/assets/asset-groups/${result}`);
    } catch (err) {
      handleApiError(err, setError);
    }
  }

  const onInvalid = (formErrors: FieldErrors<CreateAssetGroupInput>) => {
    const firstError = Object.keys(formErrors)[0];
    setActiveTab(ACCOUNT_FIELD_NAMES.includes(firstError) ? 'accounts' : 'params');
  };

  return (
    <Page
      title="إنشاء مجموعة أصول"
      description="إنشاء مجموعة أصول جديدة"
      maxWidth="xl"
      onBack={() => navigate('/assets/asset-groups')}
    >
      <form onSubmit={handleSubmit(onSubmit, onInvalid)} className="flex flex-col gap-4" aria-label="نموذج مجموعة الأصول">
        {errors.root && (
          <Alert variant="error" role="alert">{errors.root.message}</Alert>
        )}

        <Card>
          <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">المعلومات الأساسية</h3>
          <div className="flex flex-wrap items-end gap-3">
            <div className="w-36 shrink-0">
              <Input label="الكود" {...register('code')} error={errors.code?.message} required />
            </div>
            <div className="min-w-56 flex-1">
              <Input label="الاسم" {...register('name')} error={errors.name?.message} required />
            </div>
            <div className="min-w-44 flex-1">
              <Select
                label="الفئة"
                {...register('assetCategory')}
                options={assetCategoryOptions}
                error={errors.assetCategory?.message}
                required
              />
            </div>
            <div className="min-w-44 flex-1">
              <Select
                label="المجموعة الأب"
                {...register('parentAssetGroupId', { setValueAs: toOptionalNumber })}
                options={[{ value: '', label: '— بدون —' }, ...parentOptions]}
                error={errors.parentAssetGroupId?.message}
              />
            </div>
            <div className="min-w-64 flex-[2]">
              <Input label="الوصف" {...register('description')} error={errors.description?.message} />
            </div>
          </div>
        </Card>

        <Tabs
          value={activeTab}
          onChange={setActiveTab}
          tabs={[
            {
              key: 'params',
              label: 'معلمات الإهلاك',
              icon: <Coins size={15} />,
              content: (
                <Card>
                  <div className="flex flex-wrap items-end gap-3">
                    <div className="min-w-48 flex-1">
                      <Select
                        label="طريقة الإهلاك"
                        {...register('depreciationMethod')}
                        options={depreciationMethodOptions}
                        error={errors.depreciationMethod?.message}
                        required
                      />
                    </div>
                    <div className="min-w-36 flex-1">
                      <Input
                        label="العمر الإنتاجي (سنوات)"
                        type="number"
                        {...register('defaultUsefulLifeYears', { setValueAs: toOptionalNumber })}
                        error={errors.defaultUsefulLifeYears?.message}
                      />
                    </div>
                    <div className="min-w-36 flex-1">
                      <Input
                        label="نسبة القيمة التخريدية (%)"
                        type="number"
                        step="0.01"
                        {...register('residualValuePercentage', { setValueAs: toOptionalNumber })}
                        error={errors.residualValuePercentage?.message}
                      />
                    </div>
                    <div className="min-w-36 flex-1">
                      <Input
                        label="معدل الإهلاك (%)"
                        type="number"
                        step="0.0001"
                        {...register('depreciationRate', { setValueAs: toOptionalNumber })}
                        error={errors.depreciationRate?.message}
                      />
                    </div>
                    <div className="shrink-0">
                      <Controller
                        control={control}
                        name="isDepreciable"
                        render={({ field }) => (
                          <Switch
                            checked={!!field.value}
                            onChange={field.onChange}
                            label="قابل للإهلاك"
                          />
                        )}
                      />
                    </div>
                  </div>
                </Card>
              ),
            },
            {
              key: 'accounts',
              label: 'الحسابات المحاسبية',
              icon: <BookOpen size={15} />,
              content: (
                <Card>
                  <div className="flex flex-wrap items-end gap-3">
                    <div className="min-w-56 flex-1">
                      <Controller
                        control={control}
                        name="assetAccountId"
                        render={({ field }) => (
                          <Combobox
                            label="حساب الأصول"
                            options={accountOptions}
                            value={field.value ? String(field.value) : undefined}
                            onChange={(v) => field.onChange(v ? Number(v) : null)}
                            placeholder="اختر الحساب..."
                            searchPlaceholder="بحث..."
                          />
                        )}
                      />
                    </div>
                    <div className="min-w-56 flex-1">
                      <Controller
                        control={control}
                        name="accumulatedDepreciationAccountId"
                        render={({ field }) => (
                          <Combobox
                            label="حساب مجمع الإهلاك"
                            options={accountOptions}
                            value={field.value ? String(field.value) : undefined}
                            onChange={(v) => field.onChange(v ? Number(v) : null)}
                            placeholder="اختر الحساب..."
                            searchPlaceholder="بحث..."
                          />
                        )}
                      />
                    </div>
                    <div className="min-w-56 flex-1">
                      <Controller
                        control={control}
                        name="depreciationExpenseAccountId"
                        render={({ field }) => (
                          <Combobox
                            label="حساب مصروف الإهلاك"
                            options={accountOptions}
                            value={field.value ? String(field.value) : undefined}
                            onChange={(v) => field.onChange(v ? Number(v) : null)}
                            placeholder="اختر الحساب..."
                            searchPlaceholder="بحث..."
                          />
                        )}
                      />
                    </div>
                    <div className="min-w-56 flex-1">
                      <Controller
                        control={control}
                        name="disposalAccountId"
                        render={({ field }) => (
                          <Combobox
                            label="حساب التخلص"
                            options={accountOptions}
                            value={field.value ? String(field.value) : undefined}
                            onChange={(v) => field.onChange(v ? Number(v) : null)}
                            placeholder="اختر الحساب..."
                            searchPlaceholder="بحث..."
                          />
                        )}
                      />
                    </div>
                  </div>
                </Card>
              ),
            },
          ]}
        />

        <div className="flex justify-end gap-2">
          <Button variant="ghost" type="button" onClick={() => navigate('/assets/asset-groups')}>
            إلغاء
          </Button>
          <Button type="submit" variant="primary" loading={createMutation.isPending || isSubmitting}>
            إنشاء المجموعة
          </Button>
        </div>
      </form>
    </Page>
  );
}
