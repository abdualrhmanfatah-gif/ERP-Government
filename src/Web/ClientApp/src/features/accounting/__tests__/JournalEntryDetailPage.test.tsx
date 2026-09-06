import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useParams: () => ({ id: '1' }), useNavigate: () => vi.fn() };
});

vi.mock('@/shared/hooks/usePermission', () => ({
  usePermission: () => ({ hasPermission: true, isLoading: false }),
}));

const mockEntry = (overrides: Record<string, unknown> = {}) => ({
  id: 1,
  entryNumber: 'JE-000001',
  documentDate: '2026-01-15',
  entryStatus: 'Draft',
  journalName: 'General',
  periodName: 'January 2026',
  fiscalYearName: '2026',
  narration: 'Test entry',
  ref: 'REF-001',
  totalDebit: 1000,
  totalCredit: 1000,
  isSystemGenerated: false,
  rowVersion: 'AAAA',
  periodId: 1,
  fiscalYearId: 1,
  lines: [
    { id: 1, journalEntryId: 1, sequence: 1, accountId: 1001, accountCode: '1001', accountName: 'Cash', currencyId: 1, exchangeRate: 1, debit: 1000, credit: 0, rowVersion: 'AAAA' },
    { id: 2, journalEntryId: 1, sequence: 2, accountId: 5001, accountCode: '5001', accountName: 'Expense', currencyId: 1, exchangeRate: 1, debit: 0, credit: 1000, rowVersion: 'BBBB' },
  ],
  ...overrides,
});

const mockUseJournalEntry = vi.fn(() => ({ data: mockEntry(), isLoading: false, error: null, refetch: vi.fn() }));

vi.mock('../hooks/useJournalEntries', () => ({
  useJournalEntry: (...args: unknown[]) => mockUseJournalEntry(...args),
  useSubmitJournalEntry: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useApproveJournalEntry: () => ({ mutateAsync: vi.fn(), isPending: false }),
  usePostJournalEntry: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useReverseJournalEntry: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useCancelJournalEntry: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

vi.mock('../../documents/components/ApprovalsPanel', () => ({
  ApprovalsPanel: () => <div data-testid="approvals-panel">Approvals</div>,
}));

vi.mock('../../documents/components/StatusLogPanel', () => ({
  StatusLogPanel: () => <div data-testid="status-log-panel">Status Log</div>,
}));

import { JournalEntryDetailPage } from '../pages/JournalEntryDetailPage';

function renderPage() {
  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <JournalEntryDetailPage />
      </QueryClientProvider>
    </MemoryRouter>,
  );
}

describe('JournalEntryDetailPage — lifecycle actions per state', () => {
  beforeEach(() => vi.clearAllMocks());

  it('Draft: shows Submit + Cancel buttons', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Draft' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByRole('button', { name: /تقديم/ })).toBeDefined();
    expect(screen.getByRole('button', { name: /إلغاء/ })).toBeDefined();
    expect(screen.queryByRole('button', { name: /موافقة/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /تسجيل/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
  });

  it('Submitted: shows Approve + Cancel buttons', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Submitted' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByRole('button', { name: /موافقة/ })).toBeDefined();
    expect(screen.getByRole('button', { name: /إلغاء/ })).toBeDefined();
    expect(screen.queryByRole('button', { name: /تقديم/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /تسجيل/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
  });

  it('Approved: shows only Post button', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Approved' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByRole('button', { name: /تسجيل/ })).toBeDefined();
    expect(screen.queryByRole('button', { name: /تقديم/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /موافقة/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
  });

  it('Posted: shows only Reverse button', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Posted' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByRole('button', { name: /عكس/ })).toBeDefined();
    expect(screen.queryByRole('button', { name: /تقديم/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /موافقة/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /تسجيل/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
  });

  it('Reversed: shows no action buttons', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Reversed' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByRole('button', { name: /تقديم/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /موافقة/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /تسجيل/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
  });

  it('Cancelled: shows no action buttons', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Cancelled' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByRole('button', { name: /تقديم/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /موافقة/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /تسجيل/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
  });

  it('Cancel not offered for Approved', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Approved' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
  });

  it('Cancel not offered for Posted', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Posted' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByRole('button', { name: /إلغاء/ })).toBeNull();
  });
});

describe('JournalEntryDetailPage — system badge read-only', () => {
  beforeEach(() => vi.clearAllMocks());

  it('hides action bar for system-generated entries', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Posted', isSystemGenerated: true }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByRole('button', { name: /عكس/ })).toBeNull();
    expect(screen.getByText('نظام')).toBeDefined();
  });
});

describe('JournalEntryDetailPage — reversal linkage', () => {
  beforeEach(() => vi.clearAllMocks());

  it('shows reversal origin link when reversalOfId exists', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ reversalOfId: 42, reversalReason: 'Error correction' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByText(/عكس لـ/)).toBeDefined();
    expect(screen.getByText(/القيد رقم 42/)).toBeDefined();
    expect(screen.getByText(/Error correction/)).toBeDefined();
  });

  it('does not show reversal link when reversalOfId is null', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ reversalOfId: null }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByText(/عكس لـ/)).toBeNull();
  });
});

describe('JournalEntryDetailPage — conflict toast', () => {
  beforeEach(() => vi.clearAllMocks());

  it('shows conflict error message', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry(), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByText(/تم تعديل القيد بواسطة مستخدم آخر/)).toBeNull();
  });
});

describe('JournalEntryDetailPage — ApprovalsPanel visibility', () => {
  beforeEach(() => vi.clearAllMocks());

  it('shows ApprovalsPanel for Submitted entries', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Submitted' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByTestId('approvals-panel')).toBeDefined();
  });

  it('shows ApprovalsPanel for Approved entries', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Approved' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.getByTestId('approvals-panel')).toBeDefined();
  });

  it('hides ApprovalsPanel for Draft entries', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Draft' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByTestId('approvals-panel')).toBeNull();
  });

  it('hides ApprovalsPanel for Posted entries', () => {
    mockUseJournalEntry.mockReturnValue({ data: mockEntry({ entryStatus: 'Posted' }), isLoading: false, error: null, refetch: vi.fn() });
    renderPage();
    expect(screen.queryByTestId('approvals-panel')).toBeNull();
  });
});
