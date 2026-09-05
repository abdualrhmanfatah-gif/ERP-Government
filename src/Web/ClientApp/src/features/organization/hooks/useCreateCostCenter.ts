import { useMutation, useQueryClient } from '@tanstack/react-query';
import { costCentersClient } from '../client';
import type { CreateCostCenterCommand } from '../types';

export function useCreateCostCenter() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCostCenterCommand) => costCentersClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['cost-centers'] }),
  });
}
