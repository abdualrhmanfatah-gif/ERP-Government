// Budgeting Shared Client — typed API functions + cache key factory
// Mirrors backend endpoints from specs/010-rebuild-budgeting-module/contracts/budgeting-api.md

import type {
  BudgetTypeDto,
  FundDto,
  BudgetClassificationDto,
  BudgetClassificationTreeDto,
  BudgetDto,
  BudgetItemDto,
  AppropriationDto,
  ItemAvailabilityDto,
  EncumbranceAvailabilityDto,
  CreateBudgetTypeCommand,
  UpdateBudgetTypeCommand,
  ToggleBudgetTypeActiveCommand,
  CreateFundCommand,
  UpdateFundCommand,
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

const BUDGETING_KEY = 'budgeting';

export const budgetingKeys = {
  all: [BUDGETING_KEY] as const,

  budgetTypes: {
    all: [BUDGETING_KEY, 'budget-types'] as const,
    list: (filters?: Record<string, unknown>) => [BUDGETING_KEY, 'budget-types', 'list', filters] as const,
    detail: (id: number) => [BUDGETING_KEY, 'budget-types', 'detail', id] as const,
  },

  funds: {
    all: [BUDGETING_KEY, 'funds'] as const,
    list: (filters?: Record<string, unknown>) => [BUDGETING_KEY, 'funds', 'list', filters] as const,
    detail: (id: number) => [BUDGETING_KEY, 'funds', 'detail', id] as const,
  },

  budgetClassifications: {
    all: [BUDGETING_KEY, 'budget-classifications'] as const,
    tree: () => [BUDGETING_KEY, 'budget-classifications', 'tree'] as const,
    detail: (id: number) => [BUDGETING_KEY, 'budget-classifications', 'detail', id] as const,
  },

  budgets: {
    all: [BUDGETING_KEY, 'budgets'] as const,
    list: (filters?: Record<string, unknown>) => [BUDGETING_KEY, 'budgets', 'list', filters] as const,
    detail: (id: number) => [BUDGETING_KEY, 'budgets', 'detail', id] as const,
    tree: (id: number) => [BUDGETING_KEY, 'budgets', 'tree', id] as const,
  },

  appropriations: {
    all: [BUDGETING_KEY, 'appropriations'] as const,
    list: (filters?: Record<string, unknown>) => [BUDGETING_KEY, 'appropriations', 'list', filters] as const,
    detail: (id: number) => [BUDGETING_KEY, 'appropriations', 'detail', id] as const,
    availability: (budgetItemId: number) => [BUDGETING_KEY, 'appropriations', 'availability', budgetItemId] as const,
  },

  encumbrances: {
    all: [BUDGETING_KEY, 'encumbrances'] as const,
    list: (filters?: Record<string, unknown>) => [BUDGETING_KEY, 'encumbrances', 'list', filters] as const,
    detail: (id: number) => [BUDGETING_KEY, 'encumbrances', 'detail', id] as const,
    availability: (appropriationId: number) => [BUDGETING_KEY, 'encumbrances', 'availability', appropriationId] as const,
  },
};

// ─── BudgetTypes Client ───────────────────────────────────────────────────────

export const budgetTypesClient = {
  async list(isActive?: boolean): Promise<BudgetTypeDto[]> {
    return handleResponse(
      await fetch(`/api/BudgetTypes${buildQuery({ isActive })}`),
    );
  },

  async getById(id: number): Promise<BudgetTypeDto> {
    return handleResponse(await fetch(`/api/BudgetTypes/${id}`));
  },

  async create(data: CreateBudgetTypeCommand): Promise<{ id: number }> {
    return handleResponse(
      await fetch('/api/BudgetTypes', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateBudgetTypeCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/BudgetTypes/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async toggleActive(id: number, data: ToggleBudgetTypeActiveCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/BudgetTypes/${id}/toggle-active`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },
};

// ─── Funds Client ─────────────────────────────────────────────────────────────

export const fundsClient = {
  async list(isActive?: boolean): Promise<FundDto[]> {
    return handleResponse(
      await fetch(`/api/Funds${buildQuery({ isActive })}`),
    );
  },

  async getById(id: number): Promise<FundDto> {
    return handleResponse(await fetch(`/api/Funds/${id}`));
  },

  async create(data: CreateFundCommand): Promise<{ id: number }> {
    return handleResponse(
      await fetch('/api/Funds', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: UpdateFundCommand): Promise<void> {
    return handleResponse(
      await fetch(`/api/Funds/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async activate(id: number, rowVersion: string): Promise<void> {
    return handleResponse(
      await fetch(`/api/Funds/${id}/activate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      }),
    );
  },

  async deactivate(id: number, rowVersion: string): Promise<void> {
    return handleResponse(
      await fetch(`/api/Funds/${id}/deactivate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      }),
    );
  },
};

// ─── BudgetClassifications Client ─────────────────────────────────────────────

export const budgetClassificationsClient = {
  async tree(): Promise<BudgetClassificationTreeDto[]> {
    return handleResponse(await fetch('/api/BudgetClassifications/tree'));
  },

  async getById(id: number): Promise<BudgetClassificationDto> {
    return handleResponse(await fetch(`/api/BudgetClassifications/${id}`));
  },

  async create(data: { code: string; name: string; parentId?: number }): Promise<{ id: number }> {
    return handleResponse(
      await fetch('/api/BudgetClassifications', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: { id: number; rowVersion: string; code: string; name: string; parentId?: number }): Promise<void> {
    return handleResponse(
      await fetch(`/api/BudgetClassifications/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async toggleActive(id: number, data: { id: number; rowVersion: string; isActive: boolean }): Promise<void> {
    return handleResponse(
      await fetch(`/api/BudgetClassifications/${id}/toggle-active`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },
};

// ─── Budgets Client ───────────────────────────────────────────────────────────

export const budgetsClient = {
  async list(filters?: Record<string, unknown>): Promise<BudgetDto[]> {
    return handleResponse(await fetch(`/api/Budgets${buildQuery(filters ?? {})}`));
  },

  async getById(id: number): Promise<BudgetDto> {
    return handleResponse(await fetch(`/api/Budgets/${id}`));
  },

  async create(data: Record<string, unknown>): Promise<{ id: number }> {
    return handleResponse(
      await fetch('/api/Budgets', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: Record<string, unknown>): Promise<void> {
    return handleResponse(
      await fetch(`/api/Budgets/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async transition(id: number, action: string, data: Record<string, unknown>): Promise<void> {
    return handleResponse(
      await fetch(`/api/Budgets/${id}/${action}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async getItemsTree(budgetId: number): Promise<BudgetItemDto[]> {
    return handleResponse(await fetch(`/api/Budgets/${budgetId}/items/tree`));
  },

  async createItem(budgetId: number, data: Record<string, unknown>): Promise<{ id: number }> {
    return handleResponse(
      await fetch(`/api/Budgets/${budgetId}/items`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async updateItem(itemId: number, data: Record<string, unknown>): Promise<void> {
    return handleResponse(
      await fetch(`/api/Budgets/items/${itemId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id: itemId }),
      }),
    );
  },

  async deleteItem(itemId: number, rowVersion: string): Promise<void> {
    return handleResponse(
      await fetch(`/api/Budgets/items/${itemId}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ rowVersion }),
      }),
    );
  },
};

// ─── Appropriations Client ────────────────────────────────────────────────────

export const appropriationsClient = {
  async list(filters?: Record<string, unknown>): Promise<AppropriationDto[]> {
    return handleResponse(await fetch(`/api/Appropriations${buildQuery(filters ?? {})}`));
  },

  async getById(id: number): Promise<AppropriationDto> {
    return handleResponse(await fetch(`/api/Appropriations/${id}`));
  },

  async getAvailability(budgetItemId: number): Promise<ItemAvailabilityDto> {
    return handleResponse(await fetch(`/api/Appropriations/availability?budgetItemId=${budgetItemId}`));
  },

  async create(data: Record<string, unknown>): Promise<{ id: number }> {
    return handleResponse(
      await fetch('/api/Appropriations', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      }),
    );
  },

  async update(id: number, data: Record<string, unknown>): Promise<void> {
    return handleResponse(
      await fetch(`/api/Appropriations/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },

  async delete(id: number, rowVersion: string): Promise<void> {
    return handleResponse(
      await fetch(`/api/Appropriations/${id}`, {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ rowVersion }),
      }),
    );
  },

  async transition(id: number, action: string, data: Record<string, unknown>): Promise<void> {
    return handleResponse(
      await fetch(`/api/Appropriations/${id}/${action}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...data, id }),
      }),
    );
  },
};

// ─── Encumbrances Client ──────────────────────────────────────────────────────

export const encumbrancesClient = {
  async getAvailability(appropriationId: number): Promise<EncumbranceAvailabilityDto> {
    return handleResponse(await fetch(`/api/Encumbrances/availability?appropriationId=${appropriationId}`));
  },
};
