import { useMutation, useQueryClient } from '@tanstack/react-query';
import { costCentersClient } from '../client';
import type { UpdateCostCenterCommand } from '../types';

export function useUpdateCostCenter() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateCostCenterCommand) => costCentersClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['cost-centers'] }),
  });
}
