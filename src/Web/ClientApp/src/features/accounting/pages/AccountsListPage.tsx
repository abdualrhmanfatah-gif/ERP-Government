import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Page, Button, FilterBar, FilterSearch, FilterSelect } from '@/components/ui';
import { AccountGrid } from '@/components/AccountingAccountGrid';
import { useAccountsList } from '../hooks/useAccountsList';
import { useAccountGroups } from '../hooks/useAccountGroups';

export function AccountsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [accountGroupId, setAccountGroupId] = useState<number | undefined>(undefined);
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const [isPostable, setIsPostable] = useState<boolean | undefined>(undefined);

  const { data: accounts = [], isLoading, error, refetch } = useAccountsList({
    isActive,
    accountGroupId,
    isPostable,
  });
  const { data: groups = [] } = useAccountGroups();

  const filtered = useMemo(() => {
    if (!search) return accounts;
    const s = search.toLowerCase();
    return accounts.filter((a) =>
      (a.code?.toLowerCase().includes(s)) || (a.name?.toLowerCase().includes(s))
    );
  }, [accounts, search]);

  const hasFilters = !!search || accountGroupId !== undefined || isActive !== undefined || isPostable !== undefined;

  const clearAll = () => {
    setSearch('');
    setAccountGroupId(undefined);
    setIsActive(undefined);
    setIsPostable(undefined);
  };

  return (
    <div>
      <PageHeader
        title="دليل الحسابات"
        description="إدارة الحسابات المالية"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => navigate('/accounting/accounts/create')}>
            إنشاء حساب
          </Button>
        }
      />
      <FilterBar hasFilters={hasFilters} onClear={clearAll}>
        <FilterSearch
          value={search}
          onChange={setSearch}
          placeholder="بحث بالرمز أو الاسم..."
          className="flex-1 min-w-48"
        />
        <FilterSelect
          label="المجموعة"
          value={accountGroupId !== undefined ? String(accountGroupId) : ''}
          onChange={(v) => setAccountGroupId(v ? Number(v) : undefined)}
          options={groups.map((g) => ({
            value: String(g.id),
            label: g.name ?? '',
          }))}
        />
        <FilterSelect
          label="الحالة"
          value={isActive === undefined ? '' : String(isActive)}
          onChange={(v) => setIsActive(v === '' ? undefined : v === 'true')}
          options={[
            { value: 'true', label: 'نشط' },
            { value: 'false', label: 'غير نشط' },
          ]}
        />
        <FilterSelect
          label="الترحيل"
          value={isPostable === undefined ? '' : String(isPostable)}
          onChange={(v) => setIsPostable(v === '' ? undefined : v === 'true')}
          options={[
            { value: 'true', label: 'قابل للترحيل' },
            { value: 'false', label: 'غير قابل للترحيل' },
          ]}
        />
      </FilterBar>
      <AccountGrid
        data={filtered}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
      />
    </div>
  );
}
