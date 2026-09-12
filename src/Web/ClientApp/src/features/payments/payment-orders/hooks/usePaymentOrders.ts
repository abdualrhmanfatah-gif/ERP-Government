import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  PaymentOrdersClient,
  PaymentOrderStatus,
  CreatePaymentOrderCommand,
  UpdatePaymentOrderCommand,
} from '../../../../web-api-client';

const client = new PaymentOrdersClient();

export function useCreatePaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: CreatePaymentOrderCommand) => client.paymentOrdersPOST(cmd),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
    },
  });
}

export function useUpdatePaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, command }: { id: number; command: UpdatePaymentOrderCommand }) =>
      client.paymentOrdersPUT(id, command),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function usePaymentOrdersList(filters?: {
  status?: PaymentOrderStatus | null;
  fundId?: number | null;
}) {
  return useQuery({
    queryKey: ['payment-orders', filters],
    queryFn: () =>
      client.paymentOrdersAll(
        filters?.status ?? undefined,
        undefined,
        filters?.fundId ?? undefined,
      ),
  });
}

export function usePaymentOrderDetail(id: number) {
  return useQuery({
    queryKey: ['payment-orders', id],
    queryFn: () => client.paymentOrdersGET(id),
    enabled: Number.isFinite(id),
  });
}

export function usePaymentOrderTotals(id: number) {
  return useQuery({
    queryKey: ['payment-orders', id, 'totals'],
    queryFn: () => client.totals(id),
    enabled: Number.isFinite(id),
  });
}

export function useSubmitPaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; fundId: number; budgetItemAllocationId: number; accountId?: number; rowVersion: string }) =>
      client.submitPOST2(cmd.id, { id: cmd.id, fundId: cmd.fundId, budgetItemAllocationId: cmd.budgetItemAllocationId, accountId: cmd.accountId, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function useApprovePaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; rowVersion: string }) =>
      client.approvePOST6(cmd.id, { id: cmd.id, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function useRejectPaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; rejectionReason: string; rowVersion: string }) =>
      client.rejectPOST3(cmd.id, { id: cmd.id, rejectionReason: cmd.rejectionReason, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function useCancelPaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; cancellationReason: string; rowVersion: string }) =>
      client.cancelPOST5(cmd.id, { id: cmd.id, cancellationReason: cmd.cancellationReason, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function useSendToTreasuryPaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; rowVersion: string }) =>
      client.sendToTreasury(cmd.id, { id: cmd.id, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}

export function useVoidPaymentOrder() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cmd: { id: number; voidReason: string; rowVersion: string }) =>
      client.void(cmd.id, { id: cmd.id, voidReason: cmd.voidReason, rowVersion: cmd.rowVersion }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['payment-orders'] });
      qc.invalidateQueries({ queryKey: ['payment-orders', variables.id] });
    },
  });
}
