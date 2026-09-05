import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersPermissionsClient } from '../client';
import type { AssignUserPermissionCommand } from '../types';

export function useUserPermissions(userId: number) {
  return useQuery({
    queryKey: ['users', userId, 'permissions'],
    queryFn: () => usersPermissionsClient.list(userId),
    enabled: userId > 0,
  });
}

export function useAssignUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, data }: { userId: number; data: AssignUserPermissionCommand }) =>
      usersPermissionsClient.assign(userId, { ...data, userId }),
    onSuccess: (_result, { userId }) => {
      qc.invalidateQueries({ queryKey: ['users', userId, 'permissions'] });
    },
  });
}

export function useRemoveUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, permissionId }: { userId: number; permissionId: number }) =>
      usersPermissionsClient.remove(userId, permissionId),
    onSuccess: (_result, { userId }) => {
      qc.invalidateQueries({ queryKey: ['users', userId, 'permissions'] });
    },
  });
}
