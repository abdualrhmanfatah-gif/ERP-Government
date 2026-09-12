import { useMemo, useState } from 'react';
import { Page, Button, Switch, FilterBar, FilterSearch, FilterSelect, Dialog, ConfirmDialog, Input, Textarea, DataGrid } from '@/components/ui';
import { type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Pencil } from 'lucide-react';
import { useWarehousesList, useCreateWarehouse, useUpdateWarehouse, useToggleWarehouseActive } from '../hooks/useWarehouses';
import type { Warehouse } from '../hooks/useWarehouses';
import { notify } from '@/features/notifications/notify';

const isActiveOptions = [
  { value: 'true', label: 'نشط' },
  { value: 'false', label: 'غير نشط' },
];

export default function WarehousesListPage() {
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<Warehouse | null>(null);
  const [formCode, setFormCode] = useState('');
  const [formName, setFormName] = useState('');
  const [formAddress, setFormAddress] = useState('');
  const [formCity, setFormCity] = useState('');
  const [formPhone, setFormPhone] = useState('');
  const [formEmail, setFormEmail] = useState('');
  const [formTotalCapacity, setFormTotalCapacity] = useState('');
  const [formIsActive, setFormIsActive] = useState(true);
  const [confirmToggle, setConfirmToggle] = useState<Warehouse | null>(null);

  const { data: warehouses = [], isLoading, error, refetch } = useWarehousesList();
  const createMutation = useCreateWarehouse();
  const updateMutation = useUpdateWarehouse();
  const toggleMutation = useToggleWarehouseActive();

  const filtered = useMemo(() => {
    return warehouses.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.code.toLowerCase().includes(q) && !item.name.toLowerCase().includes(q)) return false;
      }
      if (isActiveFilter && (isActiveFilter === 'true') !== item.isActive) return false;
      return true;
    });
  }, [warehouses, search, isActiveFilter]);

  const hasFilters = !!search || !!isActiveFilter;

  function handleClearFilters() {
    setSearch('');
    setIsActiveFilter('');
  }

  function handleCreate() {
    setEditItem(null);
    setFormCode('');
    setFormName('');
    setFormAddress('');
    setFormCity('');
    setFormPhone('');
    setFormEmail('');
    setFormTotalCapacity('');
    setFormIsActive(true);
    setDialogOpen(true);
  }

  function handleEdit(item: Warehouse) {
    setEditItem(item);
    setFormCode(item.code);
    setFormName(item.name);
    setFormAddress(item.address ?? '');
    setFormCity(item.city ?? '');
    setFormPhone(item.phone ?? '');
    setFormEmail(item.email ?? '');
    setFormTotalCapacity(item.totalCapacity != null ? String(item.totalCapacity) : '');
    setFormIsActive(item.isActive);
    setDialogOpen(true);
  }

  function handleToggle(item: Warehouse) {
    setConfirmToggle(item);
  }

  function confirmToggleAction() {
    if (!confirmToggle) return;
    toggleMutation.mutate(confirmToggle.id, {
      onSuccess: () => {
        notify({ type: 'success', title: confirmToggle.isActive ? 'تم التعطيل بنجاح' : 'تم التنشيط بنجاح' });
        setConfirmToggle(null);
      },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التبديل' }),
    });
  }

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!formCode.trim() || !formName.trim()) return;

    const data = {
      code: formCode.trim(),
      name: formName.trim(),
      address: formAddress.trim() || undefined,
      city: formCity.trim() || undefined,
      phone: formPhone.trim() || undefined,
      email: formEmail.trim() || undefined,
      totalCapacity: formTotalCapacity ? Number(formTotalCapacity) : undefined,
      isActive: formIsActive,
    };

    if (editItem) {
      updateMutation.mutate(
        { id: editItem.id, data },
        {
          onSuccess: () => {
            notify({ type: 'success', title: 'تم التحديث بنجاح' });
            setDialogOpen(false);
          },
          onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء التحديث' }),
        },
      );
    } else {
      createMutation.mutate(data, {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم الإنشاء بنجاح' });
          setDialogOpen(false);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      });
    }
  }

  const columns: DataGridColumn<Warehouse>[] = [
    { header: 'الكود', cell: (row) => <span className="font-mono">{row.code}</span> },
    { header: 'الاسم', cell: (row) => row.name },
    { header: 'الموقع', cell: (row) => row.locationName ?? row.city ?? '—' },
    { header: 'المدير', cell: (row) => row.managerName ?? '—' },
    { header: 'السعة الإجمالية', cell: (row) => row.totalCapacity != null ? <span className="tabular-nums">{row.totalCapacity}</span> : '—', align: 'right' },
    { header: 'الحمل الحالي', cell: (row) => row.currentLoad != null ? <span className="tabular-nums">{row.currentLoad}</span> : '—', align: 'right' },
    {
      header: 'الحالة',
      cell: (row) => (
        <Switch checked={row.isActive} onChange={() => handleToggle(row)} label={row.isActive ? 'نشط' : 'غير نشط'} />
      ),
    },
    {
      header: 'إجراءات',
      cell: (row) => (
        <Button variant="ghost" size="icon" onClick={() => handleEdit(row)} aria-label="تعديل">
          <Pencil size={16} />
        </Button>
      ),
    },
  ];

  return (
    <Page
      title="المستودعات"
      description="إدارة المستودعات"
      loading={isLoading}
      error={error?.message}
      onRetry={() => refetch()}
      actions={
        <Button onClick={handleCreate} icon={<Plus size={16} />}>
          إضافة مستودع
        </Button>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
          <FilterSelect
            value={isActiveFilter}
            onChange={setIsActiveFilter}
            options={isActiveOptions}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد مستودعات بعد"
        rowKey={(row) => row.id}
      />

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={editItem ? 'تعديل المستودع' : 'إضافة مستودع جديد'}
        footer={
          <Button type="submit" form="warehouse-form" disabled={createMutation.isPending || updateMutation.isPending} loading={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="warehouse-form" onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج المستودع">
          <Input
            label="الكود *"
            value={formCode}
            onChange={(e) => setFormCode(e.target.value)}
            required
          />
          <Input
            label="الاسم *"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            required
          />
          <Input
            label="المدينة"
            value={formCity}
            onChange={(e) => setFormCity(e.target.value)}
          />
          <Textarea
            label="العنوان"
            value={formAddress}
            onChange={(e) => setFormAddress(e.target.value)}
            rows={2}
          />
          <Input
            label="الهاتف"
            value={formPhone}
            onChange={(e) => setFormPhone(e.target.value)}
          />
          <Input
            label="البريد الإلكتروني"
            type="email"
            value={formEmail}
            onChange={(e) => setFormEmail(e.target.value)}
          />
          <Input
            label="السعة الإجمالية"
            type="number"
            step="0.01"
            value={formTotalCapacity}
            onChange={(e) => setFormTotalCapacity(e.target.value)}
          />
          <Switch
            label="نشط"
            checked={formIsActive}
            onChange={setFormIsActive}
          />
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!confirmToggle}
        onClose={() => setConfirmToggle(null)}
        onConfirm={confirmToggleAction}
        message={confirmToggle?.isActive ? 'هل تريد تعطيل هذا المستودع؟' : 'هل تريد تنشيط هذا المستودع؟'}
        title={confirmToggle?.isActive ? 'تعطيل المستودع' : 'تنشيط المستودع'}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
