import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FiscalYearsClient } from '../../../../web-api-client';

const client = new FiscalYearsClient();

export function useOpenFiscalYear() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: { id: number; rowVersion: string }) =>
      client.open(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalYears'] });
    },
  });
}
