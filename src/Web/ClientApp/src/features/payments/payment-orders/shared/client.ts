import { PaymentOrdersClient as generatedClient } from '../../../../web-api-client';

export const paymentOrdersClient = {
  list: (filters?: { status?: number; fundId?: number }) =>
    generatedClient.paymentOrdersAll(filters?.status, filters?.fundId),
  getById: (id: number) => generatedClient.paymentOrdersGET(id),
  getTotals: (id: number) => generatedClient.totals(id),
  create: (cmd: any) => generatedClient.paymentOrdersPOST(cmd),
  update: (id: number, cmd: any) => generatedClient.paymentOrdersPUT(id, cmd),
  submit: (id: number) => generatedClient.submit(id),
  approve: (id: number, cmd: any) => generatedClient.approve(id, cmd),
  reject: (id: number, cmd: any) => generatedClient.reject(id, cmd),
  cancel: (id: number, cmd: any) => generatedClient.cancel(id, cmd),
  sendToTreasury: (id: number, cmd: any) => generatedClient.sendToTreasury(id, cmd),
  void: (id: number, cmd: any) => generatedClient.void(id, cmd),
  exportPaymentOrderPdf: (id: number) =>
    `/api/PaymentOrders/${id}/export-pdf`,
};
