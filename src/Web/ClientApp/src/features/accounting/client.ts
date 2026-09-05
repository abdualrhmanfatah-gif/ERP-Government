import type {
  MoveDto,
  CreateMoveCommand,
  UpdateMoveCommand,
  CreateMoveLineCommand,
  UpdateMoveLineCommand,
} from './types';

const BASE_MOVES = '/api/Moves';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

async function handleVoid(response: Response): Promise<void> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
}

// Moves — per research R7 hand-written client (013 precedent)
export const movesClient = {
  async list(params?: {
    entryStatus?: string;
    journalId?: number;
    fromDate?: string;
    toDate?: string;
    isBalanced?: boolean;
  }): Promise<MoveDto[]> {
    const qs = new URLSearchParams();
    if (params?.entryStatus) qs.set('entryStatus', params.entryStatus);
    if (params?.journalId) qs.set('journalId', String(params.journalId));
    if (params?.fromDate) qs.set('fromDate', params.fromDate);
    if (params?.toDate) qs.set('toDate', params.toDate);
    if (params?.isBalanced !== undefined) qs.set('isBalanced', String(params.isBalanced));
    const query = qs.toString();
    return handleResponse<MoveDto[]>(
      await fetch(`${BASE_MOVES}${query ? `?${query}` : ''}`, {
        headers: { Accept: 'application/json' },
      })
    );
  },

  async getById(id: number): Promise<MoveDto> {
    return handleResponse<MoveDto>(
      await fetch(`${BASE_MOVES}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },

  async create(data: CreateMoveCommand): Promise<{ id: number; entryNumber?: string }> {
    return handleResponse<{ id: number; entryNumber?: string }>(
      await fetch(BASE_MOVES, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async update(id: number, data: UpdateMoveCommand & { rowVersion: string }): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async cancel(id: number, rowVersion: string): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${id}/cancel`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      })
    );
  },

  async submit(id: number, rowVersion: string): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${id}/submit`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      })
    );
  },

  async approve(id: number, rowVersion: string): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${id}/approve`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      })
    );
  },

  async post(id: number, rowVersion: string): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${id}/post`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, rowVersion }),
      })
    );
  },

  async reverse(id: number, reversalReason: string, rowVersion: string): Promise<{ reversalId: number }> {
    return handleResponse<{ reversalId: number }>(
      await fetch(`${BASE_MOVES}/${id}/reverse`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ id, reversalReason, rowVersion }),
      })
    );
  },
};

// MoveLines — nested under Moves per DEC-002
export const moveLinesClient = {
  async create(moveId: number, data: CreateMoveLineCommand): Promise<{ lineId: number }> {
    return handleResponse<{ lineId: number }>(
      await fetch(`${BASE_MOVES}/${moveId}/lines`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ ...data, moveId }),
      })
    );
  },

  async update(moveId: number, lineId: number, data: UpdateMoveLineCommand): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${moveId}/lines/${lineId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async remove(moveId: number, lineId: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_MOVES}/${moveId}/lines/${lineId}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};
