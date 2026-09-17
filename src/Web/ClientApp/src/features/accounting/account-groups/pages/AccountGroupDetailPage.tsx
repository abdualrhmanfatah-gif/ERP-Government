import { useParams, useNavigate } from 'react-router-dom';
import { Pencil, Power, PowerOff, Layers, BookOpen, Check, X } from 'lucide-react';
import { Page, StatusBadge, DataGrid, Button, Card, EmptyState, ConfirmDialog } from '@/components/ui';
import { useAccountGroupDetail } from '../hooks/useAccountGroupDetail';
import { useToggleAccountGroupActive } from '../hooks/useToggleAccountGroupActive';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { notify } from '@/features/notifications/notify';
import { useState } from 'react';
import { AccountGroupForm } from '@/components/AccountingAccountGroupForm';
import { useUpdateAccountGroup } from '../hooks/useUpdateAccountGroup';
import { getActiveStatusLabel } from '@/shared/constants/labels';
import { getQueryErrorMessage } from '@/shared/api/query-error';

export function AccountGroupDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const numericId = Number(id);
  const { data, isLoading, error, refetch } = useAccountGroupDetail(numericId);
  const toggleMut = useToggleAccountGroupActive();
  const updateMut = useUpdateAccountGroup();
  const [showEdit, setShowEdit] = useState(false);
  const [confirmToggle, setConfirmToggle] = useState(false);

  const { hasPermission: canEdit } = usePermission(PERMISSIONS.Accounting.ChartOfAccounts.Edit);

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

  if (!data) {
    return (
      <Page title="" onBack={() => navigate('/accounting/account-groups')}>
        <EmptyState message="المجموعة غير موجودة" />
      </Page>
    );
  }

  const g = data.group;

  const handleToggle = async () => {
    try {
      await toggleMut.mutateAsync({ id: g.id, isActive: !g.isActive, rowVersion: g.rowVersion });
      notify({ type: 'success', title: g.isActive ? 'تم التعطيل' : 'تم التفعيل' });
      setConfirmToggle(false);
    } catch (e: unknown) {
      notify({ type: 'error', title: e instanceof Error ? e.message : 'فشل' });
    }
  };

  const childColumns = [
    { id: 'code', accessorKey: 'code', header: 'الكود', cell: (row: { code: string }) => <span dir="ltr" className="tabular-nums">{row.code}</span> },
    { id: 'name', accessorKey: 'name', header: 'الاسم' },
    {
      id: 'isActive',
      accessorKey: 'isActive',
      header: 'الحالة',
      cell: (row: { isActive: boolean }) => (
        <StatusBadge variant={row.isActive ? 'active' : 'inactive'}>
          {getActiveStatusLabel(row.isActive)}
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
    { id: 'code', accessorKey: 'code', header: 'كود الحساب', cell: (row: { code: string }) => <span dir="ltr" className="tabular-nums">{row.code}</span> },
    { id: 'name', accessorKey: 'name', header: 'الاسم' },
    {
      id: 'isActive',
      accessorKey: 'isActive',
      header: 'الحالة',
      cell: (row: { isActive: boolean }) => (
        <StatusBadge variant={row.isActive ? 'active' : 'inactive'}>
          {getActiveStatusLabel(row.isActive)}
        </StatusBadge>
      ),
    },
    {
      id: 'isPostable',
      accessorKey: 'isPostable',
      header: 'قابل للترحيل',
      cell: (row: { isPostable: boolean }) => (
        <span className="inline-flex items-center gap-1">
          {row.isPostable ? <Check size={14} aria-hidden="true" /> : <X size={14} aria-hidden="true" />}
          <span>{row.isPostable ? 'نعم' : 'لا'}</span>
        </span>
      ),
    },
  ];

  return (
    <Page
      title={`${g.code} — ${g.name}`}
      description={
        <span className="flex items-center gap-3">
          <StatusBadge variant={g.isActive ? 'active' : 'inactive'}>
            {getActiveStatusLabel(g.isActive)}
          </StatusBadge>
          <span className="text-[var(--color-on-surface-variant)]">المستوى {g.level} • النوع {g.type}</span>
        </span>
      }
      onBack={() => navigate('/accounting/account-groups')}
      actions={
        <div className="flex gap-2">
          {canEdit && (
            <Button variant="primary" icon={<Pencil size={14} />} onClick={() => setShowEdit(true)}>
              تعديل
            </Button>
          )}
          {canEdit && (
            <Button
              variant={g.isActive ? 'destructive' : 'outline'}
              icon={g.isActive ? <PowerOff size={14} /> : <Power size={14} />}
              onClick={() => setConfirmToggle(true)}
            >
              {g.isActive ? 'تعطيل' : 'تفعيل'}
            </Button>
          )}
        </div>
      }
      loading={isLoading}
    >
      {/* Basic Info Card */}
      <Card>
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <BookOpen size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">البيانات الأساسية</h3>
        </div>
        <div className="flex flex-wrap gap-x-6 gap-y-2">
          <div>
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الكود: </span>
            <span className="text-base font-mono font-medium" dir="ltr">{g.code}</span>
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
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الأب: </span>
            <span className="text-base">{g.parentId ?? 'جذر'}</span>
          </div>
          <div className="flex-1">
            <span className="text-sm font-semibold text-[var(--color-on-surface-variant)]">الوصف: </span>
            <span className="text-base">{g.description ?? '—'}</span>
          </div>
        </div>
      </Card>

      {/* Children Section */}
      <Card>
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <Layers size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">
            المجموعات الفرعية المباشرة ({data.children.length})
          </h3>
        </div>
        {data.children.length === 0 ? (
          <EmptyState message="لا توجد مجموعات فرعية" />
        ) : (
          <DataGrid data={data.children} rowKey={(row) => row.id} columns={childColumns} />
        )}
      </Card>

      {/* Accounts Section */}
      <Card>
        <div className="flex items-center gap-2 mb-4">
          <div className="flex items-center justify-center size-8 rounded-lg bg-[var(--color-primary-container)]">
            <BookOpen size={16} className="text-[var(--color-on-primary-container)]" />
          </div>
          <h3 className="font-semibold text-[var(--color-on-surface)]">
            الحسابات المرتبطة ({data.accounts.length})
          </h3>
        </div>
        {data.accounts.length === 0 ? (
          <EmptyState message="لا توجد حسابات مرتبطة" />
        ) : (
          <DataGrid data={data.accounts} rowKey={(row) => row.id} columns={accountColumns} />
        )}
      </Card>

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
          notify({ type: 'success', title: 'تم التحديث' });
        }}
        isPending={updateMut.isPending}
      />
    </Page>
  );
}
