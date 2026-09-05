import { useMutation, useQueryClient } from '@tanstack/react-query';
import { orgUnitsClient } from '../client';

export function useDeleteOrgUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => orgUnitsClient.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['org-units'] }),
  });
}
