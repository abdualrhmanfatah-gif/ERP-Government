import { useMutation, useQueryClient } from '@tanstack/react-query';
import { userPermissionsClient } from '../client';

export function useRemoveUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, permissionId }: { userId: number; permissionId: number }) =>
      userPermissionsClient.remove(userId, permissionId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-user-permissions'] }),
  });
}
