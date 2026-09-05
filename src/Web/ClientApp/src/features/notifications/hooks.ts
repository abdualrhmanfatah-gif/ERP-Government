import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsClient } from './client';

export function useNotifications(page = 1, pageSize = 25) {
  return useQuery({
    queryKey: ['notifications', page, pageSize],
    queryFn: () => notificationsClient.list(page, pageSize),
  });
}

export function useUnreadCount() {
  return useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => notificationsClient.unreadCount(),
    refetchInterval: 30000,
  });
}

export function useMarkAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => notificationsClient.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

export function useMarkAllAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => notificationsClient.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

export function useDeleteNotification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => notificationsClient.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}

export function useClearAllNotifications() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => notificationsClient.clearAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });
}
