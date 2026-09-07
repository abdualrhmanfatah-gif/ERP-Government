import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { useCostCenters } from '@/features/organization/hooks';
import type { ProjectDto, CreateProjectCommand } from '@/features/organization/types';

interface ProjectFormProps {
  initialData?: ProjectDto;
  isEdit?: boolean;
  onSubmit: (data: CreateProjectCommand) => void;
  serverError?: string;
  loading?: boolean;
}

export function ProjectForm({ initialData, isEdit, onSubmit, serverError, loading }: ProjectFormProps) {
  const { data: costCenters = [] } = useCostCenters();
  const [code, setCode] = useState(initialData?.code ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [costCenterId, setCostCenterId] = useState(initialData?.costCenterId?.toString() ?? '');
  const [startDate, setStartDate] = useState(initialData?.startDate ?? '');
  const [endDate, setEndDate] = useState(initialData?.endDate ?? '');
  const [budgetAmount, setBudgetAmount] = useState(initialData?.budgetAmount?.toString() ?? '');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      code,
      name,
      costCenterId: costCenterId ? parseInt(costCenterId, 10) : undefined,
      startDate: startDate || undefined,
      endDate: endDate || undefined,
      budgetAmount: budgetAmount ? parseFloat(budgetAmount) : undefined,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {serverError && (
        <div className="p-3 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded-lg text-sm">{serverError}</div>
      )}

      <div>
        <label htmlFor="code" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الكود <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
        <input
          id="code"
          type="text"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          disabled={isEdit}
          required
          maxLength={50}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] disabled:opacity-50"
          dir="ltr"
        />
      </div>

      <div>
        <label htmlFor="name" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الاسم <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
        <input
          id="name"
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          maxLength={200}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        />
      </div>

      <div>
        <label htmlFor="costCenterId" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">مركز التكلفة</label>
        <select
          id="costCenterId"
          value={costCenterId}
          onChange={(e) => setCostCenterId(e.target.value)}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        >
          <option value="">— لا يوجد —</option>
          {costCenters.map(cc => (
            <option key={cc.id} value={cc.id}>{cc.name}</option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="startDate" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">تاريخ البدء</label>
          <input
            id="startDate"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          />
        </div>
        <div>
          <label htmlFor="endDate" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">تاريخ الانتهاء</label>
          <input
            id="endDate"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          />
        </div>
      </div>

      <div>
        <label htmlFor="budgetAmount" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">المبلغ المخصص</label>
        <input
          id="budgetAmount"
          type="number"
          value={budgetAmount}
          onChange={(e) => setBudgetAmount(e.target.value)}
          min={0}
          step={0.01}
          dir="ltr"
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] tabular-nums"
        />
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
