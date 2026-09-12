import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PaymentOrderForm } from '@/components/PaymentOrderForm';
import { Button, Page } from '@/components/ui';
import { ArrowRight } from 'lucide-react';

export function PaymentOrderCreatePage() {
  const navigate = useNavigate();
  const [formState, setFormState] = useState({ canSave: false, isSaving: false });

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
