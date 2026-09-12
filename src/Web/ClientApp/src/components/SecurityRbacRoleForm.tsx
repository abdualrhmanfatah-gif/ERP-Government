import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Textarea } from '@/components/ui/Textarea';
import { Switch } from '@/components/ui/Switch';
import type { SecurityRoleDto, CreateRoleCommand } from '@/features/security/rbac/types';

const roleLevelOptions = [
  { value: 'Organization', label: 'الهيكل التنظيمي' },
  { value: 'Department', label: 'القسم' },
  { value: 'Section', label: 'الشعبة' },
  { value: 'Custom', label: 'مخصص' },
];

interface RoleFormProps {
  initialData?: SecurityRoleDto;
  isEdit?: boolean;
  onSubmit: (data: CreateRoleCommand) => void;
  serverError?: string;
  loading?: boolean;
}

export function RoleForm({ initialData, isEdit, onSubmit, serverError, loading }: RoleFormProps) {
  const [code, setCode] = useState(initialData?.code ?? '');
  const [name, setName] = useState(initialData?.name ?? '');
  const [description, setDescription] = useState(initialData?.description ?? '');
  const [roleLevel, setRoleLevel] = useState(initialData?.roleLevel ?? 'Organization');
  const [requiresMfa, setRequiresMfa] = useState(initialData?.requiresMfa ?? false);
  const [maxSessionDuration, setMaxSessionDuration] = useState(initialData?.maxSessionDuration?.toString() ?? '');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({
      code,
      name,
      description: description || undefined,
      roleLevel,
      requiresMfa,
      maxSessionDuration: maxSessionDuration ? parseInt(maxSessionDuration, 10) : undefined,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4" aria-label="نموذج الدور">
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

      <Textarea
        label="الوصف"
        id="description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        maxLength={500}
        rows={3}
      />

      <Select
        label="مستوى الدور"
        id="roleLevel"
        value={roleLevel}
        onChange={(e) => setRoleLevel(e.target.value)}
        options={roleLevelOptions}
      />

      <Input
        label="مدة الجلسة القصوى (دقيقة)"
        id="maxSessionDuration"
        type="number"
        value={maxSessionDuration}
        onChange={(e) => setMaxSessionDuration(e.target.value)}
        min={1}
        dir="ltr"
      />

      <Switch
        id="requiresMfa"
        label="يتطلب مصادقة ثنائية"
        checked={requiresMfa}
        onChange={setRequiresMfa}
      />

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
