import {
  ChecksClient,
  CheckDto,
  CheckDetailDto,
  CheckStatus,
  ClearCheckCommand,
  BounceCheckCommand,
  ReplaceCheckCommand,
} from '@/web-api-client';

const client = new ChecksClient();

export async function getChecks(params: {
  from: string;
  to: string;
  status?: string;
}): Promise<CheckDto[]> {
  const status = params.status ? (params.status as CheckStatus) : undefined;
  return client.checksAll(new Date(params.from), new Date(params.to), status);
}

export async function getCheckById(id: number): Promise<CheckDetailDto> {
  return client.checks(id);
}

export async function clearCheck(
  id: number,
  data: { clearedAt: string; rowVersion: string }
): Promise<void> {
  await client.clear(id, new ClearCheckCommand({
    clearedAt: new Date(data.clearedAt),
    rowVersion: data.rowVersion,
  }));
}

export async function bounceCheck(
  id: number,
  data: { bouncedAt: string; reason?: string; rowVersion: string }
): Promise<void> {
  await client.bounce(id, new BounceCheckCommand({
    bouncedAt: new Date(data.bouncedAt),
    reason: data.reason,
    rowVersion: data.rowVersion,
  }));
}

export async function replaceCheck(
  id: number,
  data: {
    paymentMethod: string;
    voucherDate: string;
    checkDetails?: { bankName: string; checkNumber: string; checkDate: string; amount: number };
    rowVersion: string;
  }
): Promise<void> {
  await client.replace(id, new ReplaceCheckCommand({
    checkId: id,
    paymentMethod: data.paymentMethod as any,
    voucherDate: new Date(data.voucherDate),
    checkDetails: data.checkDetails
      ? {
          bankName: data.checkDetails.bankName,
          checkNumber: data.checkDetails.checkNumber,
          checkDate: new Date(data.checkDetails.checkDate),
          amount: data.checkDetails.amount,
        }
      : undefined,
    rowVersion: data.rowVersion,
  }));
}

export type { CheckDto, CheckDetailDto };
