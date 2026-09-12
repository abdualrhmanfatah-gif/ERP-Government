import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { DepositSlipsClient, FormType } from '../../../web-api-client';
import { DepositSlipForm } from '../components/DepositSlipForm';

const client = new DepositSlipsClient();

export default function CreateDepositSlipPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: { slipDate: string; formType: FormType; voucherIds: number[] }) =>
      client.depositSlipsPOST({
        slipDate: new Date(data.slipDate),
        formType: data.formType as any,
        voucherIds: data.voucherIds,
      } as any),
    onSuccess: (id: number) => {
      queryClient.invalidateQueries({ queryKey: ['deposit-slips'] });
      notify({ type: 'success', title: 'تم إنشاء بطاقة الإيداع' });
      navigate(`/treasury/deposit-slips/${id}`);
    },
    onError: () => notify({ type: 'error', title: 'فشل إنشاء بطاقة الإيداع' }),
  });

  return (
    <Page title="إنشاء بطاقة إيداع">
      <DepositSlipForm
        onSubmit={(data) => createMutation.mutate(data)}
        onCancel={() => navigate('/treasury/deposit-slips')}
        isPending={createMutation.isPending}
      />
    </Page>
  );
}
