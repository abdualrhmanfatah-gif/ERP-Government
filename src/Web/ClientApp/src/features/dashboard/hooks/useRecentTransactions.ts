import { useQuery } from '@tanstack/react-query';

export interface RecentTransaction {
  id: string;
  date: string;
  description: string;
  account: string;
  debit: number;
  credit: number;
  status: 'posted' | 'pending' | 'draft';
}

// Mock data — replace with real API call when backend endpoint is available
const mockData: RecentTransaction[] = [
  {
    id: '1',
    date: '2026-09-03',
    description: 'شراء معدات مكتبية',
    account: 'حساب المشتريات',
    debit: 15000,
    credit: 0,
    status: 'posted',
  },
  {
    id: '2',
    date: '2026-09-02',
    description: 'دفع رواتب الموظفين',
    account: 'حساب الرواتب',
    debit: 85000,
    credit: 0,
    status: 'posted',
  },
  {
    id: '3',
    date: '2026-09-02',
    description: 'إيرادات خدمات استشارية',
    account: 'حساب الإيرادات',
    debit: 0,
    credit: 45000,
    status: 'posted',
  },
  {
    id: '4',
    date: '2026-09-01',
    description: 'دفع فواتير المرافق',
    account: 'حساب المرافق',
    debit: 8500,
    credit: 0,
    status: 'pending',
  },
  {
    id: '5',
    date: '2026-09-01',
    description: 'إيداع مبلغ في الحساب الجاري',
    account: 'الحساب الجاري',
    debit: 0,
    credit: 120000,
    status: 'posted',
  },
  {
    id: '6',
    date: '2026-08-31',
    description: 'شراء لوازم مكتبية',
    account: 'حساب اللوازم',
    debit: 3200,
    credit: 0,
    status: 'draft',
  },
  {
    id: '7',
    date: '2026-08-30',
    description: 'دفع إيجار المكتب',
    account: 'حساب الإيجار',
    debit: 25000,
    credit: 0,
    status: 'posted',
  },
  {
    id: '8',
    date: '2026-08-29',
    description: 'إيرادات بيع منتجات',
    account: 'حساب الإيرادات',
    debit: 0,
    credit: 67500,
    status: 'posted',
  },
  {
    id: '9',
    date: '2026-08-28',
    description: 'تحويل بين الحسابات',
    account: 'الحساب الجاري',
    debit: 50000,
    credit: 0,
    status: 'posted',
  },
  {
    id: '10',
    date: '2026-08-27',
    description: 'دفع اشتراكات البرامج',
    account: 'حساب الاشتراكات',
    debit: 12000,
    credit: 0,
    status: 'posted',
  },
];

export function useRecentTransactions() {
  return useQuery({
    queryKey: ['dashboard-recent-transactions'],
    queryFn: async (): Promise<RecentTransaction[]> => {
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockData;
    },
  });
}
