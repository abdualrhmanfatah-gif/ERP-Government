import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Input, Select, DatePicker, Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateCollectionOrder } from '../../hooks/useCollectionOrders';
import { RevenueClaimsClient, CreateCollectionOrderCommand } from '@/web-api-client';

const revenueClaimsClient = new RevenueClaimsClient();

export default function CreateCollectionOrderPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const preselectedClaimId = Number(searchParams.get('claimId')) || 0;
  const createMutation = useCreateCollectionOrder();

  const { data: claims = [] } = useQuery({
    queryKey: ['revenue-claims', 'active'],
    queryFn: () => revenueClaimsClient.revenueClaimsAll(undefined, undefined),
  });

  const openClaims = claims.filter((c) => c.status === 'Open' || c.status === 'PartiallySettled');

  const [revenueClaimId, setRevenueClaimId] = useState(preselectedClaimId);
  const [orderDate, setOrderDate] = useState(new Date().toISOString().slice(0, 10));
  const [authorizedAmount, setAuthorizedAmount] = useState(0);
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const selectedClaim = claims.find((c) => c.id === revenueClaimId);
  const maxAmount = selectedClaim?.availableAmount ?? 0;

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!revenueClaimId) e.revenueClaimId = 'يجب اختيار مطالبة إيرادية';
    if (!orderDate) e.orderDate = 'تاريخ الأمر مطلوب';
    if (authorizedAmount <= 0) e.authorizedAmount = 'المبلغ يجب أن يكون أكبر من صفر';
    if (authorizedAmount > maxAmount) e.authorizedAmount = `المبلغ يتجاوز المتاح (${maxAmount.toLocaleString('ar-YE')})`;
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    try {
      const result = await createMutation.mutateAsync(
        new CreateCollectionOrderCommand({
          revenueClaimId,
          orderDate: new Date(orderDate),
          authorizedAmount,
          notes: notes || undefined,
        }),
      );
      const order = result.value;
      notify({ type: 'success', title: `تم إصدار أمر التحصيل برقم ${order?.orderNumber ?? ''}` });
      navigate('/treasury/collection-orders');
    } catch (err: unknown) {
      let message = 'حدث خطأ أثناء الحفظ';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string; status: number };
        try {
          const body = JSON.parse(ex.response);
          message = Array.isArray(body) ? body.join('\n') : (body.detail ?? body.title ?? ex.message);
        } catch {
          message = ex.message || `HTTP ${ex.status}`;
        }
      }
      notify({ type: 'error', title: message });
    }
  }

  return (
    <Page
      title="أمر تحصيل جديد"
      onBack={() => navigate('/treasury/collection-orders')}
    >
      <form
        onSubmit={handleSubmit}
        className="space-y-6 rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6"
        aria-label="أمر تحصيل جديد"
      >
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <Select
              label="المطالبة الإيرادية *"
              value={String(revenueClaimId)}
              onChange={(e) => {
                const id = Number(e.target.value);
                setRevenueClaimId(id);
                setAuthorizedAmount(0);
              }}
              options={[
                { value: '0', label: 'اختر المطالبة...' },
                ...openClaims.map((c) => ({
                  value: String(c.id),
                  label: `${c.claimNumber} - ${c.partyName} (المتاح: ${(c.availableAmount ?? 0).toLocaleString('ar-YE')})`,
                })),
              ]}
            />
            {errors.revenueClaimId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.revenueClaimId}</p>}
          </div>

          <div>
            <DatePicker
              label="تاريخ الأمر *"
              value={orderDate}
              onChange={setOrderDate}
              required
              error={errors.orderDate}
            />
          </div>

          <div>
            <Input
              label={`المبلغ المفوض * ${maxAmount > 0 ? `(المتاح: ${maxAmount.toLocaleString('ar-YE')})` : ''}`}
              type="number"
              value={authorizedAmount || ''}
              onChange={(e) => setAuthorizedAmount(Number(e.target.value))}
              min={0}
              max={maxAmount}
              step={0.01}
              error={errors.authorizedAmount}
            />
          </div>

          <div className="md:col-span-2">
            <Input
              label="ملاحظات"
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>

        {selectedClaim && (
          <div className="rounded bg-[var(--color-surface-container)] px-4 py-3 text-sm">
            <span className="text-[var(--color-on-surface-variant)]">المطالبة المحددة: </span>
            <span className="font-medium">{selectedClaim.claimNumber}</span>
            <span className="text-[var(--color-on-surface-variant)]"> — </span>
            <span>{selectedClaim.partyName}</span>
            <span className="text-[var(--color-on-surface-variant)]"> — المتاح: </span>
            <span className="font-medium tabular-nums">{maxAmount.toLocaleString('ar-YE')} ريال</span>
          </div>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate('/treasury/collection-orders')}>
            إلغاء
          </Button>
          <Button type="submit" disabled={createMutation.isPending} loading={createMutation.isPending}>
            إصدار أمر التحصيل
          </Button>
        </div>
      </form>
    </Page>
  );
}
