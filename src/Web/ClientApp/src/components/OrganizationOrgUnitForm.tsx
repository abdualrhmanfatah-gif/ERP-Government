import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
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
    <form onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الوحدة التنظيمية">
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
        label="الوحدة الأب"
        id="parentId"
        value={parentId}
        onChange={(e) => setParentId(e.target.value)}
        options={[
          { value: '', label: '— لا يوجد —' },
          ...orgUnits.map((ou) => ({ value: String(ou.id), label: ou.name })),
        ]}
      />

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
