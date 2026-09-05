import { useMutation, useQueryClient } from '@tanstack/react-query';
import { usersClient, usersLifecycleClient } from '../client';
import type { CreateUserCommand, UpdateUserCommand } from '../types';

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateUserCommand) => usersClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateUserCommand) => usersClient.update(data),
    onSuccess: (_result, variables) => {
      qc.invalidateQueries({ queryKey: ['users'] });
      qc.invalidateQueries({ queryKey: ['users', variables.id] });
    },
  });
}

export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersLifecycleClient.deactivate(userId),
    onSuccess: (_result, userId) => {
      qc.invalidateQueries({ queryKey: ['users'] });
      qc.invalidateQueries({ queryKey: ['users', userId] });
    },
  });
}

export function useReactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersLifecycleClient.reactivate(userId),
    onSuccess: (_result, userId) => {
      qc.invalidateQueries({ queryKey: ['users'] });
      qc.invalidateQueries({ queryKey: ['users', userId] });
    },
  });
}

export function useResetFailedLoginAttempts() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (userId: number) => usersLifecycleClient.resetFailedLoginAttempts(userId),
    onSuccess: (_result, userId) => {
      qc.invalidateQueries({ queryKey: ['users', userId] });
    },
  });
}
