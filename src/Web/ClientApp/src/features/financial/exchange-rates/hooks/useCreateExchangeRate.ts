import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ExchangeRatesClient, CreateExchangeRateCommand } from '../../../../web-api-client';

const client = new ExchangeRatesClient();

export function useCreateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateExchangeRateCommand) => client.exchangeRatesPOST(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['exchangeRates'] }),
  });
}
