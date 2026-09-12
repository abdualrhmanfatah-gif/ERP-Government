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

export interface AttachmentRequirement {
  id: number;
  documentType: string;
  attachmentTypeCode: string;
  titleAr: string;
  isMandatory: boolean;
  isActive: boolean;
}

export type AttachmentGateCheckResponse = string[];

export interface UploadAttachmentResponse {
  value: number;
  succeeded: boolean;
  errors: string[];
}

export interface ApprovalsPanelProps {
  documentType: string;
  documentId: number;
}

export interface StatusLogPanelProps {
  documentType: string;
  documentId: number;
}

export interface AttachmentsPanelProps {
  documentType: string;
  documentId: number;
  showGate?: boolean;
  onGateChange?: (isMet: boolean) => void;
}
