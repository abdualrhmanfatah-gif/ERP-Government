import { useQuery } from '@tanstack/react-query';

interface FiscalYearPeriodResult {
  fiscalYearId: number;
  fiscalYearCode?: string;
  fiscalYearName?: string;
  fiscalPeriodId: number;
  fiscalPeriodName?: string;
}

export function useFiscalYearByDate(date: string) {
  return useQuery({
    queryKey: ['fiscalYear', 'by-date', date],
    queryFn: async (): Promise<FiscalYearPeriodResult> => {
      const res = await fetch(`/api/FiscalYears/by-date?date=${date}`, {
        headers: { Accept: 'application/json' },
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return res.json();
    },
    enabled: !!date,
    staleTime: 60_000,
  });
}
