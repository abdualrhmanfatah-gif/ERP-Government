import { useNavigate, useSearchParams } from 'react-router-dom';
import { Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateReceiptVoucher } from '../hooks/useReceiptVouchers';
import { ReceiptVoucherForm } from '../components/ReceiptVoucherForm';
import type { CreateReceiptVoucherCommand } from '../shared/types';

export default function CreateReceiptVoucherPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const collectionOrderId = Number(searchParams.get('collectionOrderId')) || undefined;
  const createMutation = useCreateReceiptVoucher();

  async function handleSubmit(dto: CreateReceiptVoucherCommand) {
    try {
      const result = await createMutation.mutateAsync(dto);
      const voucher = result.value;
      notify({ type: 'success', title: `تم إنشاء السند برقم ${voucher?.voucherNumber ?? ''}` });
      navigate(`/treasury/receipt-vouchers/${voucher?.id}`);
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
      title="سند قبض جديد"
      onBack={() => navigate('/treasury/receipt-vouchers')} 
    >
      <ReceiptVoucherForm
        onSubmit={handleSubmit}
        onCancel={() => navigate('/treasury/receipt-vouchers')}
        isPending={createMutation.isPending}
        initialData={collectionOrderId ? { collectionOrderId, voucherDate: new Date().toISOString().slice(0, 10), partyId: 0, paymentMethod: 0, lines: [{ revenueAccountId: 0, amount: 0 }] } : undefined}
      />
    </Page>
  );
}