// Receipt voucher hooks — generated NSwag client + React Query.
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { handleLifecycleError } from '@/shared/api/result-to-ui';
import {
  ReceiptVouchersClient,
  ApproveReceiptVoucherCommand,
  CancelReceiptVoucherCommand,
  CreateReceiptVoucherCommand,
  UpdateReceiptVoucherCommand,
  ReceiptVoucherStatus,
} from '../../../web-api-client';

const client = new ReceiptVouchersClient();

export function useReceiptVouchers(filters?: {
  collectionOrderId?: number;
  partyId?: number;
  status?: ReceiptVoucherStatus;
}) {
  return useQuery({
    queryKey: ['receipt-vouchers', 'list', filters],
    queryFn: () =>
      client.receiptVouchersAll(
        filters?.collectionOrderId,
        filters?.partyId,
        filters?.status,
      ),
  });
}

export function useReceiptVoucher(id: number) {
  return useQuery({
    queryKey: ['receipt-vouchers', 'detail', id],
    queryFn: () => client.receiptVouchersGET(id),
    enabled: Number.isFinite(id) && id > 0,
  });
}

export function useCreateReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateReceiptVoucherCommand) => client.receiptVouchersPOST(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
    onError: handleLifecycleError,
  });
}

export function useUpdateReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateReceiptVoucherCommand) => client.receiptVouchersPUT(data.id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
    onError: handleLifecycleError,
  });
}

export function useSubmitReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string }) =>
      client.approvePOST7(
        args.id,
        new ApproveReceiptVoucherCommand({ id: args.id, rowVersion: args.rowVersion }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
    onError: handleLifecycleError,
  });
}

export function useApproveReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string; reason?: string | undefined }) =>
      client.approvePOST7(
        args.id,
        new ApproveReceiptVoucherCommand({ id: args.id, rowVersion: args.rowVersion, reason: args.reason }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
    onError: handleLifecycleError,
  });
}

export function useCancelReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string; reason: string }) =>
      client.cancelPOST4(
        args.id,
        new CancelReceiptVoucherCommand({ id: args.id, rowVersion: args.rowVersion, reason: args.reason }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
    onError: handleLifecycleError,
  });
}
