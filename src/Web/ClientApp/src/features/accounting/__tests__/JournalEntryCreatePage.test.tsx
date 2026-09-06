import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { JournalEntryCreatePage } from '../pages/JournalEntryCreatePage';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

vi.mock('react-router-dom', () => ({ useNavigate: () => vi.fn() }));
vi.mock('../hooks/useJournalEntries', () => ({
  useCreateJournalEntry: () => ({ mutateAsync: vi.fn().mockResolvedValue(1), isPending: false }),
}));
vi.mock('../hooks/useJournalEntryLines', () => ({
  useCreateJournalEntryLine: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));
vi.mock('../hooks/useFiscalYearByDate', () => ({
  useFiscalYearByDate: () => ({ data: { fiscalYearId: 1, fiscalPeriodId: 1 }, isLoading: false }),
}));
vi.mock('../hooks/useJournalsList', () => ({
  useJournalsList: () => ({ data: [{ id: 1, code: 'GEN', name: 'General' }], isLoading: false }),
}));
vi.mock('../hooks/useCurrenciesList', () => ({
  useCurrenciesList: () => ({ data: [{ id: 1, code: 'YER', name: 'ريال يمني' }], isLoading: false }),
}));
vi.mock('../hooks/useAccountsList', () => ({
  useAccountsList: () => ({ data: [{ id: 1001, code: '1001', name: 'Cash' }], isLoading: false }),
}));

function renderPage() {
  return render(<QueryClientProvider client={queryClient}><JournalEntryCreatePage /></QueryClientProvider>);
}

describe('JournalEntryCreatePage', () => {
  it('renders heading', () => { renderPage(); expect(screen.getByText(/إنشاء قيد يومية/)).toBeDefined(); });
  it('renders all form fields', () => {
    renderPage();
    expect(screen.getByLabelText(/التاريخ/)).toBeDefined();
    expect(screen.getByLabelText(/اليومية/)).toBeDefined();
    expect(screen.getByLabelText(/نوع القيد/)).toBeDefined();
    expect(screen.getByLabelText(/العملة/)).toBeDefined();
    expect(screen.getByLabelText(/الوصف/)).toBeDefined();
    expect(screen.getByLabelText(/المرجع/)).toBeDefined();
  });
  it('renders lines section', () => { renderPage(); expect(screen.getByText(/أسطر القيد/)).toBeDefined(); });
  it('renders save and cancel buttons', () => {
    renderPage();
    expect(screen.getByRole('button', { name: /حفظ القيد/ })).toBeDefined();
    expect(screen.getByRole('button', { name: /إلغاء/ })).toBeDefined();
  });
  it('save button disabled when no lines', () => {
    renderPage();
    expect(screen.getByRole('button', { name: /حفظ القيد/ })).toHaveProperty('disabled', true);
  });
});
