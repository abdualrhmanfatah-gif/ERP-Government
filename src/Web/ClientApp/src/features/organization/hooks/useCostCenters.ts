import { useQuery } from '@tanstack/react-query';
import { costCentersClient } from '../client';

export function useCostCenters() {
  return useQuery({
    queryKey: ['cost-centers'],
    queryFn: () => costCentersClient.list(),
  });
}
