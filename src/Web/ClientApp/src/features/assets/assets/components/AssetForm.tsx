import { useMemo, useState } from 'react';
import { useForm, useWatch, type FieldErrors } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ClipboardList, Coins, History, Images, ShoppingCart, X } from 'lucide-react';
import { Alert, Badge, Button, Card, EmptyState, Input, MoneyDisplay, Select, StatusBadge, Tabs, Textarea } from '@/components/ui';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';
import { FileUploadZone } from '@/components/FileUploadZone';
import { notify } from '@/features/notifications/notify';
import { formatFileSize } from '@/shared/utils/file-utils';
import { createAssetSchema, updateAssetSchema, type UpdateAssetInput } from '../shared/schemas';
import { acquisitionTypeLabels, assetStatusOptions, type AssetAttributeValueInput, type AssetDetail } from '../shared/types';
import { getAssetStatusBadge } from '../../shared/status';
import { handleApiError } from '@/shared/api/result-to-ui';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { useAssetGroupDetail } from '../../asset-groups/hooks/useAssetGroups';
import {
  AssetAttributeInputs,
  buildAttributeValues,
  findMissingRequiredAttributes,
  sortAttributeBindings,
  stateFromExistingValues,
  type AssetAttributeInputState,
} from './AssetAttributeInputs';
import { AssetDepreciationTable } from './AssetDepreciationTable';
import { AssetTransactionsTable } from './AssetTransactionsTable';

export type AssetFormValues = UpdateAssetInput;
export type AssetFormMode = 'create' | 'edit' | 'detail';

interface NumberOption {
  value: number;
  label: string;
}

interface AssetFormProps {
  mode?: AssetFormMode;
  initialData?: AssetDetail;
  onSubmit?: (data: AssetFormValues, attributeValues: AssetAttributeValueInput[], files: File[]) => Promise<unknown>;
  onSuccess?: () => void;
  onCancel?: () => void;
  isPending?: boolean;
  categoryOptions?: NumberOption[];
  locationOptions?: NumberOption[];
  employeeOptions?: NumberOption[];
  currencyOptions?: NumberOption[];
  exchangeRateOptions?: NumberOption[];
  formId?: string;
  showActions?: boolean;
}

const LOCKED_STATUSES = ['Active', 'UnderMaintenance', 'Disposed', 'WrittenOff'];

const toOptionalNumber = (value: unknown) =>
  value === '' || value === null || value === undefined ? undefined : Number(value);

function toOptions(options: NumberOption[]) {
  return options.map((option) => ({ value: String(option.value), label: option.label }));
}

function DetailField({
  label,
  value,
  ltr,
  className,
}: {
  label: string;
  value: string | number | null | undefined;
  ltr?: boolean;
  className?: string;
}) {
  const text = value === null || value === undefined || value === '' ? '—' : String(value);
  return (
    <div className={['flex flex-col gap-1', className ?? ''].filter(Boolean).join(' ')}>
      <span className="text-label-md text-[var(--color-on-surface)]">{label}</span>
      <div
        dir={ltr ? 'ltr' : undefined}
        title={text}
        className={[
          'flex h-[var(--density-compact-control-height)] items-center rounded-lg border-2 border-[var(--color-container-border)] bg-[var(--color-surface-container-low)] px-3 text-sm text-[var(--color-on-surface)]',
          ltr ? 'tabular-nums font-mono' : '',
        ].filter(Boolean).join(' ')}
      >
        <span className="truncate">{text}</span>
      </div>
    </div>
  );
}

function Field({
  detail,
  label,
  value,
  ltr,
  className,
  children,
}: {
  detail: boolean;
  label: string;
  value?: string | number | null;
  ltr?: boolean;
  className?: string;
  children: React.ReactNode;
}) {
  if (detail) return <DetailField label={label} value={value} ltr={ltr} className={className} />;
  return <div className={className}>{children}</div>;
}

export function AssetForm({
  mode = 'create',
  initialData,
  onSubmit,
  onSuccess,
  onCancel,
  isPending,
  categoryOptions = [],
  locationOptions = [],
  employeeOptions = [],
  currencyOptions = [],
  exchangeRateOptions = [],
  formId = 'asset-form',
  showActions = true,
}: AssetFormProps) {
  const isDetail = mode === 'detail';
  const isEdit = !!initialData;
  const [activeTab, setActiveTab] = useState('main');
  const [submitting, setSubmitting] = useState(false);
  const isLocked = isEdit && LOCKED_STATUSES.includes(initialData!.status);
  const statusBadge = initialData ? getAssetStatusBadge(initialData.status) : null;

  const {
    register,
    handleSubmit,
    setError,
    clearErrors,
    control,
    formState: { errors },
  } = useForm<AssetFormValues>({
    resolver: zodResolver(
      isEdit ? updateAssetSchema : (createAssetSchema as unknown as typeof updateAssetSchema)
    ),
    defaultValues: initialData
      ? {
          name: initialData.name,
          description: initialData.description ?? '',
          assetGroupId: initialData.assetGroupId,
          locationId: initialData.locationId ?? undefined,
          employeeId: initialData.employeeId ?? undefined,
          assetTag: initialData.assetTag ?? '',
          barcode: initialData.barcode ?? '',
          serialNumber: initialData.serialNumber ?? '',
          currencyId: initialData.currencyId,
          exchangeRateId: initialData.exchangeRateId ?? undefined,
          originalValue: initialData.originalValue,
          acquisitionCost: initialData.acquisitionCost ?? undefined,
          purchaseDate: initialData.purchaseDate ?? '',
          depreciationStartDate: initialData.depreciationStartDate ?? '',
          acquisitionType: initialData.acquisitionType,
          usefulLifeYears: initialData.usefulLifeYears ?? undefined,
          notes: initialData.notes ?? '',
          status: initialData.status,
          rowVersion: initialData.rowVersion,
        }
      : {
          name: '',
          description: '',
          assetTag: '',
          barcode: '',
          serialNumber: '',
          purchaseDate: '',
          depreciationStartDate: '',
          acquisitionType: '',
          notes: '',
        },
  });
  const selectedGroupId = Number(useWatch({ control, name: 'assetGroupId' })) || 0;
  const { data: groupDetail, isLoading: groupLoading, error: groupError } = useAssetGroupDetail(selectedGroupId);
  const attributeBindings = useMemo(
    () => sortAttributeBindings(groupDetail?.attributeBindings ?? []),
    [groupDetail]
  );

  const [attributeSeed, setAttributeSeed] = useState<{ key: string; values: AssetAttributeInputState }>({
    key: 'none',
    values: {},
  });
  const [pendingFiles, setPendingFiles] = useState<File[]>([]);

  const attributesSeedKey =
    initialData && groupDetail && groupDetail.id === initialData.assetGroupId
      ? `asset-${initialData.id}-group-${groupDetail.id}`
      : `group-${selectedGroupId}`;

  if (attributeSeed.key !== attributesSeedKey) {
    const values =
      initialData && groupDetail && groupDetail.id === initialData.assetGroupId
        ? stateFromExistingValues(attributeBindings, initialData.attributeValues ?? [])
        : {};
    setAttributeSeed({ key: attributesSeedKey, values });
  }

  const updateAttributeValue = (definitionId: number, value: AssetAttributeInputState[number]) =>
    setAttributeSeed((previous) => ({
      ...previous,
      values: { ...previous.values, [definitionId]: value },
    }));

  const updateValueNumbers = { setValueAs: toOptionalNumber };

  const tabFieldMap: Record<string, readonly string[]> = {
    purchase: ['originalValue', 'acquisitionCost', 'purchaseDate', 'acquisitionType', 'currencyId', 'exchangeRateId'],
    depreciation: ['depreciationStartDate', 'usefulLifeYears'],
  };

  const onInvalid = (formErrors: FieldErrors<AssetFormValues>) => {
    const first = Object.keys(formErrors)[0];
    const tab = Object.entries(tabFieldMap).find(([, fields]) => fields.includes(first))?.[0];
    setActiveTab(tab ?? 'main');
  };

  const submit = async (data: AssetFormValues) => {
    if (!onSubmit) return;
    clearErrors('root');
    const missingAttributes = findMissingRequiredAttributes(attributeBindings, attributeSeed.values);
    if (missingAttributes.length > 0) {
      setError('root', { message: `المواصفات المطلوبة: ${missingAttributes.join('، ')}` });
      setActiveTab('attributes');
      return;
    }
    setSubmitting(true);
    try {
      await onSubmit(data, buildAttributeValues(attributeBindings, attributeSeed.values), pendingFiles);
      onSuccess?.();
    } catch (err) {
      handleApiError(err, setError);
    } finally {
      setSubmitting(false);
    }
  };

  const groupLabel =
    initialData?.assetGroupName ?? categoryOptions.find((option) => option.value === initialData?.assetGroupId)?.label;
  const locationLabel =
    initialData?.locationName ?? locationOptions.find((option) => option.value === initialData?.locationId)?.label;
  const employeeLabel =
    initialData?.employeeName ?? employeeOptions.find((option) => option.value === initialData?.employeeId)?.label;
  const currencyLabel =
    initialData?.currencyCode ?? currencyOptions.find((option) => option.value === initialData?.currencyId)?.label;

  const mainCard = (
    <Card>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-12">
        {(isEdit || isDetail) && (
          <Field detail={isDetail} label="الكود" value={initialData?.code} ltr className="sm:col-span-2">
            <Input label="الكود" value={initialData?.code ?? ''} disabled readOnly dir="ltr" className="tabular-nums font-mono" />
          </Field>
        )}
        <Field detail={isDetail} label="اسم الأصل" value={initialData?.name} className="sm:col-span-3">
          <Input label="اسم الأصل" {...register('name')} error={errors.name?.message} required disabled={isDetail} />
        </Field>
        <Field detail={isDetail} label="المجموعة" value={groupLabel} className="sm:col-span-3">
          <Select
            label="المجموعة"
            {...register('assetGroupId', updateValueNumbers)}
            options={[{ value: '', label: 'اختر المجموعة' }, ...toOptions(categoryOptions)]}
            error={errors.assetGroupId?.message}
            required
            disabled={isLocked}
          />
        </Field>
        <Field detail={isDetail} label="الموقع" value={locationLabel} className={isEdit || isDetail ? 'sm:col-span-2' : 'sm:col-span-3'}>
          <Select
            label="الموقع"
            {...register('locationId', updateValueNumbers)}
            options={[{ value: '', label: 'اختر الموقع' }, ...toOptions(locationOptions)]}
            disabled={isDetail}
          />
        </Field>
        <Field detail={isDetail} label="الحارس" value={employeeLabel} className={isEdit || isDetail ? 'sm:col-span-2' : 'sm:col-span-3'}>
          <Select
            label="الحارس"
            {...register('employeeId', updateValueNumbers)}
            options={[{ value: '', label: 'اختر الحارس' }, ...toOptions(employeeOptions)]}
            disabled={isDetail || isLocked}
          />
        </Field>
        <Field detail={isDetail} label="الوصف" value={initialData?.description} className="sm:col-span-6">
          <Textarea
            label="الوصف"
            {...register('description')}
            error={errors.description?.message}
            rows={1}
            disabled={isDetail}
            className="!min-h-0 h-[var(--density-comfortable-control-height)] resize-none"
          />
        </Field>
        <Field detail={isDetail} label="ملاحظات" value={initialData?.notes} className="sm:col-span-6">
          <Textarea
            label="ملاحظات"
            {...register('notes')}
            error={errors.notes?.message}
            rows={1}
            disabled={isDetail}
            className="!min-h-0 h-[var(--density-comfortable-control-height)] resize-none"
          />
        </Field>
        <Field detail={isDetail} label="الوسم" value={initialData?.assetTag} className={isEdit || isDetail ? 'sm:col-span-3' : 'sm:col-span-4'}>
          <Input label="الوسم" {...register('assetTag')} disabled={isDetail} />
        </Field>
        <Field detail={isDetail} label="الباركود" value={initialData?.barcode} ltr className={isEdit || isDetail ? 'sm:col-span-3' : 'sm:col-span-4'}>
          <Input label="الباركود" {...register('barcode')} dir="ltr" className="tabular-nums font-mono" disabled={isDetail} />
        </Field>
        <Field detail={isDetail} label="الرقم التسلسلي" value={initialData?.serialNumber} ltr className={isEdit || isDetail ? 'sm:col-span-3' : 'sm:col-span-4'}>
          <Input label="الرقم التسلسلي" {...register('serialNumber')} dir="ltr" className="tabular-nums font-mono" disabled={isDetail} />
        </Field>
        {!isDetail && isEdit && initialData!.status === 'Draft' && (
          <Field detail={false} label="الحالة" className="sm:col-span-3">
            <Select label="الحالة" {...register('status')} options={assetStatusOptions} error={errors.status?.message} />
          </Field>
        )}
        {!isDetail && isEdit && initialData!.status !== 'Draft' && statusBadge && (
          <div className="flex flex-col gap-1 sm:col-span-3">
            <span className="text-label-md text-[var(--color-on-surface)]">الحالة</span>
            <div className="flex min-h-[var(--density-comfortable-control-height)] items-center">
              <StatusBadge variant={statusBadge.variant}>{statusBadge.label}</StatusBadge>
            </div>
          </div>
        )}
        {isDetail && (
          <DetailField label="القسم الحالي" value={initialData?.currentDepartment} className="sm:col-span-3" />
        )}
      </div>
      {isDetail && (
        <div className="mt-3 grid grid-cols-1 gap-3 border-t border-[var(--color-container-border)] pt-3 sm:grid-cols-4">
          <DetailField label="تاريخ الإنشاء" value={initialData?.created ? new Date(initialData.created).toLocaleDateString('ar') : undefined} />
          <DetailField label="أنشئ بواسطة" value={initialData?.createdBy} />
          <DetailField label="آخر تعديل" value={initialData?.lastModified ? new Date(initialData.lastModified).toLocaleDateString('ar') : undefined} />
          <DetailField label="عدّل بواسطة" value={initialData?.lastModifiedBy} />
        </div>
      )}
    </Card>
  );

  const purchaseCard = (
    <Card>
      <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">معلومات الشراء والاستحواذ</h3>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-12">
        <Field detail={isDetail} label="القيمة الأصلية" value={initialData?.originalValue} className="sm:col-span-3">
          <Input
            type="number"
            step="0.01"
            label="القيمة الأصلية"
            {...register('originalValue', updateValueNumbers)}
            error={errors.originalValue?.message}
            required
            disabled={isLocked}
          />
        </Field>
        <Field detail={isDetail} label="تكلفة الاستحواذ" value={initialData?.acquisitionCost} className="sm:col-span-3">
          <Input
            type="number"
            step="0.01"
            label="تكلفة الاستحواذ"
            {...register('acquisitionCost', updateValueNumbers)}
            error={errors.acquisitionCost?.message}
            disabled={isDetail || isLocked}
          />
        </Field>
        <Field detail={isDetail} label="تاريخ الشراء" value={initialData?.purchaseDate} className="sm:col-span-3">
          <Input type="date" label="تاريخ الشراء" {...register('purchaseDate')} error={errors.purchaseDate?.message} required disabled={isLocked} />
        </Field>
        <Field detail={isDetail} label="نوع الاستحواذ" value={initialData ? (acquisitionTypeLabels[initialData.acquisitionType as keyof typeof acquisitionTypeLabels] ?? initialData.acquisitionType) : undefined} className="sm:col-span-3">
          <Select
            label="نوع الاستحواذ"
            {...register('acquisitionType')}
            options={[{ value: '', label: 'اختر النوع' }, ...Object.entries(acquisitionTypeLabels).map(([value, label]) => ({ value, label }))]}
            error={errors.acquisitionType?.message}
            required
            disabled={isDetail}
          />
        </Field>
        <Field detail={isDetail} label="العملة" value={currencyLabel} className="sm:col-span-3">
          <Select
            label="العملة"
            {...register('currencyId', updateValueNumbers)}
            options={[{ value: '', label: 'اختر العملة' }, ...toOptions(currencyOptions)]}
            error={errors.currencyId?.message}
            required
            disabled={isDetail}
          />
        </Field>
        <Field detail={isDetail} label="سعر الصرف" value={initialData?.exchangeRate} className="sm:col-span-3">
          <Select
            label="سعر الصرف"
            {...register('exchangeRateId', updateValueNumbers)}
            options={[{ value: '', label: 'اختر سعر الصرف' }, ...toOptions(exchangeRateOptions)]}
            disabled={isDetail}
          />
        </Field>
        <DetailField label="تاريخ التفعيل" value={initialData?.activationDate} className="sm:col-span-3" />
      </div>
    </Card>
  );

  const depreciationCard = (
    <Card>
      <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">الإهلاك</h3>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-12">
        <Field detail={isDetail} label="تاريخ بدء الإهلاك" value={initialData?.depreciationStartDate} className="sm:col-span-3">
          <Input type="date" label="تاريخ بدء الإهلاك" {...register('depreciationStartDate')} error={errors.depreciationStartDate?.message} required disabled={isDetail} />
        </Field>
        <Field detail={isDetail} label="العمر الإنتاجي (سنوات)" value={initialData?.usefulLifeYears} className="sm:col-span-3">
          <Input
            type="number"
            label="العمر الإنتاجي (سنوات)"
            {...register('usefulLifeYears', updateValueNumbers)}
            error={errors.usefulLifeYears?.message}
            disabled={isDetail}
          />
        </Field>
        {(isEdit || isDetail) && (
          <>
            <div className="flex flex-col gap-1 sm:col-span-3">
              <span className="text-label-md text-[var(--color-on-surface)]">الإهلاك المتراكم</span>
              <div className="flex min-h-[var(--density-comfortable-control-height)] items-center text-sm text-[var(--color-on-surface)]">
                <MoneyDisplay value={initialData?.accumulatedDepreciation ?? 0} />
              </div>
            </div>
            <div className="flex flex-col gap-1 sm:col-span-3">
              <span className="text-label-md text-[var(--color-on-surface)]">القيمة الحالية</span>
              <div className="flex min-h-[var(--density-comfortable-control-height)] items-center text-sm text-[var(--color-on-surface)]">
                <MoneyDisplay value={initialData?.currentValue ?? 0} />
              </div>
            </div>
            <DetailField label="تاريخ آخر إهلاك" value={initialData?.lastDepreciationDate} className="sm:col-span-3" />
            <div className="flex flex-col gap-1 sm:col-span-3">
              <span className="text-label-md text-[var(--color-on-surface)]">مُهلاك بالكامل</span>
              <div className="flex min-h-[var(--density-comfortable-control-height)] items-center">
                <Badge variant={initialData?.isFullyDepreciated ? 'success' : 'default'}>
                  {initialData?.isFullyDepreciated ? 'نعم' : 'لا'}
                </Badge>
              </div>
            </div>
          </>
        )}
      </div>
      {(isEdit || isDetail) && (
        <div className="mt-4 border-t border-[var(--color-container-border)] pt-3">
          <AssetDepreciationTable assetId={initialData!.id} />
        </div>
      )}
    </Card>
  );

  const movementsCard = (
    <Card>
      <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">حركة الأصل</h3>
      {isEdit || isDetail ? (
        <AssetTransactionsTable assetId={initialData!.id} />
      ) : (
        <EmptyState message="تُسجَّل حركات الأصل بعد إنشائه" />
      )}
    </Card>
  );

  const attributesCard = (
    <Card>
      <h3 className="mb-3 text-base font-semibold text-[var(--color-on-surface)]">المواصفات</h3>
      {selectedGroupId === 0 ? (
        <EmptyState message="اختر المجموعة أولاً لعرض المواصفات" />
      ) : groupLoading ? (
        <div className="p-4 text-sm text-[var(--color-on-surface-variant)]">جاري تحميل المواصفات...</div>
      ) : groupError ? (
        <Alert variant="error" role="alert">{getQueryErrorMessage(groupError)}</Alert>
      ) : (
        <AssetAttributeInputs
          bindings={attributeBindings}
          values={attributeSeed.values}
          onChange={updateAttributeValue}
          disabled={isDetail || isLocked}
        />
      )}
    </Card>
  );

  const attachmentsCard =
    isEdit || isDetail ? (
      <AttachmentsPanel documentType="Asset" documentId={initialData!.id} />
    ) : (
      <Card>
        <div className="space-y-3">
          <p className="text-sm text-[var(--color-on-surface-variant)]">
            ستُرفع الملفات تلقائياً بعد إنشاء الأصل.
          </p>
          <FileUploadZone
            onUpload={async (file) => {
              setPendingFiles((previous) => [...previous, file]);
              notify({ type: 'info', title: 'سيتم رفع الملف بعد إنشاء الأصل' });
            }}
          />
          {pendingFiles.length > 0 && (
            <ul className="space-y-2">
              {pendingFiles.map((file, index) => (
                <li
                  key={`${file.name}-${index}`}
                  className="flex items-center gap-3 rounded-lg bg-[var(--color-surface-container)] p-3"
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium">{file.name}</p>
                    <p className="text-xs text-[var(--color-on-surface-variant)]">{formatFileSize(file.size)}</p>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    title="إزالة"
                    onClick={() => setPendingFiles((files) => files.filter((_, fileIndex) => fileIndex !== index))}
                  >
                    <X size={14} className="text-[var(--color-error)]" />
                  </Button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </Card>
    );

  return (
    <form id={formId} onSubmit={handleSubmit(submit, onInvalid)} className="flex flex-col gap-4" aria-label="نموذج الأصل">
      {errors.root && (
        <Alert variant="error" role="alert">{errors.root.message}</Alert>
      )}

      {mainCard}

      <Tabs
        value={activeTab}
        onChange={setActiveTab}
        tabs={[
          {
            key: 'purchase',
            label: 'الشراء والاستحواذ',
            icon: <ShoppingCart size={15} />,
            content: purchaseCard,
          },
          {
            key: 'depreciation',
            label: 'الإهلاك',
            icon: <Coins size={15} />,
            content: depreciationCard,
          },
          {
            key: 'movements',
            label: 'حركة الأصل',
            icon: <History size={15} />,
            content: movementsCard,
          },
          {
            key: 'attributes',
            label: `المواصفات (${attributeBindings.length})`,
            icon: <ClipboardList size={15} />,
            content: attributesCard,
          },
          {
            key: 'attachments',
            label: 'الصور والمرفقات',
            icon: <Images size={15} />,
            content: attachmentsCard,
          },
        ]}
      />

      {!isDetail && showActions && (
        <div className="flex justify-end gap-2">
          {onCancel && (
            <Button type="button" variant="ghost" onClick={onCancel}>إلغاء</Button>
          )}
          <Button type="submit" variant="primary" loading={isPending || submitting}>
            {isEdit ? 'حفظ التعديلات' : 'إنشاء الأصل'}
          </Button>
        </div>
      )}
    </form>
  );
}
