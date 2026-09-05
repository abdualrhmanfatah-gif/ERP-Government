import { useMutation, useQueryClient } from '@tanstack/react-query';
import { moveLinesClient } from '../client';
import type { CreateMoveLineCommand, UpdateMoveLineCommand } from '../types';

export function useCreateMoveLine() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ moveId, data }: { moveId: number; data: CreateMoveLineCommand }) =>
      moveLinesClient.create(moveId, data),
    onSuccess: (_data, vars) => qc.invalidateQueries({ queryKey: ['moves', vars.moveId] }),
  });
}

export function useUpdateMoveLine() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      moveId,
      lineId,
      data,
    }: {
      moveId: number;
      lineId: number;
      data: UpdateMoveLineCommand;
    }) => moveLinesClient.update(moveId, lineId, data),
    onSuccess: (_data, vars) => qc.invalidateQueries({ queryKey: ['moves', vars.moveId] }),
  });
}

export function useRemoveMoveLine() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ moveId, lineId }: { moveId: number; lineId: number }) =>
      moveLinesClient.remove(moveId, lineId),
    onSuccess: (_data, vars) => qc.invalidateQueries({ queryKey: ['moves', vars.moveId] }),
  });
}
