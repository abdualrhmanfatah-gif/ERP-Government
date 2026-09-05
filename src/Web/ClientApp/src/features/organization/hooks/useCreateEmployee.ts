import { useMutation, useQueryClient } from '@tanstack/react-query';
import { employeesClient } from '../client';
import type { CreateEmployeeCommand } from '../types';

export function useCreateEmployee() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateEmployeeCommand) => employeesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['employees'] }),
  });
}
