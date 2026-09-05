import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FiscalPeriodsClient } from '../../../../web-api-client';

const client = new FiscalPeriodsClient();

export function useUnlockFiscalPeriod() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id }: { id: number; rowVersion: string }) =>
      client.unlock(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalPeriods'] });
    },
  });
}
