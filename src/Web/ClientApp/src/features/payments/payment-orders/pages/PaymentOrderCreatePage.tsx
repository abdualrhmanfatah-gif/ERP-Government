import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { PaymentOrderForm } from '@/components/PaymentOrderForm';
import { Button, Page } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export function PaymentOrderCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [formState, setFormState] = useState({ canSave: false, isSaving: false });
  const accrualJournalEntryId = Number(searchParams.get('accrualJournalEntryId') ?? 0);
  const amountGross = Number(searchParams.get('amountGross') ?? 0);
  const currencyId = Number(searchParams.get('currencyId') ?? 0);
  const fiscalYearId = Number(searchParams.get('fiscalYearId') ?? 0);
  const beneficiaryName = searchParams.get('beneficiaryName') ?? '';
  const requestNumber = searchParams.get('requestNumber') ?? '';

  return (
    <Page
      title="أمر دفع جديد"
      maxWidth="lg"
      actions={
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)} aria-label="العودة">
          <ArrowRight size={18} />
        </Button>
      }
    >
      <PaymentOrderForm
        mode="create"
        initialData={{
          id: 0,
          paymentOrderType: 'Standard',
          fiscalYearId: fiscalYearId || undefined,
          currencyId: currencyId || undefined,
          amountGross: amountGross || undefined,
          beneficiaryName,
          accrualJournalEntryId: accrualJournalEntryId || undefined,
          notes: requestNumber ? `أمر صرف مرتبط بطلب الصرف ${requestNumber}` : undefined,
        }}
        onStateChange={setFormState}
        onSaved={(id) => navigate(id ? `/payments/payment-orders/${id}` : '/payments/payment-orders')}
      />
      <div className="mt-4 flex justify-end gap-2">
        <Button variant="ghost" type="button" onClick={() => navigate(-1)} disabled={formState.isSaving}>
          إلغاء
        </Button>
        <Button
          variant="primary"
          type="submit"
          form="payment-order-form"
          disabled={!formState.canSave || formState.isSaving}
          loading={formState.isSaving}
        >
          حفظ
        </Button>
      </div>
    </Page>
  );
}
