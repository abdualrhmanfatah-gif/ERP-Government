# Data Model: Unified Party Registry & Shared Document Panels

**Feature**: 022-unified-party-registry
**Date**: 2026-09-06

> This file defines frontend DTOs and types. Backend entities are in `src/Domain/` and are NOT modified by this feature.

## Party Types (features/parties/shared/types.ts)

```typescript
// Mirrors src/Domain/Parties/Enums/PartyType.cs
export enum PartyType {
  Supplier = 0,
  Customer = 1,
  GovEntity = 2,
  TaxAuthority = 3,
  Other = 4,
}

export const PARTY_TYPE_LABELS: Record<PartyType, string> = {
  [PartyType.Supplier]: 'مورد',
  [PartyType.Customer]: 'عميل',
  [PartyType.GovEntity]: 'جهة حكومية',
  [PartyType.TaxAuthority]: 'جهة ضريبية',
  [PartyType.Other]: 'أخرى',
};

// GET /api/Parties → PartyResponse
export interface PartyResponse {
  id: number;
  partyCode: string;
  partyType: PartyType;
  nameAr: string;
  nameEn: string | null;
  taxNumber: string | null;
  nationalId: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  notes: string | null;
  isActive: boolean;
}

// POST /api/Parties body
export interface CreatePartyCommand {
  partyType: PartyType;
  nameAr: string;
  nameEn?: string;
  taxNumber?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  notes?: string;
}

// PUT /api/Parties/{id} body
export interface UpdatePartyCommand {
  partyType: PartyType;
  nameAr: string;
  nameEn?: string;
  taxNumber?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  notes?: string;
}

// Party list filters
export interface PartyFilters {
  partyType?: PartyType;
  isActive?: boolean;
  search?: string;
}

// Related documents query response
export interface PartyDocumentResponse {
  documentType: string;
  documentId: number;
  documentNumber: string;
  status: string;
  date: string;
  amount: number;
}
```

## Document Shared Types (features/documents/shared/types.ts)

```typescript
// GET /api/Documents/{documentType}/{documentId}/approvals → ApprovalHistory[]
export interface ApprovalRecord {
  id: number;
  documentType: string;
  documentId: number;
  approvalStep: number;
  action: number;
  approverUserId: number;
  requiredRole: string;
  decision: string;
  decisionAt: string;
  reason: string | null;
  evaluationSnapshot: string | null;
}

// GET /api/Documents/{documentType}/{documentId}/status-log → DocumentStatusLog[]
export interface StatusLogRecord {
  id: number;
  entityName: string;
  documentId: number;
  fromStatus: string;
  toStatus: string;
  changedById: number;
  changedAt: string;
  reason: string | null;
}

// GET /api/Documents/{documentType}/{documentId}/attachments → Attachment[]
export interface AttachmentRecord {
  id: number;
  entityName: string;
  documentId: number;
  documentType: string;
  attachmentTypeCode: string;
  isRequired: boolean;
  fileName: string;
  mimeType: string;
  storagePath: string;
  sizeBytes: number;
  fileHash: string | null;
  uploadedById: number;
  createdAt: string;
}

// GET /api/Documents/{documentType}/attachment-requirements → DocumentAttachmentRequirement[]
export interface AttachmentRequirement {
  id: number;
  documentType: string;
  attachmentTypeCode: string;
  titleAr: string;
  isMandatory: boolean;
  isActive: boolean;
}

// GET /api/Documents/{documentType}/{documentId}/attachment-gate-check → string[]
// Response: list of MISSING mandatory attachment type codes
export type AttachmentGateCheckResponse = string[];

// POST /api/Documents/{documentType}/{documentId}/attachments response
export interface UploadAttachmentResponse {
  value: number;
  succeeded: boolean;
  errors: string[];
}
```

## Shared Panel Props

```typescript
// ApprovalsPanel props
export interface ApprovalsPanelProps {
  documentType: string;
  documentId: number;
}

// StatusLogPanel props
export interface StatusLogPanelProps {
  documentType: string;
  documentId: number;
}

// AttachmentsPanel props
export interface AttachmentsPanelProps {
  documentType: string;
  documentId: number;
  /** If true, show gate badge and disable approval when mandatory attachments missing */
  showGate?: boolean;
  /** Callback when gate state changes — parent can use to enable/disable approve button */
  onGateChange?: (isMet: boolean) => void;
}
```

## Cache Key Factories

```typescript
// features/parties/shared/client.ts
export const partiesKeys = {
  all: ['parties'] as const,
  lists: () => [...partiesKeys.all, 'list'] as const,
  list: (filters: PartyFilters) => [...partiesKeys.lists(), filters] as const,
  details: () => [...partiesKeys.all, 'detail'] as const,
  detail: (id: number) => [...partiesKeys.details(), id] as const,
  documents: (id: number) => [...partiesKeys.detail(id), 'documents'] as const,
};

// features/documents/shared/client.ts
export const documentsKeys = {
  all: ['documents'] as const,
  approvals: (type: string, id: number) => [...documentsKeys.all, 'approvals', type, id] as const,
  statusLog: (type: string, id: number) => [...documentsKeys.all, 'statusLog', type, id] as const,
  attachments: (type: string, id: number) => [...documentsKeys.all, 'attachments', type, id] as const,
  requirements: (type: string) => [...documentsKeys.all, 'requirements', type] as const,
  gateCheck: (type: string, id: number) => [...documentsKeys.all, 'gateCheck', type, id] as const,
};
```
