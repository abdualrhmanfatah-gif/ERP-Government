import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CurrenciesClient, ActivateCurrencyCommand } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useActivateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion?: string }) => {
      const cmd = ActivateCurrencyCommand.fromJS({ id, rowVersion });
      return client.activate(id, cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['currencies'] }),
  });
}
