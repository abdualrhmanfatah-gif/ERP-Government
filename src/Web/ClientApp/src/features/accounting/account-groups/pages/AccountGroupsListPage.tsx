import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Power, PowerOff, Eye, Pencil } from 'lucide-react';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { DataGrid } from '@/components/ui/DataGrid';
import { Button } from '@/components/ui/Button';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import { FilterBar, FilterSelect } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { GroupTree } from '../components/GroupTree';
import { AccountGroupForm } from '../components/AccountGroupForm';
import { useAccountGroupsList } from '../hooks/useAccountGroupsList';
import { useCreateAccountGroup } from '../hooks/useCreateAccountGroup';
import { useUpdateAccountGroup } from '../hooks/useUpdateAccountGroup';
import { useToggleAccountGroupActive } from '../hooks/useToggleAccountGroupActive';
import type { AccountGroupDto } from '../types';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';

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

  const { data, isLoading, error } = useAccountGroupsList({
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
      toast.success('تم إنشاء المجموعة بنجاح');
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'فشل الإنشاء';
      toast.error(msg);
      throw e;
    }
  };

  const handleToggle = async () => {
    if (!confirmToggle) return;
    try {
      await toggleMut.mutateAsync({ id: confirmToggle.id, isActive: !confirmToggle.isActive, rowVersion: confirmToggle.rowVersion });
      toast.success(confirmToggle.isActive ? 'تم التعطيل' : 'تم التفعيل');
      setConfirmToggle(null);
    } catch (e: unknown) {
      toast.error(e instanceof Error ? e.message : 'فشل العملية');
    }
  };

  const handleUpdate = async (payload: Parameters<ReturnType<typeof useUpdateAccountGroup>['mutateAsync']>[0] | Record<string, unknown>) => {
    if (!editingGroup) return;
    try {
      await updateMut.mutateAsync({ id: editingGroup.id, rowVersion: editingGroup.rowVersion, ...(payload as { name:string; type:string; normalBalance:string; description?:string; parentId?:number|null }) } as never);
      toast.success('تم التحديث بنجاح');
      setEditingGroup(null);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : 'فشل التحديث';
      toast.error(msg);
      throw e;
    }
  };

  const isTreeMode = data?.mode === 'tree';
  const items = data?.items ?? [];

  return (
    <div className="space-y-4 p-4">
      <PageHeader title="مجموعات الحسابات" description="إدارة هرمية لتصنيف دليل الحسابات (5 مستويات كحد أقصى)" actions={canCreate && <Button onClick={()=>setShowCreate(true)}><Plus className="h-4 w-4" />إنشاء مجموعة</Button>} />
      <FilterBar>
        <Input placeholder="بحث بالكود أو الاسم..." value={search} onChange={(e)=>{setSearch(e.target.value); setPage(1);}} className="max-w-sm" />
        <FilterSelect label="النوع" value={filterType} onChange={(v: string)=>{setFilterType(v); setPage(1);}} options={[{value:'All',label:'الكل'},{value:'Asset',label:'أصل'},{value:'Liability',label:'التزام'},{value:'Equity',label:'حقوق ملكية'},{value:'Revenue',label:'إيراد'},{value:'Expense',label:'مصروف'}]} />
        <FilterSelect label="الحالة" value={filterActive} onChange={(v: string)=>{setFilterActive(v); setPage(1);}} options={[{value:'All',label:'الكل'},{value:'active',label:'نشط'},{value:'inactive',label:'معطل'}]} />
      </FilterBar>

      {isLoading && <p className="text-sm text-[var(--color-on-surface-variant)]">جاري التحميل...</p>}
      {error && <p className="text-sm text-[var(--color-error)]">خطأ في التحميل</p>}

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
            { id:'code', accessorKey:'code', header:'الكود' },
            { id:'name', accessorKey:'name', header:'الاسم' },
            { id:'type', accessorKey:'type', header:'النوع' },
            { id:'normalBalance', accessorKey:'normalBalance', header:'الرصيد' },
            { id:'level', accessorKey:'level', header:'المستوى' },
            { id:'isActive', accessorKey:'isActive', header:'الحالة', cell: (row)=> <StatusBadge variant={row.isActive?'active':'closed'}>{row.isActive?'نشط':'معطل'}</StatusBadge> },
            { id:'ancestorPath', accessorKey:'ancestorPath', header:'المسار', cell: (row)=> <span className="text-xs">{(row.ancestorPath ?? []).map((a)=>a.code).join(' / ')}</span> },
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

      <div className="flex items-center justify-between">
        <span className="text-sm text-[var(--color-on-surface-variant)]">الصفحة {data?.page ?? 1} من {data?.totalPages ?? 1} — الإجمالي {data?.totalCount ?? 0}</span>
        <div className="flex gap-2"><Button variant="outline" disabled={page<=1} onClick={()=>setPage(p=>p-1)}>السابق</Button><Button variant="outline" disabled={page>= (data?.totalPages ?? 1)} onClick={()=>setPage(p=>p+1)}>التالي</Button></div>
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
    </div>
  );
}
