import { useMemo, useState } from 'react';
import { Button, ConfirmDialog, DataGrid, FilterBar, FilterSelect, StatusBadge } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { notify } from '@/features/notifications/notify';
import { PERMISSIONS } from '@/shared/constants/permissions';
import { usePermission } from '@/shared/hooks/usePermission';
import { useFiscalYearsList } from '@/features/financial-settings/hooks/useFiscalYears';
import { useFiscalPeriodsList } from '@/features/financial-settings/hooks/useFiscalPeriods';
import { useAccountsList } from '@/features/accounting/hooks/useAccountsList';
import { useAccountBalances, useFinalizePeriod, useUnfinalizePeriod, useRebuildBalances } from '../hooks/useAccountBalances';
import type { AccountBalanceDto } from '../shared/types';

const directionLabels: Record<string, string> = {
  Debit: 'مدين',
  Credit: 'دائن',
  Zero: 'صفري',
};

interface PendingAction {
  fiscalYearId: number;
  fiscalPeriodId: number;
  type: 'finalize' | 'unfinalize' | 'rebuild';
}

export function AccountBalancesPage() {
  const [fiscalYearId, setFiscalYearId] = useState('');
  const [fiscalPeriodId, setFiscalPeriodId] = useState('');
  const [accountId, setAccountId] = useState('');
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null);

  const { hasPermission: canFinalize } = usePermission(PERMISSIONS.Accounting.Balances.Finalize);
  const { hasPermission: canUnfinalize } = usePermission(PERMISSIONS.Accounting.Balances.Unfinalize);
  const { hasPermission: canRebuild } = usePermission(PERMISSIONS.Accounting.Balances.Rebuild);

  const { data: fiscalYears = [] } = useFiscalYearsList(true);
  const yearId = Number(fiscalYearId) || 0;
  const { data: fiscalPeriods = [] } = useFiscalPeriodsList(yearId);
  const { data: accounts = [] } = useAccountsList({ isPostable: true });

  const { data: balances = [], isLoading, isError, refetch } = useAccountBalances({
    fiscalYearId: yearId,
    fiscalPeriodId: Number(fiscalPeriodId) || 0,
    accountId: Number(accountId) || undefined,
  });

  const finalizeMutation = useFinalizePeriod();
  const unfinalizeMutation = useUnfinalizePeriod();
  const rebuildMutation = useRebuildBalances();
  const actionPending = finalizeMutation.isPending || unfinalizeMutation.isPending || rebuildMutation.isPending;

  const handleConfirm = () => {
    if (!pendingAction) return;
    const { fiscalYearId: yearId, fiscalPeriodId: periodId, type } = pendingAction;
    if (type === 'rebuild') {
      rebuildMutation.mutate(
        { fiscalYearId: yearId, fiscalPeriodId: periodId },
        {
          onSuccess: () => {
            notify({ type: 'success', title: 'تم إعادة بناء الأرصدة' });
            setPendingAction(null);
          },
          onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء تنفيذ العملية' }),
        },
      );
      return;
    }
    const mutation = type === 'finalize' ? finalizeMutation : unfinalizeMutation;
    mutation.mutate(
      { fiscalYearId: yearId, fiscalPeriodId: periodId },
      {
        onSuccess: () => {
          notify({
            type: 'success',
            title: type === 'finalize' ? 'تم إغلاق الفترة' : 'تم فتح الفترة',
          });
          setPendingAction(null);
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء تنفيذ العملية' }),
      },
    );
  };

  const yearOptions = fiscalYears.map((fy) => ({
    value: String(fy.id ?? ''),
    label: `${fy.name ?? ''} (${fy.yearNumber ?? ''})`,
  }));

  const periodOptions = fiscalPeriods.map((p) => ({
    value: String(p.id ?? ''),
    label: p.name ?? '',
  }));

  const accountOptions = accounts.map((a) => ({
    value: String(a.id ?? ''),
    label: `${a.code} - ${a.name}`,
  }));

  const columns: DataGridColumn<AccountBalanceDto>[] = [
    {
      header: 'الحساب',
      cell: (row) => (
        <span className="font-mono font-medium">
          {row.accountCode} - {row.accountName}
        </span>
      ),
    },
    { header: 'العملة', cell: (row) => row.currencyCode },
    { header: 'افتتاحي مدين', align: 'right', cell: (row) => <MoneyDisplay value={row.openingDebit ?? 0} /> },
    { header: 'افتتاحي دائن', align: 'right', cell: (row) => <MoneyDisplay value={row.openingCredit ?? 0} /> },
    { header: 'دوران مدين', align: 'right', cell: (row) => <MoneyDisplay value={row.debit ?? 0} /> },
    { header: 'دوران دائن', align: 'right', cell: (row) => <MoneyDisplay value={row.credit ?? 0} /> },
    { header: 'ختامي مدين', align: 'right', cell: (row) => <MoneyDisplay value={row.closingDebit ?? 0} /> },
    { header: 'ختامي دائن', align: 'right', cell: (row) => <MoneyDisplay value={row.closingCredit ?? 0} /> },
    { header: 'الاتجاه', cell: (row) => directionLabels[row.balanceDirection ?? ''] ?? row.balanceDirection },
    {
      header: 'الحالة',
      cell: (row) =>
        row.isFinalized ? (
          <StatusBadge variant="closed">مغلقة</StatusBadge>
        ) : (
          <StatusBadge variant="draft">مفتوحة</StatusBadge>
        ),
    },
    {
      header: 'إجراءات',
      cell: (row) => {
        const canAct = row.isFinalized ? canUnfinalize : canFinalize;
        if (!canAct) return null;
        return row.isFinalized ? (
          <Button
            variant="secondary"
            size="sm"
            disabled={actionPending}
            onClick={() =>
              setPendingAction({
                fiscalYearId: row.fiscalYearId ?? 0,
                fiscalPeriodId: row.fiscalPeriodId ?? 0,
                type: 'unfinalize',
              })
            }
          >
            فتح
          </Button>
        ) : (
          <Button
            variant="secondary"
            size="sm"
            disabled={actionPending}
            onClick={() =>
              setPendingAction({
                fiscalYearId: row.fiscalYearId ?? 0,
                fiscalPeriodId: row.fiscalPeriodId ?? 0,
                type: 'finalize',
              })
            }
          >
            إغلاق
          </Button>
        );
      },
    },
  ];

  const hasFilters = useMemo(
    () => Boolean(fiscalYearId || fiscalPeriodId || accountId),
    [fiscalYearId, fiscalPeriodId, accountId],
  );

  return (
    <div className="space-y-5">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">أرصدة الحسابات</h1>
          <p className="text-body-sm text-[var(--color-on-surface-variant)] mt-1">أرصدة الحسابات حسب السنة والفترة المالية</p>
        </div>
        {canRebuild && fiscalYearId && fiscalPeriodId && (
          <Button
            variant="secondary"
            disabled={actionPending}
            onClick={() =>
              setPendingAction({
                fiscalYearId: Number(fiscalYearId),
                fiscalPeriodId: Number(fiscalPeriodId),
                type: 'rebuild',
              })
            }
          >
            إعادة بناء الأرصدة
          </Button>
        )}
      </div>

      <FilterBar
        hasFilters={hasFilters}
        onClear={() => {
          setFiscalYearId('');
          setFiscalPeriodId('');
          setAccountId('');
        }}
      >
        <FilterSelect
          label="السنة المالية"
          value={fiscalYearId}
          onChange={(v) => {
            setFiscalYearId(v);
            setFiscalPeriodId('');
          }}
          options={yearOptions}
          placeholder="اختر السنة المالية"
        />
        <FilterSelect
          label="الفترة المالية"
          value={fiscalPeriodId}
          onChange={setFiscalPeriodId}
          options={periodOptions}
          placeholder="اختر الفترة المالية"
          disabled={!fiscalYearId}
        />
        <FilterSelect
          label="الحساب"
          value={accountId}
          onChange={setAccountId}
          options={accountOptions}
          placeholder="جميع الحسابات"
        />
      </FilterBar>

      {!fiscalYearId || !fiscalPeriodId ? (
        <div className="text-center py-12 border-2 border-dashed border-[var(--color-border-container)] rounded-xl">
          <p className="text-[var(--color-on-surface-variant)]">اختر السنة والفترة المالية لعرض الأرصدة</p>
        </div>
      ) : (
        <DataGrid
          columns={columns}
          data={balances}
          loading={isLoading}
          error={isError ? 'حدث خطأ أثناء تحميل الأرصدة' : undefined}
          onRetry={() => refetch()}
          emptyMessage="لا توجد أرصدة للفترة المحددة"
          rowKey={(row) => row.id}
        />
      )}

      <ConfirmDialog
        open={pendingAction !== null}
        onClose={() => setPendingAction(null)}
        onConfirm={handleConfirm}
        title={
          pendingAction?.type === 'finalize'
            ? 'إغلاق الفترة'
            : pendingAction?.type === 'rebuild'
              ? 'إعادة بناء الأرصدة'
              : 'فتح الفترة'
        }
        message={
          pendingAction?.type === 'finalize'
            ? 'هل أنت متأكد من إغلاق هذه الفترة؟ لن يُسمح بالترحيل عليها بعد الإغلاق.'
            : pendingAction?.type === 'rebuild'
              ? 'هل أنت متأكد من إعادة بناء الأرصدة؟ سيُعاد حسابها من القيود المرحّلة. هذا الإجراء خطير.'
              : 'هل أنت متأكد من فتح هذه الفترة؟ سيُسمح بالترحيل عليها مجدداً.'
        }
        confirmLabel={
          pendingAction?.type === 'finalize'
            ? 'إغلاق'
            : pendingAction?.type === 'rebuild'
              ? 'إعادة بناء'
              : 'فتح'
        }
        loading={actionPending}
      />
    </div>
  );
}
