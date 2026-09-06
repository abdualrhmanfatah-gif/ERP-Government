import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import AttachmentsPanel from '../components/AttachmentsPanel';
import { useAttachments, useAttachmentRequirements, useAttachmentGateCheck, useUploadAttachment, useDeleteAttachment } from '../hooks/useDocuments';

vi.mock('../hooks/useDocuments');

const mockAttachments = [
  { id: 1, entityName: 'Party', documentId: 1, documentType: 'Party', attachmentTypeCode: 'GENERAL', isRequired: false, fileName: 'doc.pdf', mimeType: 'application/pdf', storagePath: '/files/doc.pdf', sizeBytes: 1024, fileHash: null, uploadedById: 1, createdAt: '2026-09-01T10:00:00Z' },
];

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(useAttachmentRequirements).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);
  vi.mocked(useAttachmentGateCheck).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);
  vi.mocked(useUploadAttachment).mockReturnValue({ mutateAsync: vi.fn() as any, isPending: false } as any);
  vi.mocked(useDeleteAttachment).mockReturnValue({ mutateAsync: vi.fn() as any, isPending: false } as any);
});

describe('AttachmentsPanel', () => {
  it('renders attachment list', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: mockAttachments, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    render(<AttachmentsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText('doc.pdf')).toBeInTheDocument();
  });

  it('renders upload button', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: mockAttachments, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    render(<AttachmentsPanel documentType="Party" documentId={1} />);
    expect(screen.getByRole('button', { name: /رفع مرفق/i })).toBeInTheDocument();
  });

  it('shows error state with retry button when query fails', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: undefined, isLoading: false, isError: true, error: new Error('Network error'), refetch: vi.fn() } as any);
    render(<AttachmentsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/حدث خطأ/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /إعادة المحاولة/i })).toBeInTheDocument();
  });

  it('shows empty state when no attachments exist', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    render(<AttachmentsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/لا توجد مرفقات/i)).toBeInTheDocument();
  });
});

describe('AttachmentsPanel gate-check states', () => {
  it('shows satisfied gate when all requirements met', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: mockAttachments, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    vi.mocked(useAttachmentRequirements).mockReturnValue({
      data: [{ attachmentTypeCode: 'GENERAL', titleAr: 'عام', isMandatory: true }],
      isLoading: false, isError: false, error: null,
    } as any);
    vi.mocked(useAttachmentGateCheck).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);

    render(<AttachmentsPanel documentType="Party" documentId={1} showGate={true} />);
    expect(screen.getByText(/جميع المرفقات المطلوبة متوفرة/i)).toBeInTheDocument();
  });

  it('shows missing gate when requirements not met', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    vi.mocked(useAttachmentRequirements).mockReturnValue({
      data: [{ attachmentTypeCode: 'TAX_CERT', titleAr: 'شهادة ضريبية', isMandatory: true }],
      isLoading: false, isError: false, error: null,
    } as any);
    vi.mocked(useAttachmentGateCheck).mockReturnValue({ data: ['TAX_CERT'], isLoading: false, isError: false, error: null } as any);

    render(<AttachmentsPanel documentType="Party" documentId={1} showGate={true} />);
    expect(screen.getByText(/مرفق مطلوب مفقود/i)).toBeInTheDocument();
    expect(screen.getByText(/TAX_CERT/)).toBeInTheDocument();
  });

  it('hides gate indicator when showGate is false', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    vi.mocked(useAttachmentGateCheck).mockReturnValue({ data: ['TAX_CERT'], isLoading: false, isError: false, error: null } as any);

    render(<AttachmentsPanel documentType="Party" documentId={1} showGate={false} />);
    expect(screen.queryByText(/مرفق مطلوب مفقود/i)).not.toBeInTheDocument();
    expect(screen.queryByText(/جميع المرفقات المطلوبة متوفرة/i)).not.toBeInTheDocument();
  });

  it('shows no-gate state when no requirements configured', () => {
    vi.mocked(useAttachments).mockReturnValue({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    vi.mocked(useAttachmentRequirements).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);
    vi.mocked(useAttachmentGateCheck).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);

    render(<AttachmentsPanel documentType="Party" documentId={1} showGate={true} />);
    expect(screen.queryByText(/مرفق مطلوب مفقود/i)).not.toBeInTheDocument();
    expect(screen.queryByText(/جميع المرفقات المطلوبة متوفرة/i)).not.toBeInTheDocument();
  });
});
