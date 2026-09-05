import { useMutation, useQueryClient } from '@tanstack/react-query';
import { costCentersClient } from '../client';

export function useDeleteCostCenter() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => costCentersClient.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['cost-centers'] }),
  });
}
