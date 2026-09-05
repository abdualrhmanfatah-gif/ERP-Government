import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CreateFiscalYearCommand, FiscalYearsClient } from '../../../../web-api-client';

const client = new FiscalYearsClient();

export function useCreateFiscalYear() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { name: string; yearNumber: number; startDate: Date; endDate: Date }) =>
      client.fiscalYearsPOST(new CreateFiscalYearCommand(data)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalYears'] });
    },
  });
}
