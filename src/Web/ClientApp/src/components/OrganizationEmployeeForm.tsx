import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useOrganizationalUnits } from '@/features/organization/hooks';
import type { EmployeeDto, CreateEmployeeCommand, UpdateEmployeeCommand } from '@/features/organization/types';

const employmentStatusOptions = [
  { value: 'Active', label: 'نشط' },
  { value: 'Suspended', label: 'موقوف' },
  { value: 'Terminated', label: 'منتهي' },
];

interface EmployeeFormProps {
  initialData?: EmployeeDto;
  isEdit?: boolean;
  onSubmit: (data: CreateEmployeeCommand | UpdateEmployeeCommand) => void;
  serverError?: string;
  loading?: boolean;
}

export function EmployeeForm({ initialData, isEdit, onSubmit, serverError, loading }: EmployeeFormProps) {
  const { data: orgUnits = [] } = useOrganizationalUnits();
  const [employeeNumber, setEmployeeNumber] = useState(initialData?.employeeNumber ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [organizationalUnitId, setOrganizationalUnitId] = useState(initialData?.organizationalUnitId?.toString() ?? '');
  const [jobTitle, setJobTitle] = useState(initialData?.jobTitle ?? '');
  const [jobGrade, setJobGrade] = useState(initialData?.jobGrade ?? '');
  const [hireDate, setHireDate] = useState(initialData?.hireDate ?? '');
  const [employmentStatus, setEmploymentStatus] = useState(initialData?.employmentStatus ?? 'Active');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      employeeNumber,
      name,
      organizationalUnitId: parseInt(organizationalUnitId, 10),
      jobTitle,
      jobGrade: jobGrade || undefined,
      hireDate,
      employmentStatus,
    } as CreateEmployeeCommand | UpdateEmployeeCommand);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الموظف">
      {serverError && (
        <div role="alert" className="p-3 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded-lg text-sm">{serverError}</div>
      )}

      <Input
        label="رقم الموظف"
        id="employeeNumber"
        type="text"
        value={employeeNumber}
        onChange={(e) => setEmployeeNumber(e.target.value)}
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
        label="الوحدة التنظيمية"
        id="organizationalUnitId"
        value={organizationalUnitId}
        onChange={(e) => setOrganizationalUnitId(e.target.value)}
        required
        options={[
          { value: '', label: '— اختر —' },
          ...orgUnits.map((u) => ({ value: String(u.id), label: u.name })),
        ]}
      />

      <div className="grid grid-cols-2 gap-4">
        <Input
          label="المسمى الوظيفي"
          id="jobTitle"
          type="text"
          value={jobTitle}
          onChange={(e) => setJobTitle(e.target.value)}
          required
          maxLength={200}
        />
        <Input
          label="الدرجة الوظيفية"
          id="jobGrade"
          type="text"
          value={jobGrade}
          onChange={(e) => setJobGrade(e.target.value)}
          maxLength={50}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <Input
          label="تاريخ التعيين"
          id="hireDate"
          type="date"
          value={hireDate}
          onChange={(e) => setHireDate(e.target.value)}
          required
        />
        <Select
          label="الحالة الوظيفية"
          id="employmentStatus"
          value={employmentStatus}
          onChange={(e) => setEmploymentStatus(e.target.value)}
          options={employmentStatusOptions}
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
