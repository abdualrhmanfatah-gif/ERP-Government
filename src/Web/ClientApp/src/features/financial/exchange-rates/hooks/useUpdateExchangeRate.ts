import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ExchangeRatesClient, UpdateExchangeRateCommand } from '../../../../web-api-client';

const client = new ExchangeRatesClient();

interface UpdateArgs {
  id: number;
  data: UpdateExchangeRateCommand;
}

export function useUpdateExchangeRate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: UpdateArgs) => client.exchangeRatesPUT(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['exchangeRates'] }),
  });
}
