import { useQuery } from '@tanstack/react-query';
import { dashboardClient } from '../client';
import type { PendingApprovalDto } from '../../../web-api-client';

export function usePendingApprovalsCount() {
  return useQuery({
    queryKey: ['dashboard-pending-approvals'],
    queryFn: async () => {
      const items = await dashboardClient.getPendingApprovals();
      return {
        count: items.length,
        items: items as PendingApprovalDto[],
      };
    },
  });
}
