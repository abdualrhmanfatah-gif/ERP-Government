import type { AccountGroupDto } from '@/features/accounting/account-groups/types';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Button } from '@/components/ui/Button';
import { Eye, Power, PowerOff, Pencil, ChevronRight } from 'lucide-react';

type Props = {
  groups: AccountGroupDto[];
  onSelect?: (g: AccountGroupDto) => void;
  onToggle?: (g: AccountGroupDto) => void;
  onEdit?: (g: AccountGroupDto) => void;
  canEdit?: boolean;
};

export function GroupTree({ groups, onSelect, onToggle, onEdit, canEdit }: Props) {
  return (
    <div className="flex flex-col">
      {groups.map((g, idx) => (
        <div
          key={g.id}
          className={`group flex items-center gap-3 border-e border-b border-[color-mix(in_srgb,var(--color-primary-container)_20%,transparent)] px-3 py-1.5 transition-all duration-150 hover:bg-[color-mix(in_srgb,var(--color-primary-container)_5%,transparent)] cursor-pointer ${idx % 2 === 1 ? 'bg-[var(--color-surface-container-low)]' : 'bg-[var(--color-surface-container-lowest)]'}`}
          style={{ marginInlineStart: `${(g.level - 1) * 24}px` }}
          onClick={() => onSelect?.(g)}
          onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onSelect?.(g); } }}
          tabIndex={0}
          role="button"
        >
          {g.level > 1 && (
            <ChevronRight className="h-4 w-4 text-[var(--color-on-surface-variant)] flex-shrink-0" />
          )}
          <span className="font-mono text-sm text-[var(--color-primary)] min-w-[60px]">
            {g.code}
          </span>
          <span className="flex-1 text-sm text-[var(--color-on-surface)]">
            {g.name}
          </span>
          <span className="text-xs text-[var(--color-on-surface-variant)] hidden sm:inline">
            {g.type} / {g.normalBalance}
          </span>
          <StatusBadge variant={g.isActive ? 'active' : 'closed'}>
            {g.isActive ? 'نشط' : 'معطل'}
          </StatusBadge>
          <span className="text-xs text-[var(--color-on-surface-variant)] hidden md:inline">
            مستوى {g.level}
          </span>
          <div
            className="flex gap-1 ms-2 opacity-0 group-hover:opacity-100 transition-opacity duration-150"
            onClick={(e) => e.stopPropagation()}
          >
            <Button variant="ghost" size="icon" aria-label="عرض" onClick={() => onSelect?.(g)}>
              <Eye className="h-4 w-4" />
            </Button>
            {canEdit && onEdit && (
              <Button variant="ghost" size="icon" aria-label="تعديل" onClick={() => onEdit(g)}>
                <Pencil className="h-4 w-4" />
              </Button>
            )}
            {canEdit && onToggle && (
              <Button variant="ghost" size="icon" aria-label={g.isActive ? 'تعطيل' : 'تفعيل'} onClick={() => onToggle(g)}>
                {g.isActive ? <PowerOff className="h-4 w-4" /> : <Power className="h-4 w-4" />}
              </Button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
