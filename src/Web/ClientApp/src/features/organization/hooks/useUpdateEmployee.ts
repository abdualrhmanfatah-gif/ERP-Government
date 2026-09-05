import { useMutation, useQueryClient } from '@tanstack/react-query';
import { employeesClient } from '../client';
import type { UpdateEmployeeCommand } from '../types';

export function useUpdateEmployee() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateEmployeeCommand) => employeesClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['employees'] }),
  });
}
