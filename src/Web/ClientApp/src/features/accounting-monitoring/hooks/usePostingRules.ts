import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { postingRulesClient } from '../shared/client';

export function usePostingRules() {
  return useQuery({
    queryKey: ['postingRules'],
    queryFn: () => postingRulesClient.postingRulesAll(),
  });
}

export function useCreatePostingRule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Parameters<typeof postingRulesClient.postingRules>[0]) =>
      postingRulesClient.postingRules(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['postingRules'] });
    },
  });
}

export function useUpdatePostingRule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: Parameters<typeof postingRulesClient.postingRules2>[1] }) =>
      postingRulesClient.postingRules2(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['postingRules'] });
    },
  });
}

export function useDeletePostingRule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => postingRulesClient.postingRules3(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['postingRules'] });
    },
  });
}
