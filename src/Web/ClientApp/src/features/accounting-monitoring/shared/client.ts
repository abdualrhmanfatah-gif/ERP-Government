import {
  AccountingEventsClient,
  PostingRulesClient,
} from '../../../web-api-client';
import type { AccountBalanceDto } from './types';

export const accountingEventsClient = new AccountingEventsClient();
export const postingRulesClient = new PostingRulesClient();

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(text || `HTTP ${response.status}`);
  }
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

function buildQuery(params: Record<string, unknown>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      search.append(key, String(value));
    }
  }
  const qs = search.toString();
  return qs ? `?${qs}` : '';
}

export const accountingBalancesClient = {
  async accountingBalances(
    fiscalYearId: number,
    fiscalPeriodId: number,
    accountId?: number,
    currencyId?: number,
  ): Promise<AccountBalanceDto[]> {
    return handleResponse(
      await fetch(
        `/api/AccountingBalances${buildQuery({ fiscalYearId, fiscalPeriodId, accountId, currencyId })}`,
      ),
    );
  },

  async rebuild(data: { fiscalYearId: number; fiscalPeriodId?: number }): Promise<void> {
    const res = await fetch('/api/AccountingBalances/rebuild', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    await handleResponse<unknown>(res);
  },

  async finalize(data: { fiscalYearId: number; fiscalPeriodId: number }): Promise<void> {
    const res = await fetch('/api/AccountingBalances/finalize', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    await handleResponse<unknown>(res);
  },

  async unfinalize(data: { fiscalYearId: number; fiscalPeriodId: number }): Promise<void> {
    const res = await fetch('/api/AccountingBalances/unfinalize', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    await handleResponse<unknown>(res);
  },
};
