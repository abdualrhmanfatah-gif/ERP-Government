import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
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
    <form onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج المشروع">
      {serverError && (
        <div role="alert" className="p-3 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded-lg text-sm">{serverError}</div>
      )}

      <Input
        label="الكود"
        id="code"
        type="text"
        value={code}
        onChange={(e) => setCode(e.target.value)}
        disabled={isEdit}
        required
        maxLength={50}
        dir="ltr"
      />

      <Input
        label="الاسم"
        id="name"
        type="text"
        value={name}
        onChange={(e) => setName(e.target.value)}
        required
        maxLength={200}
      />

      <Select
        label="مركز التكلفة"
        id="costCenterId"
        value={costCenterId}
        onChange={(e) => setCostCenterId(e.target.value)}
        options={[
          { value: '', label: '— لا يوجد —' },
          ...costCenters.map((cc) => ({ value: String(cc.id), label: cc.name })),
        ]}
      />

      <div className="grid grid-cols-2 gap-4">
        <Input
          label="تاريخ البدء"
          id="startDate"
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
        />
        <Input
          label="تاريخ الانتهاء"
          id="endDate"
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
        />
      </div>

      <Input
        label="المبلغ المخصص"
        id="budgetAmount"
        type="number"
        value={budgetAmount}
        onChange={(e) => setBudgetAmount(e.target.value)}
        min={0}
        step={0.01}
        dir="ltr"
      />

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
