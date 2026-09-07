import { useState } from 'react';
import { Button } from '@/components/ui/Button';
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
    <form onSubmit={handleSubmit} className="space-y-4">
      {serverError && (
        <div className="p-3 bg-[var(--color-error-container)] text-[var(--color-on-error-container)] rounded-lg text-sm">{serverError}</div>
      )}

      <div>
        <label htmlFor="employeeNumber" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">رقم الموظف <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
        <input
          id="employeeNumber"
          type="text"
          value={employeeNumber}
          onChange={(e) => setEmployeeNumber(e.target.value)}
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
        <label htmlFor="organizationalUnitId" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الوحدة التنظيمية <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
        <select
          id="organizationalUnitId"
          value={organizationalUnitId}
          onChange={(e) => setOrganizationalUnitId(e.target.value)}
          required
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        >
          <option value="">— اختر —</option>
          {orgUnits.map((u) => (
            <option key={u.id} value={u.id}>{u.name}</option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="jobTitle" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">المسمى الوظيفي <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
          <input
            id="jobTitle"
            type="text"
            value={jobTitle}
            onChange={(e) => setJobTitle(e.target.value)}
            required
            maxLength={200}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          />
        </div>
        <div>
          <label htmlFor="jobGrade" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الدرجة الوظيفية</label>
          <input
            id="jobGrade"
            type="text"
            value={jobGrade}
            onChange={(e) => setJobGrade(e.target.value)}
            maxLength={50}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="hireDate" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">تاريخ التعيين <span aria-hidden="true" className="text-[var(--color-error)]">*</span></label>
          <input
            id="hireDate"
            type="date"
            value={hireDate}
            onChange={(e) => setHireDate(e.target.value)}
            required
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          />
        </div>
        <div>
          <label htmlFor="employmentStatus" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الحالة الوظيفية</label>
          <select
            id="employmentStatus"
            value={employmentStatus}
            onChange={(e) => setEmploymentStatus(e.target.value)}
            className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          >
            {employmentStatusOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
