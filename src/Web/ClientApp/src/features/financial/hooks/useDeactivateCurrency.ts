import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CurrenciesClient, DeactivateCurrencyCommand } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useDeactivateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion?: string }) => {
      const cmd = DeactivateCurrencyCommand.fromJS({ id, rowVersion });
      return client.deactivatePOST2(id, cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['currencies'] }),
  });
}
