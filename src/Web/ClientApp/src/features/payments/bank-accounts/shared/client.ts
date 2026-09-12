import { BankAccountsClient as generatedClient } from '../../../../web-api-client';

export const bankAccountsClient = {
  list: () => generatedClient.bankAccountsAll(),
  getById: (id: number) => generatedClient.bankAccountsGET(id),
  create: (cmd: any) => generatedClient.bankAccountsPOST(cmd),
  update: (id: number, cmd: any) => generatedClient.bankAccountsPUT(id, cmd),
  activate: (id: number, cmd: any) => generatedClient.activate(id, cmd),
  deactivate: (id: number, cmd: any) => generatedClient.deactivate(id, cmd),
};
