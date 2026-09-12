import { useQuery } from '@tanstack/react-query';
import { getAuthUser, getToken } from '../utils/auth-token';

interface UsePermissionResult {
  hasPermission: boolean;
  isLoading: boolean;
}

interface EffectivePermissionDto {
  permissionId: number;
  code: string;
  name: string;
  source: string;
  isGranted: boolean;
  reason?: string;
}

async function fetchUserPermissions(userId: number): Promise<EffectivePermissionDto[]> {
  const token = getToken();
  const response = await fetch(`/api/Users/${userId}/permissions`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Accept': 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error('Failed to fetch permissions');
  }

  return response.json();
}

/**
 * Permission hook — fetches real permissions from backend API.
 *
 * @param policy - Permission policy string (e.g., 'Currencies.Create')
 * @returns { hasPermission: boolean, isLoading: boolean }
 */
export function usePermission(policy?: string): UsePermissionResult {
  const authUser = getAuthUser();
  const userId = authUser?.userId;

  const { data: permissions = [], isLoading } = useQuery({
    queryKey: ['user-permissions', userId],
    queryFn: () => fetchUserPermissions(userId!),
    enabled: !!userId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  if (!policy) {
    return { hasPermission: true, isLoading };
  }

  const hasPermission = permissions.some((p) => p.code === policy && p.isGranted);

  return { hasPermission, isLoading };
}
