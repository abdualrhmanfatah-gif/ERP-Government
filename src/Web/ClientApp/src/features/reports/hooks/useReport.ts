import { useQuery } from '@tanstack/react-query';

const API_BASE = '/api/Reports';

interface UseReportOptions<T> {
  endpoint: string;
  params: Record<string, string | number | undefined>;
  enabled?: boolean;
}

export function useReport<T>({ endpoint, params, enabled = true }: UseReportOptions<T>) {
  const queryString = new URLSearchParams(
    Object.entries(params)
      .filter(([, v]) => v !== undefined && v !== '')
      .map(([k, v]) => [k, String(v)])
  ).toString();

  const url = `${API_BASE}${endpoint}${queryString ? `?${queryString}` : ''}`;

  return useQuery<T>({
    queryKey: [endpoint, params],
    queryFn: async () => {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Report request failed: ${response.statusText}`);
      }
      return response.json();
    },
    enabled,
    staleTime: 5 * 60 * 1000,
  });
}
