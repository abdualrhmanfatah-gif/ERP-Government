import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { movesClient } from '../client';
import type { CreateMoveCommand, UpdateMoveCommand } from '../types';

export function useMovesList(
  filters?: {
    entryStatus?: string;
    journalId?: number;
    fromDate?: string;
    toDate?: string;
    isBalanced?: boolean;
  },
  pagination?: { page: number; pageSize: number }
) {
  const { data: allData, ...query } = useQuery({
    queryKey: ['moves', filters],
    queryFn: () => movesClient.list(filters),
  });

  const page = pagination?.page ?? 1;
  const pageSize = pagination?.pageSize ?? 20;
  const totalItems = allData?.length ?? 0;
  const totalPages = Math.ceil(totalItems / pageSize);
  const startIndex = (page - 1) * pageSize;
  const pagedData = allData?.slice(startIndex, startIndex + pageSize);

  return {
    ...query,
    data: pagedData,
    allData,
    totalItems,
    totalPages,
    page,
    pageSize,
  };
}

export function useMove(id: number) {
  return useQuery({
    queryKey: ['moves', id],
    queryFn: () => movesClient.getById(id),
    enabled: id > 0,
  });
}

export function useCreateMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateMoveCommand) => movesClient.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['moves'] }),
  });
}

export function useUpdateMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateMoveCommand }) =>
      movesClient.update(id, data),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves'] });
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
    },
  });
}

export function useCancelMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      movesClient.cancel(id, rowVersion),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves'] });
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
    },
  });
}

export function useSubmitMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      movesClient.submit(id, rowVersion),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
      qc.invalidateQueries({ queryKey: ['moves'] });
    },
  });
}

export function useApproveMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      movesClient.approve(id, rowVersion),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
      qc.invalidateQueries({ queryKey: ['moves'] });
    },
  });
}

export function usePostMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, rowVersion }: { id: number; rowVersion: string }) =>
      movesClient.post(id, rowVersion),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
      qc.invalidateQueries({ queryKey: ['moves'] });
    },
  });
}

export function useReverseMove() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      reversalReason,
      rowVersion,
    }: {
      id: number;
      reversalReason: string;
      rowVersion: string;
    }) => movesClient.reverse(id, reversalReason, rowVersion),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: ['moves', vars.id] });
      qc.invalidateQueries({ queryKey: ['moves'] });
    },
  });
}
