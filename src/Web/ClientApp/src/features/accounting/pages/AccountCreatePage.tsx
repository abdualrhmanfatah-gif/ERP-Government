import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { AccountForm } from '@/components/AccountingAccountForm';
import { useCreateAccount } from '../hooks/useCreateAccount';
import { useAccountGroups } from '../hooks/useAccountGroups';
import { useAccountsList } from '../hooks/useAccountsList';
import { notify } from '@/features/notifications/notify';

export function AccountCreatePage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateAccount();
  const { data: groups = [] } = useAccountGroups();
  const { data: allAccounts = [] } = useAccountsList();

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync(data as Parameters<typeof mutateAsync>[0]);
      notify({ type: 'success', title: 'تم إنشاء الحساب بنجاح' });
      navigate('/accounting/accounts');
    } catch {
      notify({ type: 'error', title: 'فشل إنشاء الحساب' });
    }
  };

  return (
    <Page title="إنشاء حساب جديد" description="إضافة حساب جديد في دليل الحسابات" maxWidth="sm">
      <AccountForm
        accountGroups={groups}
        parentAccounts={allAccounts}
        onSubmit={handleSubmit}
        loading={isPending}
      />
    </Page>
  );
}
