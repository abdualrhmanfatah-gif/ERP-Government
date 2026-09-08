import type { BudgetClassificationTreeDto } from '../../shared/types';
import { useState, useCallback, useMemo, useRef, useEffect, type FormEvent, type KeyboardEvent } from 'react';
import { useClassificationsTree, useCreateClassification, useUpdateClassification, useToggleClassificationActive } from '../hooks/useClassifications';
import { getExcludedDescendantIds } from '../utils/classification-utils';
import { usePermission } from '../../../../shared/hooks/usePermission';
import { BUDGET_PERMISSIONS } from '../../../../shared/constants/permissions';
import { ChevronRight, ChevronLeft, Plus, Pencil } from 'lucide-react';
import { Page, FilterBar, FilterSearch, FilterSelect, Dialog, Switch, ConfirmDialog, Button, Input, Select, Badge } from '../../../../components/ui';
import { notify } from '@/features/notifications/notify';

export function normalizeTree(
  nodes: BudgetClassificationTreeDto[],
): BudgetClassificationTreeDto[] {
  const allIds = new Set<number>();
  const collectIds = (list: BudgetClassificationTreeDto[]) => {
    for (const node of list) {
      allIds.add(node.id);
      if (node.children?.length) collectIds(node.children);
    }
  };
  collectIds(nodes);

  const descendantMap = new Map<number, Set<number>>();
  const collectDescendants = (node: BudgetClassificationTreeDto): Set<number> => {
    const desc = new Set<number>();
    for (const child of node.children ?? []) {
      desc.add(child.id);
      for (const d of collectDescendants(child)) desc.add(d);
    }
    descendantMap.set(node.id, desc);
    return desc;
  };
  for (const node of nodes) collectDescendants(node);

  const orphans: BudgetClassificationTreeDto[] = [];

  const collectAllDescendants = (node: BudgetClassificationTreeDto): BudgetClassificationTreeDto[] => {
    const result: BudgetClassificationTreeDto[] = [];
    const stack = [node];
    while (stack.length) {
      const current = stack.pop()!;
      result.push({ ...current, parentId: undefined, children: [] });
      for (const child of current.children ?? []) stack.push(child);
    }
    return result;
  };

  const normalize = (list: BudgetClassificationTreeDto[]): BudgetClassificationTreeDto[] => {
    const result: BudgetClassificationTreeDto[] = [];
    for (const node of list) {
      const pid = node.parentId;
      const hasInvalidParent = pid !== undefined && pid !== null && !allIds.has(pid);
      const hasCycle =
        pid !== undefined && pid !== null && descendantMap.get(node.id)?.has(pid) === true;
      if (hasInvalidParent || hasCycle) {
        orphans.push(...collectAllDescendants(node));
      } else {
        result.push({
          ...node,
          children: node.children?.length ? normalize(node.children) : [],
        });
      }
    }
    return result;
  };

  return [...normalize(nodes), ...orphans];
}

function getInitialExpanded(nodes: BudgetClassificationTreeDto[]): Set<number> {
  const expanded = new Set<number>();
  for (const node of nodes) {
    if (node.children?.length) expanded.add(node.id);
  }
  return expanded;
}

function TreeItem({
  node,
  expanded,
  onToggle,
  onEdit,
  onToggleActive,
  canEdit,
  canUpdate,
  depth,
}: {
  node: BudgetClassificationTreeDto;
  expanded: Set<number>;
  onToggle: (id: number) => void;
  onEdit: (node: BudgetClassificationTreeDto) => void;
  onToggleActive: (node: BudgetClassificationTreeDto) => void;
  canEdit: boolean;
  canUpdate: boolean;
  depth: number;
}) {
  const isExpanded = expanded.has(node.id);
  const hasChildren = node.children?.length ? node.children.length > 0 : false;
  const itemRef = useRef<HTMLDivElement>(null);

  const handleKeyDown = useCallback(
    (e: KeyboardEvent<HTMLDivElement>) => {
      if (e.key === 'ArrowRight' && hasChildren && !isExpanded) {
        e.preventDefault();
        onToggle(node.id);
      } else if (e.key === 'ArrowLeft' && hasChildren && isExpanded) {
        e.preventDefault();
        onToggle(node.id);
      } else if (e.key === 'ArrowDown') {
        e.preventDefault();
        const next = itemRef.current?.parentElement?.nextElementSibling?.querySelector('[role="treeitem"]') as HTMLElement;
        next?.focus();
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        const prev = itemRef.current?.parentElement?.previousElementSibling?.querySelector('[role="treeitem"]') as HTMLElement;
        prev?.focus();
      }
    },
    [hasChildren, isExpanded, node.id, onToggle],
  );

  return (
    <div
      ref={itemRef}
      role="treeitem"
      tabIndex={0}
      aria-expanded={hasChildren ? isExpanded : undefined}
      onKeyDown={handleKeyDown}
    >
      <div
        className="flex items-center gap-2 py-1 px-2 hover:bg-[var(--color-surface-container)] rounded cursor-pointer"
        style={{ paddingInlineStart: `${depth * 1.5}rem` }}
        onClick={() => hasChildren && onToggle(node.id)}
      >
        {hasChildren ? (
          <span className="text-[var(--color-on-surface-variant)]">
            {isExpanded ? <ChevronLeft size={16} /> : <ChevronRight size={16} />}
          </span>
        ) : (
          <span className="w-4" />
        )}
        <span className="font-medium text-[var(--color-on-surface)]">{node.code}</span>
        <span className="text-[var(--color-on-surface-variant)]">{node.name}</span>
        <Badge variant="primary">
          مستوى {node.level}
        </Badge>
        {canUpdate && (
          <Switch
            checked={node.isActive}
            onChange={() => onToggleActive(node)}
            label={node.isActive ? 'نشط' : 'معطل'}
          />
        )}
        {canEdit && (
          <Button
            variant="ghost"
            size="icon-xs"
            onClick={(e) => { e.stopPropagation(); onEdit(node); }}
            className="ms-auto"
            aria-label="تعديل"
          >
            <Pencil size={14} />
          </Button>
        )}
      </div>
      {isExpanded && hasChildren && (
        <div role="group">
          {node.children!.map((child) => (
            <TreeItem
              key={child.id}
              node={child}
              expanded={expanded}
              onToggle={onToggle}
              onEdit={onEdit}
              onToggleActive={onToggleActive}
              canEdit={canEdit}
              canUpdate={canUpdate}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function ClassificationsListPage() {
  const { data: rawTree, isLoading, error } = useClassificationsTree();
  const createMutation = useCreateClassification();
  const updateMutation = useUpdateClassification();
  const toggleMutation = useToggleClassificationActive();
  const canCreate = usePermission(BUDGET_PERMISSIONS.BudgetClassifications.Create).hasPermission;
  const canUpdate = usePermission(BUDGET_PERMISSIONS.BudgetClassifications.Update).hasPermission;

  const [expanded, setExpanded] = useState<Set<number>>(new Set());
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('All');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState<BudgetClassificationTreeDto | null>(null);
  const [selectedParentId, setSelectedParentId] = useState<number | undefined>(undefined);
  const [formIsActive, setFormIsActive] = useState(true);
  const [parentIdError, setParentIdError] = useState('');
  const [toggleItem, setToggleItem] = useState<BudgetClassificationTreeDto | null>(null);
  const [initialized, setInitialized] = useState(false);

  const tree = useMemo(() => (rawTree ? normalizeTree(rawTree) : []), [rawTree]);

  useEffect(() => {
    if (rawTree && !initialized) {
      setExpanded(getInitialExpanded(rawTree));
      setInitialized(true);
    }
  }, [rawTree, initialized]);

  const matchesSearch = useCallback(
    (node: BudgetClassificationTreeDto): boolean => {
      if (!search) return true;
      const q = search.toLowerCase();
      return node.code.toLowerCase().includes(q) || node.name.toLowerCase().includes(q);
    },
    [search],
  );

  const matchesFilter = useCallback(
    (node: BudgetClassificationTreeDto): boolean => {
      if (isActiveFilter === 'All') return true;
      if (isActiveFilter === 'active') return node.isActive;
      if (isActiveFilter === 'inactive') return !node.isActive;
      return true;
    },
    [isActiveFilter],
  );

  const filteredTree = useMemo(() => {
    if (!search && isActiveFilter === 'All') return tree;

    const filterNode = (node: BudgetClassificationTreeDto): BudgetClassificationTreeDto | null => {
      const childResults = (node.children ?? [])
        .map(filterNode)
        .filter((c): c is BudgetClassificationTreeDto => c !== null);

      const nodeMatchesSearch = matchesSearch(node);
      const nodeMatchesFilter = matchesFilter(node);

      if (nodeMatchesSearch && nodeMatchesFilter) {
        return { ...node, children: childResults };
      }
      if (childResults.length > 0) {
        return { ...node, children: childResults };
      }
      return null;
    };

    return tree.map(filterNode).filter((n): n is BudgetClassificationTreeDto => n !== null);
  }, [tree, search, isActiveFilter, matchesSearch, matchesFilter]);

  const searchExpanded = useMemo(() => {
    if (!search) return expanded;

    const ancestors = new Set<number>();
    const findAncestors = (nodes: BudgetClassificationTreeDto[], path: number[]) => {
      for (const node of nodes) {
        const currentPath = [...path, node.id];
        if (matchesSearch(node)) {
          for (const id of currentPath) ancestors.add(id);
        }
        if (node.children?.length) findAncestors(node.children, currentPath);
      }
    };
    findAncestors(tree, []);
    return ancestors;
  }, [tree, search, expanded, matchesSearch]);

  const effectiveExpanded = search ? searchExpanded : expanded;

  const handleToggle = useCallback((id: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const canEditNode = useCallback(
    (node: BudgetClassificationTreeDto) => {
      return canUpdate || node.id === 0;
    },
    [canUpdate],
  );

  const excludedIds = useMemo(() => {
    if (!editItem || !rawTree) return new Set<number>();
    return getExcludedDescendantIds(rawTree, editItem.id);
  }, [editItem, rawTree]);

  const parentOptions = useMemo(() => {
    if (!rawTree) return [];
    const options: { value: string; label: string; disabled?: boolean }[] = [
      { value: '', label: '— رئيسي (بدون أب) —' },
    ];
    const walk = (nodes: BudgetClassificationTreeDto[], depth: number) => {
      for (const node of nodes) {
        const disabled = excludedIds.has(node.id);
        options.push({
          value: String(node.id),
          label: `${'—'.repeat(depth)} ${node.code} ${node.name}`,
          disabled,
        });
        if (node.children?.length) walk(node.children, depth + 1);
      }
    };
    walk(rawTree, 1);
    return options;
  }, [rawTree, excludedIds]);

  const handleOpenCreate = useCallback(() => {
    setEditItem(null);
    setSelectedParentId(undefined);
    setFormIsActive(true);
    setParentIdError('');
    setDialogOpen(true);
  }, []);

  const handleOpenEdit = useCallback(
    (node: BudgetClassificationTreeDto) => {
      setEditItem(node);
      setSelectedParentId(node.parentId);
      setFormIsActive(node.isActive);
      setParentIdError('');
      setDialogOpen(true);
    },
    [],
  );

  const handleCloseDialog = useCallback(() => {
    setDialogOpen(false);
    setEditItem(null);
    setParentIdError('');
  }, []);

  const handleOpenToggle = useCallback((node: BudgetClassificationTreeDto) => {
    setToggleItem(node);
  }, []);

  const handleConfirmToggle = useCallback(() => {
    if (!toggleItem) return;
    toggleMutation.mutate(
      { id: toggleItem.id, data: { id: toggleItem.id, rowVersion: toggleItem.rowVersion, isActive: !toggleItem.isActive } },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم تحديث الحالة بنجاح' });
          setToggleItem(null);
        },
        onError: (err: any) => {
          if (err?.status === 409) {
            notify({ type: 'error', title: 'تعارض في البيانات. جاري تحديث البيانات...' });
          } else {
            notify({ type: 'error', title: 'خطأ في تحديث الحالة' });
          }
          setToggleItem(null);
        },
      },
    );
  }, [toggleItem, toggleMutation]);

  const handleCancelToggle = useCallback(() => {
    setToggleItem(null);
  }, []);

  const handleSubmit = useCallback(
    (e: FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      const form = new FormData(e.currentTarget);
      const code = (form.get('code') as string)?.trim();
      const name = (form.get('name') as string)?.trim();

      if (!code || !name) return;

      if (editItem) {
        updateMutation.mutate(
          { id: editItem.id, data: { id: editItem.id, rowVersion: editItem.rowVersion, code, name, parentId: selectedParentId, isActive: formIsActive } },
          { onSuccess: () => handleCloseDialog() },
        );
      } else {
        createMutation.mutate(
          { code, name, parentId: selectedParentId, isActive: formIsActive },
          { onSuccess: () => handleCloseDialog() },
        );
      }
    },
    [editItem, selectedParentId, formIsActive, createMutation, updateMutation, handleCloseDialog],
  );

  return (
    <Page
      title="التصنيفات المالية"
      loading={isLoading}
      error={error ? 'خطأ في تحميل التصنيفات' : undefined}
      onRetry={error ? () => window.location.reload() : undefined}
      actions={
        canCreate ? (
          <Button variant="primary" onClick={handleOpenCreate} icon={<Plus size={16} />}>
            إضافة تصنيف جديد
          </Button>
        ) : undefined
      }
      toolbar={
        <FilterBar hasFilters={!!(search || isActiveFilter !== 'All')} onClear={() => { setSearch(''); setIsActiveFilter('All'); }}>
          <FilterSearch value={search} onChange={setSearch} placeholder="بحث بالكود أو الاسم..." />
          <FilterSelect
            value={isActiveFilter}
            onChange={setIsActiveFilter}
            options={[
              { value: 'All', label: 'الكل' },
              { value: 'active', label: 'نشط' },
              { value: 'inactive', label: 'معطل' },
            ]}
            placeholder="الحالة"
            label="الحالة"
          />
        </FilterBar>
      }
    >
      {tree.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-[var(--color-on-surface-variant)]">لا توجد تصنيفات بعد</p>
          <Button variant="link" className="mt-2">
            إضافة تصنيف رئيسي
          </Button>
        </div>
      ) : (
        <div className="border border-[var(--color-outline-variant)] rounded-lg overflow-hidden" role="tree">
          {filteredTree.map((node) => (
            <TreeItem
              key={node.id}
              node={node}
              expanded={effectiveExpanded}
              onToggle={handleToggle}
              onEdit={handleOpenEdit}
              onToggleActive={handleOpenToggle}
              canEdit={canEditNode(node)}
              canUpdate={canUpdate}
              depth={0}
            />
          ))}
        </div>
      )}

      <Dialog
        open={dialogOpen}
        onClose={handleCloseDialog}
        title={editItem ? 'تعديل التصنيف' : 'إضافة تصنيف جديد'}
        footer={
          <Button type="submit" form="classification-form" variant="primary" disabled={createMutation.isPending || updateMutation.isPending} loading={createMutation.isPending || updateMutation.isPending}>
            {editItem ? 'حفظ التعديلات' : 'إنشاء'}
          </Button>
        }
      >
        <form id="classification-form" onSubmit={handleSubmit} className="space-y-4">
          <Input
            id="code"
            name="code"
            type="text"
            required
            defaultValue={editItem?.code}
            label="الكود"
          />
          <Input
            id="name"
            name="name"
            type="text"
            required
            defaultValue={editItem?.name}
            label="الاسم"
          />
          <Select
            id="parentId"
            label="التصنيف الأب"
            value={selectedParentId === undefined ? '' : String(selectedParentId)}
            onChange={(e) => {
              const val = e.target.value;
              setSelectedParentId(val ? Number(val) : undefined);
              setParentIdError('');
            }}
            options={parentOptions.map((opt) => ({ value: opt.value, label: opt.label, disabled: opt.disabled }))}
          />
          {parentIdError && <p className="text-xs text-[var(--color-error)] mt-1">{parentIdError}</p>}
          <Switch
            checked={formIsActive}
            onChange={setFormIsActive}
            label="نشط"
          />
        </form>
      </Dialog>

      <ConfirmDialog
        open={!!toggleItem}
        onClose={handleCancelToggle}
        onConfirm={handleConfirmToggle}
        message={toggleItem?.isActive ? 'هل تريد تعطيل هذا التصنيف؟' : 'هل تريد تفعيل هذا التصنيف؟'}
        title="تأكيد تغيير الحالة"
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
