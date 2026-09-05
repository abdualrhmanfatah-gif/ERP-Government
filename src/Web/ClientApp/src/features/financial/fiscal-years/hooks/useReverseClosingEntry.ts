import { useMutation, useQueryClient } from '@tanstack/react-query';
import { closingEntriesClient } from '../client';

export function useReverseClosingEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: number; reason?: string }) =>
      closingEntriesClient.reverse(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['closingEntries'] });
      queryClient.invalidateQueries({ queryKey: ['closingEntry'] });
      queryClient.invalidateQueries({ queryKey: ['fiscalYear'] });
    },
  });
}
