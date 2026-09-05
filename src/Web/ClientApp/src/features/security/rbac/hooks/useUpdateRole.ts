import { useMutation, useQueryClient } from '@tanstack/react-query';
import { rolesClient } from '../client';
import type { UpdateRoleCommand } from '../types';

export function useUpdateRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateRoleCommand) => rolesClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-roles'] }),
  });
}
