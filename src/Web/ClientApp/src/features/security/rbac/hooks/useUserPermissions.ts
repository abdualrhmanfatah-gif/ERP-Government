import { useQuery } from '@tanstack/react-query';
import { userPermissionsClient } from '../client';

export function useUserPermissions(userId: number) {
  return useQuery({
    queryKey: ['rbac-user-permissions', userId],
    queryFn: () => userPermissionsClient.list(userId),
    enabled: userId > 0,
  });
}
