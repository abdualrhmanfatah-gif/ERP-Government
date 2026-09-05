import { useQuery } from '@tanstack/react-query';

interface TotalTransactions {
  count: number;
  thisMonth: number;
  lastMonth: number;
  changePercent: number;
}

// Mock data — replace with real API call when backend endpoint is available
const mockData: TotalTransactions = {
  count: 1247,
  thisMonth: 186,
  lastMonth: 172,
  changePercent: 8.14,
};

export function useTotalTransactions() {
  return useQuery({
    queryKey: ['dashboard-total-transactions'],
    queryFn: async (): Promise<TotalTransactions> => {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockData;
    },
  });
}
