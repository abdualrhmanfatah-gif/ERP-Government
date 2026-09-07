import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateRecurringEntry } from '../hooks/useRecurringEntries';
import { FREQUENCY_LABELS } from '../shared/types';
import type { CreateRecurringEntryCommand } from '../shared/types';

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
    <div className="max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">إنشاء جدول دوري جديد</h1>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">اسم الجدول *</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="w-full border rounded-lg px-3 py-2"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">رقم الدفتر *</label>
          <input
            type="number"
            value={form.journalId || ''}
            onChange={(e) => setForm({ ...form, journalId: Number(e.target.value) })}
            className="w-full border rounded-lg px-3 py-2"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">الدورية *</label>
          <select
            value={form.frequency}
            onChange={(e) => setForm({ ...form, frequency: e.target.value as any })}
            className="w-full border rounded-lg px-3 py-2"
          >
            {Object.entries(FREQUENCY_LABELS).map(([key, label]) => (
              <option key={key} value={key}>{label}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">تاريخ البداية *</label>
          <input
            type="date"
            value={form.startDate instanceof Date ? form.startDate.toISOString().split('T')[0] : ''}
            onChange={(e) => setForm({ ...form, startDate: new Date(e.target.value) })}
            className="w-full border rounded-lg px-3 py-2"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">تاريخ النهاية</label>
          <input
            type="date"
            value={form.endDate instanceof Date ? form.endDate.toISOString().split('T')[0] : ''}
            onChange={(e) => setForm({ ...form, endDate: e.target.value ? new Date(e.target.value) : undefined })}
            className="w-full border rounded-lg px-3 py-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">المبلغ</label>
          <input
            type="number"
            value={form.amount || ''}
            onChange={(e) => setForm({ ...form, amount: e.target.value ? Number(e.target.value) : undefined })}
            className="w-full border rounded-lg px-3 py-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium mb-1">رقم القالب</label>
          <input
            type="number"
            value={form.templateId || ''}
            onChange={(e) => setForm({ ...form, templateId: e.target.value ? Number(e.target.value) : undefined })}
            className="w-full border rounded-lg px-3 py-2"
          />
        </div>

        <div className="flex gap-2 justify-end pt-4">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="px-4 py-2 border rounded-lg"
          >
            إلغاء
          </button>
          <button
            type="submit"
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            إنشاء
          </button>
        </div>
      </form>
    </div>
  );
}
