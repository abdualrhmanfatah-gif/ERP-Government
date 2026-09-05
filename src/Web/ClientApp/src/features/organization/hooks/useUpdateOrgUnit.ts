import { useMutation, useQueryClient } from '@tanstack/react-query';
import { orgUnitsClient } from '../client';
import type { UpdateOrgUnitCommand } from '../types';

export function useUpdateOrgUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateOrgUnitCommand) => orgUnitsClient.update(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['org-units'] }),
  });
}
