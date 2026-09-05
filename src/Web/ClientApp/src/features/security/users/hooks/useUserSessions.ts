import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersSessionsClient } from '../client';

export function useUserSessions(userId: number) {
  return useQuery({
    queryKey: ['users', userId, 'sessions'],
    queryFn: () => usersSessionsClient.list(userId),
    enabled: userId > 0,
  });
}

export function useRevokeUserSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, sessionId }: { userId: number; sessionId: number }) =>
      usersSessionsClient.revoke(userId, sessionId),
    onSuccess: (_result, { userId }) => {
      qc.invalidateQueries({ queryKey: ['users', userId, 'sessions'] });
    },
  });
}

export function useRevokeAllUserSessions() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersSessionsClient.revokeAll(userId),
    onSuccess: (_result, userId) => {
      qc.invalidateQueries({ queryKey: ['users', userId, 'sessions'] });
    },
  });
}
