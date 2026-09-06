import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it, vi, beforeEach } from 'vitest';

const mockCurrency = {
  id: 1,
  code: 'YER',
  name: 'Yemeni Rial',
  symbol: '﷼',
  decimalPlaces: 2,
  roundingPrecision: 0.01,
  isBase: true,
  isActive: true,
  rowVersion: 'AAAAAA==',
  createdAt: '2026-01-01T00:00:00Z',
  createdBy: 'admin',
  modifiedAt: '2026-01-02T00:00:00Z',
  modifiedBy: 'admin',
};

vi.mock('@/features/financial-settings/hooks/useCurrencies', () => ({
  useCurrencyDetail: (_id: number) => ({ data: mockCurrency, isLoading: false, error: null }),
  useUpdateCurrency: () => ({ mutate: vi.fn(), isPending: false }),
  useActivateCurrency: () => ({ mutate: vi.fn(), isPending: false }),
  useDeactivateCurrency: () => ({ mutate: vi.fn(), isPending: false }),
}));

import CurrencyDetailPage from '../currencies/pages/CurrencyDetailPage';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

function renderWithRouter(ui: React.ReactElement) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>
  );
}

describe('CurrencyDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('T-010: renders currency code in heading', () => {
    renderWithRouter(<CurrencyDetailPage />);
    const yerElements = screen.getAllByText('YER');
    expect(yerElements.length).toBeGreaterThanOrEqual(1);
  });

  it('T-010: renders currency name', () => {
    renderWithRouter(<CurrencyDetailPage />);
    expect(screen.getByText('Yemeni Rial')).toBeInTheDocument();
  });

  it('T-010: renders decimal places and rounding precision', () => {
    renderWithRouter(<CurrencyDetailPage />);
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('0.01')).toBeInTheDocument();
  });

  it('T-011: shows IsBase badge when true', () => {
    renderWithRouter(<CurrencyDetailPage />);
    expect(screen.getByText('أساسية')).toBeInTheDocument();
  });

  it('T-012: shows IsActive status', () => {
    renderWithRouter(<CurrencyDetailPage />);
    expect(screen.getByText('نشط')).toBeInTheDocument();
  });

  it('T-013: edit mode enables field editing', () => {
    renderWithRouter(<CurrencyDetailPage />);
    const editButton = screen.getByText('تعديل');
    fireEvent.click(editButton);
    expect(screen.getByDisplayValue('Yemeni Rial')).toBeInTheDocument();
    expect(screen.getByDisplayValue('﷼')).toBeInTheDocument();
  });

  it('T-016: renders audit trail section', () => {
    renderWithRouter(<CurrencyDetailPage />);
    expect(screen.getByText('سجل التدقيق')).toBeInTheDocument();
    const admins = screen.getAllByText('admin');
    expect(admins.length).toBeGreaterThanOrEqual(1);
  });
});
