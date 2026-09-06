import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import BudgetsListPage from '../budgets/pages/BudgetsListPage';

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

describe('BudgetsListPage', () => {
  it('renders empty state when no budgets exist', () => {
    render(<BudgetsListPage />);
    expect(screen.getByText('لا توجد موازنات بعد')).toBeInTheDocument();
  });

  it('shows create button', () => {
    render(<BudgetsListPage />);
    expect(screen.getByText('موازنة جديدة')).toBeInTheDocument();
  });
});
