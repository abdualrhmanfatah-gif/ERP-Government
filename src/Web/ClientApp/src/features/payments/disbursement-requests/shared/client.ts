import type { PartyFilters } from '@/features/parties/shared/types';
import { partiesClient } from '@/features/parties/shared/client';

function buildQuery(params: Record<string, unknown>): string {
  const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== null);
  if (entries.length === 0) return '';
  return '?' + new URLSearchParams(entries.map(([k, v]) => [k, String(v)])).toString();
}

export const disbursementRequestsClient = {
  list: async (filters?: { status?: number; requestedById?: number }) => {
    const res = await fetch(`/api/DisbursementRequests${buildQuery(filters ?? {})}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
  getById: async (id: number) => {
    const res = await fetch(`/api/DisbursementRequests/${id}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
  create: async (cmd: any) => {
    const res = await fetch('/api/DisbursementRequests', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  update: async (id: number, cmd: any) => {
    const res = await fetch(`/api/DisbursementRequests/${id}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  submit: async (id: number) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/submit`, { method: 'PATCH' });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  approve: async (id: number, cmd: any) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/approve`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      console.error('[approve]', res.status, text, cmd);
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  reject: async (id: number, cmd: any) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/reject`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  cancel: async (id: number, cmd: any) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/cancel`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  createAccrualEntry: async (id: number, cmd: any) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/accrual-entry`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
      body: JSON.stringify(cmd),
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
  getAccrualEntry: async (id: number) => {
    const res = await fetch(`/api/DisbursementRequests/${id}/accrual-entry`);
    if (!res.ok) {
      if (res.status === 404) return null;
      const text = await res.text();
      throw new Error(text || `HTTP ${res.status}`);
    }
    return res.json();
  },
};

export const partiesForBeneficiaryClient = {
  search: (filters?: PartyFilters) => partiesClient.list(filters),
};
