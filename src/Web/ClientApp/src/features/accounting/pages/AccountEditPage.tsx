import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { AccountForm } from '../components/AccountForm';
import { useAccountDetail } from '../hooks/useAccountDetail';
import { useUpdateAccount } from '../hooks/useUpdateAccount';
import { useAccountGroups } from '../hooks/useAccountGroups';
import { useAccountsList } from '../hooks/useAccountsList';
import { showToast } from '@/components/ui/Toast';

export function AccountEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = id ? Number(id) : null;
  const { data: account, isLoading } = useAccountDetail(accountId);
  const { mutateAsync, isPending } = useUpdateAccount();
  const { data: groups = [] } = useAccountGroups();
  const { data: allAccounts = [] } = useAccountsList();

  if (isLoading) {
    return <div role="status" aria-busy="true" className="p-12 text-center">جاري التحميل...</div>;
  }

  if (!account) {
    return (
      <div className="p-12 text-center">
        الحساب غير موجود
        <div className="mt-3">
          <Button variant="outline" onClick={() => navigate('/accounting/accounts')}>
            العودة للقائمة
          </Button>
        </div>
      </div>
    );
  }

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync({
        id: accountId!,
        data: {
          id: accountId!,
          name: data.name as string,
          description: data.description as string | undefined,
          accountGroupId: data.accountGroupId as number,
          normalBalance: data.normalBalance as number,
          isPostable: data.isPostable as boolean,
          isReconcilable: data.isReconcilable as boolean,
          rowVersion: account.rowVersion,
        },
      });
      showToast('success', 'تم تعديل الحساب بنجاح');
      navigate(`/accounting/accounts/${accountId}`);
    } catch {
      showToast('error', 'فشل تعديل الحساب');
    }
  };

  return (
    <div>
      <PageHeader title={`تعديل الحساب: ${account.name}`} description={`الرمز: ${account.code}`} />
      <AccountForm
        initialData={account}
        accountGroups={groups}
        parentAccounts={allAccounts.filter((a) => a.id !== accountId)}
        onSubmit={handleSubmit}
        loading={isPending}
      />
    </div>
  );
}
