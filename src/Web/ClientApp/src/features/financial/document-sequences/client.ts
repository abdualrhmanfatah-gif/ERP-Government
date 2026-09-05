import type { DocumentSequenceDto, CreateDocumentSequenceCommand, UpdateDocumentSequenceCommand } from './types';

const BASE = '/api/DocumentSequences';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

export const documentSequencesClient = {
  async list(isActive?: boolean): Promise<DocumentSequenceDto[]> {
    const url = isActive !== undefined ? `${BASE}?IsActive=${isActive}` : BASE;
    return handleResponse<DocumentSequenceDto[]>(
      await fetch(url, { headers: { Accept: 'application/json' } })
    );
  },

  async getById(id: number): Promise<DocumentSequenceDto> {
    return handleResponse<DocumentSequenceDto>(
      await fetch(`${BASE}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },

  async create(data: CreateDocumentSequenceCommand): Promise<DocumentSequenceDto> {
    return handleResponse<DocumentSequenceDto>(
      await fetch(BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async update(data: UpdateDocumentSequenceCommand): Promise<DocumentSequenceDto> {
    return handleResponse<DocumentSequenceDto>(
      await fetch(`${BASE}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },

  async deactivate(id: number): Promise<void> {
    await fetch(`${BASE}/${id}/deactivate`, {
      method: 'POST',
      headers: { Accept: 'application/json' },
    });
  },
};
