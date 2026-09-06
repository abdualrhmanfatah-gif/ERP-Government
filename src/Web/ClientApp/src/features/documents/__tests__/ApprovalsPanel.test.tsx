import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ApprovalsPanel from '../components/ApprovalsPanel';
import StatusLogPanel from '../components/StatusLogPanel';
import { useApprovals, useStatusLog } from '../hooks/useDocuments';

vi.mock('../hooks/useDocuments');

beforeEach(() => {
  vi.clearAllMocks();
});

describe('ApprovalsPanel', () => {
  it('renders approval records', () => {
    vi.mocked(useApprovals).mockReturnValue({
      data: [
        { id: 1, documentType: 'Party', documentId: 1, approvalStep: 1, action: 0, approverUserId: 1, requiredRole: 'Admin', decision: 'Approved', decisionAt: '2026-09-01T10:00:00Z', reason: 'تم الاعتماد', evaluationSnapshot: null },
      ],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<ApprovalsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText('تم الاعتماد')).toBeInTheDocument();
    expect(screen.getByText('Approved')).toBeInTheDocument();
  });

  it('renders collapsible header', () => {
    vi.mocked(useApprovals).mockReturnValue({
      data: [
        { id: 1, documentType: 'Party', documentId: 1, approvalStep: 1, action: 0, approverUserId: 1, requiredRole: 'Admin', decision: 'Approved', decisionAt: '2026-09-01T10:00:00Z', reason: null, evaluationSnapshot: null },
      ],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<ApprovalsPanel documentType="Party" documentId={1} />);
    expect(screen.getByRole('button', { name: /اعتمادات/i })).toBeInTheDocument();
  });

  it('shows error state with retry button when query fails', () => {
    vi.mocked(useApprovals).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error('Network error'),
      refetch: vi.fn(),
    } as any);
    render(<ApprovalsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/حدث خطأ/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /إعادة المحاولة/i })).toBeInTheDocument();
  });

  it('shows empty state when no approvals exist', () => {
    vi.mocked(useApprovals).mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<ApprovalsPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/لا توجد اعتمادات بعد/i)).toBeInTheDocument();
  });
});

describe('StatusLogPanel', () => {
  it('renders status log entries', () => {
    vi.mocked(useStatusLog).mockReturnValue({
      data: [
        { id: 1, entityName: 'Party', documentId: 1, fromStatus: 'Draft', toStatus: 'Active', changedById: 1, changedAt: '2026-09-01T09:00:00Z', reason: null },
      ],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<StatusLogPanel documentType="Party" documentId={1} />);
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
  });

  it('renders collapsible header', () => {
    vi.mocked(useStatusLog).mockReturnValue({
      data: [
        { id: 1, entityName: 'Party', documentId: 1, fromStatus: 'Draft', toStatus: 'Active', changedById: 1, changedAt: '2026-09-01T09:00:00Z', reason: null },
      ],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<StatusLogPanel documentType="Party" documentId={1} />);
    expect(screen.getByRole('button', { name: /سجل الحالات/i })).toBeInTheDocument();
  });

  it('shows error state with retry button when query fails', () => {
    vi.mocked(useStatusLog).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error('Network error'),
      refetch: vi.fn(),
    } as any);
    render(<StatusLogPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/حدث خطأ/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /إعادة المحاولة/i })).toBeInTheDocument();
  });

  it('shows empty state when no status log exists', () => {
    vi.mocked(useStatusLog).mockReturnValue({
      data: [],
      isLoading: false,
      isError: false,
      error: null,
      refetch: vi.fn(),
    } as any);
    render(<StatusLogPanel documentType="Party" documentId={1} />);
    expect(screen.getByText(/لا توجد تغييرات حالة/i)).toBeInTheDocument();
  });
});
