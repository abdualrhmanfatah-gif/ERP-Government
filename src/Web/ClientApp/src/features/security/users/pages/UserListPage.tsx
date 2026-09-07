import { useState } from 'react';
import { Plus, Eye } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { FilterBar, FilterSearch, FilterSelect } from '@/components/ui';
import { useUsers } from '../hooks';
import { CreateUserDialog } from '@/components/SecurityUsersCreateDialog';

export function UserListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const pageSize = 20;

  const { data: users = [], isLoading, error, refetch } = useUsers({
    search: search || undefined,
    status: status || undefined,
    page,
    pageSize,
  });

  return (
    <div className="space-y-4">
      <PageHeader
        title="إدارة المستخدمين"
        description="إدارة حسابات المستخدمين وصلاحياتهم"
        actions={
          <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={() => setShowCreate(true)}>
            مستخدم جديد
          </Button>
        }
      />
      <FilterBar hasFilters={!!search || !!status} onClear={() => { setSearch(''); setStatus(''); setPage(1); }}>
        <FilterSearch
          value={search}
          onChange={(v) => { setSearch(v); setPage(1); }}
          placeholder="بحث بالاسم أو تسجيل الدخول..."
          className="flex-1 min-w-48"
        />
        <FilterSelect
          label="الحالة"
          value={status}
          onChange={(v) => { setStatus(v); setPage(1); }}
          options={[
            { value: '', label: 'الكل' },
            { value: 'active', label: 'نشط' },
            { value: 'inactive', label: 'غير نشط' },
          ]}
        />
      </FilterBar>
      <DataGrid
        columns={[
          { key: 'login', header: 'تسجيل الدخول', width: 150, render: (r) => <span dir="ltr">{r.login}</span> },
          { key: 'departmentName', header: 'القسم', width: 150, render: (r) => r.departmentName ?? '—' },
          { key: 'isActive', header: 'الحالة', width: 100, render: (r) => (
            <StatusBadge variant={r.isActive ? 'active' : 'draft'}>
              {r.isActive ? 'نشط' : 'غير نشط'}
            </StatusBadge>
          )},
          { key: 'mfaEnabled', header: 'المصادقة الثنائية', width: 120, render: (r) => (
            <StatusBadge variant={r.mfaEnabled ? 'active' : 'draft'}>
              {r.mfaEnabled ? 'مفعّل' : 'غير مفعّل'}
            </StatusBadge>
          )},
          { key: 'lastLoginAt', header: 'آخر دخول', width: 150, render: (r) => (
            r.lastLoginAt ? new Date(r.lastLoginAt).toLocaleDateString('ar') : '—'
          )},
          { key: 'actions', header: 'الإجراءات', width: 80, render: (r) => (
            <Button variant="ghost" size="icon-xs" onClick={() => navigate(`/security/users/${r.id}`)} title="عرض">
              <Eye size={14} />
            </Button>
          )},
        ]}
        data={users}
        loading={isLoading}
        error={error ? 'فشل تحميل البيانات' : undefined}
        onRetry={() => refetch()}
        rowKey={(r) => String(r.id)}
        emptyMessage="لا يوجد مستخدمون"
        onRowClick={(r) => navigate(`/security/users/${r.id}`)}
      />
      <CreateUserDialog open={showCreate} onClose={() => setShowCreate(false)} onCreated={(id) => navigate(`/security/users/${id}`)} />
    </div>
  );
}
