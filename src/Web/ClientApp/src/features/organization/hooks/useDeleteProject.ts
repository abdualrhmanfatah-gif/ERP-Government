import { useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsClient } from '../client';

export function useDeleteProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => projectsClient.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}
