import { useMutation, useQueryClient } from '@tanstack/react-query';
import { usersRoleClient } from '../client';
import type { SetUserRoleCommand } from '../types';

export function useSetUserRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, data }: { userId: number; data: SetUserRoleCommand }) =>
      usersRoleClient.set(userId, { ...data, userId }),
    onSuccess: (_result, { userId }) => {
      qc.invalidateQueries({ queryKey: ['users', userId] });
    },
  });
}
