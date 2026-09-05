import { useQuery } from '@tanstack/react-query';
import { orgUnitsClient } from '../client';

export function useOrganizationalUnit(id: number) {
  return useQuery({
    queryKey: ['org-units', id],
    queryFn: () => orgUnitsClient.getById(id),
    enabled: id > 0,
  });
}
