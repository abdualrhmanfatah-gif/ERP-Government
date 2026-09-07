import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import type { BudgetItemTreeDto } from '@/web-api-client';

interface BudgetItemTreeProps {
  nodes: BudgetItemTreeDto[];
  selectedId?: number;
  onSelect?: (id: number) => void;
  onAddChild?: (parentId: number | undefined) => void;
  canEdit?: boolean;
}

function TreeNode({
  node,
  depth,
  selectedId,
  onSelect,
  onAddChild,
  canEdit,
}: {
  node: BudgetItemTreeDto;
  depth: number;
  selectedId?: number;
  onSelect?: (id: number) => void;
  onAddChild?: (parentId: number | undefined) => void;
  canEdit?: boolean;
}) {
  const [expanded, setExpanded] = useState(depth < 2);
  const hasChildren = (node.children?.length ?? 0) > 0;

  return (
    <li>
      <div
        role="treeitem"
        aria-expanded={hasChildren ? expanded : undefined}
        aria-selected={selectedId === node.id}
        tabIndex={0}
        onKeyDown={(e) => {
          if ((e.key === 'Enter' || e.key === ' ') && node.id !== undefined) {
            e.preventDefault();
            onSelect?.(node.id);
          }
        }}
        className={`flex items-center gap-2 rounded px-2 py-1.5 text-sm outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)] ${
          selectedId === node.id ? 'bg-[var(--color-primary-container)]' : 'hover:bg-[var(--color-surface-variant)]'
        }`}
        style={{ paddingInlineStart: `${depth * 1.25 + 0.5}rem` }}
      >
        {hasChildren ? (
          <button
            type="button"
            aria-label={expanded ? 'طي' : 'توسيع'}
            onClick={() => setExpanded((v) => !v)}
            className="w-5 shrink-0 text-center"
          >
            {expanded ? '▾' : '▸'}
          </button>
        ) : (
          <span className="w-5 shrink-0" aria-hidden="true" />
        )}
        <button
          type="button"
          className="flex-1 text-start"
          onClick={() => node.id !== undefined && onSelect?.(node.id)}
        >
          <span className="font-medium">{node.itemCode}</span>
          <span className="ms-2 text-[var(--color-on-surface-variant)]">{node.itemName}</span>
        </button>
        <span className="text-xs text-[var(--color-on-surface-variant)]">مستوى {node.level}</span>
        {node.isActive === false ? <StatusBadge variant="closed" size="sm">معطل</StatusBadge> : null}
        {canEdit && node.id !== undefined ? (
          <Button variant="ghost" size="sm" onClick={() => onAddChild?.(node.id)}>
            + فرعي
          </Button>
        ) : null}
      </div>
      {hasChildren && expanded ? (
        <ul role="group">
          {node.children!.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              depth={depth + 1}
              selectedId={selectedId}
              onSelect={onSelect}
              onAddChild={onAddChild}
              canEdit={canEdit}
            />
          ))}
        </ul>
      ) : null}
    </li>
  );
}

export function BudgetItemTree(props: BudgetItemTreeProps) {
  const { nodes, onAddChild, canEdit } = props;

  if (nodes.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-[var(--color-outline)] p-6 text-center">
        <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد بنود بعد</p>
        {canEdit ? (
          <Button variant="primary" size="sm" className="mt-2" onClick={() => onAddChild?.(undefined)}>
            إضافة بند رئيسي
          </Button>
        ) : null}
      </div>
    );
  }

  return (
    <ul role="tree" aria-label="شجرة بنود الموازنة" className="space-y-0.5">
      {nodes.map((node) => (
        <TreeNode key={node.id} node={node} depth={0} {...props} />
      ))}
    </ul>
  );
}
