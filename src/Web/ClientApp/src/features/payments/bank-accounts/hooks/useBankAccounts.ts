import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { bankAccountsClient } from '../shared/client';

export function useBankAccountsList() {
  return useQuery({
    queryKey: ['bank-accounts'],
    queryFn: () => bankAccountsClient.list(),
  });
}

export function useBankAccountDetail(id: number) {
  return useQuery({
    queryKey: ['bank-accounts', id],
    queryFn: () => bankAccountsClient.getById(id),
    enabled: Number.isFinite(id),
  });
}

export function useCreateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: any) => bankAccountsClient.create(cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bank-accounts'] }),
  });
}

export function useUpdateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => bankAccountsClient.update(id, cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bank-accounts'] }),
  });
}

export function useActivateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => bankAccountsClient.activate(id, cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bank-accounts'] }),
  });
}

export function useDeactivateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, cmd }: { id: number; cmd: any }) => bankAccountsClient.deactivate(id, cmd),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['bank-accounts'] }),
  });
}
