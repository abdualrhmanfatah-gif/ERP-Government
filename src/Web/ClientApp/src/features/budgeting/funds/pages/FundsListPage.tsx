import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { BUDGET_PERMISSIONS } from '@/shared/constants/permissions';
import { Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Badge } from '@/components/ui';
import { Plus, Pencil, Eye } from 'lucide-react';
import { toast } from 'sonner';
import { fundTypeLabels, fundCategoryLabels, FundType, FundCategory } from '../../shared/types';
import type { FundDto } from '../../shared/types';
import { useFundsList, useCreateFund, useUpdateFund, useActivateFund, useDeactivateFund } from '../hooks/useFunds';

const fundTypeOptions = Object.entries(fundTypeLabels).map(([value, label]) => ({
  value,
  label,
}));

const fundCategoryOptions = Object.entries(fundCategoryLabels).map(([value, label]) => ({
  value,
  label,
}));

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'معطل' },
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
  const activateMutation = useActivateFund();
  const deactivateMutation = useDeactivateFund();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (
          !item.fundNumber.toLowerCase().includes(q) &&
          !item.fundName.toLowerCase().includes(q) &&
          !item.legalAuthority.toLowerCase().includes(q)
        ) return false;
      }
      if (fundTypeFilter && Number(fundTypeFilter) !== item.fundType) return false;
      if (fundCategoryFilter && Number(fundCategoryFilter) !== item.fundCategory) return false;
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
    const mutation = confirmToggle.isActive ? deactivateMutation : activateMutation;
    mutation.mutate(
      { id: confirmToggle.id, rowVersion: confirmToggle.rowVersion },
      {
        onSuccess: () => {
          toast.success(confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح');
          setConfirmToggle(null);
        },
        onError: () => toast.error('حدث خطأ أثناء التبديل'),
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
      fiscalYearId: form.get('fiscalYearId') ? Number(form.get('fiscalYearId')) : undefined,
      defaultRevenueDebitAccountId: form.get('defaultRevenueDebitAccountId')
        ? Number(form.get('defaultRevenueDebitAccountId'))
        : undefined,
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, rowVersion: editItem.rowVersion, ...data },
        {
          onSuccess: () => {
            toast.success('تم التحديث بنجاح');
            setDialogOpen(false);
          },
          onError: () => toast.error('حدث خطأ أثناء التحديث'),
        },
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          toast.success('تم الإنشاء بنجاح');
          setDialogOpen(false);
        },
        onError: () => toast.error('حدث خطأ أثناء الإنشاء'),
      });
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">صناديق الميزانية</h1>
          <p className="text-body-sm text-[var(--color-on-surface-variant)] mt-1">إدارة صناديق الميزانية والبحث والتصفية</p>
        </div>
        {canManage && (
          <Button onClick={handleCreate} icon={<Plus size={16} />} className="self-start sm:self-auto cursor-pointer shadow-sm hover:shadow transition-shadow">
            إضافة صندوق
          </Button>
        )}
      </div>

      <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم الصندوق أو الاسم..." />
        <FilterSelect
          value={fundTypeFilter}
          onChange={setFundTypeFilter}
          options={fundTypeOptions}
          placeholder="النوع"
          label="النوع"
        />
        <FilterSelect
          value={fundCategoryFilter}
          onChange={setFundCategoryFilter}
          options={fundCategoryOptions}
          placeholder="الفئة"
          label="الفئة"
        />
        <FilterSelect
          value={isActiveFilter}
          onChange={setIsActiveFilter}
          options={isActiveOptions}
          placeholder="الحالة"
          label="الحالة"
        />
      </FilterBar>

      <div className="flex items-center gap-2 text-sm">
        <span className="inline-flex items-center rounded-full bg-[var(--color-surface-container)] px-3 py-1 font-medium text-[var(--color-on-surface)] border border-[var(--color-border-container)]">{filtered.length} نتيجة</span>
        {items.length > 0 && <span className="text-[var(--color-on-surface-variant)]">من أصل {items.length} إجمالي</span>}
      </div>

      <div className="w-full overflow-x-auto rounded-xl border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] shadow-sm">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-[var(--color-border-container)] bg-[var(--color-surface-container)]">
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap">رقم الصندوق</th>
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap">اسم الصندوق</th>
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap">النوع</th>
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap">الفئة</th>
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap">الحالة</th>
              <th className="px-4 py-3.5 text-right font-semibold text-[var(--color-on-surface)] whitespace-nowrap w-[120px]">إجراءات</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((item) => (
              <tr key={item.id} className="border-b border-[var(--color-border-container)] last:border-0 hover:bg-[var(--color-surface-container)] transition-colors duration-150">
                <td className="px-4 py-3.5 font-mono font-medium text-[var(--color-on-surface)] whitespace-nowrap">{item.fundNumber}</td>
                <td className="px-4 py-3.5 text-[var(--color-on-surface)] max-w-[220px] truncate">{item.fundName}</td>
                <td className="px-4 py-3.5">
                  <Badge variant="outline" className="whitespace-nowrap">{fundTypeLabels[item.fundType]}</Badge>
                </td>
                <td className="px-4 py-3.5">
                  <Badge variant="outline" className="whitespace-nowrap">{fundCategoryLabels[item.fundCategory]}</Badge>
                </td>
                <td className="px-4 py-3.5">
                  {canManage ? (
                    <Switch
                      checked={item.isActive}
                      onChange={() => handleToggle(item)}
                      label={item.isActive ? 'نشط' : 'معطل'}
                    />
                  ) : (
                    <Badge variant={item.isActive ? 'success' : 'danger'}>
                      {item.isActive ? 'نشط' : 'معطل'}
                    </Badge>
                  )}
                </td>
                <td className="px-4 py-3.5">
                  <div className="flex items-center gap-1">
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/budgeting/funds/${item.id}`)} aria-label="عرض" className="cursor-pointer hover:bg-[var(--color-surface-container-high)] transition-colors duration-200">
                      <Eye size={16} />
                    </Button>
                    {canManage && (
                      <Button variant="ghost" size="icon" onClick={() => handleEdit(item)} aria-label="تعديل" className="cursor-pointer hover:bg-[var(--color-surface-container-high)] transition-colors duration-200">
                        <Pencil size={16} />
                      </Button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

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
        <form id="fund-form" onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="fundNumber" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">رقم الصندوق *</label>
            <input
              id="fundNumber"
              name="fundNumber"
              type="text"
              required
              defaultValue={editItem?.fundNumber}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div>
            <label htmlFor="fundName" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">اسم الصندوق *</label>
            <input
              id="fundName"
              name="fundName"
              type="text"
              required
              defaultValue={editItem?.fundName}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label htmlFor="fundType" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">النوع *</label>
              <select
                id="fundType"
                name="fundType"
                required
                defaultValue={editItem?.fundType}
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              >
                {Object.entries(fundTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="fundCategory" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الفئة *</label>
              <select
                id="fundCategory"
                name="fundCategory"
                required
                defaultValue={editItem?.fundCategory}
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              >
                {Object.entries(fundCategoryLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </div>
          </div>
          <div>
            <label htmlFor="legalAuthority" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الجهة القانونية *</label>
            <input
              id="legalAuthority"
              name="legalAuthority"
              type="text"
              required
              defaultValue={editItem?.legalAuthority}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
          <div>
            <label htmlFor="description" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">الوصف</label>
            <textarea
              id="description"
              name="description"
              rows={3}
              defaultValue={editItem?.description}
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
          </div>
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا الصندوق؟' : 'هل تريد تنشيط هذا الصندوق؟'}
        title={confirmToggle?.isActive ? 'تعطيل الصندوق' : 'تنشيط الصندوق'}
        loading={activateMutation.isPending || deactivateMutation.isPending}
      />
    </div>
  );
}
