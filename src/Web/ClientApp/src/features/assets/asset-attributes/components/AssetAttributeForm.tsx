import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Input, Select, Textarea, Card, Alert, Switch } from '@/components/ui';
import { attributeDefinitionSchema, type AttributeDefinitionInput } from '../shared/schemas';
import { dataTypeOptions } from '../shared/types';
import { handleApiError } from '@/shared/api/result-to-ui';

interface AssetAttributeFormProps {
  mode: 'create' | 'edit';
  defaultValues?: AttributeDefinitionInput;
  onSubmit: (data: AttributeDefinitionInput) => Promise<void>;
  onCancel: () => void;
  saving?: boolean;
}

const CREATE_DEFAULTS: AttributeDefinitionInput = {
  code: '',
  name: '',
  description: '',
  unit: '',
  sortOrder: 0,
  attributeDataType: 'Text',
  isActive: true,
};

export function AssetAttributeForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  saving,
}: AssetAttributeFormProps) {
  const {
    register,
    handleSubmit,
    control,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<AttributeDefinitionInput>({
    resolver: zodResolver(attributeDefinitionSchema),
    defaultValues: defaultValues ?? CREATE_DEFAULTS,
  });

  async function submit(data: AttributeDefinitionInput) {
    try {
      await onSubmit(data);
    } catch (err) {
      handleApiError(err, setError);
    }
  }

  return (
    <form
      onSubmit={handleSubmit(submit)}
      className="flex flex-col gap-4"
      aria-label="نموذج تعريف المواصفة"
    >
      {errors.root && (
        <Alert variant="error" role="alert">{errors.root.message}</Alert>
      )}

      <Card>
        <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">
          المعلومات الأساسية
        </h3>
        <div className="flex flex-wrap items-end gap-3">
          <div className="w-44 shrink-0">
            <Input
              label="الكود"
              {...register('code')}
              error={errors.code?.message}
              readOnly={mode === 'edit'}
              disabled={mode === 'edit'}
              required
              dir="ltr"
              className="tabular-nums font-mono"
              data-testid="attr-code"
            />
          </div>
          <div className="min-w-56 flex-1">
            <Input
              label="الاسم"
              {...register('name')}
              error={errors.name?.message}
              required
              data-testid="attr-name"
            />
          </div>
          <div className="min-w-44 flex-1">
            <Select
              label="نوع البيانات"
              {...register('attributeDataType')}
              options={dataTypeOptions}
              error={errors.attributeDataType?.message}
              required
              data-testid="attr-datatype"
            />
          </div>
          <div className="min-w-36 flex-1">
            <Input
              label="الوحدة"
              {...register('unit')}
              error={errors.unit?.message}
              data-testid="attr-unit"
            />
          </div>
          <div className="w-32 shrink-0">
            <Input
              label="ترتيب العرض"
              type="number"
              {...register('sortOrder', { valueAsNumber: true })}
              error={errors.sortOrder?.message}
              data-testid="attr-sortorder"
            />
          </div>
        </div>
      </Card>

      <Card>
        <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">
          إعدادات إضافية
        </h3>
        <div className="flex flex-wrap items-end gap-3">
          <div className="min-w-64 flex-[2]">
            <Textarea
              label="الوصف"
              rows={2}
              {...register('description')}
              error={errors.description?.message}
            />
          </div>
          {mode === 'edit' && (
            <div className="shrink-0 pb-2" data-testid="attr-active">
              <Controller
                control={control}
                name="isActive"
                render={({ field }) => (
                  <Switch
                    checked={!!field.value}
                    onChange={field.onChange}
                    label="نشط"
                  />
                )}
              />
            </div>
          )}
        </div>
      </Card>

      <div className="flex justify-end gap-2">
        <Button variant="ghost" type="button" onClick={onCancel}>
          إلغاء
        </Button>
        <Button
          type="submit"
          variant="primary"
          loading={saving || isSubmitting}
          data-testid="attr-save"
        >
          {mode === 'create' ? 'إنشاء التعريف' : 'حفظ التعديلات'}
        </Button>
      </div>
    </form>
  );
}
