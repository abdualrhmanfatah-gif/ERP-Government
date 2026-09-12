import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { Page, Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Badge, Input, Select, Textarea } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil, Eye } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { fundTypeLabels, fundCategoryLabels, FundType, FundCategory } from '../../shared/types';
import type { FundDto } from '../../shared/types';
import { activeStatusLabels, getActiveStatusLabel } from '@/shared/constants/labels';
import { useFundsList, useCreateFund, useUpdateFund, useToggleFundActive } from '../hooks/useFunds';

const fundTypeOptions = Object.entries(fundTypeLabels).map(([value, label]) => ({
  value,
  label,
}));

const fundCategoryOptions = Object.entries(fundCategoryLabels).map(([value, label]) => ({
  value,
  label,
}));

const isActiveOptions = [
  { value: 'true', label: activeStatusLabels.active },
  { value: 'false', label: activeStatusLabels.disabled },
];

export default function FundsListPage() {
  const navigate = useNavigate();
  const canManage = usePermission(BUDGET_PERMISSIONS.Funds.Create);
  const [search, setSearch] = useState('');
  const [fundTypeFilter, setFundTypeFilter] = useState<string>('');
  const [fundCategoryFilter, setFundCategoryFilter] = useState<string>('');
  const [isActiveFilter, setIsActiveFilter] = useState<string>('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<FundDto | null>(null);
  const [confirmToggle, setConfirmToggle] = useState<FundDto | null>(null);

  const { data: items = [], isLoading } = useFundsList();
  const createMutation = useCreateFund();
  const updateMutation = useUpdateFund();
  const toggleMutation = useToggleFundActive();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.fundNumber.toLowerCase().includes(q) && !item.fundName.toLowerCase().includes(q) && !item.legalAuthority.toLowerCase().includes(q)) return false;
      }
      if (fundTypeFilter && fundTypeFilter !== item.fundType) return false;
      if (fundCategoryFilter && fundCategoryFilter !== item.fundCategory) return false;
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [items, search, fundTypeFilter, fundCategoryFilter, isActiveFilter]);

  const hasFilters = !!search || !!fundTypeFilter || !!fundCategoryFilter || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setFundTypeFilter('');
    setFundCategoryFilter('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setDialogOpen(true);
  }

  function handleEdit(item: FundDto) {
    setEditItem(item);
    setDialogOpen(true);
  }

  function handleToggle(item: FundDto) {
    setConfirmToggle(item);
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    toggleMutation.mutate(
      { id: confirmToggle.id, rowVersion: confirmToggle.rowVersion },
      {
        onSuccess: () => {
          notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
          setConfirmToggle(null);
        },
        onError: (err) => notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ أثناء التبديل' }),
      },
    );
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    const data = {
      fundNumber: form.get('fundNumber') as string,
      fundName: form.get('fundName') as string,
      fundType: Number(form.get('fundType')) as FundType,
      fundCategory: Number(form.get('fundCategory')) as FundCategory,
      legalAuthority: form.get('legalAuthority') as string,
      description: (form.get('description') as string) || undefined,
      defaultRevenueDebitAccountId: form.get('defaultRevenueDebitAccountId') ? Number(form.get('defaultRevenueDebitAccountId')) : undefined,
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, rowVersion: editItem.rowVersion, ...data },
        {
          onSuccess: () => { notify({ type: 'success', title: 'تم التحديث بنجاح' }); setDialogOpen(false); },
          onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التحديث' }),
        },
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => { notify({ type: 'success', title: 'تم الإنشاء بنجاح' }); setDialogOpen(false); },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      });
    }
  }

  const columns: DataGridColumn<FundDto>[] = [
    { header: 'رقم الصندوق', cell: (row) => <span className="font-mono font-medium whitespace-nowrap">{row.fundNumber}</span> },
    { header: 'اسم الصندوق', cell: (row) => <span className="max-w-[220px] truncate">{row.fundName}</span> },
    { header: 'النوع', cell: (row) => <Badge variant="outline" className="whitespace-nowrap">{fundTypeLabels[row.fundType]}</Badge> },
    { header: 'الفئة', cell: (row) => <Badge variant="outline" className="whitespace-nowrap">{fundCategoryLabels[row.fundCategory]}</Badge> },
    {
      header: 'الحالة',
      cell: (row) => canManage
        ? <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={getActiveStatusLabel(row.isActive)} />
        : <Badge variant={row.isActive ? 'success' : 'danger'}>{getActiveStatusLabel(row.isActive)}</Badge>,
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" onClick={() => navigate(`/budgeting/funds/${row.id}`)} aria-label="عرض" className="cursor-pointer"><Eye size={16} /></Button>
          {canManage && <Button variant="ghost" size="icon" onClick={() => handleEdit(row)} aria-label="تعديل" className="cursor-pointer"><Pencil size={16} /></Button>}
        </div>
      ),
    },
  ];

  return (
    <Page
      title="صناديق الميزانية"
      description="إدارة صناديق الميزانية والبحث والتصفية"
      actions={
        canManage ? (
          <Button onClick={handleCreate} icon={<Plus size={16} />} className="self-start sm:self-auto cursor-pointer shadow-sm hover:shadow transition-shadow">
            إضافة صندوق
          </Button>
        ) : undefined
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم الصندوق أو الاسم..." />
          <FilterSelect value={fundTypeFilter} onChange={setFundTypeFilter} options={fundTypeOptions} placeholder="النوع" label="النوع" />
          <FilterSelect value={fundCategoryFilter} onChange={setFundCategoryFilter} options={fundCategoryOptions} placeholder="الفئة" label="الفئة" />
          <FilterSelect value={isActiveFilter} onChange={setIsActiveFilter} options={isActiveOptions} placeholder="الحالة" label="الحالة" />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد صناديق بعد"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل الصندوق' : 'إضافة صندوق جديد'}
        footer={
          <Button type="submit" form="fund-form" disabled={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="fund-form" onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الصندوق">
          <Input id="fundNumber" name="fundNumber" type="text" required defaultValue={editItem?.fundNumber} label="رقم الصندوق" />
          <Input id="fundName" name="fundName" type="text" required defaultValue={editItem?.fundName} label="اسم الصندوق" />
          <div className="grid grid-cols-2 gap-4">
            <Select id="fundType" name="fundType" required defaultValue={String(editItem?.fundType ?? '')} label="النوع" options={Object.entries(fundTypeLabels).map(([value, label]) => ({ value, label }))} />
            <Select id="fundCategory" name="fundCategory" required defaultValue={String(editItem?.fundCategory ?? '')} label="الفئة" options={Object.entries(fundCategoryLabels).map(([value, label]) => ({ value, label }))} />
          </div>
          <Input id="legalAuthority" name="legalAuthority" type="text" required defaultValue={editItem?.legalAuthority} label="الجهة القانونية" />
          <Textarea id="description" name="description" rows={3} defaultValue={editItem?.description} label="الوصف" />
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا الصندوق؟' : 'هل تريد تنشيط هذا الصندوق؟'}
        title={confirmToggle?.isActive ? 'تعطيل الصندوق' : 'تنشيط الصندوق'}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
