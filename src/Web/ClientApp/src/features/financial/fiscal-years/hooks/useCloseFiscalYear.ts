import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FiscalYearsClient } from '../../../../web-api-client';

const client = new FiscalYearsClient();

export function useCloseFiscalYear() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: { id: number; rowVersion: string }) =>
      client.close(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalYears'] });
    },
  });
}
