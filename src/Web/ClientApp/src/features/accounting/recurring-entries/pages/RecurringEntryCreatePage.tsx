import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateRecurringEntry } from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS } from '../shared/types';
import type { CreateRecurringEntryCommand } from '../shared/types';
import { Page, Button, Input, Select } from '@/components/ui';

export default function RecurringEntryCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateRecurringEntry();

  const [form, setForm] = useState<CreateRecurringEntryCommand>({
    journalId: 0,
    name: '',
    frequency: 'Monthly',
    startDate: new Date(),
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createMutation.mutateAsync(form);
    navigate('/accounting/recurring-entries');
  };

  return (
    <Page title="إنشاء جدول دوري جديد" maxWidth="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="اسم الجدول"
          type="text"
          value={form.name}
          onChange={(e) => setForm({ ...form, name: e.target.value })}
          required
        />

        <Input
          label="رقم الدفتر"
          type="number"
          value={form.journalId || ''}
          onChange={(e) => setForm({ ...form, journalId: Number(e.target.value) })}
          required
        />

        <Select
          label="الدورية"
          required
          value={form.frequency}
          onChange={(e) => setForm({ ...form, frequency: e.target.value as any })}
          options={Object.entries(FREQUENCY_LABELS).map(([key, label]) => ({ value: key, label }))}
        />

        <Input
          label="تاريخ البداية"
          type="date"
          value={form.startDate instanceof Date ? form.startDate.toISOString().split('T')[0] : ''}
          onChange={(e) => setForm({ ...form, startDate: new Date(e.target.value) })}
          required
        />

        <Input
          label="تاريخ النهاية"
          type="date"
          value={form.endDate instanceof Date ? form.endDate.toISOString().split('T')[0] : ''}
          onChange={(e) => setForm({ ...form, endDate: e.target.value ? new Date(e.target.value) : undefined })}
        />

        <Input
          label="المبلغ"
          type="number"
          value={form.amount || ''}
          onChange={(e) => setForm({ ...form, amount: e.target.value ? Number(e.target.value) : undefined })}
        />

        <Input
          label="رقم القالب"
          type="number"
          value={form.templateId || ''}
          onChange={(e) => setForm({ ...form, templateId: e.target.value ? Number(e.target.value) : undefined })}
        />

        <div className="flex gap-2 justify-end pt-4">
          <Button variant="outline" type="button" onClick={() => navigate(-1)}>
            إلغاء
          </Button>
          <Button variant="primary" type="submit" loading={createMutation.isPending}>
            إنشاء
          </Button>
        </div>
      </form>
    </Page>
  );
}
