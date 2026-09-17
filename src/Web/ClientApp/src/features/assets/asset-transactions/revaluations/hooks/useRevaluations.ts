import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Revaluation, RevaluationDetail } from '../shared/types';

export interface PaginatedList<T> { items: T[]; totalCount: number; page: number; pageSize: number; }

const queryKeys = { all: ['assetRevaluations'] as const, list: (p?: Record<string, unknown>) => [...queryKeys.all, 'list', p] as const, detail: (id: number) => [...queryKeys.all, 'detail', id] as const };

export function useRevaluationsList(params?: { search?: string; status?: string; page?: number }) {
  return useQuery({ queryKey: queryKeys.list(params), queryFn: async () => { const sp = new URLSearchParams(); if (params?.search) sp.set('search', params.search); if (params?.status) sp.set('status', params.status); if (params?.page) sp.set('page', params.page.toString()); return await api.get<PaginatedList<Revaluation>>(`/api/AssetRevaluations?${sp.toString()}`); } });
}
export function useRevaluationDetail(id: number) { return useQuery({ queryKey: queryKeys.detail(id), queryFn: () => api.get<RevaluationDetail>(`/api/AssetRevaluations/${id}`), enabled: id > 0 }); }
export function useCreateRevaluation() { const qc = useQueryClient(); return useMutation({ mutationFn: (d: Record<string, unknown>) => api.post<number>('/api/AssetRevaluations', d), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
