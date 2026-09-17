import { useForm, Controller, type Resolver } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Card, Button, Input, Textarea, Combobox, Alert } from '@/components/ui';
import type { ComboboxOption } from '@/components/ui/Combobox';
import { transferFormSchema, type TransferFormInput } from '../shared/schemas';

interface TransferFormProps {
  mode: 'create' | 'edit';
  assetOptions: ComboboxOption[];
  locationOptions: ComboboxOption[];
  employeeOptions: ComboboxOption[];
  defaultValues?: Partial<TransferFormInput>;
  assetLabel?: string;
  fromSummary?: {
    locationName?: string | null;
    employeeName?: string | null;
    departmentName?: string | null;
  } | null;
  onSubmit: (values: TransferFormInput) => Promise<void>;
  onCancel: () => void;
  isPending?: boolean;
  error?: string;
}

export function TransferForm({
  mode,
  assetOptions,
  locationOptions,
  employeeOptions,
  defaultValues,
  assetLabel,
  fromSummary,
  onSubmit,
  onCancel,
  isPending,
  error,
}: TransferFormProps) {
  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<TransferFormInput>({
    resolver: zodResolver(transferFormSchema) as Resolver<TransferFormInput>,
    defaultValues: {
      assetId: undefined,
      transactionDate: new Date().toISOString().slice(0, 10),
      toLocationId: null,
      toEmployeeId: null,
      notes: '',
      ...defaultValues,
    },
  });

  const isEdit = mode === 'edit';

  return (
    <form
      onSubmit={handleSubmit(async (values) => {
        await onSubmit(values);
      })}
    >
      <div className="grid gap-4">
        {error && <Alert variant="error">{error}</Alert>}

        {fromSummary && (
          <Card>
            <div className="grid gap-3 md:grid-cols-3">
              <div>
                <span className="text-label-sm text-[var(--color-on-surface-variant)]">من موقع: </span>
                <span className="font-medium">{fromSummary.locationName ?? '—'}</span>
              </div>
              <div>
                <span className="text-label-sm text-[var(--color-on-surface-variant)]">من حارس: </span>
                <span className="font-medium">{fromSummary.employeeName ?? '—'}</span>
              </div>
              <div>
                <span className="text-label-sm text-[var(--color-on-surface-variant)]">من إدارة: </span>
                <span className="font-medium">{fromSummary.departmentName ?? '—'}</span>
              </div>
            </div>
          </Card>
        )}

        <Card>
          <div className="grid gap-4 md:grid-cols-2">
            <Controller
              control={control}
              name="assetId"
              render={({ field }) => (
                <Combobox
                  label="الأصل"
                  options={assetOptions}
                  value={isEdit && assetLabel ? assetLabel : field.value ? String(field.value) : undefined}
                  onChange={(v) => field.onChange(v ? Number(v) : undefined)}
                  placeholder="اختر الأصل..."
                  searchPlaceholder="بحث بالكود أو الاسم..."
                  disabled={isEdit}
                  error={errors.assetId?.message}
                />
              )}
            />

            <Controller
              control={control}
              name="transactionDate"
              render={({ field }) => (
                <Input
                  label="تاريخ النقل"
                  type="date"
                  required
                  value={field.value}
                  onChange={field.onChange}
                  error={errors.transactionDate?.message}
                />
              )}
            />

            <Controller
              control={control}
              name="toLocationId"
              render={({ field }) => (
                <Combobox
                  label="إلى موقع (اتركه فارغاً إذا لم يتغير)"
                  options={locationOptions}
                  value={field.value ? String(field.value) : undefined}
                  onChange={(v) => field.onChange(v ? Number(v) : null)}
                  placeholder="اختر الموقع..."
                  searchPlaceholder="بحث..."
                  error={errors.toLocationId?.message}
                />
              )}
            />

            <Controller
              control={control}
              name="toEmployeeId"
              render={({ field }) => (
                <Combobox
                  label="إلى حارس (اتركه فارغاً إذا لم يتغير)"
                  options={employeeOptions}
                  value={field.value ? String(field.value) : undefined}
                  onChange={(v) => field.onChange(v ? Number(v) : null)}
                  placeholder="اختر الحارس..."
                  searchPlaceholder="بحث..."
                />
              )}
            />

            <div className="md:col-span-2">
              <Controller
                control={control}
                name="notes"
                render={({ field }) => (
                  <Textarea
                    label="ملاحظات"
                    rows={3}
                    value={field.value ?? ''}
                    onChange={field.onChange}
                    error={errors.notes?.message}
                  />
                )}
              />
            </div>
          </div>
        </Card>

        <div className="flex justify-end gap-2">
          <Button variant="ghost" type="button" onClick={onCancel} disabled={isPending}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" loading={isPending}>
            {isEdit ? 'حفظ التعديلات' : 'إنشاء النقل'}
          </Button>
        </div>
      </div>
    </form>
  );
}
