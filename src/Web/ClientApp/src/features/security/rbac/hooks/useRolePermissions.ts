import { useQuery } from '@tanstack/react-query';
import { rolePermissionsClient } from '../client';

export function useRolePermissions(roleId: number) {
  return useQuery({
    queryKey: ['rbac-role-permissions', roleId],
    queryFn: () => rolePermissionsClient.list(roleId),
    enabled: roleId > 0,
  });
}
