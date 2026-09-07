import { useState, useEffect } from 'react';
import { useOrganizationalUnits } from '@/features/organization/hooks';
import type { CostCenterDto, CreateCostCenterCommand } from '@/features/organization/types';

interface CostCenterFormProps {
  initialData?: CostCenterDto;
  isEdit?: boolean;
  onSubmit: (data: CreateCostCenterCommand) => void;
  serverError?: string;
  loading?: boolean;
  id?: string;
}

export function CostCenterForm({ initialData, isEdit, onSubmit, serverError, id }: CostCenterFormProps) {
  const [code, setCode] = useState(initialData?.code ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [organizationUnitId, setOrganizationUnitId] = useState(initialData?.organizationUnitId?.toString() ?? '');
  const [budgetLimit, setBudgetLimit] = useState(initialData?.budgetLimit?.toString() ?? '');
  const [errors, setErrors] = useState<{ code?: string; name?: string }>({});
  
  const { data: orgUnits = [], isLoading: orgUnitsLoading } = useOrganizationalUnits();

  useEffect(() => {
    setCode(initialData?.code ?? '');
    setName(initialData?.name ?? '');
    setOrganizationUnitId(initialData?.organizationUnitId?.toString() ?? '');
    setBudgetLimit(initialData?.budgetLimit?.toString() ?? '');
    setErrors({});
  }, [initialData]);

  const validate = () => {
    const newErrors: { code?: string; name?: string } = {};
    if (!code.trim()) newErrors.code = 'الكود مطلوب';
    if (!name.trim()) newErrors.name = 'الاسم مطلوب';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    onSubmit({
      code: code.trim(),
      name: name.trim(),
      organizationUnitId: organizationUnitId ? parseInt(organizationUnitId, 10) : undefined,
      budgetLimit: budgetLimit ? parseFloat(budgetLimit) : undefined,
    });
  };

  return (
    <form id={id} onSubmit={handleSubmit} className="space-y-4" noValidate>
      {serverError && (
        <div role="alert" className="p-3 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded-lg text-sm">{serverError}</div>
      )}

      <div>
        <label htmlFor="code" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">
          الكود <span aria-hidden="true" className="text-[var(--color-error)]">*</span>
        </label>
        <input
          id="code"
          type="text"
          value={code}
          onChange={(e) => { setCode(e.target.value); if (errors.code) setErrors({ ...errors, code: undefined }); }}
          disabled={isEdit}
          required
          maxLength={50}
          aria-invalid={!!errors.code}
          aria-describedby={errors.code ? 'code-error' : undefined}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] disabled:opacity-50"
          dir="ltr"
        />
        {errors.code && <p id="code-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.code}</p>}
      </div>

      <div>
        <label htmlFor="name" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">
          الاسم <span aria-hidden="true" className="text-[var(--color-error)]">*</span>
        </label>
        <input
          id="name"
          type="text"
          value={name}
          onChange={(e) => { setName(e.target.value); if (errors.name) setErrors({ ...errors, name: undefined }); }}
          required
          maxLength={200}
          aria-invalid={!!errors.name}
          aria-describedby={errors.name ? 'name-error' : undefined}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        />
        {errors.name && <p id="name-error" role="alert" className="text-xs text-[var(--color-error)] mt-1">{errors.name}</p>}
      </div>

      <div>
        <label htmlFor="organizationUnitId" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">
          الوحدة التنظيمية
        </label>
        <select
          id="organizationUnitId"
          value={organizationUnitId}
          onChange={(e) => setOrganizationUnitId(e.target.value)}
          disabled={orgUnitsLoading}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] disabled:opacity-50"
        >
          <option value="">— لا يوجد —</option>
          {orgUnits.map((unit) => (
            <option key={unit.id} value={unit.id}>{unit.name}</option>
          ))}
        </select>
        {orgUnitsLoading && <span className="text-xs text-[var(--color-on-surface-variant)] mt-1">جاري تحميل الوحدات...</span>}
      </div>

      <div>
        <label htmlFor="budgetLimit" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">
          حد الميزانية
        </label>
        <input
          id="budgetLimit"
          type="number"
          value={budgetLimit}
          onChange={(e) => setBudgetLimit(e.target.value)}
          min={0}
          step={0.01}
          dir="ltr"
          placeholder="0.00"
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)] tabular-nums"
        />
      </div>
    </form>
  );
}
