import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateBankAccount } from '../hooks/useBankAccounts';
import { notify } from '@/features/notifications/notify';
import { BankAccountForm } from '../components/BankAccountForm';

export default function BankAccountCreatePage() {
  const navigate = useNavigate();
  const createAccount = useCreateBankAccount();

  function onSubmit(data: Parameters<typeof createAccount.mutateAsync>[0]) {
    createAccount.mutateAsync(data).then(
      () => {
        notify({ type: 'success', title: 'تم إنشاء الحساب بنجاح' });
        navigate('/payments/bank-accounts');
      },
      () => notify({ type: 'error', title: 'فشل إنشاء الحساب' })
    );
  }

  return (
    <Page title="إنشاء حساب بنكي جديد" maxWidth="lg">
      <BankAccountForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/payments/bank-accounts')}
        isPending={createAccount.isPending}
      />
    </Page>
  );
}
