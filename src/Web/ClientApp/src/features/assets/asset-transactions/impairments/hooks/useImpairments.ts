import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/shared/api';
import type { Impairment, ImpairmentDetail } from '../shared/types';

export interface PaginatedList<T> { items: T[]; totalCount: number; page: number; pageSize: number; }

const queryKeys = { all: ['assetImpairments'] as const, list: (p?: Record<string, unknown>) => [...queryKeys.all, 'list', p] as const, detail: (id: number) => [...queryKeys.all, 'detail', id] as const };

export function useImpairmentsList(params?: { search?: string; status?: string; page?: number }) {
  return useQuery({ queryKey: queryKeys.list(params), queryFn: async () => { const sp = new URLSearchParams(); if (params?.search) sp.set('search', params.search); if (params?.status) sp.set('status', params.status); if (params?.page) sp.set('page', params.page.toString()); return await api.get<PaginatedList<Impairment>>(`/api/AssetImpairments?${sp.toString()}`); } });
}
export function useImpairmentDetail(id: number) { return useQuery({ queryKey: queryKeys.detail(id), queryFn: () => api.get<ImpairmentDetail>(`/api/AssetImpairments/${id}`), enabled: id > 0 }); }
export function useCreateImpairment() { const qc = useQueryClient(); return useMutation({ mutationFn: (d: Record<string, unknown>) => api.post<number>('/api/AssetImpairments', d), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
export function useReverseImpairment() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, data }: { id: number; data: Record<string, unknown> }) => api.post(`/api/AssetImpairments/${id}/reverse`, data), onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.all }) }); }
