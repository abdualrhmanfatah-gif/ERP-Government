import { useQuery } from '@tanstack/react-query';
import { costCentersClient } from '../client';

export function useCostCenter(id: number) {
  return useQuery({
    queryKey: ['cost-centers', id],
    queryFn: () => costCentersClient.getById(id),
    enabled: id > 0,
  });
}
