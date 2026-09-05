import { useQuery } from '@tanstack/react-query';
import { permissionsClient } from '../client';

export function usePermissions() {
  return useQuery({
    queryKey: ['rbac-permissions'],
    queryFn: () => permissionsClient.list(),
  });
}
