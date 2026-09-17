import { useState } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';
import { Input } from '@/components/ui/Input';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { notify } from '@/features/notifications/notify';
import { getCheckById, clearCheck } from '@/features/treasury/checks/shared/client';

interface RevenueCheckClearDialogProps {
  open: boolean;
  onClose: () => void;
  checkId: number;
  onCompleted: () => void;
}

export function RevenueCheckClearDialog({
  open,
  onClose,
  checkId,
  onCompleted,
}: RevenueCheckClearDialogProps) {
  const queryClient = useQueryClient();
  const [clearedAt, setClearedAt] = useState(new Date().toISOString().slice(0, 16));
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
      await clearCheck(checkId, {
        clearedAt: new Date(clearedAt).toISOString(),
        rowVersion,
      });
    },
    onSuccess: () => {
      notify({ type: 'success', title: 'تم تحصيل الشيك بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['checks'] });
      handleClose();
      onCompleted();
    },
    onError: (err: Error) => {
      notify({ type: 'error', title: err.message || 'حدث خطأ أثناء تحصيل الشيك' });
    },
  });

  function handleClose() {
    setClearedAt(new Date().toISOString().slice(0, 16));
    setErrors({});
    onClose();
  }

  function validate() {
    if (!clearedAt) {
      setErrors({ clearedAt: 'تاريخ التحصيل مطلوب' });
      return false;
    }
    if (check) {
      const clearedDate = new Date(clearedAt);
      const checkDate = new Date(check.checkDate);
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      if (clearedDate < checkDate) {
        setErrors({ clearedAt: 'تاريخ التحصيل لا يمكن أن يسبق تاريخ الشيك' });
        return false;
      }
      if (clearedDate > today) {
        setErrors({ clearedAt: 'تاريخ التحصيل لا يمكن أن يكون في المستقبل' });
        return false;
      }
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
      title="تحصيل الشيك"
      footer={
        <>
          <Button variant="ghost" onClick={handleClose}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            loading={mutation.isPending}
            onClick={handleSubmit}
          >
            تحصيل
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4" aria-label="تحصيل الشيك">
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

        <FormField label="تاريخ التحصيل" error={errors.clearedAt}>
          <Input
            type="datetime-local"
            value={clearedAt}
            onChange={(e) => setClearedAt(e.target.value)}
          />
        </FormField>

        <p className="text-xs text-[var(--color-on-surface-variant)]">
          سيتم إنشاء قيد محاسبي: دائن حساب الإيراد / مدين حساب البنك
        </p>
      </form>
    </Dialog>
  );
}
