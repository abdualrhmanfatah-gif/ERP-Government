import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { CreateAccountGroupRequest } from '../types';

export function useCreateAccountGroup() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (payload: CreateAccountGroupRequest) => {
      const res = await fetch('/api/AccountGroups', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      if (!res.ok) throw new Error(await res.text());
      return res.json();
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['account-groups'] }),
  });
}
