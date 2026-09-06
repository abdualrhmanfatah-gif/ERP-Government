import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it, vi, beforeEach } from 'vitest';

const mockIsoCodes = [
  { code: 'YER', name: 'Yemeni Rial', decimalPlaces: 2 },
  { code: 'USD', name: 'US Dollar', decimalPlaces: 2 },
];

const mockMutate = vi.fn();

vi.mock('@/features/financial-settings/hooks/useCurrencies', () => ({
  useCreateCurrency: () => ({ mutate: mockMutate, isPending: false }),
  useIso4217Codes: (_query?: string) => ({ data: mockIsoCodes, isLoading: false, error: null }),
}));

import CurrencyCreatePage from '../currencies/pages/CurrencyCreatePage';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

function renderWithRouter(ui: React.ReactElement) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>
  );
}

describe('CurrencyCreatePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('T-005: renders ISO search input', () => {
    renderWithRouter(<CurrencyCreatePage />);
    expect(screen.getByText('عملة جديدة')).toBeInTheDocument();
    expect(screen.getByLabelText('بحث ISO 4217 *')).toBeInTheDocument();
  });

  it('T-008: submit button is disabled when no ISO selected', () => {
    renderWithRouter(<CurrencyCreatePage />);
    const submitButton = screen.getByRole('button', { name: /إنشاء/i });
    expect(submitButton).toBeDisabled();
  });

  it('T-017: shows form fields after ISO selection', () => {
    renderWithRouter(<CurrencyCreatePage />);
    const searchInput = screen.getByLabelText('بحث ISO 4217 *');
    fireEvent.focus(searchInput);
    fireEvent.change(searchInput, { target: { value: 'YER' } });
    const option = screen.getByText(/Yemeni Rial/);
    fireEvent.mouseDown(option);
    expect(screen.getByPlaceholderText('مثال:﷼')).toBeInTheDocument();
  });
});
