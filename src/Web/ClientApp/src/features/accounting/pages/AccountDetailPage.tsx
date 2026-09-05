import { useParams, useNavigate } from 'react-router-dom';
import { Pencil } from 'lucide-react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { Tabs } from '@/components/ui/Tabs';
import { Loading } from '@/components/ui/Loading';
import { AccountDetail } from '../components/AccountDetail';
import { useAccountDetail } from '../hooks/useAccountDetail';

export function AccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const accountId = id ? Number(id) : null;
  const { data: account, isLoading, error } = useAccountDetail(accountId);

  if (isLoading) {
    return <Loading />;
  }

  if (error) {
    return (
      <div role="alert" className="p-12 text-center">
        فشل تحميل البيانات
        <div className="mt-3">
          <Button variant="outline" onClick={() => navigate('/accounting/accounts')}>
            العودة للقائمة
          </Button>
        </div>
      </div>
    );
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

  return (
    <div>
      <PageHeader
        title={account.name ?? ''}
        description={`الرمز: ${account.code}`}
        actions={
          <Button variant="outline" icon={<Pencil size={14} />} onClick={() => navigate(`/accounting/accounts/${accountId}/edit`)}>
            تعديل
          </Button>
        }
      />
      <Tabs
        tabs={[
          { key: 'details', label: 'البيانات', content: <AccountDetail account={account} /> },
          { key: 'sub', label: 'الحسابات الفرعية', content: <div className="text-[var(--color-on-surface-variant)] p-4">قريبًا</div> },
        ]}
      />
    </div>
  );
}
