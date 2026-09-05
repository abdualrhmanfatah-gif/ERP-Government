import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ActivateExchangeRateCommand, ExchangeRatesClient } from '../../../../web-api-client';

const client = new ExchangeRatesClient();

export function useActivateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion?: string }) =>
      client.activate2(id, new ActivateExchangeRateCommand({ id, rowVersion })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['exchangeRates'] }),
  });
}
