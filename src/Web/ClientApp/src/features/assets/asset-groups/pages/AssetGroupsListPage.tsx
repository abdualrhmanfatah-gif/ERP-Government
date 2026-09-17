import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button, Badge, FilterBar, FilterSearch, FilterSelect, ConfirmDialog, DataGrid, EmptyState, StatusBadge, Card } from '@/components/ui';
import { Plus, Eye, ChevronLeft, ChevronDown } from 'lucide-react';
import { useAssetGroupsList, useToggleAssetGroupActive } from '../hooks/useAssetGroups';
import { assetCategoryLabels } from '../shared/types';
import type { AssetGroup } from '../shared/types';
import { getActiveBadge } from '../../shared/status';
import { getQueryErrorMessage } from '@/shared/api/query-error';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import { activeStatusLabels } from '@/shared/constants/labels';

interface TreeNode extends AssetGroup {
  children: TreeNode[];
  level: number;
}

const EMPTY_MESSAGE = 'لا توجد مجموعات بعد';
const NO_RESULTS_MESSAGE = 'لا توجد نتائج مطابقة لمعايير البحث';

function buildTree(groups: AssetGroup[]): TreeNode[] {
  const map = new Map<number, TreeNode>();
  const roots: TreeNode[] = [];

  for (const g of groups) {
    map.set(g.id, { ...g, children: [], level: 0 });
  }

  for (const g of groups) {
    const node = map.get(g.id)!;
    if (g.parentAssetGroupId && map.has(g.parentAssetGroupId)) {
      const parent = map.get(g.parentAssetGroupId)!;
      node.level = parent.level + 1;
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  }

  return roots;
}

function TreeItem({ node, onToggle, toggledIds }: { node: TreeNode; onToggle: (id: number, activate: boolean, rowVersion: string) => void; toggledIds: Set<number> }) {
  const navigate = useNavigate();
  const [expanded, setExpanded] = useState(true);
  const hasChildren = node.children.length > 0;
  const badge = getActiveBadge(node.isActive);

  return (
    <div>
      <div className="group flex items-center gap-2 border-b border-[var(--color-container-border)] bg-[var(--color-surface)] px-3 py-1.5 transition-colors last:border-b-0 hover:bg-[var(--color-surface-container-low)]">
        <span className="flex min-w-0 flex-1 items-center gap-1" style={{ paddingInlineStart: `${node.level * 20}px` }}>
          {hasChildren ? (
            <Button
              variant="ghost"
              size="icon-xs"
              onClick={(e) => { e.stopPropagation(); setExpanded(!expanded); }}
              aria-label={expanded ? 'طي' : 'توسيع'}
              aria-expanded={expanded}
            >
              {expanded ? <ChevronDown size={14} /> : <ChevronLeft size={14} />}
            </Button>
          ) : (
            <span className="inline-block w-9 shrink-0" />
          )}
          <button
            type="button"
            onClick={() => navigate(`/assets/asset-groups/${node.id}`)}
            className="min-w-0 truncate rounded text-start font-medium hover:text-[var(--color-primary)] focus-visible:outline-2 focus-visible:outline-[var(--color-focus-ring)]"
          >
            {node.name}
          </button>
        </span>

        <span className="w-32 shrink-0 truncate font-mono text-xs text-[var(--color-on-surface-variant)]" dir="ltr">
          {node.code}
        </span>

        <span className="w-32 shrink-0">
          <StatusBadge variant={badge.variant} size="sm">{badge.label}</StatusBadge>
        </span>

        <span className="flex w-44 shrink-0 items-center justify-end gap-1">
          {!node.isActive ? (
            <Button
              variant="ghost"
              size="xs"
              onClick={() => onToggle(node.id, true, node.rowVersion)}
              disabled={toggledIds.has(node.id)}
            >
              تفعيل
            </Button>
          ) : (
            <Button
              variant="ghost"
              size="xs"
              className="text-[var(--color-error)]"
              onClick={() => onToggle(node.id, false, node.rowVersion)}
              disabled={toggledIds.has(node.id)}
            >
              تعطيل
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon-xs"
            aria-label="عرض المجموعة"
            onClick={() => navigate(`/assets/asset-groups/${node.id}`)}
          >
            <Eye size={14} />
          </Button>
        </span>
      </div>

      {expanded && hasChildren && (
        <div>
          {node.children.map((child) => (
            <TreeItem key={child.id} node={child} onToggle={onToggle} toggledIds={toggledIds} />
          ))}
        </div>
      )}
    </div>
  );
}

export default function AssetGroupsListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [viewMode, setViewMode] = useState<'tree' | 'grid'>('tree');
  const [confirmDialog, setConfirmDialog] = useState<{ groupId: number; activate: boolean; rowVersion: string } | null>(null);
  const [toggledIds, setToggledIds] = useState<Set<number>>(new Set());

  const params = useMemo(() => ({
    search: search || undefined,
    isActive: isActiveFilter ? isActiveFilter === 'true' : undefined,
  }), [search, isActiveFilter]);

  const { data, isLoading, isFetching, error, refetch } = useAssetGroupsList(params);
  const toggleMutation = useToggleAssetGroupActive();
  const groups = useMemo(() => data ?? [], [data]);

  const hasFilters = !!search || !!isActiveFilter;
  const emptyMessage = hasFilters ? NO_RESULTS_MESSAGE : EMPTY_MESSAGE;

  function handleClearFilters() {
    setSearch('');
    setIsActiveFilter('');
  }

  function handleToggle(groupId: number, activate: boolean, rowVersion: string) {
    setConfirmDialog({ groupId, activate, rowVersion });
  }

  async function confirmToggle() {
    if (!confirmDialog) return;
    const { groupId } = confirmDialog;

    setToggledIds((prev) => new Set(prev).add(groupId));
    try {
      await toggleMutation.mutateAsync({
        id: confirmDialog.groupId,
        activate: confirmDialog.activate,
        rowVersion: confirmDialog.rowVersion,
      });
    } catch (err) {
      handleLifecycleError(err);
    } finally {
      setToggledIds((prev) => {
        const next = new Set(prev);
        next.delete(groupId);
        return next;
      });
      setConfirmDialog(null);
    }
  }

  const tree = useMemo(() => buildTree(groups), [groups]);

  return (
    <Page
      title="مجموعات الأصول"
      description="إدارة تصنيفات الأصول"
      actions={
        <div className="flex items-center gap-2">
          <div className="flex overflow-hidden rounded-lg border border-[var(--color-container-border)]">
            <Button
              variant={viewMode === 'tree' ? 'outline' : 'ghost'}
              size="sm"
              className="rounded-none"
              aria-pressed={viewMode === 'tree'}
              onClick={() => setViewMode('tree')}
            >
              شجرة
            </Button>
            <Button
              variant={viewMode === 'grid' ? 'outline' : 'ghost'}
              size="sm"
              className="rounded-none"
              aria-pressed={viewMode === 'grid'}
              onClick={() => setViewMode('grid')}
            >
              جدول
            </Button>
          </div>
          <Button
            variant="primary"
            size="sm"
            icon={<Plus size={16} />}
            onClick={() => navigate('/assets/asset-groups/create')}
          >
            إضافة مجموعة
          </Button>
        </div>
      }
      toolbar={
        <FilterBar hasFilters={hasFilters} onClear={handleClearFilters}>
          <FilterSearch
            value={search}
            onChange={setSearch}
            placeholder="بحث بالكود أو الاسم..."
            className="flex-1 min-w-48"
          />
          <FilterSelect
            label="الحالة"
            value={isActiveFilter}
            onChange={setIsActiveFilter}
            options={[
              { value: 'true', label: activeStatusLabels.active },
              { value: 'false', label: activeStatusLabels.inactive },
            ]}
          />
        </FilterBar>
      }
      loading={isLoading}
      error={error ? getQueryErrorMessage(error) : undefined}
      onRetry={error ? () => refetch() : undefined}
    >
      {viewMode === 'tree' ? (
        <Card padding="none" className="overflow-hidden">
          <div className="flex items-center gap-2 bg-[var(--color-primary)] px-3 py-2 text-xs font-semibold text-[var(--color-on-primary)]">
            <span className="flex-1">المجموعة</span>
            <span className="w-32 shrink-0">الكود</span>
            <span className="w-32 shrink-0">الحالة</span>
            <span className="w-44 shrink-0 text-end">إجراءات</span>
          </div>
          {tree.length === 0 ? (
            <EmptyState message={emptyMessage} />
          ) : (
            tree.map((node) => (
              <TreeItem key={node.id} node={node} onToggle={handleToggle} toggledIds={toggledIds} />
            ))
          )}
        </Card>
      ) : (
        <DataGrid<AssetGroup>
          data={groups}
          rowKey={(row) => row.id}
          onRowClick={(row) => navigate(`/assets/asset-groups/${row.id}`)}
          loading={isFetching && !isLoading}
          emptyMessage={emptyMessage}
          columns={[
            {
              id: 'code',
              accessorKey: 'code',
              header: 'الكود',
              width: 120,
              cell: (row) => <span dir="ltr" className="tabular-nums font-mono">{row.code}</span>,
            },
            { id: 'name', accessorKey: 'name', header: 'الاسم' },
            {
              id: 'parentName',
              accessorKey: 'parentName',
              header: 'المجموعة الأب',
              cell: (row) => row.parentName ?? '—',
            },
            {
              id: 'assetCategory',
              accessorKey: 'assetCategory',
              header: 'الفئة',
              cell: (row) => (
                <Badge variant="outline">{assetCategoryLabels[row.assetCategory] ?? row.assetCategory}</Badge>
              ),
            },
            {
              id: 'isActive',
              accessorKey: 'isActive',
              header: 'الحالة',
              width: 130,
              cell: (row) => {
                const badge = getActiveBadge(row.isActive);
                return <StatusBadge variant={badge.variant}>{badge.label}</StatusBadge>;
              },
            },
            {
              id: 'actions',
              header: 'إجراءات',
              cell: (row) => (
                <div className="flex justify-end gap-1">
                  {!row.isActive ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={(e) => { e.stopPropagation(); handleToggle(row.id, true, row.rowVersion); }}
                    >
                      تفعيل
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-[var(--color-error)]"
                      onClick={(e) => { e.stopPropagation(); handleToggle(row.id, false, row.rowVersion); }}
                    >
                      تعطيل
                    </Button>
                  )}
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label="عرض المجموعة"
                    onClick={(e) => { e.stopPropagation(); navigate(`/assets/asset-groups/${row.id}`); }}
                  >
                    <Eye size={16} />
                  </Button>
                </div>
              ),
            },
          ]}
        />
      )}

      <ConfirmDialog
        open={!!confirmDialog}
        onClose={() => setConfirmDialog(null)}
        title={confirmDialog?.activate ? 'تفعيل المجموعة' : 'تعطيل المجموعة'}
        message={
          confirmDialog?.activate
            ? 'هل أنت متأكد من تفعيل هذه المجموعة؟'
            : 'هل أنت متأكد من تعطيل هذه المجموعة؟ لن تظهر في قوائم اختيار الأصول الجديدة.'
        }
        confirmLabel={confirmDialog?.activate ? 'تفعيل' : 'تعطيل'}
        onConfirm={confirmToggle}
        destructive={!confirmDialog?.activate}
        loading={toggleMutation.isPending}
      />
    </Page>
  );
}
