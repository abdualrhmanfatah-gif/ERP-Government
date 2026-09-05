import { useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsClient } from '../client';
import type { CreateProjectCommand } from '../types';

export function useCreateProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProjectCommand) => projectsClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}
