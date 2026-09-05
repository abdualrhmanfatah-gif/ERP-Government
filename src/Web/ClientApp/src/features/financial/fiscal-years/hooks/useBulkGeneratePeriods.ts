import { useMutation, useQueryClient } from '@tanstack/react-query';
import { BulkGeneratePeriodsCommand, FiscalPeriodsClient } from '../../../../web-api-client';

const client = new FiscalPeriodsClient();

export function useBulkGeneratePeriods() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ fiscalYearId }: { fiscalYearId: number }) =>
      client.bulkGenerate(new BulkGeneratePeriodsCommand({ fiscalYearId })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fiscalPeriods'] });
    },
  });
}
