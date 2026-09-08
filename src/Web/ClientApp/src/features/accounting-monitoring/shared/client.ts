import {
  AccountingEventsClient,
  PostingRulesClient,
} from '../../../web-api-client';

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

