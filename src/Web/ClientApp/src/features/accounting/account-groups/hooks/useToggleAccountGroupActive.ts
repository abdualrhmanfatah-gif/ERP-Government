import { useMutation, useQueryClient } from '@tanstack/react-query';

export function useToggleAccountGroupActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, isActive, rowVersion }: { id: number; isActive: boolean; rowVersion: string }) => {
      const res = await fetch(`/api/AccountGroups/${id}/toggle-active`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isActive, rowVersion }),
      });
      if (res.status === 409) throw new Error('تم تعديل البيانات من قبل مستخدم آخر');
      if (!res.ok) throw new Error(await res.text());
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['account-groups'] });
      qc.invalidateQueries({ queryKey: ['account-group-detail'] });
    },
  });
}
