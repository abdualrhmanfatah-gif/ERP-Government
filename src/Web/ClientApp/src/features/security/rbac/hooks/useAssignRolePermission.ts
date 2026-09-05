import { useMutation, useQueryClient } from '@tanstack/react-query';
import { rolePermissionsClient } from '../client';
import type { AssignRolePermissionCommand } from '../types';

export function useAssignRolePermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: AssignRolePermissionCommand) => rolePermissionsClient.assign(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-role-permissions'] }),
  });
}
