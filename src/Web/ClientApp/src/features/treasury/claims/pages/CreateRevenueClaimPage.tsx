import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Input, Select, DatePicker, Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateRevenueClaim } from '../../hooks/useRevenueClaims';
import { partiesClient } from '@/features/parties/shared/client';
import { CreateRevenueClaimCommand } from '@/web-api-client';

export default function CreateRevenueClaimPage() {
  const navigate = useNavigate();
  const createMutation = useCreateRevenueClaim();

  const { data: parties = [] } = useQuery({
    queryKey: ['parties', 'active'],
    queryFn: () => partiesClient.list({ isActive: true }),
  });

  const [claimDate, setClaimDate] = useState(new Date().toISOString().slice(0, 10));
  const [partyId, setPartyId] = useState(0);
  const [totalAmount, setTotalAmount] = useState(0);
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const e: Record<string, string> = {};
    if (!claimDate) e.claimDate = 'تاريخ المطالبة مطلوب';
    if (!partyId) e.partyId = 'الجهة مطلوبة';
    if (totalAmount <= 0) e.totalAmount = 'المبلغ يجب أن يكون أكبر من صفر';
    setErrors(e);
    return Object.keys(e).length === 0;
  }

  async function handleSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    if (!validate()) return;
    try {
      const result = await createMutation.mutateAsync(
        new CreateRevenueClaimCommand({
          claimDate: new Date(claimDate),
          partyId,
          totalAmount,
          notes: notes || undefined,
        }),
      );
      const claim = result.value;
      notify({ type: 'success', title: `تم إنشاء المطالبة برقم ${claim?.claimNumber ?? ''}` });
      navigate('/treasury/revenue-claims');
    } catch (err: unknown) {
      let message = 'حدث خطأ أثناء الحفظ';
      if (err && typeof err === 'object' && 'response' in err) {
        const ex = err as { response: string; message: string; status: number };
        console.error('[RevenueClaim API Error]', ex.status, ex.response);
        try {
          const body = JSON.parse(ex.response);
          if (Array.isArray(body)) {
            message = body.join('\n');
          } else if (body.errors) {
            message = Object.values(body.errors).flat().join('\n');
          } else {
            message = body.detail ?? body.title ?? ex.message;
          }
        } catch {
          message = ex.message || `HTTP ${ex.status}`;
        }
      }
      notify({ type: 'error', title: message });
    }
  }

  return (
    <Page
      title="مطالبة إيرادية جديدة"
      onBack={() => navigate('/treasury/revenue-claims')}
    >
      <form
        onSubmit={handleSubmit}
        className="space-y-6 rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-6"
        aria-label="مطالبة إيرادية جديدة"
      >
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div>
            <DatePicker
              label="تاريخ المطالبة *"
              value={claimDate}
              onChange={setClaimDate}
              required
              error={errors.claimDate}
            />
          </div>
          <div>
            <Select
              label="الجهة *"
              value={String(partyId)}
              onChange={(e) => setPartyId(Number(e.target.value))}
              options={[
                { value: '0', label: 'اختر الجهة...' },
                ...parties.map((p) => ({ value: String(p.id), label: p.nameAr })),
              ]}
            />
            {errors.partyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.partyId}</p>}
          </div>
          <div>
            <Input
              label="المبلغ الإجمالي *"
              type="number"
              value={totalAmount || ''}
              onChange={(e) => setTotalAmount(Number(e.target.value))}
              min={0}
              step={0.01}
              error={errors.totalAmount}
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

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate('/treasury/revenue-claims')}>
            إلغاء
          </Button>
          <Button type="submit" disabled={createMutation.isPending} loading={createMutation.isPending}>
            حفظ المطالبة
          </Button>
        </div>
      </form>
    </Page>
  );
}
