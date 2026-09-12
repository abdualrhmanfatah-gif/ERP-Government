import { useNavigate } from 'react-router-dom';
import { Button, Page } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCreateReceiptVoucher } from '../hooks/useReceiptVouchers';
import { ReceiptVoucherForm } from '../components/ReceiptVoucherForm';
import type { CreateReceiptVoucherCommand } from '../shared/types';

export default function CreateReceiptVoucherPage() {
  const navigate = useNavigate();
  const createMutation = useCreateReceiptVoucher();

  async function handleSubmit(dto: CreateReceiptVoucherCommand) {
    try {
      const result = await createMutation.mutateAsync(dto);
      notify({ type: 'success', title: `تم إنشاء السند برقم ${result.voucherNumber}` });
      navigate(`/treasury/receipt-vouchers/${result.id}`);
    } catch (err: unknown) {
      const problem = err as { detail?: string; title?: string };
      notify({ type: 'error', title: problem.detail ?? problem.title ?? 'حدث خطأ أثناء الحفظ' });
    }
  }

  return (
    <Page 
      title="سند قبض جديد"
      // إضافة الخاصية هنا لإظهار سهم الرجوع
      onBack={() => navigate('/treasury/receipt-vouchers')} 
    >
      <ReceiptVoucherForm
        onSubmit={handleSubmit}
        onCancel={() => navigate('/treasury/receipt-vouchers')}
        isPending={createMutation.isPending}
      />
    </Page>
  );
}