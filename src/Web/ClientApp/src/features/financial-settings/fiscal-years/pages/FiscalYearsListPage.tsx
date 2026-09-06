import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePermission } from '@/shared/hooks/usePermission';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { Button, FilterBar, FilterSearch, Loading } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye } from 'lucide-react';
import { FiscalYearStatusBadge } from '../../components/FiscalYearStatusBadge';
import { useFiscalYearsList } from '../../hooks/useFiscalYears';

export default function FiscalYearsListPage() {
  const navigate = useNavigate();
  const canCreate = usePermission(PERMISSIONS.FiscalYears.Create);
  const [search, setSearch] = useState('');

  const { data: items = [], isLoading } = useFiscalYearsList();

  const filtered = useMemo(() => {
    return items.filter((item) => {
      if (search) {
        const q = search.toLowerCase();
        if (!item.name.toLowerCase().includes(q) && !String(item.yearNumber).includes(q)) return false;
      }
      return true;
    });
  }, [items, search]);

  const columns: DataGridColumn<typeof items[0]>[] = [
    { header: 'الاسم', cell: (row) => <span className="font-medium">{row.name}</span> },
    { header: 'رقم السنة', cell: (row) => <span className="font-mono">{row.yearNumber}</span> },
    { header: 'تاريخ البداية', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-EG', { dateStyle: 'medium' }).format(new Date(row.startDate))}</span> },
    { header: 'تاريخ النهاية', cell: (row) => <span className="whitespace-nowrap">{new Intl.DateTimeFormat('ar-EG', { dateStyle: 'medium' }).format(new Date(row.endDate))}</span> },
    { header: 'الحالة', cell: (row) => <FiscalYearStatusBadge status={row.status} /> },
    {
      header: 'إجراءات',
      cell: (row) => <Button variant="ghost" size="icon" onClick={() => navigate(`/financial-settings/fiscal-years/${row.id}`)} aria-label="عرض" className="cursor-pointer"><Eye size={16} /></Button>,
    },
  ];

  return (
    <div className="space-y-5">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">السنوات المالية</h1>
          <p className="text-body-sm text-[var(--color-on-surface-variant)] mt-1">إدارة السنوات المالية ولفتراتها المحاسبية</p>
        </div>
        {canCreate && (
          <Button onClick={() => navigate('/financial-settings/fiscal-years/new')} icon={<Plus size={16} />} className="self-start sm:self-auto cursor-pointer shadow-sm hover:shadow transition-shadow">
            سنة مالية جديدة
          </Button>
        )}
      </div>

      <FilterBar hasFilters={!!search} onClear={() => setSearch('')}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالاسم أو رقم السنة..." />
      </FilterBar>

      <div className="flex items-center gap-2 text-sm">
        <span className="inline-flex items-center rounded-full bg-[var(--color-surface-container)] px-3 py-1 font-medium text-[var(--color-on-surface)] border border-[var(--color-border-container)]">{filtered.length} نتيجة</span>
        {items.length > 0 && <span className="text-[var(--color-on-surface-variant)]">من أصل {items.length} إجمالي</span>}
      </div>

      <DataGrid
        columns={columns}
        data={filtered}
        loading={isLoading}
        emptyMessage="لا توجد سنوات مالية بعد"
        rowKey={(row) => row.id}
      />
    </div>
  );
}
