import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { documentsClient, documentsKeys } from '../shared/client';

export function useApprovals(documentType: string, documentId: number) {
  return useQuery({
    queryKey: documentsKeys.approvals(documentType, documentId),
    queryFn: () => documentsClient.getApprovals(documentType, documentId),
    enabled: !!documentType && Number.isFinite(documentId),
  });
}

export function useStatusLog(documentType: string, documentId: number) {
  return useQuery({
    queryKey: documentsKeys.statusLog(documentType, documentId),
    queryFn: () => documentsClient.getStatusLog(documentType, documentId),
    enabled: !!documentType && Number.isFinite(documentId),
  });
}

export function useAttachments(documentType: string, documentId: number) {
  return useQuery({
    queryKey: documentsKeys.attachments(documentType, documentId),
    queryFn: () => documentsClient.getAttachments(documentType, documentId),
    enabled: !!documentType && Number.isFinite(documentId),
  });
}

export function useAttachmentRequirements(documentType: string) {
  return useQuery({
    queryKey: documentsKeys.requirements(documentType),
    queryFn: () => documentsClient.getRequirements(documentType),
    enabled: !!documentType,
  });
}

export function useAttachmentGateCheck(documentType: string, documentId: number) {
  return useQuery({
    queryKey: documentsKeys.gateCheck(documentType, documentId),
    queryFn: () => documentsClient.checkGate(documentType, documentId),
    enabled: !!documentType && Number.isFinite(documentId),
  });
}

export function useUploadAttachment(documentType: string, documentId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ file, attachmentTypeCode }: { file: File; attachmentTypeCode?: string }) =>
      documentsClient.upload(documentType, documentId, file, attachmentTypeCode),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: documentsKeys.attachments(documentType, documentId) });
      qc.invalidateQueries({ queryKey: documentsKeys.gateCheck(documentType, documentId) });
    },
  });
}

export function useDeleteAttachment(documentType: string, documentId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => documentsClient.deleteAttachment(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: documentsKeys.attachments(documentType, documentId) });
      qc.invalidateQueries({ queryKey: documentsKeys.gateCheck(documentType, documentId) });
    },
  });
}
