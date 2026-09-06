import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it, vi, beforeEach } from 'vitest';

vi.mock('react-router-dom', async (importOriginal) => {
  const orig = await importOriginal<typeof import('react-router-dom')>();
  return {
    ...orig,
    useNavigate: () => vi.fn(),
  };
});

const mockCurrencies = [
  { id: 1, code: 'YER', name: 'Yemeni Rial', symbol: '﷼', decimalPlaces: 2, roundingPrecision: 0.01, isBase: true, isActive: true, rowVersion: 'AAAAAA==' },
  { id: 2, code: 'USD', name: 'US Dollar', symbol: '$', decimalPlaces: 2, roundingPrecision: 0.01, isBase: false, isActive: true, rowVersion: 'BBBBBB==' },
  { id: 3, code: 'SAR', name: 'Saudi Riyal', symbol: '﷼', decimalPlaces: 2, roundingPrecision: 0.01, isBase: false, isActive: false, rowVersion: 'CCCCCC==' },
];

let mockData = mockCurrencies;

vi.mock('@/features/financial-settings/hooks/useCurrencies', () => ({
  useCurrenciesList: () => ({ data: mockData, isLoading: false, error: null }),
  useActivateCurrency: () => ({ mutate: vi.fn(), isPending: false }),
  useDeactivateCurrency: () => ({ mutate: vi.fn(), isPending: false }),
}));

import CurrenciesListPage from '../currencies/pages/CurrenciesListPage';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

function renderWithRouter(ui: React.ReactElement) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>
  );
}

describe('CurrenciesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockData = mockCurrencies;
  });

  it('T-001: renders empty state when no currencies exist', () => {
    mockData = [];
    renderWithRouter(<CurrenciesListPage />);
    expect(screen.getByText('لا توجد عملات بعد')).toBeInTheDocument();
  });

  it('T-002: renders currency rows with correct columns', () => {
    renderWithRouter(<CurrenciesListPage />);
    expect(screen.getAllByText('YER').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Yemeni Rial').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('USD').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('US Dollar').length).toBeGreaterThanOrEqual(1);
  });

  it('T-003: search filters by code and name', () => {
    renderWithRouter(<CurrenciesListPage />);
    const searchInput = screen.getByPlaceholderText('بحث بالكود أو الاسم...');
    fireEvent.change(searchInput, { target: { value: 'USD' } });
    const usdElements = screen.getAllByText('USD');
    expect(usdElements.length).toBeGreaterThanOrEqual(1);
    expect(screen.queryByText('YER')).not.toBeInTheDocument();
  });

  it('T-016: renders create button', () => {
    renderWithRouter(<CurrenciesListPage />);
    expect(screen.getByText('عملة جديدة')).toBeInTheDocument();
  });
});
