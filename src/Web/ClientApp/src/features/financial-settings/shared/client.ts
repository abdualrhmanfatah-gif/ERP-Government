// Financial Settings Shared Client — typed API functions + cache key factory
// Mirrors backend endpoints from contracts/financial-settings-api.md

import type {
  FiscalYearDto,
  FiscalPeriodDto,
  DocumentSequenceDto,
  CurrencyDto,
  Iso4217CodeDto,
  ExchangeRateDto,
  ExchangeRateLookupDto,
  ClosingEntryDto,
  CreateFiscalYearCommand,
  UpdateFiscalYearCommand,
  CreateFiscalPeriodCommand,
  BulkGeneratePeriodsCommand,
  CreateDocumentSequenceCommand,
  UpdateDocumentSequenceCommand,
  CreateCurrencyCommand,
  UpdateCurrencyCommand,
  CreateExchangeRateCommand,
  UpdateExchangeRateCommand,
  GenerateYearEndClosingCommand,
  ReverseClosingEntryCommand,
  ActivateDeactivateCommand,
  ProblemDetails,
} from './types';

// ─── Error Handling ───────────────────────────────────────────────────────────

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problemDetails: ProblemDetails,
  ) {
    super(problemDetails.detail ?? problemDetails.title ?? `HTTP ${status}`);
    this.name = 'ApiError';
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let problemDetails: ProblemDetails;
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

// ─── Cache Key Factory ────────────────────────────────────────────────────────

const FS_KEY = 'financial-settings';

export const financialSettingsKeys = {
  all: [FS_KEY] as const,

  fiscalYears: {
    all: [FS_KEY, 'fiscal-years'] as const,
    list: (filters?: Record<string, unknown>) => [FS_KEY, 'fiscal-years', 'list', filters] as const,
    detail: (id: number) => [FS_KEY, 'fiscal-years', 'detail', id] as const,
  },

  fiscalPeriods: {
    all: [FS_KEY, 'fiscal-periods'] as const,
    list: (fiscalYearId: number) => [FS_KEY, 'fiscal-periods', 'list', fiscalYearId] as const,
    detail: (id: number) => [FS_KEY, 'fiscal-periods', 'detail', id] as const,
  },

  documentSequences: {
    all: [FS_KEY, 'document-sequences'] as const,
    list: (filters?: Record<string, unknown>) => [FS_KEY, 'document-sequences', 'list', filters] as const,
    detail: (id: number) => [FS_KEY, 'document-sequences', 'detail', id] as const,
  },

  currencies: {
    all: [FS_KEY, 'currencies'] as const,
    list: (filters?: Record<string, unknown>) => [FS_KEY, 'currencies', 'list', filters] as const,
    detail: (id: number) => [FS_KEY, 'currencies', 'detail', id] as const,
    iso4217: (query?: string) => [FS_KEY, 'currencies', 'iso4217', query] as const,
  },

  exchangeRates: {
    all: [FS_KEY, 'exchange-rates'] as const,
    list: (filters?: Record<string, unknown>) => [FS_KEY, 'exchange-rates', 'list', filters] as const,
    detail: (id: number) => [FS_KEY, 'exchange-rates', 'detail', id] as const,
    lookup: (params: Record<string, unknown>) => [FS_KEY, 'exchange-rates', 'lookup', params] as const,
  },

  closingEntries: {
    all: [FS_KEY, 'closing-entries'] as const,
    list: (fiscalYearId: number) => [FS_KEY, 'closing-entries', 'list', fiscalYearId] as const,
    detail: (id: number) => [FS_KEY, 'closing-entries', 'detail', id] as const,
  },
};

// ─── Fiscal Years Client ─────────────────────────────────────────────────────

export const fiscalYearsClient = {
  async list(isActive?: boolean): Promise<FiscalYearDto[]> {
    return handleResponse(
      await fetch(`/api/FiscalYears${buildQuery({ isActive })}`),
    );
  },

  async getById(id: number): Promise<FiscalYearDto> {
    return handleResponse(await fetch(`/api/FiscalYears/${id}`));
  },

  async create(data: CreateFiscalYearCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/FiscalYears', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateFiscalYearCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/FiscalYears/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async open(id: number): Promise<void> {
    return handleResponse(
      await fetch(`/api/FiscalYears/${id}/open`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  },

  async close(id: number): Promise<void> {
    return handleResponse(
      await fetch(`/api/FiscalYears/${id}/close`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  },

  async byDate(date: string): Promise<{ fiscalYearId: number; fiscalYearCode?: string; fiscalYearName?: string; fiscalPeriodId: number; fiscalPeriodName?: string }> {
    return handleResponse(
      await fetch(`/api/FiscalYears/by-date?date=${encodeURIComponent(date)}`, {
        headers: { Accept: 'application/json' },
      }),
    );
  },
};

// ─── Fiscal Periods Client ───────────────────────────────────────────────────

export const fiscalPeriodsClient = {
  async list(fiscalYearId: number): Promise<FiscalPeriodDto[]> {
    return handleResponse(
      await fetch(`/api/FiscalPeriods${buildQuery({ fiscalYearId })}`),
    );
  },

  async getById(id: number): Promise<FiscalPeriodDto> {
    return handleResponse(await fetch(`/api/FiscalPeriods/${id}`));
  },

  async create(data: CreateFiscalPeriodCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/FiscalPeriods', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async lock(id: number): Promise<void> {
    return handleResponse(
      await fetch(`/api/FiscalPeriods/${id}/lock`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  },

  async unlock(id: number): Promise<void> {
    return handleResponse(
      await fetch(`/api/FiscalPeriods/${id}/unlock`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  },

  async bulkGenerate(data: BulkGeneratePeriodsCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/FiscalPeriods/bulk-generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },
};

// ─── Document Sequences Client ───────────────────────────────────────────────

export const documentSequencesClient = {
  async list(isActive?: boolean): Promise<DocumentSequenceDto[]> {
    return handleResponse(
      await fetch(`/api/DocumentSequences${buildQuery({ isActive })}`),
    );
  },

  async getById(id: number): Promise<DocumentSequenceDto> {
    return handleResponse(await fetch(`/api/DocumentSequences/${id}`));
  },

  async create(data: CreateDocumentSequenceCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/DocumentSequences', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateDocumentSequenceCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/DocumentSequences/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async deactivate(id: number, rowVersion: string): Promise<void> {
    return handleResponse(
      await fetch(`/api/DocumentSequences/${id}/deactivate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      }),
    );
  },
};

// ─── Currencies Client ───────────────────────────────────────────────────────

export const currenciesClient = {
  async list(isActive?: boolean): Promise<CurrencyDto[]> {
    return handleResponse(
      await fetch(`/api/Currencies${buildQuery({ isActive })}`),
    );
  },

  async getById(id: number): Promise<CurrencyDto> {
    return handleResponse(await fetch(`/api/Currencies/${id}`));
  },

  async getIso4217(query?: string): Promise<Iso4217CodeDto[]> {
    return handleResponse(
      await fetch(`/api/Currencies/iso4217${buildQuery({ query })}`),
    );
  },

  async create(data: CreateCurrencyCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/Currencies', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateCurrencyCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/Currencies/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async activate(data: ActivateDeactivateCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/Currencies/${data.id}/activate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async deactivate(data: ActivateDeactivateCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/Currencies/${data.id}/deactivate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },
};

// ─── Exchange Rates Client ───────────────────────────────────────────────────

export const exchangeRatesClient = {
  async list(filters?: {
    currencyId?: number;
    rateType?: number;
    fromDate?: string;
    toDate?: string;
    isActive?: boolean;
  }): Promise<ExchangeRateDto[]> {
    return handleResponse(
      await fetch(`/api/ExchangeRates${buildQuery(filters ?? {})}`),
    );
  },

  async getById(id: number): Promise<ExchangeRateDto> {
    return handleResponse(await fetch(`/api/ExchangeRates/${id}`));
  },

  async lookup(params: {
    baseCurrencyId: number;
    currencyId: number;
    date: string;
  }): Promise<ExchangeRateLookupDto> {
    return handleResponse(
      await fetch(`/api/ExchangeRates/lookup${buildQuery(params)}`),
    );
  },

  async create(data: CreateExchangeRateCommand): Promise<void> {
    return handleResponse(
      await fetch('/api/ExchangeRates', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateExchangeRateCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/ExchangeRates/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async activate(data: ActivateDeactivateCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/ExchangeRates/${data.id}/activate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async deactivate(data: ActivateDeactivateCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/ExchangeRates/${data.id}/deactivate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },
};

// ─── Closing Entries Client ──────────────────────────────────────────────────

export const closingEntriesClient = {
  async listByFiscalYear(fiscalYearId: number): Promise<ClosingEntryDto[]> {
    return handleResponse(
      await fetch(`/api/ClosingEntries/by-fiscal-year/${fiscalYearId}`),
    );
  },

  async getById(id: number): Promise<ClosingEntryDto> {
    return handleResponse(await fetch(`/api/ClosingEntries/${id}`));
  },

  async generate(data: GenerateYearEndClosingCommand): Promise<number> {
    return handleResponse(
      await fetch('/api/ClosingEntries/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async approve(id: number): Promise<void> {
    return handleResponse(
      await fetch(`/api/ClosingEntries/${id}/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      }),
    );
  },

  async reverse(id: number, data: ReverseClosingEntryCommand): Promise<number> {
    return handleResponse(
      await fetch(`/api/ClosingEntries/${id}/reverse`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },
};
