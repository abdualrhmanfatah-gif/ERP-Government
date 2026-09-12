import { useParams, useNavigate } from 'react-router-dom';
import { Pencil } from 'lucide-react';
import { Page, Button, Tabs } from '@/components/ui';
import { AccountDetail } from '@/components/AccountingAccountDetail';
import { useAccountDetail } from '../hooks/useAccountDetail';

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = id ? Number(id) : null;
  const { data: account, isLoading, error } = useAccountDetail(accountId);

  if (!account) {
    return <Page title="" loading={isLoading} error={error ? 'فشل تحميل البيانات' : undefined} onRetry={() => navigate('/accounting/accounts')} />;
  }

  return (
    <Page
      title={account.name ?? ''}
      description={`الرمز: ${account.code}`}
      actions={
        <Button variant="outline" icon={<Pencil size={14} />} onClick={() => navigate(`/accounting/accounts/${accountId}/edit`)}>
          تعديل
        </Button>
      }
      loading={isLoading}
      error={error ? 'فشل تحميل البيانات' : undefined}
    >
      <Tabs
        tabs={[
          { key: 'details', label: 'البيانات', content: <AccountDetail account={account} /> },
          { key: 'sub', label: 'الحسابات الفرعية', content: <div className="text-[var(--color-on-surface-variant)] p-4">قريبًا</div> },
        ]}
      />
    </Page>
  );
}
