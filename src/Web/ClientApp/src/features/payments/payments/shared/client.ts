import { PaymentsClient } from '../../../../web-api-client';

const generatedClient = new PaymentsClient();

export const paymentsClient = {
  list: (filters?: { method?: string; status?: string; from?: string; to?: string }) =>
    generatedClient.paymentsAll(filters?.method as any, filters?.status as any, filters?.from, filters?.to),
  getById: (id: number) => generatedClient.paymentsGET(id),
  record: (cmd: any) => generatedClient.paymentsPOST(cmd),
};
