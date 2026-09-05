import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { AccountForm } from '../components/AccountForm';
import { useCreateAccount } from '../hooks/useCreateAccount';
import { useAccountGroups } from '../hooks/useAccountGroups';
import { useAccountsList } from '../hooks/useAccountsList';
import { showToast } from '@/components/ui/Toast';

export function AccountCreatePage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateAccount();
  const { data: groups = [] } = useAccountGroups();
  const { data: allAccounts = [] } = useAccountsList();

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync(data as Parameters<typeof mutateAsync>[0]);
      showToast('success', 'تم إنشاء الحساب بنجاح');
      navigate('/accounting/accounts');
    } catch {
      showToast('error', 'فشل إنشاء الحساب');
    }
  };

  return (
    <div>
      <PageHeader title="إنشاء حساب جديد" description="إضافة حساب جديد في دليل الحسابات" />
      <AccountForm
        accountGroups={groups}
        parentAccounts={allAccounts}
        onSubmit={handleSubmit}
        loading={isPending}
      />
    </div>
  );
}
