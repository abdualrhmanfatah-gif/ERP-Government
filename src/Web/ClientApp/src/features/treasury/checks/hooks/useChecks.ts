import { useQuery } from '@tanstack/react-query';
import { getChecks, getCheckById } from '../shared/client';

export function useChecksList(params: { from: string; to: string; status?: string }) {
  return useQuery({
    queryKey: ['checks', params],
    queryFn: () => getChecks(params),
  });
}

export function useCheckDetail(id: number) {
  return useQuery({
    queryKey: ['checks', id],
    queryFn: () => getCheckById(id),
    enabled: id > 0,
  });
}
