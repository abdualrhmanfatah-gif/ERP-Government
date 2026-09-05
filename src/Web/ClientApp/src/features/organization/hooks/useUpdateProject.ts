import { useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsClient } from '../client';
import type { UpdateProjectCommand } from '../types';

export function useUpdateProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateProjectCommand) => projectsClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}
