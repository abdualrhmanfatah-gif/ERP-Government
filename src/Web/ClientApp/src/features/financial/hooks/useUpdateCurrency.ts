import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CurrenciesClient, UpdateCurrencyCommand } from '../../../web-api-client';

const client = new CurrenciesClient();

export function useUpdateCurrency() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: {
      id?: number; name?: string; symbol?: string;
      decimalPlaces?: number; roundingPrecision?: number; rowVersion?: string;
    } }) => {
      const cmd = UpdateCurrencyCommand.fromJS(data);
      return client.currenciesPUT(id, cmd);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      qc.invalidateQueries({ queryKey: ['currency'] });
    },
  });
}
