import { type ReactNode, useState } from 'react';
import { ChevronDown, Plus, FolderOpen } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { StatusBadge } from '@/components/ui/StatusBadge';
import type { BudgetItemDto } from '@/web-api-client';

interface BudgetItemTreeProps {
  nodes: BudgetItemDto[];
  selectedId?: number;
  onSelect?: (id: number) => void;
  onSelectItem?: (id: number) => void;
  onAddChild?: (parentId: number | undefined) => void;
  canEdit?: boolean;
  selectedItemDetail?: ReactNode;
}

function TreeNode({
  node,
  depth,
  selectedId,
  onSelect,
  onAddChild,
  canEdit,
  selectedItemDetail,
}: {
  node: BudgetItemDto;
  depth: number;
  selectedId?: number;
  onSelect?: (id: number) => void;
  onAddChild?: (parentId: number | undefined) => void;
  canEdit?: boolean;
  selectedItemDetail?: ReactNode;
}) {
  const [expanded, setExpanded] = useState(depth < 2);
  const hasChildren = (node.children?.length ?? 0) > 0;
  const isSelected = selectedId === node.id;

  return (
    <li className="relative">
      <div
        role="treeitem"
        aria-expanded={hasChildren ? expanded : undefined}
        aria-selected={isSelected}
        tabIndex={0}
        onKeyDown={(e) => {
          if ((e.key === 'Enter' || e.key === ' ') && node.id !== undefined) {
            e.preventDefault();
            onSelect?.(node.id);
          } else if (e.key === 'ArrowRight' && hasChildren) {
            setExpanded(true);
          } else if (e.key === 'ArrowLeft' && hasChildren) {
            setExpanded(false);
          }
        }}
        className={`group flex items-center gap-3 rounded-lg py-2.5 pe-3 text-sm transition-colors duration-150 outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-focus-ring)] focus-visible:ring-offset-2 cursor-pointer ${
          isSelected
            ? 'bg-[var(--color-primary-container)]'
            : 'hover:bg-[var(--color-surface-container-low)]'
        }`}
        style={{ paddingInlineStart: `calc(${depth * 1.5 + 0.5}rem)` }}
        onClick={(e) => {
          const target = e.target as HTMLElement;
          if (!target.closest('button') && node.id !== undefined) {
            onSelect?.(node.id);
          }
        }}
      >
        {hasChildren ? (
          <Button
            variant="ghost"
            size="icon-xs"
            type="button"
            aria-label={expanded ? 'طي' : 'توسيع'}
            onClick={(e) => {
              e.stopPropagation();
              setExpanded((v) => !v);
            }}
            className={`w-6 h-6 shrink-0 transition-transform duration-150 ${isSelected ? 'text-[var(--color-primary)]' : 'text-[var(--color-on-surface-variant)]'}`}
          >
            <ChevronDown
              className={`size-4 transition-transform duration-150 ${expanded ? 'rotate-0' : '-rotate-90'}`}
            />
          </Button>
        ) : (
          <span className="w-6 shrink-0 inline-block" aria-hidden="true" />
        )}

        <div className="flex flex-1 items-center gap-3 truncate">
          <span className={`font-bold shrink-0 ${isSelected ? 'text-[var(--color-primary)]' : 'text-[var(--color-on-surface)]'}`}>
            {node.itemCode}
          </span>
          <span className={`truncate ${isSelected ? 'text-[var(--color-on-primary-container)] font-medium' : 'text-[var(--color-on-surface-variant)]'}`}>
            {node.itemName}
          </span>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <span className="text-xs font-medium text-[var(--color-outline)] bg-[var(--color-surface-container)] px-2 py-0.5 rounded-full">
            مستوى {node.level}
          </span>
          {node.isActive === false && (
            <StatusBadge variant="closed" size="sm">معطل</StatusBadge>
          )}
        </div>

        {canEdit && node.id !== undefined && (
          <div className="opacity-0 group-hover:opacity-100 group-focus-within:opacity-100 transition-opacity duration-150 flex-shrink-0 ms-2">
            <Button
              variant="ghost"
              size="sm"
              type="button"
              className="h-7 px-2 text-[var(--color-primary)] hover:bg-[var(--color-primary-container)]"
              onClick={(e) => {
                e.stopPropagation();
                onAddChild?.(node.id);
              }}
            >
              <Plus className="size-3.5" />
              <span className="ms-1.5 text-xs">فرعي</span>
            </Button>
          </div>
        )}
      </div>

      {isSelected && selectedItemDetail && (
        <div className="ms-8 mt-1">
          {selectedItemDetail}
        </div>
      )}

      {hasChildren && expanded && (
        <ul role="group" className="mt-0.5 space-y-0.5 relative">
          <div
            className="absolute top-0 bottom-0 border-e border-dashed border-[var(--color-outline-variant)] opacity-40"
            style={{ insetInlineEnd: `calc(${depth * 1.5 + 1.25}rem)` }}
          />
          {node.children!.map((child: BudgetItemDto) => (
            <TreeNode
              key={child.id}
              node={child}
              depth={depth + 1}
              selectedId={selectedId}
              onSelect={onSelect}
              onAddChild={onAddChild}
              canEdit={canEdit}
              selectedItemDetail={selectedItemDetail}
            />
          ))}
        </ul>
      )}
    </li>
  );
}

export function BudgetItemTree(props: BudgetItemTreeProps) {
  const { nodes, onAddChild, canEdit, selectedItemDetail } = props;

  if (nodes.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-10 text-center transition-colors duration-150 hover:border-[var(--color-outline)]">
        <div className="bg-[var(--color-surface-container)] p-3 rounded-full mb-4">
          <FolderOpen className="w-6 h-6 text-[var(--color-on-surface-variant)]" />
        </div>
        <h3 className="text-sm font-semibold text-[var(--color-on-surface)] mb-1">دليل الموازنة فارغ</h3>
        <p className="text-sm text-[var(--color-on-surface-variant)] mb-5 max-w-[250px]">
          لم يتم إضافة أي بنود رئيسية لشجرة الموازنة حتى الآن.
        </p>
        {canEdit && (
          <Button type="button" variant="primary" onClick={() => onAddChild?.(undefined)}>
            <Plus className="size-4" />
            <span className="ms-2">إضافة بند رئيسي</span>
          </Button>
        )}
      </div>
    );
  }

  return (
    <div className="relative">
      {canEdit && (
        <div className="flex justify-end mb-3 border-b border-[var(--color-outline-variant)] pb-3">
          <Button type="button" variant="outline" size="sm" onClick={() => onAddChild?.(undefined)}>
            <Plus className="size-4" />
            <span className="ms-2">بند رئيسي جديد</span>
          </Button>
        </div>
      )}

      <ul role="tree" aria-label="شجرة بنود الموازنة" className="space-y-1">
        {nodes.map((node) => (
          <TreeNode key={node.id} node={node} depth={0} {...props} />
        ))}
      </ul>
    </div>
  );
}
