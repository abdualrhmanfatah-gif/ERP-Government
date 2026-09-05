import { useQuery } from '@tanstack/react-query';
import { rolesClient } from '../client';

export function useRoles() {
  return useQuery({
    queryKey: ['rbac-roles'],
    queryFn: () => rolesClient.list(),
  });
}
