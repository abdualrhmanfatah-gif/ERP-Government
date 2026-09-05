import { useQuery } from '@tanstack/react-query';
import { orgUnitsClient } from '../client';

export function useOrganizationalUnits() {
  return useQuery({
    queryKey: ['org-units'],
    queryFn: () => orgUnitsClient.list(),
  });
}
