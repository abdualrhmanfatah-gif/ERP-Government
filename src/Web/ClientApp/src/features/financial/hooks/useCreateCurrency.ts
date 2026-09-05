import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CurrenciesClient, CreateCurrencyCommand } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useCreateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: Record<string, unknown>) => {
      const cmd = CreateCurrencyCommand.fromJS(data);
      return client.currenciesPOST(cmd);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['currencies'] }),
  });
}
