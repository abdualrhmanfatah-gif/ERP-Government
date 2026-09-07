import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, EmptyState } from '@/components/ui';
import { AccountForm } from '@/components/AccountingAccountForm';
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

  if (!account) {
    return (
      <EmptyState
        message="الحساب غير موجود"
        action={
          <Button variant="outline" onClick={() => navigate('/accounting/accounts')}>
            العودة للقائمة
          </Button>
        }
      />
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
    <Page title={`تعديل الحساب: ${account.name}`} description={`الرمز: ${account.code}`} maxWidth="sm" loading={isLoading}>
      <AccountForm
        initialData={account}
        accountGroups={groups}
        parentAccounts={allAccounts.filter((a) => a.id !== accountId)}
        onSubmit={handleSubmit}
        loading={isPending}
      />
    </Page>
  );
}
