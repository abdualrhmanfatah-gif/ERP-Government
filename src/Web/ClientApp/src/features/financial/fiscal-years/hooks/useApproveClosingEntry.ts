import { useMutation, useQueryClient } from '@tanstack/react-query';
import { closingEntriesClient } from '../client';

export function useApproveClosingEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => closingEntriesClient.approve(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['closingEntries'] });
      queryClient.invalidateQueries({ queryKey: ['closingEntry'] });
    },
  });
}
