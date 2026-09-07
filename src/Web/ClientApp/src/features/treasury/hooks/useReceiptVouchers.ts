// Receipt voucher hooks — generated NSwag client + React Query.
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ReceiptVouchersClient,
  SubmitReceiptVoucherCommand,
  ApproveReceiptVoucherCommand,
  CancelReceiptVoucherCommand,
  CreateReceiptVoucherCommand,
  PaymentMethod,
  ReceiptVoucherStatus,
} from '../../../web-api-client';

const client = new ReceiptVouchersClient();

export function useReceiptVouchers(filters?: {
  partyId?: number;
  paymentMethod?: PaymentMethod;
  status?: ReceiptVoucherStatus;
  fromDate?: Date;
  toDate?: Date;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: ['receipt-vouchers', 'list', filters],
    queryFn: () =>
      client.receiptVouchersAll(
        filters?.partyId,
        filters?.paymentMethod,
        filters?.status,
        filters?.fromDate,
        filters?.toDate,
        filters?.page ?? 1,
        filters?.pageSize ?? 20,
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
  });
}

export function useSubmitReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string }) =>
      client.submitPOST(args.id, new SubmitReceiptVoucherCommand({ id: args.id, rowVersion: args.rowVersion })),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
  });
}

export function useApproveReceiptVoucher() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (args: { id: number; rowVersion: string; reason?: string | undefined }) =>
      client.approvePOST5(
        args.id,
        new ApproveReceiptVoucherCommand({ id: args.id, rowVersion: args.rowVersion, reason: args.reason }),
      ),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['receipt-vouchers'] }),
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
  });
}
