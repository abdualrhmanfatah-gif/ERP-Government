import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { UpdateAccountGroupRequest } from '../types';

export function useUpdateAccountGroup() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (payload: UpdateAccountGroupRequest) => {
      const res = await fetch(`/api/AccountGroups/${payload.id}`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });
      if (res.status === 409) {
        const body = await res.json().catch(() => ({}));
        throw Object.assign(new Error('تم تعديل البيانات من قبل مستخدم آخر، يرجى التحديث والمحاولة مرة أخرى.'), { status: 409, body });
      }
      if (!res.ok) throw new Error(await res.text());
    },
    onSuccess: (_, vars) => {
      qc.invalidateQueries({ queryKey: ['account-groups'] });
      qc.invalidateQueries({ queryKey: ['account-group-detail', vars.id] });
    },
  });
}
