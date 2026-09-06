import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import BudgetDetailPage from '../budgets/pages/BudgetDetailPage';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: null, isLoading: false, error: null }),
    useQueryClient: () => ({ invalidateQueries: vi.fn() }),
    useMutation: () => ({ mutateAsync: vi.fn(), isPending: false }),
  };
});

vi.mock('react-router-dom', () => ({
  useParams: () => ({ id: '1' }),
  useNavigate: () => vi.fn(),
}));

describe('BudgetDetailPage', () => {
  it('shows not-found message when budget is null', () => {
    render(<BudgetDetailPage />);
    expect(screen.getByText('لم يتم العثور على الموازنة')).toBeInTheDocument();
  });
});
