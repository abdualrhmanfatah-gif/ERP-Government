import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FiscalPeriodsClient } from '../../../../web-api-client';

const client = new FiscalPeriodsClient();

export function useLockFiscalPeriod() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: { id: number; rowVersion: string }) =>
      client.lock(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalPeriods'] });
    },
  });
}
