import { useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, FilterBar, FilterSearch, FilterSelect, StatusBadge, Badge, Skeleton, ErrorState, ConfirmDialog, Textarea } from '@/components/ui';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { Plus, Eye, Send, CheckCircle, XCircle, Ban } from 'lucide-react';
import {
  purchaseRequestStatusLabels,
  purchaseRequestPriorityLabels,
  purchaseRequestStatusVariant,
  purchaseRequestPriorityVariant,
  type PurchaseRequest,
} from '../shared/types';
import {
  usePurchaseRequestsList,
  useSubmitPurchaseRequest,
  useApprovePurchaseRequest,
  useRejectPurchaseRequest,
  useCancelPurchaseRequest,
} from '../hooks/usePurchaseRequests';
import { handleLifecycleError } from '@/shared/api/result-to-ui';

const PAGE_SIZE = 20;

const statusOptions = Object.entries(purchaseRequestStatusLabels).map(([value, label]) => ({ value, label }));
const priorityOptions = Object.entries(purchaseRequestPriorityLabels).map(([value, label]) => ({ value, label }));

export default function PurchaseRequestsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [priorityFilter, setPriorityFilter] = useState<string>('');
  const [page, setPage] = useState(0);

  const queryParams = useMemo(() => ({
    search: search || undefined,
    status: statusFilter || undefined,
    priority: priorityFilter || undefined,
    page: page + 1,
    pageSize: PAGE_SIZE,
  }), [search, statusFilter, priorityFilter, page]);

  const { data: response, isLoading, error, refetch } = usePurchaseRequestsList(queryParams);

  const submitMutation = useSubmitPurchaseRequest();
  const approveMutation = useApprovePurchaseRequest();
  const rejectMutation = useRejectPurchaseRequest();
  const cancelMutation = useCancelPurchaseRequest();

  const items = response?.items ?? [];
  const totalCount = response?.totalCount ?? 0;

  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectTargetId, setRejectTargetId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelTargetId, setCancelTargetId] = useState<number | null>(null);
  const [cancelReason, setCancelReason] = useState('');

  const handleSearchChange = useCallback((value: string) => {
    setSearch(value);
    setPage(0);
  }, []);

  const handleStatusChange = useCallback((value: string) => {
    setStatusFilter(value);
    setPage(0);
  }, []);

  const handlePriorityChange = useCallback((value: string) => {
    setPriorityFilter(value);
    setPage(0);
  }, []);

  function handleClearFilters() {
    setSearch('');
    setStatusFilter('');
    setPriorityFilter('');
    setPage(0);
  }

  const hasFilters = !!search || !!statusFilter || !!priorityFilter;

  function handleReject() {
    if (!rejectTargetId || !rejectReason.trim()) return;
    rejectMutation.mutate(
      { id: rejectTargetId, reason: rejectReason },
      {
        onSuccess: () => { setRejectOpen(false); setRejectTargetId(null); setRejectReason(''); },
        onError: handleLifecycleError,
      },
    );
  }

  function handleCancel() {
    if (!cancelTargetId) return;
    cancelMutation.mutate(
      { id: cancelTargetId, reason: cancelReason || undefined },
      {
        onSuccess: () => { setCancelOpen(false); setCancelTargetId(null); setCancelReason(''); },
        onError: handleLifecycleError,
      },
    );
  }

  function openRejectDialog(id: number) {
    setRejectTargetId(id);
    setRejectReason('');
    setRejectOpen(true);
  }

  function openCancelDialog(id: number) {
    setCancelTargetId(id);
    setCancelReason('');
    setCancelOpen(true);
  }

  const columns: DataGridColumn<PurchaseRequest>[] = useMemo(() => [
    {
      key: 'requestNumber',
      header: 'رقم الطلب',
      width: 150,
      align: 'left',
      cell: (row) => <span dir="ltr" className="font-mono">{row.requestNumber}</span>,
    },
    {
      key: 'requestDate',
      header: 'تاريخ الطلب',
      width: 120,
      cell: (row) => new Date(row.requestDate).toLocaleDateString('ar-YE'),
    },
    {
      key: 'requesterName',
      header: 'مقدم الطلب',
      width: 150,
    },
    {
      key: 'priority',
      header: 'الأولوية',
      width: 100,
      cell: (row) => (
        <Badge variant={purchaseRequestPriorityVariant[row.priority]}>
          {purchaseRequestPriorityLabels[row.priority]}
        </Badge>
      ),
    },
    {
      key: 'status',
      header: 'الحالة',
      width: 120,
      cell: (row) => (
        <StatusBadge variant={purchaseRequestStatusVariant[row.status]}>
          {purchaseRequestStatusLabels[row.status]}
        </StatusBadge>
      ),
    },
    {
      key: 'totalEstimatedCost',
      header: 'التكلفة التقديرية',
      width: 150,
      align: 'left',
      cell: (row) => row.totalEstimatedCost
        ? <span className="tabular-nums font-mono">{row.totalEstimatedCost.toLocaleString('ar-YE')} ر.ي</span>
        : '-',
    },
    {
      key: 'lineCount',
      header: 'البنود',
      width: 80,
      align: 'center',
    },
    {
      key: 'actions',
      header: 'إجراءات',
      width: 200,
      cell: (row) => (
        <div className="flex gap-1">
          <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/purchase-requests/${row.id}`)} aria-label="عرض التفاصيل">
            <Eye className="h-4 w-4" />
          </Button>
          {row.status === 'Draft' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={() => submitMutation.mutate(row.id, { onError: handleLifecycleError })}
              loading={submitMutation.isPending}
              aria-label="تقديم الطلب"
            >
              <Send className="h-4 w-4" />
            </Button>
          )}
          {row.status === 'Submitted' && (
            <>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => approveMutation.mutate(row.id, { onError: handleLifecycleError })}
                loading={approveMutation.isPending}
                aria-label="اعتماد الطلب"
              >
                <CheckCircle className="h-4 w-4" />
              </Button>
              <Button size="sm" variant="ghost" onClick={() => openRejectDialog(row.id)} aria-label="رفض الطلب">
                <XCircle className="h-4 w-4" />
              </Button>
            </>
          )}
          {row.status === 'Approved' && (
            <Button size="sm" variant="ghost" onClick={() => openCancelDialog(row.id)} aria-label="إلغاء الطلب">
              <Ban className="h-4 w-4" />
            </Button>
          )}
        </div>
      ),
    },
  ], [navigate, submitMutation, approveMutation]);

  if (error) {
    return (
      <Page title="طلبات الشراء" actions={<Button onClick={() => navigate('/procurement/purchase-requests/create')}><Plus className="h-4 w-4 ms-1" />طلب جديد</Button>}>
        <ErrorState message="فشل تحميل طلبات الشراء" onRetry={() => refetch()} />
      </Page>
    );
  }

  return (
    <Page
      title="طلبات الشراء"
      actions={
        <Button onClick={() => navigate('/procurement/purchase-requests/create')}>
          <Plus className="h-4 w-4 ms-1" />طلب جديد
        </Button>
      }
    >
      <FilterBar onClear={handleClearFilters} hasFilters={hasFilters}>
        <FilterSearch value={search} onChange={handleSearchChange} placeholder="بحث برقم الطلب أو الملاحظات..." />
        <FilterSelect label="الحالة" value={statusFilter} onChange={handleStatusChange} options={statusOptions} placeholder="الحالة" />
        <FilterSelect label="الأولوية" value={priorityFilter} onChange={handlePriorityChange} options={priorityOptions} placeholder="الأولوية" />
      </FilterBar>

      <div className="mt-4">
        {isLoading ? (
          <Skeleton variant="table" lines={8} />
        ) : (
          <DataGrid
            data={items}
            columns={columns}
            loading={false}
            emptyMessage="لا توجد طلبات شراء"
            rowKey={(row) => row.id}
            pagination={{ pageIndex: page, pageSize: PAGE_SIZE }}
            totalItems={totalCount}
            onPageChange={setPage}
          />
        )}
      </div>

      <ConfirmDialog
        open={rejectOpen}
        onClose={() => { setRejectOpen(false); setRejectTargetId(null); setRejectReason(''); }}
        onConfirm={handleReject}
        title="رفض طلب الشراء"
        confirmLabel="رفض"
        destructive
        loading={rejectMutation.isPending}
        message={
          <div className="space-y-2">
            <p>هل أنت متأكد من رفض هذا الطلب؟</p>
            <Textarea
              label="سبب الرفض *"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              rows={3}
              placeholder="أدخل سبب الرفض..."
            />
          </div>
        }
      />

      <ConfirmDialog
        open={cancelOpen}
        onClose={() => { setCancelOpen(false); setCancelTargetId(null); setCancelReason(''); }}
        onConfirm={handleCancel}
        title="إلغاء طلب الشراء"
        confirmLabel="إلغاء"
        destructive
        loading={cancelMutation.isPending}
        message={
          <div className="space-y-2">
            <p>هل أنت متأكد من إلغاء هذا الطلب؟</p>
            <Textarea
              label="سبب الإلغاء (اختياري)"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              rows={2}
              placeholder="أدخل سبب الإلغاء..."
            />
          </div>
        }
      />
    </Page>
  );
}
