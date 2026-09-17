import { useParams, useNavigate } from 'react-router-dom';
import { Page, EmptyState, Button } from '@/components/ui';
import { AccountForm } from '@/components/AccountingAccountForm';
import { useAccountDetail } from '../hooks/useAccountDetail';
import { useUpdateAccount } from '../hooks/useUpdateAccount';
import { useAccountGroups } from '../hooks/useAccountGroups';
import { useAccountsList } from '../hooks/useAccountsList';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import type { AccountFormData } from '../shared/schemas';

export function AccountEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = id ? Number(id) : null;
  const { data: account, isLoading, error, refetch } = useAccountDetail(accountId);
  const { mutateAsync, isPending } = useUpdateAccount();
  const { data: groups = [] } = useAccountGroups();
  const { data: allAccounts = [] } = useAccountsList();

  if (isLoading) {
    return <Page title="" loading />;
  }

  if (error) {
    return (
      <Page
        title=""
        error={getQueryErrorMessage(error)}
        onRetry={() => refetch()}
      />
    );
  }

  if (!account) {
    return (
      <Page title="" onBack={() => navigate('/accounting/accounts')}>
        <EmptyState
          message="الحساب غير موجود"
          action={
            <Button variant="outline" onClick={() => navigate('/accounting/accounts')}>
              العودة للقائمة
            </Button>
          }
        />
      </Page>
    );
  }

  const handleSubmit = async (data: AccountFormData) => {
    await mutateAsync({
      id: accountId!,
      data: {
        id: accountId!,
        name: data.name,
        description: data.description,
        accountGroupId: data.accountGroupId,
        normalBalance: data.normalBalance,
        isPostable: data.isPostable,
        isReconcilable: data.isReconcilable,
        rowVersion: account.rowVersion,
      },
    });
  };

  return (
    <Page title={`تعديل الحساب: ${account.name}`} description={`الرمز: ${account.code}`} maxWidth="sm">
      <AccountForm
        initialData={account}
        accountGroups={groups}
        parentAccounts={allAccounts.filter((a) => a.id !== accountId)}
        onSubmit={handleSubmit}
        onSuccess={() => navigate(`/accounting/accounts/${accountId}`)}
        onCancel={() => navigate(`/accounting/accounts/${accountId}`)}
        loading={isPending}
      />
    </Page>
  );
}
