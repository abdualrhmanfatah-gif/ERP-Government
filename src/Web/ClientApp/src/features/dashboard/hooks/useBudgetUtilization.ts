import { useQuery } from '@tanstack/react-query';
import { dashboardClient } from '../client';

interface BudgetUtilization {
  totalAllocated: number;
  totalSpent: number;
  utilizationPercent: number | null;
}

export function useBudgetUtilization() {
  return useQuery({
    queryKey: ['dashboard-budget-utilization'],
    queryFn: async (): Promise<BudgetUtilization> => {
      const budgets = await dashboardClient.getBudgets();

      const totalAllocated = budgets.reduce(
        (sum, b) => sum + (b.totalAmount ?? 0),
        0
      );
      const totalSpent = budgets.reduce(
        (sum, b) => sum + (b.paidAmount ?? 0),
        0
      );

      if (totalAllocated === 0) {
        return { totalAllocated: 0, totalSpent: 0, utilizationPercent: null };
      }

      return {
        totalAllocated,
        totalSpent,
        utilizationPercent: (totalSpent / totalAllocated) * 100,
      };
    },
  });
}
