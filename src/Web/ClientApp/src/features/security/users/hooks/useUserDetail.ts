import { useQuery } from '@tanstack/react-query';
import { usersClient } from '../client';

export function useUserDetail(userId: number) {
  return useQuery({
    queryKey: ['users', userId],
    queryFn: () => usersClient.getById(userId),
    enabled: userId > 0,
  });
}
