import { useState } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';
import { Input } from '@/components/ui/Input';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { notify } from '@/features/notifications/notify';
import { getCheckById, bounceCheck } from '@/features/treasury/checks/shared/client';

interface RevenueCheckBounceDialogProps {
  open: boolean;
  onClose: () => void;
  checkId: number;
  onCompleted: () => void;
}

export function RevenueCheckBounceDialog({
  open,
  onClose,
  checkId,
  onCompleted,
}: RevenueCheckBounceDialogProps) {
  const queryClient = useQueryClient();
  const [bouncedAt, setBouncedAt] = useState(new Date().toISOString().slice(0, 16));
  const [reason, setReason] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});

  const { data: check } = useQuery({
    queryKey: ['checks', checkId],
    queryFn: () => getCheckById(checkId),
    enabled: open && checkId > 0,
  });

  const mutation = useMutation({
    mutationFn: async () => {
      if (!check) throw new Error('Check not loaded');
      const rowVersion = (check as any).rowVersion ?? '';
      await bounceCheck(checkId, {
        bouncedAt: new Date(bouncedAt).toISOString(),
        reason: reason || undefined,
        rowVersion,
      });
    },
    onSuccess: () => {
      notify({ type: 'success', title: 'تم ارتجاع الشيك بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['checks'] });
      handleClose();
      onCompleted();
    },
    onError: (err: Error) => {
      notify({ type: 'error', title: err.message || 'حدث خطأ أثناء ارتجاع الشيك' });
    },
  });

  function handleClose() {
    setBouncedAt(new Date().toISOString().slice(0, 16));
    setReason('');
    setErrors({});
    onClose();
  }

  function validate() {
    if (!bouncedAt) {
      setErrors({ bouncedAt: 'تاريخ الارتجاع مطلوب' });
      return false;
    }
    if (!reason.trim()) {
      setErrors({ reason: 'سبب الارتجاع مطلوب' });
      return false;
    }
    setErrors({});
    return true;
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;
    mutation.mutate();
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      title="ارتجاع الشيك"
      footer={
        <>
          <Button variant="ghost" onClick={handleClose}>
            إلغاء
          </Button>
          <Button
            variant="destructive"
            loading={mutation.isPending}
            onClick={handleSubmit}
          >
            ارتجاع
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4" aria-label="ارتجاع الشيك">
        {check && (
          <div className="rounded bg-[var(--color-surface-container)] p-3 text-sm">
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">رقم الشيك:</span>
              <span className="font-medium tabular-nums">{check.checkNumber}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">البنك:</span>
              <span>{check.bankName}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-[var(--color-on-surface-variant)]">المبلغ:</span>
              <MoneyDisplay value={check.amount ?? 0} />
            </div>
          </div>
        )}

        <FormField label="تاريخ الارتجاع" error={errors.bouncedAt}>
          <Input
            type="datetime-local"
            value={bouncedAt}
            onChange={(e) => setBouncedAt(e.target.value)}
          />
        </FormField>

        <FormField label="سبب الارتجاع *" error={errors.reason}>
          <Input
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="عدم كفاية الرصيد"
            required
          />
        </FormField>

        <p className="text-xs text-[var(--color-on-surface-variant)]">
          سيتم إنشاء قيد محاسبي عكسي: دائن حساب المتحصلات المعلقة / مدين حساب شيكات تحت التحصيل
        </p>
      </form>
    </Dialog>
  );
}
