import type { ClosingEntryDto } from './types';

const BASE = '/api/ClosingEntries';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

export const closingEntriesClient = {
  async listByFiscalYear(fiscalYearId: number): Promise<ClosingEntryDto[]> {
    return handleResponse<ClosingEntryDto[]>(
      await fetch(`${BASE}/by-fiscal-year/${fiscalYearId}`, {
        headers: { Accept: 'application/json' },
      })
    );
  },

  async getById(id: number): Promise<ClosingEntryDto> {
    return handleResponse<ClosingEntryDto>(
      await fetch(`${BASE}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },

  async generate(data: {
    fiscalYearId: number;
    description?: string;
  }): Promise<number> {
    return handleResponse<number>(
      await fetch(`${BASE}/generate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async approve(id: number): Promise<void> {
    await handleResponse<void>(
      await fetch(`${BASE}/${id}/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id }),
      })
    );
  },

  async reverse(id: number, reason?: string): Promise<number> {
    return handleResponse<number>(
      await fetch(`${BASE}/${id}/reverse`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, reason }),
      })
    );
  },
};
