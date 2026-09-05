import { useMutation, useQueryClient } from '@tanstack/react-query';
import { closingEntriesClient } from '../client';

export function useGenerateClosingEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { fiscalYearId: number; description?: string }) =>
      closingEntriesClient.generate(data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['closingEntries', variables.fiscalYearId] });
      queryClient.invalidateQueries({ queryKey: ['fiscalYear', variables.fiscalYearId] });
      queryClient.invalidateQueries({ queryKey: ['fiscalYears'] });
    },
  });
}
