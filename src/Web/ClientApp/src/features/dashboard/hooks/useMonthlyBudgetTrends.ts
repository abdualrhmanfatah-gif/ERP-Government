import { useQuery } from '@tanstack/react-query';

interface MonthlyBudgetData {
  month: string;
  allocated: number;
  spent: number;
}

// Mock data — replace with real API call when backend endpoint is available
const mockData: MonthlyBudgetData[] = [
  { month: 'يناير', allocated: 150000, spent: 120000 },
  { month: 'فبراير', allocated: 180000, spent: 165000 },
  { month: 'مارس', allocated: 200000, spent: 175000 },
  { month: 'أبريل', allocated: 175000, spent: 160000 },
  { month: 'مايو', allocated: 220000, spent: 195000 },
  { month: 'يونيو', allocated: 190000, spent: 180000 },
  { month: 'يوليو', allocated: 210000, spent: 190000 },
  { month: 'أغسطس', allocated: 185000, spent: 170000 },
  { month: 'سبتمبر', allocated: 230000, spent: 210000 },
  { month: 'أكتوبر', allocated: 200000, spent: 185000 },
  { month: 'نوفمبر', allocated: 195000, spent: 175000 },
  { month: 'ديسمبر', allocated: 250000, spent: 220000 },
];

export function useMonthlyBudgetTrends() {
  return useQuery({
    queryKey: ['dashboard-monthly-budget-trends'],
    queryFn: async (): Promise<MonthlyBudgetData[]> => {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockData;
    },
  });
}
