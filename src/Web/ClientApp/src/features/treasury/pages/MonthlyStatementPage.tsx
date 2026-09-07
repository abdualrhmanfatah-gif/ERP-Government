// Monthly statement — US4: month + fund selectors, summary, vouchers + clearings tables
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, MoneyDisplay, Loading, EmptyState, Input, Select } from '@/components/ui';
import { DepositSlipsClient, FundsClient } from '../../../web-api-client';

const slipClient = new DepositSlipsClient();
const fundsClient = new FundsClient();

export default function MonthlyStatementPage() {
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [fundId, setFundId] = useState<number | undefined>();

  const { data: funds = [] } = useQuery({
    queryKey: ['funds'],
    queryFn: () => fundsClient.fundsAll(),
  });

  const { data: statement, isLoading } = useQuery({
    queryKey: ['monthly-statement', year, month, fundId],
    queryFn: () => slipClient.monthlyStatement(year, month, fundId!),
    enabled: fundId != null,
  });

  const hasData = statement && (
    (statement.summary?.totalDeposited ?? 0) > 0 ||
    (statement.summary?.totalCleared ?? 0) > 0 ||
    (statement.vouchers?.length ?? 0) > 0
  );

  return (
    <div className="space-y-6" dir="rtl">
      <PageHeader title="كشف حساب شهري" />

      <div className="flex gap-4 items-end">
        <Input
          label="السنة"
          type="number"
          value={year}
          onChange={(e) => setYear(Number(e.target.value))}
        />
        <Select
          label="الشهر"
          value={month}
          onChange={(e) => setMonth(Number(e.target.value))}
          options={Array.from({ length: 12 }, (_, i) => ({ value: String(i + 1), label: String(i + 1) }))}
        />
        <Select
          label="الصندوق"
          value={fundId ?? ''}
          onChange={(e) => setFundId(e.target.value ? Number(e.target.value) : undefined)}
          options={[
            { value: '', label: '— اختر الصندوق —' },
            ...funds.map((f: any) => ({ value: String(f.id), label: f.name })),
          ]}
        />
      </div>

      {fundId == null ? (
        <EmptyState message="اختر صندوقاً لعرض الكشف" />
      ) : isLoading ? (
        <Loading />
      ) : !hasData ? (
        <EmptyState message="لا توجد بيانات لهذا الشهر" />
      ) : (
        <>
          {/* Summary */}
          <div className="grid grid-cols-3 md:grid-cols-6 gap-4 text-sm border rounded p-4">
            <div>
              <span className="text-muted-foreground block">تحصيل نقدي</span>
              <MoneyDisplay value={statement!.summary?.totalCashCollections ?? 0} />
            </div>
            <div>
              <span className="text-muted-foreground block">تحصيل شيكات</span>
              <MoneyDisplay value={statement!.summary?.totalCheckCollections ?? 0} />
            </div>
            <div>
              <span className="text-muted-foreground block">مودع</span>
              <MoneyDisplay value={statement!.summary?.totalDeposited ?? 0} />
            </div>
            <div>
              <span className="text-muted-foreground block">تحت التحصيل</span>
              <MoneyDisplay value={statement!.summary?.totalUnderCollection ?? 0} />
            </div>
            <div>
              <span className="text-muted-foreground block">محصّل</span>
              <MoneyDisplay value={statement!.summary?.totalCleared ?? 0} />
            </div>
            <div>
              <span className="text-muted-foreground block">مرتجع</span>
              <MoneyDisplay value={statement!.summary?.totalBounced ?? 0} />
            </div>
          </div>

          {/* Vouchers table */}
          {(statement!.vouchers?.length ?? 0) > 0 && (
            <div>
              <h2 className="text-lg font-semibold mb-2">سندات الإيراد</h2>
              <div className="border rounded divide-y">
                <div className="flex gap-4 px-4 py-2 text-sm font-medium text-muted-foreground bg-muted/30">
                  <span className="w-24">الرقم</span>
                  <span className="w-32">التاريخ</span>
                  <span className="flex-1">المستلم</span>
                  <span className="w-20">الطريقة</span>
                  <span className="w-24 text-left">المبلغ</span>
                </div>
                {statement!.vouchers?.map((v: any) => (
                  <div key={v.id} className="flex gap-4 px-4 py-3 text-sm">
                    <span className="w-24 font-medium tabular-nums">{v.voucherNumber}</span>
                    <span className="w-32">{new Date(v.receivedDate).toLocaleDateString('ar-YE')}</span>
                    <span className="flex-1">{v.receivedFrom}</span>
                    <span className="w-20">{v.paymentMethodLabel}</span>
                    <span className="w-24 text-left">
                      <MoneyDisplay value={v.totalAmount ?? 0} />
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Clearings table */}
          {(statement!.clearings?.length ?? 0) > 0 && (
            <div>
              <h2 className="text-lg font-semibold mb-2">التحصيلات</h2>
              <div className="border rounded divide-y">
                <div className="flex gap-4 px-4 py-2 text-sm font-medium text-muted-foreground bg-muted/30">
                  <span className="w-24">رقم الشيك</span>
                  <span className="w-32">البنك</span>
                  <span className="flex-1">تاريخ التحصيل</span>
                  <span className="w-24 text-left">المبلغ</span>
                </div>
                {statement!.clearings?.map((c: any, i: number) => (
                  <div key={i} className="flex gap-4 px-4 py-3 text-sm">
                    <span className="w-24 tabular-nums">{c.checkNumber}</span>
                    <span className="w-32">{c.bankName}</span>
                    <span className="flex-1">{c.clearedAt ? new Date(c.clearedAt).toLocaleDateString('ar-YE') : '—'}</span>
                    <span className="w-24 text-left">
                      <MoneyDisplay value={c.amount ?? 0} />
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
