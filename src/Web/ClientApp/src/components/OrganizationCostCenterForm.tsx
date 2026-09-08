import { useState, useEffect, useRef } from 'react';
import { useOrganizationalUnits } from '@/features/organization/hooks';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
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
  const prevInitialDataRef = useRef(initialData);

  const { data: orgUnits = [], isLoading: orgUnitsLoading } = useOrganizationalUnits();

  useEffect(() => {
    if (prevInitialDataRef.current !== initialData) {
      prevInitialDataRef.current = initialData;
      setCode(initialData?.code ?? '');
      setName(initialData?.name ?? '');
      setOrganizationUnitId(initialData?.organizationUnitId?.toString() ?? '');
      setBudgetLimit(initialData?.budgetLimit?.toString() ?? '');
      setErrors({});
    }
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

      <Input
        label="الكود"
        id="code"
        type="text"
        value={code}
        onChange={(e) => { setCode(e.target.value); if (errors.code) setErrors({ ...errors, code: undefined }); }}
        disabled={isEdit}
        required
        maxLength={50}
        error={errors.code}
        dir="ltr"
      />

      <Input
        label="الاسم"
        id="name"
        type="text"
        value={name}
        onChange={(e) => { setName(e.target.value); if (errors.name) setErrors({ ...errors, name: undefined }); }}
        required
        maxLength={200}
        error={errors.name}
      />

      <Select
        label="الوحدة التنظيمية"
        id="organizationUnitId"
        value={organizationUnitId}
        onChange={(e) => setOrganizationUnitId(e.target.value)}
        disabled={orgUnitsLoading}
        options={[
          { value: '', label: '— لا يوجد —' },
          ...orgUnits.map((unit) => ({ value: String(unit.id), label: unit.name })),
        ]}
      />

      <Input
        label="حد الميزانية"
        id="budgetLimit"
        type="number"
        value={budgetLimit}
        onChange={(e) => setBudgetLimit(e.target.value)}
        min={0}
        step={0.01}
        dir="ltr"
        placeholder="0.00"
      />
    </form>
  );
}
