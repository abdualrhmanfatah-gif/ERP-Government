import { useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pencil } from 'lucide-react';
import { Page, Button, Tabs, StatusBadge, EmptyState, ErrorState, DataGrid } from '@/components/ui';
import { AccountDetail } from '@/components/AccountingAccountDetail';
import { useAccountDetail } from '../hooks/useAccountDetail';
import { useAccountsList } from '../hooks/useAccountsList';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { getActiveStatusLabel } from '@/shared/constants/labels';

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = id ? Number(id) : null;
  const { data: account, isLoading, error } = useAccountDetail(accountId);
  const { data: allAccounts, isLoading: isAccountsLoading, error: accountsError, refetch: refetchAccounts } = useAccountsList();

  const subAccounts = useMemo(() => {
    if (!allAccounts || !account) return [];
    return allAccounts
      .filter((a) => a.parentId === account.id)
      .sort((a, b) => (a.code ?? '').localeCompare(b.code ?? ''));
  }, [allAccounts, account]);

  if (isLoading) {
    return <Page title="" loading />;
  }

  if (error) {
    return (
      <Page
        title=""
        error={getQueryErrorMessage(error)}
        onRetry={() => navigate('/accounting/accounts')}
      />
    );
  }

  if (!account) {
    return (
      <Page title="" onBack={() => navigate('/accounting/accounts')}>
        <EmptyState message="الحساب غير موجود" />
      </Page>
    );
  }

  return (
    <Page
      title={account.name ?? ''}
      description={
        <span className="flex items-center gap-3">
          <span dir="ltr" className="tabular-nums">{account.code}</span>
          <StatusBadge variant={account.isActive ? 'active' : 'inactive'}>
            {getActiveStatusLabel(account.isActive ?? false)}
          </StatusBadge>
        </span>
      }
      onBack={() => navigate('/accounting/accounts')}
      actions={
        <Button
          variant="primary"
          icon={<Pencil size={14} />}
          onClick={() => navigate(`/accounting/accounts/${accountId}/edit`)}
        >
          تعديل
        </Button>
      }
      toolbar={
        <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm">
          <div>
            <span className="text-[var(--color-on-surface-variant)]">المجموعة: </span>
            <span className="font-medium">{account.accountGroupName ?? '—'}</span>
          </div>
          <div>
            <span className="text-[var(--color-on-surface-variant)]">المستوى: </span>
            <span className="font-medium tabular-nums">{account.level}</span>
          </div>
          <div>
            <span className="text-[var(--color-on-surface-variant)]">الرصيد: </span>
            <span className="font-medium">{account.normalBalance === 'credit' ? 'دائن' : 'مدين'}</span>
          </div>
        </div>
      }
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
    >
      <Tabs
        tabs={[
          { key: 'details', label: 'البيانات', content: <AccountDetail account={account} /> },
          {
            key: 'sub',
            label: `الحسابات الفرعية (${subAccounts.length})`,
            content: isAccountsLoading ? (
              <div className="text-sm text-[var(--color-on-surface-variant)] p-4">جاري التحميل...</div>
            ) : accountsError ? (
              <ErrorState
                message={getQueryErrorMessage(accountsError)}
                onRetry={() => refetchAccounts()}
              />
            ) : subAccounts.length === 0 ? (
              <EmptyState message="لا توجد حسابات فرعية" />
            ) : (
              <DataGrid
                data={subAccounts}
                onRowClick={(row) => navigate(`/accounting/accounts/${row.id}`)}
                rowKey={(row) => row.id}
                columns={[
                  { id: 'code', accessorKey: 'code', header: 'الكود', cell: (row) => <span dir="ltr" className="tabular-nums">{row.code}</span> },
                  { id: 'name', accessorKey: 'name', header: 'الاسم' },
                  { id: 'accountGroupName', accessorKey: 'accountGroupName', header: 'المجموعة' },
                  { id: 'isActive', accessorKey: 'isActive', header: 'الحالة', cell: (row) => <StatusBadge variant={row.isActive ? 'active' : 'inactive'}>{getActiveStatusLabel(row.isActive ?? false)}</StatusBadge> },
                ]}
              />
            ),
          },
        ]}
      />
    </Page>
  );
}
