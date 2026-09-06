import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBudgetDetail } from '../../hooks/useBudgets';
import { useBudgetItemsTree } from '../../hooks/useBudgetItems';
import { useAppropriationsList } from '../../hooks/useAppropriations';
import { useEncumbrancesList } from '../../hooks/useEncumbrances';
import { budgetStatusLabels, appropriationTypeLabels, encumbranceTypeLabels } from '../../shared/types';
import { Badge } from '@/components/ui';
import { ChevronDown, ChevronRight } from 'lucide-react';

interface ExecutionDrillDownProps {
  budgetId: number;
}

export function ExecutionDrillDown({ budgetId }: ExecutionDrillDownProps) {
  const navigate = useNavigate();
  const [expandedItems, setExpandedItems] = useState<Set<number>>(new Set());
  const [expandedAppropriations, setExpandedAppropriations] = useState<Set<number>>(new Set());

  const { data: budget, isLoading: budgetLoading } = useBudgetDetail(budgetId);
  const { data: tree, isLoading: treeLoading } = useBudgetItemsTree(budgetId);
  const { data: appropriations, isLoading: appsLoading } = useAppropriationsList({ budgetId });
  const { data: encumbrances, isLoading: encsLoading } = useEncumbrancesList({});

  const isLoading = budgetLoading || treeLoading || appsLoading || encsLoading;

  if (isLoading) {
    return <div className="p-6 text-center text-[var(--color-on-surface-variant)]">جارٍ التحميل...</div>;
  }

  if (!budget) {
    return <div className="p-6 text-center text-[var(--color-error)]">لم يتم العثور على الموازنة</div>;
  }

  const totalAppropriated = appropriations?.reduce((sum, a) => sum + a.amount, 0) ?? 0;
  const totalEncumbered = encumbrances?.reduce((sum, e) => sum + e.amount, 0) ?? 0;

  function toggleItem(id: number) {
    setExpandedItems((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  function toggleAppropriation(id: number) {
    setExpandedAppropriations((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <div className="space-y-4">
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
        <h2 className="text-sm font-semibold text-[var(--color-on-surface)] mb-3">ملخص التنفيذ</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)]">إجمالي الموازنة</span>
            <span className="block font-mono font-semibold">{budget.totalAmount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)]">إجمالي التخصيصات</span>
            <span className="block font-mono font-semibold text-[var(--color-primary)]">{totalAppropriated.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)]">إجمالي الالتزامات</span>
            <span className="block font-mono font-semibold text-[var(--color-warning)]">{totalEncumbered.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
          <div>
            <span className="block text-xs text-[var(--color-on-surface-variant)]">المتبقي</span>
            <span className="block font-mono font-semibold text-[var(--color-success)]">{(budget.totalAmount - totalEncumbered).toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
          </div>
        </div>
      </div>

      {tree && tree.length > 0 && (
        <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
          <h2 className="text-sm font-semibold text-[var(--color-on-surface)] mb-3">بنود الموازنة</h2>
          <div className="space-y-1">
            {tree.map((item) => {
              const itemApps = appropriations?.filter((a) => a.budgetItemId === item.id) ?? [];
              const itemTotal = itemApps.reduce((sum, a) => sum + a.amount, 0);
              const isExpanded = expandedItems.has(item.id);

              return (
                <div key={item.id}>
                  <button
                    className="flex items-center gap-2 w-full px-3 py-2 text-sm hover:bg-[var(--color-surface-variant)] rounded text-start"
                    onClick={() => toggleItem(item.id)}
                  >
                    {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                    <span className="font-mono text-xs text-[var(--color-on-surface-variant)]">{item.itemCode}</span>
                    <span className="flex-1">{item.itemName}</span>
                    <span className="font-mono text-xs">{itemTotal.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
                    <Badge variant="outline">{itemApps.length}</Badge>
                  </button>

                  {isExpanded && itemApps.length > 0 && (
                    <div className="mr-8 space-y-1">
                      {itemApps.map((app) => {
                        const appEncumbrances = encumbrances?.filter((e) => e.appropriationId === app.id) ?? [];
                        const appEncTotal = appEncumbrances.reduce((sum, e) => sum + e.amount, 0);
                        const isAppExpanded = expandedAppropriations.has(app.id);

                        return (
                          <div key={app.id}>
                            <button
                              className="flex items-center gap-2 w-full px-3 py-1.5 text-sm hover:bg-[var(--color-surface-variant)] rounded text-start"
                              onClick={() => toggleAppropriation(app.id)}
                            >
                              {isAppExpanded ? <ChevronDown size={12} /> : <ChevronRight size={12} />}
                              <span className="font-mono text-xs text-[var(--color-on-surface-variant)]">{app.appropriationNumber}</span>
                              <Badge variant="outline">{appropriationTypeLabels[app.appropriationType]}</Badge>
                              <span className="flex-1" />
                              <span className="font-mono text-xs">{app.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
                              <Badge variant="outline">{appEncumbrances.length}</Badge>
                            </button>

                            {isAppExpanded && appEncumbrances.length > 0 && (
                              <div className="mr-8 space-y-1">
                                {appEncumbrances.map((enc) => (
                                  <div
                                    key={enc.id}
                                    className="flex items-center gap-2 px-3 py-1.5 text-sm hover:bg-[var(--color-surface-variant)] rounded cursor-pointer"
                                    onClick={() => navigate(`/budgeting/encumbrances/${enc.id}`)}
                                  >
                                    <span className="font-mono text-xs text-[var(--color-on-surface-variant)]">{enc.encumbranceNumber}</span>
                                    <Badge variant="outline">{encumbranceTypeLabels[enc.encumbranceType]}</Badge>
                                    <span className="flex-1" />
                                    <span className="font-mono text-xs">{enc.amount.toLocaleString('ar-EG', { minimumFractionDigits: 2 })}</span>
                                  </div>
                                ))}
                              </div>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
