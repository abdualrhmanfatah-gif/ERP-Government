import { useMemo, useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import type { AccountDto } from '@/features/accounting/types';
import { ChevronLeft, ChevronDown } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { EmptyState } from '@/components/ui/EmptyState';

interface TreeNode {
  data: AccountDto;
  children: TreeNode[];
}

interface AccountGridProps {
  data: AccountDto[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
}

interface Column {
  key: string;
  header: string;
  width?: number;
}

const columns: Column[] = [
  { key: 'code', header: 'الرمز', width: 120 },
  { key: 'name', header: 'الاسم', width: 250 },
  { key: 'accountGroupName', header: 'المجموعة', width: 150 },
  { key: 'normalBalance', header: 'الرصيد الطبيعي', width: 120 },
  { key: 'level', header: 'المستوى', width: 80 },
  { key: 'isPostable', header: 'قابل للترحيل', width: 100 },
  { key: 'isActive', header: 'نشط', width: 80 },
];

function normalBalanceLabel(val?: string): string {
  if (!val) return '—';
  const v = val.toLowerCase();
  if (v === 'debit' || v === 'дебет') return 'مدين';
  if (v === 'credit' || v === 'кредит') return 'دائن';
  return val;
}

/** Flatten visible tree into ordered list for keyboard navigation. */
function flattenVisible(nodes: TreeNode[], expanded: Set<number>, depth = 0): { id: number; depth: number; hasChildren: boolean; node: TreeNode }[] {
  const result: { id: number; depth: number; hasChildren: boolean; node: TreeNode }[] = [];
  for (const n of nodes) {
    if (n.data.id == null) continue;
    result.push({ id: n.data.id, depth, hasChildren: n.children.length > 0, node: n });
    if (n.children.length > 0 && expanded.has(n.data.id)) {
      result.push(...flattenVisible(n.children, expanded, depth + 1));
    }
  }
  return result;
}

function TreeRow({
  node,
  depth,
  expanded,
  toggle,
  navigate,
  focusedId,
  setFocusedId,
  index,
}: {
  node: TreeNode;
  depth: number;
  expanded: Set<number>;
  toggle: (id: number) => void;
  navigate: (path: string) => void;
  focusedId: number | null;
  setFocusedId: (id: number) => void;
  index: number;
}) {
  const hasChildren = node.children.length > 0;
  const isExpanded = expanded.has(node.data.id!);
  const isFocused = focusedId === node.data.id;

  return (
    <>
      <tr
        role="treeitem"
        aria-expanded={hasChildren ? isExpanded : undefined}
        aria-level={depth + 1}
        tabIndex={isFocused ? 0 : -1}
        data-node-id={node.data.id}
        onClick={() => navigate(`/accounting/accounts/${node.data.id}`)}
        onFocus={() => setFocusedId(node.data.id!)}
        className={[
          'cursor-pointer outline-none transition-colors duration-150',
          isFocused ? 'outline-2 outline-[var(--color-secondary)] outline-offset-[-2px]' : '',
          index % 2 === 0 ? 'bg-[var(--color-surface-container-lowest)]' : 'bg-[color-mix(in_srgb,var(--color-surface-container-high)_20%,transparent)]',
          'hover:bg-[var(--color-surface-container-low)]',
        ].filter(Boolean).join(' ')}
      >
        {columns.map((col, idx) => (
          <td
            key={col.key}
            className={[
              'px-3 py-2.5 border-b border-[var(--color-border-container)]',
              idx === 0 ? 'text-end' : 'text-start',
              (col.key === 'code' || col.key === 'level') ? 'tabular-nums' : '',
            ].filter(Boolean).join(' ')}
            style={idx === 0 ? { paddingInlineStart: `${1.5 + depth * 1.25}rem` } : undefined}
          >
            {idx === 0 ? (
              <span className="inline-flex items-center gap-1.5">
                {hasChildren ? (
                  <Button
                    variant="ghost"
                    size="icon-xs"
                    onClick={(e) => { e.stopPropagation(); toggle(node.data.id!); }}
                    aria-label={isExpanded ? 'طي' : 'توسيع'}
                    tabIndex={-1}
                  >
                    {isExpanded ? <ChevronDown size={14} /> : <ChevronLeft size={14} />}
                  </Button>
                ) : null}
                {node.data.code}
              </span>
            ) : col.key === 'normalBalance' ? (
              normalBalanceLabel(node.data.normalBalance)
            ) : col.key === 'isPostable' ? (
              node.data.isPostable ? 'نعم' : 'لا'
            ) : col.key === 'isActive' ? (
              node.data.isActive ? (
                <span className="text-[var(--color-success)]">نشط</span>
              ) : (
                <span className="text-[var(--color-on-surface-variant)]">غير نشط</span>
              )
            ) : (
              String((node.data as Record<string, unknown>)[col.key] ?? '—')
            )}
          </td>
        ))}
      </tr>
      {isExpanded && node.children.map((child, idx) => (
        <TreeRow
          key={child.data.id}
          node={child}
          depth={depth + 1}
          expanded={expanded}
          toggle={toggle}
          navigate={navigate}
          focusedId={focusedId}
          setFocusedId={setFocusedId}
          index={index + idx + 1}
        />
      ))}
    </>
  );
}

function buildTree(accounts: AccountDto[]): TreeNode[] {
  const map = new Map<number, TreeNode>();
  const roots: TreeNode[] = [];

  for (const acc of accounts) {
    if (acc.id == null) continue;
    map.set(acc.id, { data: acc, children: [] });
  }

  for (const acc of accounts) {
    if (acc.id == null) continue;
    const node = map.get(acc.id)!;
    if (acc.parentId != null && map.has(acc.parentId)) {
      map.get(acc.parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  }

  return roots;
}

export function AccountGrid({ data, loading, error, onRetry }: AccountGridProps) {
  const [expanded, setExpanded] = useState<Set<number>>(() => {
    const initial = new Set<number>();
    for (const acc of data) {
      if (acc.level === 0 && acc.id != null) initial.add(acc.id);
    }
    return initial;
  });
  const [focusedId, setFocusedId] = useState<number | null>(() => {
    const first = flattenVisible(buildTree(data), expanded)[0];
    return first?.id ?? null;
  });
  const tableRef = useRef<HTMLTableElement>(null);
  const navigate = useNavigate();

  const tree = useMemo(() => buildTree(data), [data]);

  const toggle = useCallback((id: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }, []);

  const expandAll = useCallback(() => {
    const all = new Set<number>();
    const walk = (nodes: TreeNode[]) => {
      for (const n of nodes) {
        if (n.data.id != null && n.children.length > 0) all.add(n.data.id);
        walk(n.children);
      }
    };
    walk(tree);
    setExpanded(all);
  }, [tree]);

  const collapseAll = useCallback(() => setExpanded(new Set()), []);

  // Keyboard navigation per WAI-ARIA Treeview pattern
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    const flat = flattenVisible(tree, expanded);
    if (flat.length === 0) return;

    const currentIdx = focusedId != null ? flat.findIndex((f) => f.id === focusedId) : -1;
    const current = currentIdx >= 0 ? flat[currentIdx] : null;

    let nextIdx = -1;
    let toggleId: number | null = null;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        nextIdx = currentIdx < flat.length - 1 ? currentIdx + 1 : 0;
        break;
      case 'ArrowUp':
        e.preventDefault();
        nextIdx = currentIdx > 0 ? currentIdx - 1 : flat.length - 1;
        break;
      case 'ArrowRight':
        // RTL: ArrowRight = collapse or go to parent
        e.preventDefault();
        if (current && current.hasChildren && expanded.has(current.id)) {
          toggleId = current.id; // collapse
        } else if (current && currentIdx > 0) {
          // find parent: first item with smaller depth
          for (let i = currentIdx - 1; i >= 0; i--) {
            if (flat[i].depth < current.depth) { nextIdx = i; break; }
          }
        }
        break;
      case 'ArrowLeft':
        // RTL: ArrowLeft = expand or go to first child
        e.preventDefault();
        if (current && current.hasChildren) {
          if (!expanded.has(current.id)) {
            toggleId = current.id; // expand
          } else if (currentIdx < flat.length - 1 && flat[currentIdx + 1]?.depth === current.depth + 1) {
            nextIdx = currentIdx + 1; // move to first child
          }
        }
        break;
      case 'Home':
        e.preventDefault();
        nextIdx = 0;
        break;
      case 'End':
        e.preventDefault();
        nextIdx = flat.length - 1;
        break;
      case 'Enter':
      case ' ':
        if (current) {
          e.preventDefault();
          navigate(`/accounting/accounts/${current.id}`);
        }
        break;
      default:
        return;
    }

    if (toggleId != null) {
      toggle(toggleId);
    }

    if (nextIdx >= 0 && nextIdx < flat.length) {
      const nextNode = flat[nextIdx];
      setFocusedId(nextNode.id);
      // Focus the DOM element after render
      requestAnimationFrame(() => {
        const row = tableRef.current?.querySelector(`[data-node-id="${nextNode.id}"]`) as HTMLElement | null;
        row?.focus();
      });
    }
  }, [tree, expanded, focusedId, toggle, navigate]);

  if (loading) {
    return (
      <div role="status" aria-busy="true" className="p-12 text-center text-[var(--color-on-surface-variant)]">
        جاري التحميل...
      </div>
    );
  }

  if (error) {
    return (
      <div role="alert" className="p-12 text-center text-[var(--color-error)]">
        {error}
        {onRetry ? (
          <Button variant="destructive" size="sm" onClick={onRetry} className="mt-3">
            إعادة المحاولة
          </Button>
        ) : null}
      </div>
    );
  }

  if (data.length === 0) {
    return <EmptyState message="لا توجد حسابات" />;
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex gap-2">
        <Button variant="outline" size="sm" onClick={expandAll}>توسيع الكل</Button>
        <Button variant="outline" size="sm" onClick={collapseAll}>طي الكل</Button>
      </div>
      <div className="overflow-x-auto border-2 border-[var(--color-primary-container)] rounded-lg shadow-md">
        <table
          ref={tableRef}
          role="tree"
          aria-label="دليل الحسابات"
          onKeyDown={handleKeyDown}
          className="w-full border-collapse text-sm leading-relaxed"
        >
          <thead>
            <tr>
              {columns.map((col) => (
                <th key={col.key} scope="col" className={`px-3 py-2.5 font-semibold text-xs uppercase tracking-wide text-white bg-[var(--color-primary)] border-b-2 border-[var(--color-primary-container)] whitespace-nowrap ${col.key === 'code' ? 'text-end' : 'text-start'}`} style={{ width: col.width }}>
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {tree.map((node, idx) => (
              <TreeRow
                key={node.data.id}
                node={node}
                depth={0}
                expanded={expanded}
                toggle={toggle}
                navigate={navigate}
                focusedId={focusedId}
                setFocusedId={setFocusedId}
                index={idx}
              />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
