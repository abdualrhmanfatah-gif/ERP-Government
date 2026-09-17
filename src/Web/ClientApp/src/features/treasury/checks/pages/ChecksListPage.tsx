import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { Badge, Button, DataGrid, FilterBar, FilterDate, FilterSelect, Page } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';
import { Plus } from 'lucide-react';
import { RevenueCheckClearDialog } from '@/components/RevenueCheckClearDialog';
import { RevenueCheckBounceDialog } from '@/components/RevenueCheckBounceDialog';
import { TreasuryChecksReplaceDialog } from '@/components/TreasuryChecksReplaceDialog';
import { useChecksList, useCheckDetail } from '../hooks/useChecks';
import { checkStatusLabels } from '../../shared/types';

const statusColors: Record<string, 'warning' | 'success' | 'error'> = {
  UnderCollection: 'warning',
  Cleared: 'success',
  Bounced: 'error',
};

export default function ChecksListPage() {
  const navigate = useNavigate();
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
  const [fromDate, setFromDate] = useState(format(firstDay, 'yyyy-MM-dd'));
  const [toDate, setToDate] = useState(format(today, 'yyyy-MM-dd'));
  const [statusFilter, setStatusFilter] = useState('');
  const [clearDialogOpen, setClearDialogOpen] = useState(false);
  const [bounceDialogOpen, setBounceDialogOpen] = useState(false);
  const [replaceDialogOpen, setReplaceDialogOpen] = useState(false);
  const [selectedCheckId, setSelectedCheckId] = useState<number | null>(null);

  const { data: checks, isLoading, error, refetch } = useChecksList({
    from: fromDate,
    to: toDate,
    status: statusFilter || undefined,
  });

  useCheckDetail(selectedCheckId ?? 0);

  const columns: DataGridColumn<any>[] = [
    { key: 'checkNumber', header: 'رقم الشيك' },
    { key: 'bankName', header: 'البنك' },
    {
      key: 'checkDate',
      header: 'تاريخ الشيك',
      render: (row) => row.checkDate ? format(new Date(row.checkDate), 'yyyy-MM-dd') : '',
    },
    {
      key: 'amount',
      header: 'المبلغ',
      render: (row) => `${(row.amount ?? 0).toLocaleString('ar-YE')} ريال`,
    },
    { key: 'receiptVoucherId', header: 'رقم السند' },
    {
      key: 'status',
      header: 'الحالة',
      render: (row) => (
        <Badge variant={statusColors[row.status] || 'default'}>
          {checkStatusLabels[row.status] || row.statusName || row.status}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      render: (row) => (
        <div className="flex gap-1">
          {row.status === 'UnderCollection' && (
            <>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setSelectedCheckId(row.id);
                  setClearDialogOpen(true);
                }}
              >
                تحصيل
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setSelectedCheckId(row.id);
                  setBounceDialogOpen(true);
                }}
              >
                ارتجاع
              </Button>
            </>
          )}
          {row.status === 'Bounced' && !row.replacementVoucherId && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setSelectedCheckId(row.id);
                setReplaceDialogOpen(true);
              }}
            >
              استبدال
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <Page
      title="الشيكات"
      actions={
        <Button
          onClick={() => navigate('/treasury/receipt-vouchers/create')}
          icon={<Plus size={16} />}
        >
          إضافة شيك
        </Button>
      }
      toolbar={
        <FilterBar>
          <FilterDate
            label="من تاريخ"
            value={fromDate}
            onChange={setFromDate}
          />
          <FilterDate
            label="إلى تاريخ"
            value={toDate}
            onChange={setToDate}
          />
          <FilterSelect
            value={statusFilter}
            onChange={setStatusFilter}
            options={[
              { value: '', label: 'جميع الحالات' },
              { value: 'UnderCollection', label: 'تحت التحصيل' },
              { value: 'Cleared', label: 'محصل' },
              { value: 'Bounced', label: 'مرتجع' },
            ]}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
      loading={isLoading}
      error={error ? 'حدث خطأ في تحميل البيانات' : undefined}
      onRetry={() => refetch()}
    >
      <DataGrid
        columns={columns}
        data={checks || []}
        rowKey={(row) => row.id}
        emptyMessage="لا توجد شيكات في الفترة المحددة"
      />

      {selectedCheckId && (
        <>
          <RevenueCheckClearDialog
            open={clearDialogOpen}
            onClose={() => { setClearDialogOpen(false); setSelectedCheckId(null); }}
            checkId={selectedCheckId}
            onCompleted={() => { setClearDialogOpen(false); setSelectedCheckId(null); }}
          />
          <RevenueCheckBounceDialog
            open={bounceDialogOpen}
            onClose={() => { setBounceDialogOpen(false); setSelectedCheckId(null); }}
            checkId={selectedCheckId}
            onCompleted={() => { setBounceDialogOpen(false); setSelectedCheckId(null); }}
          />
          <TreasuryChecksReplaceDialog
            open={replaceDialogOpen}
            onClose={() => { setReplaceDialogOpen(false); setSelectedCheckId(null); }}
            checkId={selectedCheckId}
            onReplaced={() => { setReplaceDialogOpen(false); setSelectedCheckId(null); }}
          />
        </>
      )}
    </Page>
  );
}
