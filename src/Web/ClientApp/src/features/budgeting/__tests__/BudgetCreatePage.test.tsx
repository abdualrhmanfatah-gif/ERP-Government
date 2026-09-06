import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import BudgetCreatePage from '../budgets/pages/BudgetCreatePage';

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
  useNavigate: () => vi.fn(),
}));

describe('BudgetCreatePage', () => {
  it('renders the create form with all fields', () => {
    render(<BudgetCreatePage />);
    expect(screen.getByText('موازنة جديدة')).toBeInTheDocument();
    expect(screen.getByText('اسم الموازنة *')).toBeInTheDocument();
    expect(screen.getByText('تاريخ البداية *')).toBeInTheDocument();
  });
});
