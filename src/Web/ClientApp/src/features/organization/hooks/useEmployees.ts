import { useQuery } from '@tanstack/react-query';
import { employeesClient } from '../client';

export function useEmployees() {
  return useQuery({
    queryKey: ['employees'],
    queryFn: () => employeesClient.list(),
  });
}
