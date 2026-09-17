import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AccountingJournalEntryForm } from '@/components/AccountingJournalEntryForm';
import { Button, Page } from '@/components/ui';

export function JournalEntryCreatePage() {
  const navigate = useNavigate();
  const [formState, setFormState] = useState({ canSave: false, isSaving: false });

  return (
    <Page
      title="إنشاء قيد يومية"
      description="أدخل بيانات القيد ثم أضف البيان"
      maxWidth="lg"
      actions={
        <>
          <Button
            type="submit"
            form="journal-entry-form"
            variant="primary"
            disabled={!formState.canSave || formState.isSaving}
            loading={formState.isSaving}
          >
            حفظ القيد
          </Button>
          <Button
            variant="ghost"
            onClick={() => navigate('/accounting/journal-entries')}
            disabled={formState.isSaving}
          >
            إلغاء
          </Button>
        </>
      }
    >
      <AccountingJournalEntryForm
        mode="create"
        onSuccess={(id) => navigate(`/accounting/journal-entries/${id}`)}
        onStateChange={setFormState}
      />
    </Page>
  );
}