import { useQuery } from '@tanstack/react-query';

interface ExpenseCategory {
  id: string;
  label: string;
  value: number;
  color: string;
}

// Mock data — replace with real API call when backend endpoint is available
const mockData: ExpenseCategory[] = [
  { id: 'salaries', label: 'Salaries', value: 450000, color: 'var(--chart-1)' },
  { id: 'operations', label: 'Operations', value: 280000, color: 'var(--chart-2)' },
  { id: 'equipment', label: 'Equipment', value: 150000, color: 'var(--chart-3)' },
  { id: 'travel', label: 'Travel', value: 95000, color: 'var(--chart-4)' },
  { id: 'utilities', label: 'Utilities', value: 75000, color: 'var(--chart-5)' },
  { id: 'other', label: 'Other', value: 50000, color: 'var(--chart-6)' },
];

export function useExpenseBreakdown() {
  return useQuery({
    queryKey: ['dashboard-expense-breakdown'],
    queryFn: async (): Promise<ExpenseCategory[]> => {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockData;
    },
  });
}
