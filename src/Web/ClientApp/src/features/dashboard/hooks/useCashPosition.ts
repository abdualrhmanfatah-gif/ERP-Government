import { useQuery } from '@tanstack/react-query';
import { dashboardClient } from '../client';

interface CashPosition {
  totalBalance: number;
  hasBankAccounts: boolean;
}

export function useCashPosition() {
  return useQuery({
    queryKey: ['dashboard-cash-position'],
    queryFn: async (): Promise<CashPosition> => {
      const accounts = await dashboardClient.getBankAccounts();

      if (accounts.length === 0) {
        return { totalBalance: 0, hasBankAccounts: false };
      }

      const totalBalance = accounts.reduce(
        (sum, a) => sum + (a.currentBalance ?? 0),
        0
      );

      return { totalBalance, hasBankAccounts: true };
    },
  });
}
