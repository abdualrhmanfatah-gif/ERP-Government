import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { useOrganizationalUnits } from '@/features/organization/hooks';
import type { OrganizationalUnitDto, CreateOrgUnitCommand } from '@/features/organization/types';

interface OrgUnitFormProps {
  initialData?: OrganizationalUnitDto;
  isEdit?: boolean;
  onSubmit: (data: CreateOrgUnitCommand) => void;
  serverError?: string;
  loading?: boolean;
}

export function OrgUnitForm({ initialData, isEdit, onSubmit, serverError, loading }: OrgUnitFormProps) {
  const { data: orgUnits = [] } = useOrganizationalUnits();
  const [code, setCode] = useState(initialData?.code ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [parentId, setParentId] = useState(initialData?.parentId?.toString() ?? '');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      code,
      name,
      parentId: parentId ? parseInt(parentId, 10) : undefined,
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
        <label htmlFor="parentId" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الوحدة الأب</label>
        <select
          id="parentId"
          value={parentId}
          onChange={(e) => setParentId(e.target.value)}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        >
          <option value="">— لا يوجد —</option>
          {orgUnits.map(ou => (
            <option key={ou.id} value={ou.id}>{ou.name}</option>
          ))}
        </select>
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
