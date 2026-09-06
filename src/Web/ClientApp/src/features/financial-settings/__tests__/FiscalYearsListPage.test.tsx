import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import FiscalYearsListPage from '../fiscal-years/pages/FiscalYearsListPage';

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

describe('FiscalYearsListPage', () => {
  it('renders empty state when no fiscal years exist', () => {
    render(<FiscalYearsListPage />);
    expect(screen.getByText('لا توجد سنوات مالية بعد')).toBeInTheDocument();
  });

  it('shows create button', () => {
    render(<FiscalYearsListPage />);
    expect(screen.getByText('سنة مالية جديدة')).toBeInTheDocument();
  });
});
