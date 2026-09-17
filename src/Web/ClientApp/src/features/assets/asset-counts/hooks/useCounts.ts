import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { AssetCount, CountDetail } from '../shared/types';

export interface PaginatedList<T> { items: T[]; totalCount: number; page: number; pageSize: number; }

const queryKeys = { all: ['assetCounts'] as const, list: (p?: Record<string, unknown>) => [...queryKeys.all, 'list', p] as const, detail: (id: number) => [...queryKeys.all, 'detail', id] as const };

export function useCountsList(params?: { search?: string; status?: string; page?: number }) {
  return useQuery({ queryKey: queryKeys.list(params), queryFn: async () => { const sp = new URLSearchParams(); if (params?.search) sp.set('search', params.search); if (params?.status) sp.set('status', params.status); if (params?.page) sp.set('page', params.page.toString()); return await api.get<PaginatedList<AssetCount>>(`/api/AssetCounts?${sp.toString()}`); } });
}
export function useCountDetail(id: number) { return useQuery({ queryKey: queryKeys.detail(id), queryFn: () => api.get<CountDetail>(`/api/AssetCounts/${id}`), enabled: id > 0 }); }
export function useCreateCount() { const qc = useQueryClient(); return useMutation({ mutationFn: (d: Record<string, unknown>) => api.post<number>('/api/AssetCounts', d), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
export function useStartCount() { const qc = useQueryClient(); return useMutation({ mutationFn: (id: number) => api.post(`/api/AssetCounts/${id}/start`), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
export function useUpdateCountLine() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ countId, lineId, data }: { countId: number; lineId: number; data: Record<string, unknown> }) => api.put(`/api/AssetCounts/${countId}/lines/${lineId}`, data), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
export function useCompleteCount() { const qc = useQueryClient(); return useMutation({ mutationFn: (id: number) => api.post(`/api/AssetCounts/${id}/complete`), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
export function useReviewCount() { const qc = useQueryClient(); return useMutation({ mutationFn: (id: number) => api.post(`/api/AssetCounts/${id}/review`), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
