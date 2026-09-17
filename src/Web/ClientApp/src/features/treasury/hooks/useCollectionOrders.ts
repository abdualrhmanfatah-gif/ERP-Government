import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import {
  CollectionOrdersClient,
  CreateCollectionOrderCommand,
  ApproveCollectionOrderCommand,
  CollectionOrderStatus,
} from '@/web-api-client';

const client = new CollectionOrdersClient();

export function useCollectionOrders(filters?: {
  revenueClaimId?: number;
  status?: CollectionOrderStatus;
}) {
  return useQuery({
    queryKey: ['collection-orders', 'list', filters],
    queryFn: () => client.collectionOrdersAll(filters?.revenueClaimId, filters?.status),
  });
}

export function useCollectionOrder(id: number) {
  return useQuery({
    queryKey: ['collection-orders', 'detail', id],
    queryFn: () =>
      client.collectionOrdersAll(undefined, undefined).then((list) => list.find((o) => o.id === id)),
    enabled: id > 0,
  });
}

export function useCreateCollectionOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCollectionOrderCommand) => client.collectionOrders(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['collection-orders'] }),
    onError: handleLifecycleError,
  });
}

export function useApproveCollectionOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string; reason?: string }) =>
      client.approvePOST4(
        args.id,
        new ApproveCollectionOrderCommand({ id: args.id, rowVersion: args.rowVersion, reason: args.reason }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['collection-orders'] }),
    onError: handleLifecycleError,
  });
}
