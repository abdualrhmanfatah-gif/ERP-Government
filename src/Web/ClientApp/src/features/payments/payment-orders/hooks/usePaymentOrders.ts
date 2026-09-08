import { useQuery } from '@tanstack/react-query';
import {
  PaymentOrdersClient,
  PaymentOrderStatus,
} from '../../../../web-api-client';

const client = new PaymentOrdersClient();

export function usePaymentOrdersList(filters?: {
  status?: PaymentOrderStatus | null;
  vendorId?: number | null;
  fundId?: number | null;
}) {
  return useQuery({
    queryKey: ['payment-orders', filters],
    queryFn: () =>
      client.paymentOrdersAll(
        filters?.status ?? undefined,
        filters?.vendorId ?? undefined,
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
