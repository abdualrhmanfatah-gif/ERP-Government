import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { AccountForm } from '@/components/AccountingAccountForm';
import { useCreateAccount } from '../hooks/useCreateAccount';
import { useAccountGroups } from '../hooks/useAccountGroups';
import { useAccountsList } from '../hooks/useAccountsList';
import type { AccountFormData } from '../shared/schemas';

export function AccountCreatePage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateAccount();
  const { data: groups = [] } = useAccountGroups();
  const { data: allAccounts = [] } = useAccountsList();

  const handleSubmit = async (data: AccountFormData) => {
    await mutateAsync(data as Parameters<typeof mutateAsync>[0]);
  };

  return (
    <Page title="إنشاء حساب جديد" description="إضافة حساب جديد في دليل الحسابات" maxWidth="sm">
      <AccountForm
        accountGroups={groups}
        parentAccounts={allAccounts}
        onSubmit={handleSubmit}
        onSuccess={() => navigate('/accounting/accounts')}
        onCancel={() => navigate('/accounting/accounts')}
        loading={isPending}
      />
    </Page>
  );
}
