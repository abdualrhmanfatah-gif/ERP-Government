import { useState, useEffect } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { FormField } from '@/components/ui/FormField';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { notify } from '@/features/notifications/notify';
import { getCheckById, replaceCheck } from '@/features/treasury/checks/shared/client';

interface TreasuryChecksReplaceDialogProps {
  open: boolean;
  onClose: () => void;
  checkId: number;
  onReplaced: () => void;
}

export function TreasuryChecksReplaceDialog({
  open,
  onClose,
  checkId,
  onReplaced,
}: TreasuryChecksReplaceDialogProps) {
  const queryClient = useQueryClient();
  const [paymentMethod, setPaymentMethod] = useState<string>('Cash');
  const [voucherDate, setVoucherDate] = useState(new Date().toISOString().slice(0, 10));
  const [bankName, setBankName] = useState('');
  const [checkNumber, setCheckNumber] = useState('');
  const [checkDate, setCheckDate] = useState(new Date().toISOString().slice(0, 10));
  const [checkAmount, setCheckAmount] = useState(0);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const { data: check } = useQuery({
    queryKey: ['checks', checkId],
    queryFn: () => getCheckById(checkId),
    enabled: open && checkId > 0,
  });

  useEffect(() => {
    if (check && open) {
      setCheckAmount(check.amount ?? 0);
    }
  }, [check, open]);

  const mutation = useMutation({
    mutationFn: async () => {
      if (!check) throw new Error('Check not loaded');
      const rowVersion = (check as any).rowVersion ?? '';
      await replaceCheck(checkId, {
        paymentMethod,
        voucherDate,
        checkDetails:
          paymentMethod === 'Check'
            ? { bankName, checkNumber, checkDate, amount: checkAmount }
            : undefined,
        rowVersion,
      });
    },
    onSuccess: () => {
      notify({ type: 'success', title: 'تم استبدال الشيك بنجاح' });
      queryClient.invalidateQueries({ queryKey: ['checks'] });
      handleClose();
      onReplaced();
    },
    onError: (err: Error) => {
      notify({ type: 'error', title: err.message || 'حدث خطأ أثناء استبدال الشيك' });
    },
  });

  function handleClose() {
    setPaymentMethod('Cash');
    setVoucherDate(new Date().toISOString().slice(0, 10));
    setBankName('');
    setCheckNumber('');
    setCheckDate(new Date().toISOString().slice(0, 10));
    setCheckAmount(0);
    setErrors({});
    onClose();
  }

  function validate() {
    const newErrors: Record<string, string> = {};
    if (!voucherDate) newErrors.voucherDate = 'تاريخ السند مطلوب';
    if (paymentMethod === 'Check') {
      if (!bankName) newErrors.bankName = 'اسم البنك مطلوب';
      if (!checkNumber) newErrors.checkNumber = 'رقم الشيك مطلوب';
      if (!checkDate) newErrors.checkDate = 'تاريخ الشيك مطلوب';
      if (checkAmount <= 0) newErrors.checkAmount = 'مبلغ الشيك مطلوب';
    }
    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
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
      title="استبدال الشيك"
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
            استبدال
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="طريقة الدفع" error={errors.paymentMethod}>
          <Select
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value)}
          >
            <option value="Cash">نقدي</option>
            <option value="Check">شيك</option>
          </Select>
        </FormField>

        <FormField label="تاريخ السند" error={errors.voucherDate}>
          <Input
            type="date"
            value={voucherDate}
            onChange={(e) => setVoucherDate(e.target.value)}
          />
        </FormField>

        {paymentMethod === 'Check' && (
          <>
            <FormField label="اسم البنك" error={errors.bankName}>
              <Input
                value={bankName}
                onChange={(e) => setBankName(e.target.value)}
              />
            </FormField>
            <FormField label="رقم الشيك" error={errors.checkNumber}>
              <Input
                value={checkNumber}
                onChange={(e) => setCheckNumber(e.target.value)}
              />
            </FormField>
            <FormField label="تاريخ الشيك" error={errors.checkDate}>
              <Input
                type="date"
                value={checkDate}
                onChange={(e) => setCheckDate(e.target.value)}
              />
            </FormField>
            <FormField label="مبلغ الشيك" error={errors.checkAmount}>
              <Input
                type="number"
                min="0"
                step="0.01"
                value={checkAmount || ''}
                onChange={(e) => setCheckAmount(Number(e.target.value))}
                placeholder="0"
              />
            </FormField>
          </>
        )}
      </form>
    </Dialog>
  );
}
