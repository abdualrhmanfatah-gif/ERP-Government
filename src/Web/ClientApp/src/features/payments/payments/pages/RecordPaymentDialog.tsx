import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Label } from '@/components/ui/Label';
import { Select } from '@/components/ui/Select';
import { useRecordPayment } from '../hooks/usePayments';
import { PaymentMethod } from '../shared/types';
import { recordPaymentSchema } from '../shared/schemas';
import { notify } from '@/features/notifications/notify';

interface RecordPaymentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  paymentOrderId: number;
  amount: number;
}

export function RecordPaymentDialog({ open, onOpenChange, paymentOrderId, amount }: RecordPaymentDialogProps) {
  const [method, setMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [referenceNumber, setReferenceNumber] = useState('');
  const [notes, setNotes] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const recordPayment = useRecordPayment();

  const handleSubmit = async () => {
    const parsed = recordPaymentSchema.safeParse({
      paymentOrderId,
      paymentMethod: method,
      referenceNumber: referenceNumber || undefined,
      notes: notes || undefined,
    });

    if (!parsed.success) {
      const fieldErrors: Record<string, string> = {};
      parsed.error.issues.forEach((issue) => {
        const key = issue.path[0] as string;
        fieldErrors[key] = issue.message;
      });
      setErrors(fieldErrors);
      return;
    }

    setErrors({});
    try {
      await recordPayment.mutateAsync(parsed.data);
      notify({ type: 'success', title: 'تم تسجيل الدفع بنجاح' });
      onOpenChange(false);
    } catch (err: any) {
      console.error('[RecordPayment] Raw error:', err);
      console.error('[RecordPayment] err.response:', err?.response);
      let msg = 'فشل تسجيل الدفع';
      try {
        const raw = typeof err?.response === 'string' ? err.response : JSON.stringify(err?.response ?? err?.message ?? '');
        const body = JSON.parse(raw);
        msg = body?.errors?.[0] ?? body?.title ?? msg;
      } catch { /* keep default */ }
      notify({ type: 'error', title: msg });
    }
  };

  return (
    <Dialog
      open={open}
      onClose={() => onOpenChange(false)}
      title="تسجيل الدفع"
      footer={
        <>
          <Button variant="outline" onClick={() => onOpenChange(false)}>إلغاء</Button>
          <Button onClick={handleSubmit} disabled={recordPayment.isPending}>
            {recordPayment.isPending ? 'جاري التسجيل...' : 'تأكيد الدفع'}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <div>
          <Label>المبلغ</Label>
          <div className="text-lg font-bold">{amount.toLocaleString('ar-YE')} ريال</div>
        </div>
        <div>
          <Select
            label="طريقة الدفع"
            value={method}
            onChange={(e) => setMethod(e.target.value as PaymentMethod)}
            options={[
              { value: PaymentMethod.Cash, label: 'نقدي' },
              { value: PaymentMethod.Check, label: 'شيك' },
            ]}
          />
          {errors.paymentMethod && <p className="text-xs text-[var(--color-error)] mt-1">{errors.paymentMethod}</p>}
        </div>
        <div>
          <Label>رقم المرجع</Label>
          <Input value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} placeholder="اختياري" />
        </div>
        <div>
          <Label>ملاحظات</Label>
          <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="اختياري" />
        </div>
      </div>
    </Dialog>
  );
}
