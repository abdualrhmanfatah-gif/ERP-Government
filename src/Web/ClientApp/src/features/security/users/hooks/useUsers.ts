import { useQuery } from '@tanstack/react-query';
import { usersClient } from '../client';

export interface UserListParams {
  search?: string;
  status?: string;
  departmentId?: number;
  page?: number;
  pageSize?: number;
}

export function useUsers(params?: UserListParams) {
  return useQuery({
    queryKey: ['users', params],
    queryFn: () => usersClient.list(params),
  });
}
