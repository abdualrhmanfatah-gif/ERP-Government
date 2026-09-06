import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ExchangeRateCreatePage from '../exchange-rates/pages/ExchangeRateCreatePage';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: [], isLoading: false, error: null }),
    useQueryClient: () => ({ invalidateQueries: vi.fn() }),
    useMutation: () => ({ mutateAsync: vi.fn(), isPending: false }),
  };
});

vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
}));

describe('ExchangeRateCreatePage', () => {
  it('renders create form', () => {
    render(<ExchangeRateCreatePage />);
    expect(screen.getByText('سعر صرف جديد')).toBeInTheDocument();
  });
});
