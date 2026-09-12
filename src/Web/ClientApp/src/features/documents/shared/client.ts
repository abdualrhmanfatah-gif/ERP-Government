import type {
  ApprovalRecord,
  StatusLogRecord,
  AttachmentRecord,
  AttachmentRequirement,
  AttachmentGateCheckResponse,
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

export const documentsKeys = {
  all: ['documents'] as const,
  approvals: (type: string, id: number) => [...documentsKeys.all, 'approvals', type, id] as const,
  statusLog: (type: string, id: number) => [...documentsKeys.all, 'statusLog', type, id] as const,
  attachments: (type: string, id: number) => [...documentsKeys.all, 'attachments', type, id] as const,
  requirements: (type: string) => [...documentsKeys.all, 'requirements', type] as const,
  gateCheck: (type: string, id: number) => [...documentsKeys.all, 'gateCheck', type, id] as const,
};

export const documentsClient = {
  async getApprovals(documentType: string, documentId: number): Promise<ApprovalRecord[]> {
    return handleResponse(await fetch(`/api/Documents/${documentType}/${documentId}/approvals`));
  },

  async getStatusLog(documentType: string, documentId: number): Promise<StatusLogRecord[]> {
    return handleResponse(await fetch(`/api/Documents/${documentType}/${documentId}/status-log`));
  },

  async getAttachments(documentType: string, documentId: number): Promise<AttachmentRecord[]> {
    return handleResponse(await fetch(`/api/Documents/${documentType}/${documentId}/attachments`));
  },

  async getRequirements(documentType: string): Promise<AttachmentRequirement[]> {
    return handleResponse(await fetch(`/api/Documents/${documentType}/attachment-requirements`));
  },

  async checkGate(documentType: string, documentId: number): Promise<AttachmentGateCheckResponse> {
    return handleResponse(await fetch(`/api/Documents/${documentType}/${documentId}/attachment-gate-check`));
  },

  async upload(
    documentType: string,
    documentId: number,
    file: File,
    attachmentTypeCode?: string,
  ): Promise<number> {
    const formData = new FormData();
    formData.append('file', file);
    const query = attachmentTypeCode ? `?attachmentTypeCode=${encodeURIComponent(attachmentTypeCode)}` : '';
    return handleResponse(
      await fetch(`/api/Documents/${documentType}/${documentId}/attachments${query}`, {
        method: 'POST',
        body: formData,
      }),
    );
  },

  async deleteAttachment(id: number): Promise<void> {
    return handleResponse(await fetch(`/api/Documents/attachments/${id}`, {
      method: 'DELETE',
    }));
  },

  async downloadAttachment(id: number): Promise<Blob> {
    const response = await fetch(`/api/Documents/attachments/${id}/download`);
    if (!response.ok) throw new ApiError(response.status, { detail: response.statusText });
    return response.blob();
  },
};
