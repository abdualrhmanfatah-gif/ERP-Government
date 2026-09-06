import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
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
  useAccountsList: () => ({
    data: [
      { id: 1001, code: '1001', name: 'Cash' },
      { id: 5001, code: '5001', name: 'Expense' },
    ],
    isLoading: false,
  }),
}));

vi.mock('../components/DimensionPickers', () => ({
  DimensionPickers: ({ onChange }: { onChange: (dims: Record<string, number | null>) => void }) => (
    <div data-testid="dimension-pickers">
      <button type="button" onClick={() => onChange({ fundId: 1, projectId: null, budgetItemId: null, encumbranceId: null, paymentOrderId: null })}>
        Select Fund
      </button>
    </div>
  ),
}));

function renderPage() {
  return render(
    <QueryClientProvider client={queryClient}>
      <JournalEntryCreatePage />
    </QueryClientProvider>,
  );
}

describe('JournalEntry Lines Editor', () => {
  it('shows empty state when no lines', () => {
    renderPage();
    expect(screen.getByText(/لا توجد أسطر بعد/)).toBeDefined();
  });

  it('shows add line button', () => {
    renderPage();
    expect(screen.getByRole('button', { name: /\+ إضافة سطر/ })).toBeDefined();
  });

  it('opens line editor form when add line clicked', () => {
    renderPage();
    fireEvent.click(screen.getByRole('button', { name: /\+ إضافة سطر/ }));
    expect(screen.getByText('سطر جديد')).toBeDefined();
    expect(screen.getByText('الحساب')).toBeDefined();
    expect(screen.getByText('مدين')).toBeDefined();
    expect(screen.getByText('دائن')).toBeDefined();
  });

  it('shows dimension pickers in line editor', () => {
    renderPage();
    fireEvent.click(screen.getByRole('button', { name: /\+ إضافة سطر/ }));
    expect(screen.getByTestId('dimension-pickers')).toBeDefined();
  });

  it('shows balance indicator', () => {
    renderPage();
    expect(screen.getByText(/مدين/)).toBeDefined();
    expect(screen.getByText(/دائن/)).toBeDefined();
    expect(screen.getByText(/متوازن/)).toBeDefined();
  });

  it('save button disabled when no lines exist', () => {
    renderPage();
    const saveBtn = screen.getByRole('button', { name: /حفظ القيد/ });
    expect(saveBtn).toHaveProperty('disabled', true);
  });

  it('shows cancel button', () => {
    renderPage();
    expect(screen.getByRole('button', { name: /^إلغاء$/ })).toBeDefined();
  });
});
