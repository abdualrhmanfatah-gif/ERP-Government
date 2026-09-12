import type {
  PartyResponse,
  CreatePartyCommand,
  UpdatePartyCommand,
  PartyFilters,
  PartyDocumentResponse,
} from './types';

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problemDetails: { status?: number; title?: string; detail?: string },
  ) {
    super(problemDetails.detail ?? problemDetails.title ?? `HTTP ${status}`);
    this.name = 'ApiError';
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let problemDetails: { status?: number; title?: string; detail?: string };
    try {
      problemDetails = await response.json();
    } catch {
      problemDetails = { status: response.status, detail: response.statusText };
    }
    throw new ApiError(response.status, problemDetails);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

function buildQuery(params: Record<string, unknown>): string {
  const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== null);
  if (entries.length === 0) return '';
  return '?' + new URLSearchParams(entries.map(([k, v]) => [k, String(v)])).toString();
}

export const partiesKeys = {
  all: ['parties'] as const,
  lists: () => [...partiesKeys.all, 'list'] as const,
  list: (filters?: PartyFilters) => [...partiesKeys.lists(), filters] as const,
  details: () => [...partiesKeys.all, 'detail'] as const,
  detail: (id: number) => [...partiesKeys.details(), id] as const,
  documents: (id: number) => [...partiesKeys.detail(id), 'documents'] as const,
};

export const partiesClient = {
  async list(filters?: PartyFilters): Promise<PartyResponse[]> {
    return handleResponse(await fetch(`/api/Parties${buildQuery(filters ?? {})}`));
  },

  async getById(id: number): Promise<PartyResponse> {
    return handleResponse(await fetch(`/api/Parties/${id}`));
  },

  async create(data: CreatePartyCommand): Promise<number> {
    return handleResponse(await fetch('/api/Parties', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }));
  },

  async update(id: number, data: UpdatePartyCommand): Promise<void> {
    return handleResponse(await fetch(`/api/Parties/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    }));
  },

  async toggleActive(id: number): Promise<void> {
    return handleResponse(await fetch(`/api/Parties/${id}/toggle-active`, {
      method: 'PATCH',
    }));
  },

  async getDocuments(id: number): Promise<PartyDocumentResponse[]> {
    return handleResponse(await fetch(`/api/Parties/${id}/documents`));
  },

  async checkDuplicateTaxNumber(taxNumber: string, excludeId?: number): Promise<boolean> {
    const params = new URLSearchParams({ taxNumber });
    if (excludeId) params.set('excludeId', String(excludeId));
    const result = await handleResponse<{ exists: boolean }>(
      await fetch(`/api/Parties/check-tax-number?${params}`),
    );
    return result.exists;
  },
};
