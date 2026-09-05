import { useMutation, useQueryClient } from '@tanstack/react-query';
import { DeactivateExchangeRateCommand, ExchangeRatesClient } from '../../../../web-api-client';

const client = new ExchangeRatesClient();

export function useDeactivateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion?: string }) =>
      client.deactivatePOST4(id, new DeactivateExchangeRateCommand({ id, rowVersion })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['exchangeRates'] }),
  });
}
