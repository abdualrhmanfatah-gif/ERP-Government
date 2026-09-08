import { PaymentOrderStatus, BudgetCheckStatus } from '../../../../web-api-client';

export { PaymentOrderStatus, BudgetCheckStatus };

export const paymentOrderStatusLabels: Record<string, string> = {
  [PaymentOrderStatus.Draft]: 'مسودة',
  [PaymentOrderStatus.Submitted]: 'مرسلة',
  [PaymentOrderStatus.Approved]: 'موافق عليها',
  [PaymentOrderStatus.SentToTreasury]: 'مرسلة للخزينة',
  [PaymentOrderStatus.Paid]: 'مدفوعة',
  [PaymentOrderStatus.PartiallyPaid]: 'مدفوعة جزئياً',
  [PaymentOrderStatus.Cancelled]: 'ملغاة',
  [PaymentOrderStatus.Rejected]: 'مرفوضة',
  [PaymentOrderStatus.Voided]: 'ملغاة نهائياً',
};

export const budgetCheckStatusLabels: Record<string, string> = {
  [BudgetCheckStatus.Pending]: 'قيد الفحص',
  [BudgetCheckStatus.Passed]: 'ناجح',
  [BudgetCheckStatus.Failed]: 'فاشل',
  [BudgetCheckStatus.Overridden]: 'تم التجاوز',
};
