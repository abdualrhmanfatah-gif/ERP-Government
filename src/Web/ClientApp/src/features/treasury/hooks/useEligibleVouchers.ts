// Eligible voucher picker hook — FR-001 pre-filter: Approved + unassigned + method-matching
import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ReceiptVouchersClient, PaymentMethod, ReceiptVoucherStatus } from '../../../web-api-client';
import type { ReceiptVoucherDto } from '../shared/types';

const client = new ReceiptVouchersClient();

export function useEligibleVouchers(formType: 'Form47' | 'Form48') {
  const targetMethod = formType === 'Form47' ? PaymentMethod.Cash : PaymentMethod.Check;

  const { data: allApproved = [], isLoading } = useQuery({
    queryKey: ['receipt-vouchers', 'eligible', formType],
    queryFn: () =>
      client.receiptVouchersAll(
        undefined,
        targetMethod,
        ReceiptVoucherStatus.Approved,
        undefined,
        undefined,
        1,
        200,
      ),
  });

  const eligible = useMemo(
    () => allApproved.filter((v) => !v.depositSlipId),
    [allApproved],
  );

  return { eligible, isLoading };
}
