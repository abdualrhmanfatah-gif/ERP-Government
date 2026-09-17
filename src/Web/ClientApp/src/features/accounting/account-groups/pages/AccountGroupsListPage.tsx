import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Power, PowerOff, Eye, Pencil } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { Page, DataGrid, Button, ConfirmDialog, FilterBar, FilterSearch, FilterSelect, StatusBadge, Pagination } from '@/components/ui';
import { GroupTree } from '@/components/AccountingGroupTree';
import { AccountGroupForm } from '@/components/AccountingAccountGroupForm';
import { useAccountGroupsList } from '../hooks/useAccountGroupsList';
import { useCreateAccountGroup } from '../hooks/useCreateAccountGroup';
import { useUpdateAccountGroup } from '../hooks/useUpdateAccountGroup';
import { useToggleAccountGroupActive } from '../hooks/useToggleAccountGroupActive';
import type { AccountGroupDto } from '../types';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { activeStatusLabels, getActiveStatusLabel } from '@/shared/constants/labels';
import { getQueryErrorMessage } from '@/shared/api/query-error';

export function AccountGroupsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [filterType, setFilterType] = useState<string>('All');
  const [filterActive, setFilterActive] = useState<string>('All');
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const [editingGroup, setEditingGroup] = useState<AccountGroupDto | null>(null);
  const [confirmToggle, setConfirmToggle] = useState<{ id:number; isActive:boolean; rowVersion:string }|null>(null);

  const { hasPermission: canCreate } = usePermission(PERMISSIONS.Accounting.ChartOfAccounts.Create);
  const { hasPermission: canEdit } = usePermission(PERMISSIONS.Accounting.ChartOfAccounts.Edit);

  const { data, isLoading, error, refetch } = useAccountGroupsList({
    search: search || undefined,
    type: filterType !== 'All' ? filterType : undefined,
    isActive: filterActive === 'All' ? undefined : filterActive === 'active',
    page,
    pageSize: 20,
  });

  const createMut = useCreateAccountGroup();
  const updateMut = useUpdateAccountGroup();
  const toggleMut = useToggleAccountGroupActive();

  const handleCreate = async (payload: Parameters<ReturnType<typeof useCreateAccountGroup>['mutateAsync']>[0]) => {
    try {
      await createMut.mutateAsync(payload);
      notify({ type: 'success', title: 'تم إنشاء المجموعة بنجاح' });
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'فشل الإنشاء';
      notify({ type: 'error', title: msg });
      throw e;
    }
  };

  const handleToggle = async () => {
    if (!confirmToggle) return;
    try {
      await toggleMut.mutateAsync({ id: confirmToggle.id, isActive: !confirmToggle.isActive, rowVersion: confirmToggle.rowVersion });
      notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل' : 'تم التفعيل' });
      setConfirmToggle(null);
    } catch (e: unknown) {
      notify({ type: 'error', title: e instanceof Error ? e.message : 'فشل العملية' });
    }
  };

  const handleUpdate = async (payload: Parameters<ReturnType<typeof useUpdateAccountGroup>['mutateAsync']>[0] | Record<string, unknown>) => {
    if (!editingGroup) return;
    try {
      await updateMut.mutateAsync({ id: editingGroup.id, rowVersion: editingGroup.rowVersion, ...(payload as { name:string; type:string; normalBalance:string; description?:string; parentId?:number|null }) } as never);
      notify({ type: 'success', title: 'تم التحديث بنجاح' });
      setEditingGroup(null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'فشل التحديث';
      notify({ type: 'error', title: msg });
      throw e;
    }
  };

  const isTreeMode = data?.mode === 'tree';
  const items = data?.items ?? [];

  return (
    <Page
      title="مجموعات الحسابات"
      description="إدارة هرمية لتصنيف دليل الحسابات (5 مستويات كحد أقصى)"
      actions={canCreate && <Button variant="primary" size="sm" icon={<Plus size={16} />} onClick={()=>setShowCreate(true)}>إنشاء مجموعة</Button>}
      toolbar={
        <FilterBar>
          <FilterSearch
            value={search}
            onChange={(v) => { setSearch(v); setPage(1); }}
            placeholder="بحث بالكود أو الاسم..."
            className="flex-1 min-w-48"
          />
          <FilterSelect label="النوع" value={filterType} onChange={(v: string)=>{setFilterType(v); setPage(1);}} options={[{value:'All',label:'الكل'},{value:'Asset',label:'أصل'},{value:'Liability',label:'التزام'},{value:'Equity',label:'حقوق ملكية'},{value:'Revenue',label:'إيراد'},{value:'Expense',label:'مصروف'}]} />
          <FilterSelect label="الحالة" value={filterActive} onChange={(v: string)=>{setFilterActive(v); setPage(1);}} options={[{value:'All',label:'الكل'},{value:'active',label:activeStatusLabels.active},{value:'inactive',label:activeStatusLabels.disabled}]} />
        </FilterBar>
      }
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
      onRetry={error ? () => refetch() : undefined}
    >
      {isTreeMode ? (
        <GroupTree
          groups={items as never}
          canEdit={!!canEdit}
          onSelect={(g)=>navigate(`/accounting/account-groups/${(g as {id:number}).id}`)}
          onEdit={(g)=>setEditingGroup(g as unknown as AccountGroupDto)}
          onToggle={(g)=>setConfirmToggle({id:(g as {id:number; rowVersion:string}).id,isActive:(g as {isActive:boolean}).isActive,rowVersion:(g as {rowVersion:string}).rowVersion})}
        />
      ) : (
        <DataGrid
          data={items}
          rowKey={(row: (typeof items)[number]) => row.id}
          columns={[
            { id:'code', accessorKey:'code', header:'الكود', cell: (row)=> <span dir="ltr" className="tabular-nums">{row.code}</span> },
            { id:'name', accessorKey:'name', header:'الاسم' },
            { id:'type', accessorKey:'type', header:'النوع' },
            { id:'normalBalance', accessorKey:'normalBalance', header:'الرصيد' },
            { id:'level', accessorKey:'level', header:'المستوى' },
            { id:'isActive', accessorKey:'isActive', header:'الحالة', cell: (row)=> <StatusBadge variant={row.isActive?'active':'inactive'}>{getActiveStatusLabel(row.isActive ?? false)}</StatusBadge> },
            { id:'ancestorPath', accessorKey:'ancestorPath', header:'المسار', cell: (row)=> <span dir="ltr" className="text-xs tabular-nums">{(row.ancestorPath ?? []).map((a)=>a.code).join(' / ')}</span> },
            { id:'actions', header:'إجراءات', cell: (row)=> {
              return <div className="flex gap-1">
                <Button variant="ghost" size="icon" aria-label="عرض" onClick={()=>navigate(`/accounting/account-groups/${row.id}`)}><Eye className="h-4 w-4"/></Button>
                {canEdit && <Button variant="ghost" size="icon" aria-label="تعديل" onClick={()=>setEditingGroup(row as unknown as AccountGroupDto)}><Pencil className="h-4 w-4"/></Button>}
                {canEdit && <Button variant="ghost" size="icon" aria-label={row.isActive?'تعطيل':'تفعيل'} onClick={()=>setConfirmToggle({id:row.id,isActive:row.isActive,rowVersion:row.rowVersion})}>{row.isActive?<PowerOff className="h-4 w-4"/>:<Power className="h-4 w-4"/>}</Button>}
              </div>;
            }}
          ]}
        />
      )}

      <div className="flex items-center justify-between gap-2 flex-wrap">
        <span className="text-sm text-[var(--color-on-surface-variant)]">الإجمالي {data?.totalCount ?? 0}</span>
        <Pagination page={page} total={data?.totalCount ?? 0} pageSize={20} onChange={setPage} />
      </div>

      <AccountGroupForm open={showCreate} onOpenChange={setShowCreate} onSubmit={handleCreate} isPending={createMut.isPending} />
      <AccountGroupForm open={!!editingGroup} onOpenChange={(v)=>!v && setEditingGroup(null)} initial={editingGroup} onSubmit={handleUpdate as never} isPending={updateMut.isPending} />

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={()=>setConfirmToggle(null)}
        title={confirmToggle?.isActive ? 'تأكيد التعطيل' : 'تأكيد التفعيل'}
        message={confirmToggle?.isActive ? 'سيتم منع استخدام المجموعة في حسابات جديدة. لا يمكن تعطيل مجموعة لديها فروع/حسابات نشطة.' : 'سيتم إعادة تفعيل المجموعة.'}
        onConfirm={handleToggle}
        confirmLabel={confirmToggle?.isActive ? 'تعطيل' : 'تفعيل'}
        destructive={!!confirmToggle?.isActive}
      />
    </Page>
  );
}
