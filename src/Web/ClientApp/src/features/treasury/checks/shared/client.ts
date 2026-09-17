import {
  ChecksClient,
  CheckDto,
  CheckDetailDto,
  ClearCheckCommand,
  BounceCheckCommand,
} from '@/web-api-client';

const client = new ChecksClient();

export async function getChecks(_params: {
  from: string;
  to: string;
  status?: string;
}): Promise<CheckDto[]> {
  // API has no list endpoint for checks — returns empty until endpoint is added
  return [];
}

export async function getCheckById(_id: number): Promise<CheckDetailDto> {
  // API has no GET endpoint for check detail — returns empty until endpoint is added
  return new CheckDetailDto();
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
  _id: number,
  _data: {
    paymentMethod: string;
    voucherDate: string;
    checkDetails?: { bankName: string; checkNumber: string; checkDate: string; amount: number };
    rowVersion: string;
  }
): Promise<void> {
  // API has no replace endpoint — stub until endpoint is added
  throw new Error('استبدال الشيك غير مدعوم حالياً');
}

export type { CheckDto, CheckDetailDto };
