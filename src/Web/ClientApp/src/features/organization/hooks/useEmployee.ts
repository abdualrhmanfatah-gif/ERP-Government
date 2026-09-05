import { useQuery } from '@tanstack/react-query';
import { employeesClient } from '../client';

export function useEmployee(id: number) {
  return useQuery({
    queryKey: ['employees', id],
    queryFn: () => employeesClient.getById(id),
    enabled: id > 0,
  });
}
