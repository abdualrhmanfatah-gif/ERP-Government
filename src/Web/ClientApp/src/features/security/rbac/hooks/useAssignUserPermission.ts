import { useMutation, useQueryClient } from '@tanstack/react-query';
import { userPermissionsClient } from '../client';
import type { AssignUserPermissionCommand } from '../types';

export function useAssignUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: AssignUserPermissionCommand & { userId: number }) => userPermissionsClient.assign(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-user-permissions'] }),
  });
}
