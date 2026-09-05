import { useMutation, useQueryClient } from '@tanstack/react-query';
import { employeesClient } from '../client';

export function useDeleteEmployee() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => employeesClient.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['employees'] }),
  });
}
