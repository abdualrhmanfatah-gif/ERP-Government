import { useMutation, useQueryClient } from '@tanstack/react-query';
import { orgUnitsClient } from '../client';
import type { CreateOrgUnitCommand } from '../types';

export function useCreateOrgUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateOrgUnitCommand) => orgUnitsClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['org-units'] }),
  });
}
