import { useQuery } from '@tanstack/react-query';
import { projectsClient } from '../client';

export function useProject(id: number) {
  return useQuery({
    queryKey: ['projects', id],
    queryFn: () => projectsClient.getById(id),
    enabled: id > 0,
  });
}
