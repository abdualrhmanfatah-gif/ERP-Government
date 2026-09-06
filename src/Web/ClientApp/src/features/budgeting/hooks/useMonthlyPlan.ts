import { useCallback, useMemo } from 'react';

const STORAGE_PREFIX = 'monthly-plan-';

export interface MonthlyPlan {
  budgetItemId: number;
  months: [number, number, number, number, number, number, number, number, number, number, number, number];
  updatedAt: string;
}

function getStorageKey(budgetItemId: number): string {
  return `${STORAGE_PREFIX}${budgetItemId}`;
}

function loadPlan(budgetItemId: number): MonthlyPlan | null {
  try {
    const raw = localStorage.getItem(getStorageKey(budgetItemId));
    if (!raw) return null;
    const parsed = JSON.parse(raw) as MonthlyPlan;
    if (!Array.isArray(parsed.months) || parsed.months.length !== 12) return null;
    return parsed;
  } catch {
    return null;
  }
}

function savePlan(budgetItemId: number, months: MonthlyPlan['months']): MonthlyPlan {
  const plan: MonthlyPlan = {
    budgetItemId,
    months,
    updatedAt: new Date().toISOString(),
  };
  localStorage.setItem(getStorageKey(budgetItemId), JSON.stringify(plan));
  return plan;
}

export function useMonthlyPlan(budgetItemId: number | undefined) {
  const plan = useMemo(
    () => (Number.isFinite(budgetItemId) ? loadPlan(budgetItemId!) : null),
    [budgetItemId],
  );

  const months: MonthlyPlan['months'] = plan?.months ?? [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

  const total = useMemo(() => months.reduce((sum, m) => sum + (m || 0), 0), [months]);

  const setMonth = useCallback(
    (index: number, value: number) => {
      if (!Number.isFinite(budgetItemId)) return;
      const next = [...months] as MonthlyPlan['months'];
      next[index] = value;
      savePlan(budgetItemId!, next);
    },
    [budgetItemId, months],
  );

  const save = useCallback(() => {
    if (!Number.isFinite(budgetItemId)) return;
    savePlan(budgetItemId!, months);
  }, [budgetItemId, months]);

  return { plan, months, total, setMonth, save };
}
