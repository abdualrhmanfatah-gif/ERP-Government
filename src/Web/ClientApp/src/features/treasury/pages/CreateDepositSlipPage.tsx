// Create deposit slip — US1: formType toggle, slipDate picker, member batch, empty-start allowed
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Button, MoneyDisplay, EmptyState, Loading, Input, Select, Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { DepositSlipsClient, FormType } from '../../../web-api-client';
import { useEligibleVouchers } from '../hooks/useEligibleVouchers';

const client = new DepositSlipsClient();

const formTypeLabels: Record<string, string> = {
  Form47: 'نقدية (47)',
  Form48: 'شيكات (48)',
};

export default function CreateDepositSlipPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [formType, setFormType] = useState<FormType>(FormType.Form47);
  const [slipDate, setSlipDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [selectedIds, setSelectedIds] = useState<number[]>([]);

  const { eligible, isLoading } = useEligibleVouchers(
    formType === FormType.Form47 ? 'Form47' : 'Form48',
  );

  const createMutation = useMutation({
    mutationFn: () =>
      client.depositSlipsPOST({
        slipDate: new Date(slipDate),
        formType: formType as any,
        voucherIds: selectedIds,
      } as any),
    onSuccess: (id: number) => {
      queryClient.invalidateQueries({ queryKey: ['deposit-slips'] });
      notify({ type: 'success', title: 'تم إنشاء بطاقة الإيداع' });
      navigate(`/treasury/deposit-slips/${id}`);
    },
    onError: () => notify({ type: 'error', title: 'فشل إنشاء بطاقة الإيداع' }),
  });

  const total = useMemo(
    () =>
      eligible
        .filter((v) => v.id != null && selectedIds.includes(v.id))
        .reduce((sum, v) => sum + (v.totalAmount ?? 0), 0),
    [eligible, selectedIds],
  );

  const toggle = (id: number) =>
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  const selectAll = () => setSelectedIds(eligible.map((v) => v.id!).filter(Boolean));
  const clearAll = () => setSelectedIds([]);

  return (
    <Page title="إنشاء بطاقة إيداع" loading={isLoading}>

      <div className="flex gap-4 items-end">
        <Select
          label="نوع البطاقة"
          value={formType}
          onChange={(e) => {
            setFormType(e.target.value as FormType);
            setSelectedIds([]);
          }}
          options={Object.entries(formTypeLabels).map(([val, label]) => ({ value: val, label }))}
        />

        <Input
          label="تاريخ البطاقة"
          type="date"
          value={slipDate}
          onChange={(e) => setSlipDate(e.target.value)}
          max={new Date().toISOString().slice(0, 10)}
        />
      </div>

      {eligible.length === 0 ? (
        <EmptyState message="لا توجد سندات مؤهلة لهذا النوع" />
      ) : (
        <>
          <div className="flex gap-2 mb-2">
            <Button variant="outline" onClick={selectAll}>
              تحديد الكل
            </Button>
            <Button variant="outline" onClick={clearAll}>
              إلغاء التحديد
            </Button>
            <span className="mr-auto text-sm text-muted-foreground">
              {selectedIds.length} من {eligible.length}
            </span>
          </div>

          <div className="border rounded divide-y max-h-96 overflow-y-auto">
            {eligible.map((v) => (
              <label
                key={v.id}
                className="flex items-center gap-3 px-4 py-3 hover:bg-muted/50 cursor-pointer"
              >
                <input
                  type="checkbox"
                  checked={v.id != null && selectedIds.includes(v.id)}
                  onChange={() => toggle(v.id ?? 0)}
                />
                <span className="font-medium tabular-nums">{v.voucherNumber}</span>
                <span className="text-muted-foreground text-sm">{v.receivedFrom}</span>
                <span className="mr-auto">
                  <MoneyDisplay value={v.totalAmount ?? 0} />
                </span>
              </label>
            ))}
          </div>

          <div className="flex items-center justify-between border-t pt-4">
          <div className="text-lg font-semibold">
            الإجمالي (يُحسب خادمياً عند الحفظ): <MoneyDisplay value={total} />
          </div>
            <Button
              onClick={() => createMutation.mutate()}
              loading={createMutation.isPending}
            >
              إنشاء البطاقة
            </Button>
          </div>
        </>
      )}
    </Page>
  );
}
