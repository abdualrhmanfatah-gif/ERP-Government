import { useQuery } from '@tanstack/react-query';
import { projectsClient } from '../client';

export function useProjects() {
  return useQuery({
    queryKey: ['projects'],
    queryFn: () => projectsClient.list(),
  });
}
