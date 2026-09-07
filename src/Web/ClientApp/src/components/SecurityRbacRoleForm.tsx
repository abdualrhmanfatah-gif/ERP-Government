import { useState } from 'react';
import { Button } from '@/components/ui/Button';
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
        <label htmlFor="description" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">الوصف</label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          maxLength={500}
          rows={3}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        />
      </div>

      <div>
        <label htmlFor="roleLevel" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">مستوى الدور</label>
        <select
          id="roleLevel"
          value={roleLevel}
          onChange={(e) => setRoleLevel(e.target.value)}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
        >
          {roleLevelOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
      </div>

      <div>
        <label htmlFor="maxSessionDuration" className="block text-sm font-medium text-[var(--color-on-surface)] mb-1">مدة الجلسة القصوى (دقيقة)</label>
        <input
          id="maxSessionDuration"
          type="number"
          value={maxSessionDuration}
          onChange={(e) => setMaxSessionDuration(e.target.value)}
          min={1}
          className="w-full px-3 py-2 border border-[var(--color-border-container)] rounded-lg bg-[var(--color-surface-container-low)] text-[var(--color-on-surface)] focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
          dir="ltr"
        />
      </div>

      <div className="flex items-center gap-2">
        <input
          id="requiresMfa"
          type="checkbox"
          checked={requiresMfa}
          onChange={(e) => setRequiresMfa(e.target.checked)}
          className="rounded-lg border-[var(--color-border-container)]"
        />
        <label htmlFor="requiresMfa" className="text-sm font-medium text-[var(--color-on-surface)]">يتطلب مصادقة ثنائية</label>
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <Button type="submit" variant="primary" disabled={loading} loading={loading}>
          {loading ? 'جاري الحفظ...' : isEdit ? 'تحديث' : 'إنشاء'}
        </Button>
      </div>
    </form>
  );
}
