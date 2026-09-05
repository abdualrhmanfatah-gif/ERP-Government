import { useMutation, useQueryClient } from '@tanstack/react-query';
import { rolePermissionsClient } from '../client';

export function useRemoveRolePermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ roleId, permissionId }: { roleId: number; permissionId: number }) =>
      rolePermissionsClient.remove(roleId, permissionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-role-permissions'] }),
  });
}
