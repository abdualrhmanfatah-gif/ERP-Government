// Re-export from payments-level shared types (single source of truth)
export {
  PaymentOrderStatus,
  paymentOrderStatusLabels,
  paymentOrderStatusOptions,
  booleanLabels,
} from '../../shared/types';

import { DeductionType } from '../../../../web-api-client';

export const deductionTypeLabels: Record<DeductionType, string> = {
  [DeductionType.Tax]: 'ضريبة',
  [DeductionType.WithholdingTax]: 'ضريبة احتباس',
  [DeductionType.Insurance]: 'تأمين',
  [DeductionType.Penalty]: 'غرامة',
  [DeductionType.AdvanceRecovery]: 'استرداد دفعة مقدمة',
  [DeductionType.LegalDeduction]: 'خصم قانوني',
  [DeductionType.Other]: 'أخرى',
};
