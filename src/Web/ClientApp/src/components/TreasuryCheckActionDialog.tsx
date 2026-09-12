import { useState } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';
import { Input } from '@/components/ui/Input';
import { notify } from '@/features/notifications/notify';
import { getCheckById, bounceCheck, clearCheck } from '@/features/treasury/checks/shared/client';

interface TreasuryCheckActionDialogProps {
  open: boolean;
  onClose: () => void;
  checkId: number;
  action: 'bounce' | 'clear';
  onCompleted: () => void;
}

const actionConfig = {
  bounce: {
    title: 'ارتجاع الشيك',
    submitLabel: 'ارتجاع',
    dateLabel: 'تاريخ الارتجاع',
    dateField: 'bouncedAt' as const,
    requireReason: true,
    reasonLabel: 'سبب الارتجاع *',
    reasonPlaceholder: 'عدم كفاية الرصيد',
    successTitle: 'تم ارتجاع الشيك بنجاح',
    errorPrefix: 'حدث خطأ أثناء ارتجاع الشİK',
    ariaLabel: 'ارتجاع الشيك',
  },
  clear: {
    title: 'تحصيل الشيك',
    submitLabel: 'تحصيل',
    dateLabel: 'تاريخ التحصيل',
    dateField: 'clearedAt' as const,
    requireReason: false,
    reasonLabel: '',
    reasonPlaceholder: '',
    successTitle: 'تم تحصيل الشيك بنجاح',
    errorPrefix: 'حدث خطأ أثناء تحصيل الشيك',
    ariaLabel: 'تحصيل الشيك',
  },
} as const;

export function TreasuryCheckActionDialog({
  open,
  onClose,
  checkId,
  action,
  onCompleted,
}: TreasuryCheckActionDialogProps) {
  const queryClient = useQueryClient();
  const config = actionConfig[action];
  const [dateValue, setDateValue] = useState(new Date().toISOString().slice(0, 16));
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
      const payload = { [config.dateField]: new Date(dateValue).toISOString(), rowVersion };
      if (action === 'bounce') {
        await bounceCheck(checkId, { ...payload, reason: reason || undefined });
      } else {
        await clearCheck(checkId, payload);
      }
    },
    onSuccess: () => {
      notify({ type: 'success', title: config.successTitle });
      queryClient.invalidateQueries({ queryKey: ['checks'] });
      handleClose();
      onCompleted();
    },
    onError: (err: Error) => {
      notify({ type: 'error', title: err.message || config.errorPrefix });
    },
  });

  function handleClose() {
    setDateValue(new Date().toISOString().slice(0, 16));
    setReason('');
    setErrors({});
    onClose();
  }

  function validate() {
    if (!dateValue) {
      setErrors({ dateValue: `${config.dateLabel} مطلوب` });
      return false;
    }
    if (action === 'bounce' && !reason.trim()) {
      setErrors({ reason: 'سبب الارتجاع مطلوب' });
      return false;
    }
    if (action === 'clear' && check) {
      const clearedDate = new Date(dateValue);
      const checkDate = new Date(check.checkDate);
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      if (clearedDate < checkDate) {
        setErrors({ dateValue: 'تاريخ التحصيل لا يمكن أن يسبق تاريخ الشيك' });
        return false;
      }
      if (clearedDate > today) {
        setErrors({ dateValue: 'تاريخ التحصيل لا يمكن أن يكون في المستقبل' });
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
      title={config.title}
      footer={
        <>
          <Button variant="ghost" onClick={handleClose}>
            إلغاء
          </Button>
          <Button
            variant={action === 'bounce' ? 'destructive' : 'primary'}
            loading={mutation.isPending}
            onClick={handleSubmit}
          >
            {config.submitLabel}
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4" aria-label={config.ariaLabel}>
        <FormField label={config.dateLabel} error={errors.dateValue}>
          <Input
            type="datetime-local"
            value={dateValue}
            onChange={(e) => setDateValue(e.target.value)}
          />
        </FormField>
        {config.requireReason && (
          <FormField label={config.reasonLabel} error={errors.reason}>
            <Input
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder={config.reasonPlaceholder}
              required
            />
          </FormField>
        )}
      </form>
    </Dialog>
  );
}
