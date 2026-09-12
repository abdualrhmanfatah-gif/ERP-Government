import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Badge, Card, StatusBadge, FormField, Input, ConfirmDialog, Switch } from '@/components/ui';
import { Pencil, X, Check } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCurrencyDetail, useUpdateCurrency, useActivateCurrency, useDeactivateCurrency } from '../../hooks/useCurrencies';
import type { CurrencyDto } from '../../shared/types';
import { getActiveStatusLabel } from '@/shared/constants/labels';

export default function CurrencyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const currencyId = Number(id);

  const canUpdate = usePermission(PERMISSIONS.Currencies.Update);
  const canActivate = usePermission(PERMISSIONS.Currencies.Activate);
  const canDeactivate = usePermission(PERMISSIONS.Currencies.Deactivate);

  const { data: currency, isLoading, error } = useCurrencyDetail(currencyId);
  const updateMutation = useUpdateCurrency();
  const activateMutation = useActivateCurrency();
  const deactivateMutation = useDeactivateCurrency();

  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState<Partial<CurrencyDto>>({});
  const [confirmToggle, setConfirmToggle] = useState<{ action: 'activate' | 'deactivate' } | null>(null);

  function startEdit() {
    if (!currency) return;
    setEditForm({
      name: currency.name,
      symbol: currency.symbol,
      decimalPlaces: currency.decimalPlaces,
      roundingPrecision: currency.roundingPrecision,
      isBase: currency.isBase,
    });
    setIsEditing(true);
  }

  function cancelEdit() {
    setIsEditing(false);
    setEditForm({});
  }

  function handleSave() {
    if (!currency || !editForm.name || !editForm.symbol) {
      notify({ type: 'error', title: 'الاسم والرمز مطلوبان' });
      return;
    }
    updateMutation.mutate(
      {
        id: currency.id,
        rowVersion: currency.rowVersion,
        name: editForm.name,
        symbol: editForm.symbol,
        decimalPlaces: editForm.decimalPlaces ?? currency.decimalPlaces,
        roundingPrecision: editForm.roundingPrecision ?? currency.roundingPrecision,
        isBase: editForm.isBase ?? currency.isBase,
      },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم تحديث العملة بنجاح' });
          setIsEditing(false);
          setEditForm({});
        },
        onError: (err: any) => {
          const msg = err?.message || 'حدث خطأ أثناء التحديث';
          notify({ type: 'error', title: msg });
        },
      },
    );
  }

  function handleToggleActive() {
    if (!confirmToggle || !currency) return;
    const mutation = confirmToggle.action === 'activate' ? activateMutation : deactivateMutation;
    mutation.mutate(
      { id: currency.id, rowVersion: currency.rowVersion },
      {
        onSuccess: () => {
          notify({ type: 'success', title: confirmToggle.action === 'activate' ? 'تم التنشيط بنجاح' : 'تم التعطيل بنجاح' });
          setConfirmToggle(null);
        },
        onError: (err: any) => {
          const msg = err?.message || 'حدث خطأ';
          notify({ type: 'error', title: msg });
          setConfirmToggle(null);
        },
      },
    );
  }

  const breadcrumbs = [
    { label: 'العملات', path: '/financial-settings/currencies' },
    { label: currency?.code ?? '' },
  ];

  return (
    <Page
      title={currency?.code ?? ''}
      breadcrumbs={breadcrumbs}
      loading={isLoading}
      error={error || !currency ? 'العملة غير موجودة' : undefined}
      onRetry={() => navigate('/financial-settings/currencies')}
      actions={
        currency && (
          <>
            <div className="flex items-center gap-2">
              {currency.isBase && <Badge variant="primary">أساسية</Badge>}
              <StatusBadge variant={currency.isActive ? 'active' : 'closed'}>
                {getActiveStatusLabel(currency.isActive)}
              </StatusBadge>
            </div>
            {!isEditing && canUpdate && currency.isActive && (
              <Button variant="ghost" onClick={startEdit} icon={<Pencil size={16} />} className="cursor-pointer">
                تعديل
              </Button>
            )}
            {canActivate && !currency.isActive && (
              <Button variant="ghost" onClick={() => setConfirmToggle({ action: 'activate' })} className="cursor-pointer">
                تنشيط
              </Button>
            )}
            {canDeactivate && currency.isActive && !currency.isBase && (
              <Button variant="destructive" onClick={() => setConfirmToggle({ action: 'deactivate' })} className="cursor-pointer">
                تعطيل
              </Button>
            )}
          </>
        )
      }
    >
      {currency && (
        <>
          <Card className="bg-[var(--color-surface-container-lowest)]">
            {isEditing ? (
              <div className="space-y-4">
                <FormField label="الاسم" htmlFor="editName">
                  <Input
                    id="editName"
                    value={editForm.name ?? ''}
                    onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                  />
                </FormField>
                <FormField label="الرمز" htmlFor="editSymbol">
                  <Input
                    id="editSymbol"
                    value={editForm.symbol ?? ''}
                    onChange={(e) => setEditForm({ ...editForm, symbol: e.target.value })}
                  />
                </FormField>
                <FormField label="الكسور العشرية" htmlFor="editDecimalPlaces">
                  <Input
                    id="editDecimalPlaces"
                    type="number"
                    min={0}
                    max={6}
                    value={editForm.decimalPlaces ?? currency.decimalPlaces}
                    onChange={(e) => setEditForm({ ...editForm, decimalPlaces: Number(e.target.value) })}
                  />
                </FormField>
                <FormField label="دقة التقريب" htmlFor="editRoundingPrecision">
                  <Input
                    id="editRoundingPrecision"
                    type="number"
                    min={0.01}
                    step={0.01}
                    value={editForm.roundingPrecision ?? currency.roundingPrecision}
                    onChange={(e) => setEditForm({ ...editForm, roundingPrecision: Number(e.target.value) })}
                  />
                </FormField>
                <Switch
                  id="editIsBase"
                  label="عملة أساسية"
                  checked={editForm.isBase ?? currency.isBase}
                  onChange={(v) => setEditForm({ ...editForm, isBase: v })}
                />
                <div className="flex justify-end gap-2 pt-4">
                  <Button variant="ghost" onClick={cancelEdit} className="cursor-pointer" icon={<X size={16} />}>
                    إلغاء
                  </Button>
                  <Button onClick={handleSave} disabled={updateMutation.isPending} loading={updateMutation.isPending} className="cursor-pointer" icon={<Check size={16} />}>
                    حفظ
                  </Button>
                </div>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">الاسم</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.name}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">الرمز</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1 font-mono">{currency.symbol}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">الكسور العشرية</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.decimalPlaces}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">دقة التقريب</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.roundingPrecision}</p>
                </div>
              </div>
            )}
          </Card>

          {!isEditing && (
            <Card className="bg-[var(--color-surface-container-lowest)]">
              <h2 className="text-headline-sm font-bold text-[var(--color-on-surface)] mb-4">سجل التدقيق</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">أنشأ</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.createdBy || '—'}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">تاريخ الإنشاء</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.createdAt || '—'}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">آخر تعديل بواسطة</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.modifiedBy || '—'}</p>
                </div>
                <div>
                  <span className="text-sm text-[var(--color-on-surface-variant)]">تاريخ آخر تعديل</span>
                  <p className="text-body-md text-[var(--color-on-surface)] mt-1">{currency.modifiedAt || '—'}</p>
                </div>
              </div>
            </Card>
          )}

          <ConfirmDialog
            open={!!confirmToggle}
            onClose={() => setConfirmToggle(null)}
            onConfirm={handleToggleActive}
            title={confirmToggle?.action === 'activate' ? 'تنشيط العملة' : 'تعطيل العملة'}
            message={confirmToggle?.action === 'activate'
              ? `هل تريد تنشيط العملة ${currency.code}؟`
              : `هل تريد تعطيل العملة ${currency.code}؟`}
            loading={activateMutation.isPending || deactivateMutation.isPending}
          />
        </>
      )}
    </Page>
  );
}
