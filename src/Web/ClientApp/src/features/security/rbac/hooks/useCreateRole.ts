import { useMutation, useQueryClient } from '@tanstack/react-query';
import { rolesClient } from '../client';
import type { CreateRoleCommand } from '../types';

export function useCreateRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoleCommand) => rolesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['rbac-roles'] }),
  });
}
