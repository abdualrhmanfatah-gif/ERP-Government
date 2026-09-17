import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import {
  RevenueClaimsClient,
  CreateRevenueClaimCommand,
  ApproveRevenueClaimCommand,
  ClaimStatus,
} from '@/web-api-client';

const client = new RevenueClaimsClient();

export function useRevenueClaims(filters?: {
  partyId?: number;
  status?: ClaimStatus;
}) {
  return useQuery({
    queryKey: ['revenue-claims', 'list', filters],
    queryFn: () => client.revenueClaimsAll(filters?.partyId, filters?.status),
  });
}

export function useRevenueClaim(id: number) {
  return useQuery({
    queryKey: ['revenue-claims', 'detail', id],
    queryFn: () => client.revenueClaimsAll(undefined, undefined).then((list) => list.find((c) => c.id === id)),
    enabled: id > 0,
  });
}

export function useCreateRevenueClaim() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRevenueClaimCommand) => client.revenueClaims(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['revenue-claims'] }),
    onError: handleLifecycleError,
  });
}

export function useApproveRevenueClaim() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string; reason?: string }) =>
      client.approvePOST8(
        args.id,
        new ApproveRevenueClaimCommand({ id: args.id, rowVersion: args.rowVersion, reason: args.reason }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['revenue-claims'] }),
    onError: handleLifecycleError,
  });
}
