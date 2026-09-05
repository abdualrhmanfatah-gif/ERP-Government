import { useMutation, useQueryClient } from '@tanstack/react-query';
import { FiscalPeriodsClient, UpdateFiscalPeriodCommand } from '../../../../web-api-client';

const client = new FiscalPeriodsClient();

export function useUpdateFiscalPeriod() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: number;
      data: { name: string; startDate: string; endDate: string };
    }) =>
      client.fiscalPeriodsPUT(
        id,
        new UpdateFiscalPeriodCommand({
          id,
          name: data.name,
          startDate: new Date(data.startDate),
          endDate: new Date(data.endDate),
        }),
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalPeriods'] });
    },
  });
}
