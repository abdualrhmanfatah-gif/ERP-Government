interface AuditInfoProps {
  createdAt?: string | Date;
  createdBy?: string;
  updatedAt?: string | Date;
  updatedBy?: string;
}

export function AuditInfo({
  createdAt,
  createdBy,
  updatedAt,
  updatedBy,
}: AuditInfoProps) {
  const formatDate = (d: string | Date | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  return (
    <div className="grid grid-cols-2 gap-4">
      <div>
        <span className="text-xs text-[color-mix(in_srgb,var(--color-outline)_60%,transparent)]">أنشأ</span>
        <p className="text-sm text-[var(--color-outline)]">{createdBy ?? '—'}</p>
      </div>
      <div>
        <span className="text-xs text-[color-mix(in_srgb,var(--color-outline)_60%,transparent)]">تاريخ الإنشاء</span>
        <p className="text-sm text-[var(--color-outline)]">{formatDate(createdAt)}</p>
      </div>
      <div>
        <span className="text-xs text-[color-mix(in_srgb,var(--color-outline)_60%,transparent)]">آخر تعديل</span>
        <p className="text-sm text-[var(--color-outline)]">{updatedBy ?? '—'}</p>
      </div>
      <div>
        <span className="text-xs text-[color-mix(in_srgb,var(--color-outline)_60%,transparent)]">تاريخ التعديل</span>
        <p className="text-sm text-[var(--color-outline)]">{formatDate(updatedAt)}</p>
      </div>
    </div>
  );
}
