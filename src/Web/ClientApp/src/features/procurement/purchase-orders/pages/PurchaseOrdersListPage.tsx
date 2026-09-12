import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, FilterDate, StatusBadge, Dialog } from '@/components/ui';
import { Combobox, type ComboboxOption } from '@/components/ui/Combobox';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Eye, Send, CheckCircle, Zap, Ban, Lock, Plus, FileText, ShoppingCart } from 'lucide-react';
import { purchaseOrderStatusLabels, purchaseOrderStatusVariant } from '../shared/types';
import type { PurchaseOrder, PurchaseOrderStatus } from '../shared/types';
import { usePurchaseOrdersList, useApprovePurchaseOrder, useIssuePurchaseOrder, useCancelPurchaseOrder, useClosePurchaseOrder, useSubmitPurchaseOrder } from '../hooks/usePurchaseOrders';
import { usePurchaseRequestsList } from '@/features/procurement/purchase-requests/hooks/usePurchaseRequests';
import { useQuotationsList } from '@/features/procurement/quotations/hooks/useQuotations';
import { handleLifecycleError } from '@/shared/api/result-to-ui';

const statusOptions = Object.entries(purchaseOrderStatusLabels).map(([value, label]) => ({ value, label }));

export default function PurchaseOrdersListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');

  const [createModeOpen, setCreateModeOpen] = useState(false);
  const [selectedPrId, setSelectedPrId] = useState('');
  const [selectedQuotationId, setSelectedQuotationId] = useState('');

  const prsQuery = usePurchaseRequestsList({ status: 'Approved', pageSize: 200 });
  const quotationsQuery = useQuotationsList({ status: 'Awarded', pageSize: 200 });

  const { data: response, isLoading, isError, refetch } = usePurchaseOrdersList({
    search: search || undefined,
    status: statusFilter || undefined,
    expectedDeliveryDateFrom: dateFrom || undefined,
    expectedDeliveryDateTo: dateTo || undefined,
  });

  const approveMutation = useApprovePurchaseOrder();
  const issueMutation = useIssuePurchaseOrder();
  const cancelMutation = useCancelPurchaseOrder();
  const closeMutation = useClosePurchaseOrder();
  const submitMutation = useSubmitPurchaseOrder();

  const items = response?.items ?? [];

  const columns: DataGridColumn<PurchaseOrder>[] = useMemo(() => [
    {
      id: 'purchaseOrderNumber',
      header: 'رقم أمر الشراء',
      accessorKey: 'purchaseOrderNumber',
      width: 160,
      cell: (row) => (
        <span className="font-mono" dir="ltr">{row.purchaseOrderNumber}</span>
      ),
    },
    {
      id: 'supplierName',
      header: 'المورد',
      accessorKey: 'supplierName',
      width: 180,
      cell: (row) => row.supplierName ?? `#${row.supplierPartyId}`,
    },
    {
      id: 'purchaseRequestId',
      header: 'طلب الشراء',
      accessorKey: 'purchaseRequestId',
      width: 140,
      cell: (row) => row.purchaseRequestId ? `#${row.purchaseRequestId}` : '-',
    },
    {
      id: 'status',
      header: 'الحالة',
      accessorKey: 'status',
      width: 130,
      cell: (row) => (
        <StatusBadge variant={purchaseOrderStatusVariant[row.status as PurchaseOrderStatus]}>
          {purchaseOrderStatusLabels[row.status as PurchaseOrderStatus]}
        </StatusBadge>
      ),
    },
    {
      id: 'grandTotal',
      header: 'الإجمالي',
      accessorKey: 'grandTotal',
      width: 140,
      cell: (row) => row.grandTotal ? <span className="font-bold tabular-nums">{row.grandTotal.toLocaleString('ar-YE')} ر.ي</span> : '-',
    },
    {
      id: 'expectedDeliveryDate',
      header: 'تاريخ التسليم',
      accessorKey: 'expectedDeliveryDate',
      width: 120,
      cell: (row) => row.expectedDeliveryDate ? new Date(row.expectedDeliveryDate).toLocaleDateString('ar-YE') : '-',
    },
    {
      id: 'created',
      header: 'تاريخ الإنشاء',
      accessorKey: 'created',
      width: 120,
      cell: (row) => row.created ? new Date(row.created).toLocaleDateString('ar-YE') : '-',
    },
    {
      id: 'actions',
      header: 'إجراءات',
      width: 180,
      cell: (row) => (
        <div className="flex gap-1">
          <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/purchase-orders/${row.id}`)} aria-label="عرض التفاصيل">
            <Eye className="h-4 w-4" />
          </Button>
          {row.status === 'Draft' && (
            <>
              <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/purchase-orders/${row.id}/edit`)} aria-label="تعديل">
                <span className="sr-only">تعديل</span>
                ✏️
              </Button>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => submitMutation.mutate(row.id, { onError: handleLifecycleError })}
                disabled={submitMutation.isPending}
                aria-label="تقديم"
              >
                <Send className="h-4 w-4" />
              </Button>
            </>
          )}
          {row.status === 'Submitted' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => approveMutation.mutate(row.id, { onError: handleLifecycleError })}
              disabled={approveMutation.isPending}
              aria-label="اعتماد"
            >
              <CheckCircle className="h-4 w-4" />
            </Button>
          )}
          {row.status === 'Approved' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => issueMutation.mutate(row.id, { onError: handleLifecycleError })}
              disabled={issueMutation.isPending}
              aria-label="إصدار"
            >
              <Zap className="h-4 w-4" />
            </Button>
          )}
          {row.status !== 'Cancelled' && row.status !== 'Closed' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => cancelMutation.mutate({ id: row.id }, { onError: handleLifecycleError })}
              disabled={cancelMutation.isPending}
              aria-label="إلغاء"
            >
              <Ban className="h-4 w-4" />
            </Button>
          )}
          {(row.status === 'PartiallyReceived' || row.status === 'Received') && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => closeMutation.mutate({ id: row.id }, { onError: handleLifecycleError })}
              disabled={closeMutation.isPending}
              aria-label="إغلاق"
            >
              <Lock className="h-4 w-4" />
            </Button>
          )}
        </div>
      ),
    },
  ], [navigate, approveMutation, issueMutation, cancelMutation, closeMutation, submitMutation]);

  function handleClearFilters() {
    setSearch('');
    setStatusFilter('');
    setDateFrom('');
    setDateTo('');
  }

  function handleCreateFromPR() {
    const id = Number(selectedPrId);
    if (id > 0) {
      setCreateModeOpen(false);
      setSelectedPrId('');
      navigate(`/procurement/purchase-orders/create?purchaseRequestId=${id}`);
    }
  }

  function handleCreateFromQuotation() {
    const id = Number(selectedQuotationId);
    if (id > 0) {
      setCreateModeOpen(false);
      setSelectedQuotationId('');
      navigate(`/procurement/purchase-orders/create?quotationId=${id}`);
    }
  }

  const prOptions: ComboboxOption[] = (prsQuery.data?.items ?? []).map((pr) => ({
    value: String(pr.id),
    label: `${pr.requestNumber} — ${pr.lineCount} بنود`,
  }));

  const quotationOptions: ComboboxOption[] = (quotationsQuery.data?.items ?? []).map((q) => ({
    value: String(q.id),
    label: `${q.quotationNumber} — ${q.grandTotal?.toLocaleString('ar-YE') ?? '0'} ر.ي`,
  }));

  const hasFilters = !!search || !!statusFilter || !!dateFrom || !!dateTo;

  if (isError) return <Page title="أوامر الشراء" error="خطأ في تحميل أوامر الشراء" onRetry={() => refetch()} />;

  return (
    <Page title="أوامر الشراء" actions={<Button onClick={() => setCreateModeOpen(true)}><Plus className="h-4 w-4 ms-1" />أمر شراء جديد</Button>}>
      <FilterBar onClear={handleClearFilters} hasFilters={hasFilters}>
        <FilterSearch value={search} onChange={setSearch} placeholder="بحث برقم أمر الشراء..." />
        <FilterSelect label="الحالة" value={statusFilter} onChange={setStatusFilter} options={statusOptions} placeholder="الكل" />
        <FilterDate label="من تاريخ" value={dateFrom} onChange={setDateFrom} />
        <FilterDate label="إلى تاريخ" value={dateTo} onChange={setDateTo} />
      </FilterBar>

      <div className="mt-4">
        <DataGrid
          data={items}
          columns={columns}
          loading={isLoading}
          emptyMessage="لا توجد أوامر شراء"
          rowKey={(row) => row.id}
        />
      </div>

      <Dialog
        open={createModeOpen}
        onClose={() => { setCreateModeOpen(false); setSelectedPrId(''); setSelectedQuotationId(''); }}
        title="إنشاء أمر شراء"
        footer={<Button variant="outline" onClick={() => { setCreateModeOpen(false); setSelectedPrId(''); setSelectedQuotationId(''); }}>إغلاق</Button>}
      >
        <div className="space-y-4">
          <button
            type="button"
            className="w-full flex items-center gap-3 p-4 border border-[var(--color-outline-variant)] rounded-lg hover:bg-[color-mix(in_srgb,var(--color-primary-container)_8%,transparent)] transition-colors text-start"
            onClick={() => navigate('/procurement/purchase-orders/create')}
          >
            <div className="flex-shrink-0 w-10 h-10 rounded-full bg-[var(--color-primary-container)] flex items-center justify-center">
              <Plus className="h-5 w-5 text-[var(--color-primary)]" />
            </div>
            <div>
              <p className="font-medium">أمر شراء جديد</p>
              <p className="text-sm text-[var(--color-on-surface-variant)]">إنشاء أمر شراء فارغ مع تحديد البيانات يدوياً</p>
            </div>
          </button>

          <div className="border border-[var(--color-outline-variant)] rounded-lg p-4">
            <div className="flex items-center gap-3 mb-3">
              <div className="flex-shrink-0 w-10 h-10 rounded-full bg-[var(--color-secondary-container)] flex items-center justify-center">
                <FileText className="h-5 w-5 text-[var(--color-secondary)]" />
              </div>
              <div>
                <p className="font-medium">من طلب شراء</p>
                <p className="text-sm text-[var(--color-on-surface-variant)]">إنشاء أمر شراء من طلب شراء معتمد مع بنود جاهزة</p>
              </div>
            </div>
            <div className="flex gap-2 items-end">
              <div className="flex-1">
                <Combobox
                  options={prOptions}
                  value={selectedPrId}
                  onChange={setSelectedPrId}
                  placeholder={prsQuery.isLoading ? 'جاري التحميل...' : 'اختر طلب الشراء...'}
                  searchPlaceholder="بحث برقم طلب الشراء..."
                />
              </div>
              <Button onClick={handleCreateFromPR} disabled={!selectedPrId}>إنشاء</Button>
            </div>
          </div>

          <div className="border border-[var(--color-outline-variant)] rounded-lg p-4">
            <div className="flex items-center gap-3 mb-3">
              <div className="flex-shrink-0 w-10 h-10 rounded-full bg-[var(--color-secondary-container)] flex items-center justify-center">
                <ShoppingCart className="h-5 w-5 text-[var(--color-secondary)]" />
              </div>
              <div>
                <p className="font-medium">من عرض سعر</p>
                <p className="text-sm text-[var(--color-on-surface-variant)]">إنشاء أمر شراء من عرض سعر مرسي مع أسعار وبنود جاهزة</p>
              </div>
            </div>
            <div className="flex gap-2 items-end">
              <div className="flex-1">
                <Combobox
                  options={quotationOptions}
                  value={selectedQuotationId}
                  onChange={setSelectedQuotationId}
                  placeholder={quotationsQuery.isLoading ? 'جاري التحميل...' : 'اختر عرض السعر...'}
                  searchPlaceholder="بحث برقم عرض السعر..."
                />
              </div>
              <Button onClick={handleCreateFromQuotation} disabled={!selectedQuotationId}>إنشاء</Button>
            </div>
          </div>
        </div>
      </Dialog>
    </Page>
  );
}
