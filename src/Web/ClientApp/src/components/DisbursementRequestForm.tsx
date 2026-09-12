import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createDisbursementRequestSchema, type CreateDisbursementRequestFormData } from '../features/payments/disbursement-requests/shared/schemas';
import { useCreateDisbursementRequest, useUpdateDisbursementRequest, usePartiesForBeneficiary } from '../features/payments/disbursement-requests/hooks/useDisbursementRequests';
import { useFiscalYearByDate } from '../features/accounting/hooks/useFiscalYearByDate';
import { useCurrenciesList } from '../features/financial-settings/hooks/useCurrencies';
import { useFiscalYearsList } from '../features/financial-settings/hooks/useFiscalYears';
import { notify } from '../features/notifications/notify';
import { Card, Combobox, Input, Select, Textarea, Label } from './ui';

interface CurrencyItem {
  id: number;
  code: string;
  name: string;
  isBase?: boolean;
}

interface PartyItem {
  id: number;
  nameAr: string;
  partyCode: string;
}

interface FiscalYearItem {
  id: number;
  name: string;
}

function toBase64(rowVersion: string  | string): string {
  if (typeof rowVersion === 'string') return rowVersion;
  const bytes = new Uint8Array(rowVersion);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i]);
  return btoa(binary);
}

interface DisbursementRequestFormProps {
  mode: 'create' | 'detail';
  initialData?: {
    id: number;
    requestNumber?: string;
    beneficiaryName?: string;
    beneficiaryPartyId?: number | null;
    requestedAmount?: number;
    currencyId?: number;
    purpose?: string;
    financialYearId?: number;
    notes?: string | null;
    rowVersion?: number[] | string;
    status?: string;
    approvals?: unknown[];
  };
  editing?: boolean;
  lockedCoreFields?: boolean;
  onToggleEditing?: (editing: boolean) => void;
  onStateChange?: (state: { canSave: boolean; isSaving: boolean }) => void;
  onSaved?: (id?: number) => void;
}

export function DisbursementRequestForm({
  mode,
  initialData,
  editing = false,
  lockedCoreFields = false,
  onToggleEditing,
  onStateChange,
  onSaved,
}: DisbursementRequestFormProps) {
  const createRequest = useCreateDisbursementRequest();
  const updateRequest = useUpdateDisbursementRequest();

  const isDetail = mode === 'detail';
  const isEditable = isDetail ? editing : true;
  const coreLocked = isDetail && lockedCoreFields;

  const today = new Date().toISOString().split('T')[0];
  const { data: fiscalYearData } = useFiscalYearByDate(isDetail ? '' : today);
  const { data: currenciesRaw } = useCurrenciesList(true);
  const { data: fiscalYearsRaw } = useFiscalYearsList(true);
  const { data: partiesRaw, isLoading: partiesLoading } = usePartiesForBeneficiary();

  const currencies = useMemo(() => (currenciesRaw ?? []) as unknown as CurrencyItem[], [currenciesRaw]);
  const fiscalYears = useMemo(() => (fiscalYearsRaw ?? []) as unknown as FiscalYearItem[], [fiscalYearsRaw]);
  const parties = useMemo(() => (partiesRaw ?? []) as unknown as PartyItem[], [partiesRaw]);

  const defaultCurrencyId = currencies.find((c) => c.isBase)?.id ?? currencies[0]?.id;

  const [initialized, setInitialized] = useState(false);
  const [selectedPartyId, setSelectedPartyId] = useState<number | null>(null);
  const [selectedPartyName, setSelectedPartyName] = useState('');

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<CreateDisbursementRequestFormData>({
    resolver: zodResolver(createDisbursementRequestSchema),
    defaultValues: isDetail
      ? {
          beneficiaryName: initialData?.beneficiaryName ?? '',
          beneficiaryPartyId: initialData?.beneficiaryPartyId ?? null,
          requestedAmount: initialData?.requestedAmount ?? 0,
          currencyId: initialData?.currencyId ?? 0,
          purpose: initialData?.purpose ?? '',
          financialYearId: initialData?.financialYearId ?? 0,
          notes: initialData?.notes ?? '',
        }
      : {
          beneficiaryName: '',
          beneficiaryPartyId: null,
          requestedAmount: 1,
          currencyId: defaultCurrencyId ?? 1,
          purpose: '',
          financialYearId: fiscalYearData?.fiscalYearId ?? 1,
          notes: '',
        },
  });

  useEffect(() => {
    if (!isDetail && !initialized && defaultCurrencyId && fiscalYearData?.fiscalYearId) {
      setValue('currencyId', defaultCurrencyId);
      setValue('financialYearId', fiscalYearData.fiscalYearId);
      setInitialized(true);
    }
  }, [isDetail, initialized, defaultCurrencyId, fiscalYearData, setValue]);

  useEffect(() => {
    if (isDetail && initialData) {
      reset({
        beneficiaryName: initialData.beneficiaryName ?? '',
        beneficiaryPartyId: initialData.beneficiaryPartyId ?? null,
        requestedAmount: initialData.requestedAmount ?? 0,
        currencyId: initialData.currencyId ?? 0,
        purpose: initialData.purpose ?? '',
        financialYearId: initialData.financialYearId ?? 0,
        notes: initialData.notes ?? '',
      });
    }
  }, [isDetail, initialData, reset]);

  const watchedName = watch('beneficiaryName') ?? '';
  const watchedAmount = watch('requestedAmount') ?? 0;
  const watchedCurrencyId = watch('currencyId') ?? 0;
  const watchedPurpose = watch('purpose') ?? '';
  const watchedFyId = watch('financialYearId') ?? 0;

  const canSave =
    watchedName.trim().length > 0 &&
    watchedAmount > 0 &&
    watchedCurrencyId > 0 &&
    watchedPurpose.trim().length > 0 &&
    watchedFyId > 0;
  const isSaving = createRequest.isPending || updateRequest.isPending;

  useEffect(() => {
    onStateChange?.({ canSave, isSaving });
  }, [canSave, isSaving, onStateChange]);

  const currencyOptions = currencies.map((c) => ({
    value: String(c.id),
    label: `${c.code} — ${c.name}`,
  }));

  const partyOptions = parties.map((p) => ({
    value: String(p.id),
    label: `${p.nameAr} (${p.partyCode})`,
  }));

  const fyName = isDetail
    ? (fiscalYears.find((y) => y.id === watchedFyId)?.name ?? fiscalYearData?.fiscalYearName ?? '—')
    : (fiscalYearData?.fiscalYearName ?? '—');

  function handlePartySelect(value: string) {
    if (!value) {
      setSelectedPartyId(null);
      setSelectedPartyName('');
      setValue('beneficiaryPartyId', null);
      return;
    }
    const party = parties.find((p) => String(p.id) === value);
    if (party) {
      setSelectedPartyId(party.id);
      setSelectedPartyName(party.nameAr);
      setValue('beneficiaryPartyId', party.id);
      setValue('beneficiaryName', party.nameAr, { shouldValidate: true });
    }
  }

  function handleNameChange(e: React.ChangeEvent<HTMLInputElement>) {
    setSelectedPartyId(null);
    setSelectedPartyName('');
    setValue('beneficiaryPartyId', null);
    setValue('beneficiaryName', e.target.value, { shouldValidate: true });
  }

  async function onSubmit(data: CreateDisbursementRequestFormData) {
    try {
      if (isDetail && initialData?.id) {
        if (coreLocked) {
          await updateRequest.mutateAsync({
            id: initialData.id,
            cmd: {
              purpose: data.purpose.trim(),
              notes: data.notes?.trim() || null,
              rowVersion: toBase64(initialData.rowVersion ?? []),
            },
          });
        } else {
          await updateRequest.mutateAsync({
            id: initialData.id,
            cmd: {
              beneficiaryName: data.beneficiaryName.trim(),
              requestedAmount: data.requestedAmount,
              purpose: data.purpose.trim(),
              notes: data.notes?.trim() || null,
              rowVersion: toBase64(initialData.rowVersion ?? []),
            },
          });
        }
        notify({ type: 'success', title: 'تم حفظ التعديلات' });
        onToggleEditing?.(false);
        onSaved?.();
      } else {
        const payload = {
          beneficiaryName: data.beneficiaryName.trim(),
          beneficiaryPartyId: data.beneficiaryPartyId ?? undefined,
          requestedAmount: data.requestedAmount,
          currencyId: data.currencyId,
          purpose: data.purpose.trim(),
          financialYearId: data.financialYearId,
          notes: data.notes?.trim() || undefined,
        };
        const result = await createRequest.mutateAsync(payload);
        notify({ type: 'success', title: 'تم إنشاء طلب الصرف بنجاح' });
        const newId = (result as unknown as { id?: number })?.id;
        onSaved?.(newId);
      }
    } catch (err: unknown) {
      const problem = err as { detail?: string; message?: string };
      notify({ type: 'error', title: problem.detail ?? problem.message ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  return (
    <form
      id={isDetail ? 'disbursement-request-detail-form' : 'disbursement-request-form'}
      onSubmit={handleSubmit(onSubmit)}
      aria-label={isDetail ? 'تفاصيل طلب الصرف' : 'إنشاء طلب صرف'}
    >
      <Card className="space-y-4">
        <h2 className="text-sm font-semibold">بيانات طلب الصرف</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {!isDetail && (
            <div className="space-y-1">
              <Label>المستفيد</Label>
              <Combobox
                options={partyOptions}
                value={selectedPartyId ? String(selectedPartyId) : ''}
                onChange={handlePartySelect}
                placeholder="اختر موردًا أو اكتب اسمًا"
                searchPlaceholder="بحث..."
                loading={partiesLoading}
                emptyMessage="لا توجد نتائج"
                disabled={!isEditable}
              />
              {selectedPartyId && (
                <p className="text-xs text-[var(--color-primary)]">مرتبط: {selectedPartyName}</p>
              )}
            </div>
          )}

          {isDetail ? (
            <Input
              label="اسم المستفيد"
              {...register('beneficiaryName')}
              error={errors.beneficiaryName?.message}
              disabled={!isEditable || coreLocked}
              required
            />
          ) : (
            <Input
              label="اسم المستفيد"
              value={selectedPartyName || watchedName}
              onChange={handleNameChange}
              error={errors.beneficiaryName?.message}
              disabled={!isEditable || !!selectedPartyId}
              required
            />
          )}

          <Input
            label="المبلغ المطلوب"
            type="number"
            step="0.01"
            {...register('requestedAmount', { valueAsNumber: true })}
            error={errors.requestedAmount?.message}
            disabled={!isEditable || coreLocked}
            required
          />

          <Select
            label="العملة"
            value={watchedCurrencyId ? String(watchedCurrencyId) : ''}
            onChange={(e) => setValue('currencyId', Number(e.target.value), { shouldValidate: true })}
            options={currencyOptions}
            error={errors.currencyId?.message}
            disabled={!isEditable || isDetail}
          />

          {isDetail && initialData?.beneficiaryPartyId && (
            <p className="text-xs text-[var(--color-primary)] md:col-span-2">مرتبط بمورد مسجل</p>
          )}
        </div>

        <Input
          label="الغرض"
          {...register('purpose')}
          error={errors.purpose?.message}
          disabled={!isEditable}
          required
        />

        <Textarea
          label="ملاحظات"
          rows={3}
          {...register('notes')}
          disabled={!isEditable}
        />
      </Card>
    </form>
  );
}
