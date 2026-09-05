import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { DataGrid } from '@/components/ui/DataGrid';
import { AuditTimeline } from '@/components/ui/AuditTimeline';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Loading';
import { useAccountGroupDetail } from '../hooks/useAccountGroupDetail';
import { useToggleAccountGroupActive } from '../hooks/useToggleAccountGroupActive';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { toast } from 'sonner';
import { useState } from 'react';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { AccountGroupForm } from '../components/AccountGroupForm';
import { useUpdateAccountGroup } from '../hooks/useUpdateAccountGroup';
import { ArrowRight, Layers, BookOpen, Shield } from 'lucide-react';

export function AccountGroupDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const numericId = Number(id);
  const { data, isLoading } = useAccountGroupDetail(numericId);
  const toggleMut = useToggleAccountGroupActive();
  const updateMut = useUpdateAccountGroup();
  const [showEdit, setShowEdit] = useState(false);
  const [confirmToggle, setConfirmToggle] = useState(false);

  const { hasPermission: canEdit } = usePermission(PERMISSIONS.Accounting.ChartOfAccounts.Edit);

  if (isLoading) {
    return (
      <div className="p-6">
        <Skeleton variant="table" lines={6} />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">المجموعة غير موجودة</p>
        <Button variant="ghost" className="mt-2" onClick={() => navigate('/accounting/account-groups')}>
          العودة للقائمة
        </Button>
      </div>
    );
  }

  const g = data.group;

  const handleToggle = async () => {
    try {
      await toggleMut.mutateAsync({ id: g.id, isActive: !g.isActive, rowVersion: g.rowVersion });
      toast.success(g.isActive ? 'تم التعطيل' : 'تم التفعيل');
      setConfirmToggle(false);
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'فشل');
    }
  };

  const auditEntries = data.audit.map((a) => ({
    id: a.id,
    action: (a.action || 'update') as 'create' | 'update' | 'delete',
    user: a.userName ?? `#${a.userId}`,
    timestamp: a.timestamp,
    description: a.changeSummary ?? a.fieldChanges ?? '',
  }));

  const childColumns = [
    { id: 'code', accessorKey: 'code', header: 'الكود' },
    { id: 'name', accessorKey: 'name', header: 'الاسم' },
    {
      id: 'isActive',
      accessorKey: 'isActive',
      header: 'الحالة',
      cell: (row: { isActive: boolean }) => (
        <StatusBadge variant={row.isActive ? 'active' : 'closed'}>
          {row.isActive ? 'نشط' : 'معطل'}
        </StatusBadge>
      ),
    },
    {
      id: 'actions',
      header: 'إجراءات',
      cell: (row: { id: number }) => (
        <Button variant="ghost" size="sm" onClick={() => navigate(`/accounting/account-groups/${row.id}`)}>
          عرض
        </Button>
      ),
    },
  ];

  const accountColumns = [
    { id: 'code', accessorKey: 'code', header: 'كود الحساب' },
    { id: 'name', accessorKey: 'name', header: 'الاسم' },
    {
      id: 'isActive',
      accessorKey: 'isActive',
      header: 'الحالة',
      cell: (row: { isActive: boolean }) => (
        <StatusBadge variant={row.isActive ? 'active' : 'closed'}>
          {row.isActive ? 'نشط' : 'معطل'}
        </StatusBadge>
      ),
    },
    {
      id: 'isPostable',
      accessorKey: 'isPostable',
      header: 'قابل للترحيل',
      cell: (row: { isPostable: boolean }) => (
        <span className={row.isPostable ? 'text-[var(--color-success)]' : 'text-[var(--color-on-surface-variant)]'}>
          {row.isPostable ? 'نعم' : 'لا'}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6 p-6">
      <PageHeader
        title={`${g.code} — ${g.name}`}
        description={`المستوى ${g.level} • النوع ${g.type} • الرصيد ${g.normalBalance}`}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate('/accounting/account-groups')}>
              <ArrowRight size={16} className="ms-1" />
              رجوع
            </Button>
            {canEdit && (
              <Button onClick={() => setShowEdit(true)}>تعديل</Button>
            )}
            {canEdit && (
              <Button
                variant={g.isActive ? 'destructive' : 'default'}
                onClick={() => setConfirmToggle(true)}
              >
                {g.isActive ? 'تعطيل' : 'تفعيل'}
              </Button>
            )}
          </div>
        }
      />

      {/* ═══ Basic Info Card ═══ */}
      <div className="bg-[var(--color-surface)] rounded-xl border border-[var(--color-border-container)] p-6">
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <BookOpen size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">البيانات الأساسية</h3>
        </div>

        <div className="flex flex-wrap gap-x-6 gap-y-2">
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الكود: </span>
            <span className="text-base font-mono font-medium">{g.code}</span>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الاسم: </span>
            <span className="text-base font-medium">{g.name}</span>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">النوع: </span>
            <span className="text-base">{g.type}</span>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الرصيد: </span>
            <span className="text-base">{g.normalBalance}</span>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">المستوى: </span>
            <span className="text-base">{g.level}</span>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الحالة: </span>
            <StatusBadge variant={g.isActive ? 'active' : 'closed'}>
              {g.isActive ? 'نشط' : 'معطل'}
            </StatusBadge>
          </div>
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الأب: </span>
            <span className="text-base">{g.parentId ?? 'جذر'}</span>
          </div>
          <div className="flex-1">
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الوصف: </span>
            <span className="text-base">{g.description ?? '—'}</span>
          </div>
        </div>
      </div>

   

      {/* ═══ Children Section ═══ */}
      <section className="bg-[var(--color-surface)] rounded-xl border border-[var(--color-border-container)] p-6">
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <Layers size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">
            المجموعات الفرعية المباشرة ({data.children.length})
          </h3>
        </div>
        {data.children.length === 0 ? (
          <p className="text-sm text-[var(--color-on-surface-variant)] py-4 text-center">
            لا توجد مجموعات فرعية
          </p>
        ) : (
          <DataGrid data={data.children} rowKey={(row) => row.id} columns={childColumns} />
        )}
      </section>

      {/* ═══ Accounts Section ═══ */}
      <section className="bg-[var(--color-surface)] rounded-xl border border-[var(--color-border-container)] p-6">
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <BookOpen size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">
            الحسابات المرتبطة ({data.accounts.length})
          </h3>
        </div>
        {data.accounts.length === 0 ? (
          <p className="text-sm text-[var(--color-on-surface-variant)] py-4 text-center">
            لا توجد حسابات مرتبطة
          </p>
        ) : (
          <DataGrid data={data.accounts} rowKey={(row) => row.id} columns={accountColumns} />
        )}
      </section>

   

      <ConfirmDialog
        open={confirmToggle}
        onClose={() => setConfirmToggle(false)}
        title={g.isActive ? 'تأكيد التعطيل' : 'تأكيد التفعيل'}
        message={
          g.isActive
            ? 'سيتم فحص كل الأحفاد والحسابات. لا يمكن تعطيل مجموعة لديها فروع/حسابات نشطة.'
            : 'سيتم التفعيل.'
        }
        onConfirm={handleToggle}
        confirmLabel={g.isActive ? 'تعطيل' : 'تفعيل'}
        destructive={!!g.isActive}
      />

      <AccountGroupForm
        open={showEdit}
        onOpenChange={setShowEdit}
        initial={g}
        onSubmit={async (payload) => {
          await updateMut.mutateAsync({ id: g.id, rowVersion: g.rowVersion, ...payload } as never);
          toast.success('تم التحديث');
        }}
        isPending={updateMut.isPending}
      />
    </div>
  );
}
