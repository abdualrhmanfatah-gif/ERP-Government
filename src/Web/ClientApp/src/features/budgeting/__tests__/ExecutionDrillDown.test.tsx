import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ExecutionDrillDown } from '../execution/components/ExecutionDrillDown';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: undefined, isLoading: true, error: null }),
    useQueryClient: () => ({ invalidateQueries: vi.fn() }),
    useMutation: () => ({ mutateAsync: vi.fn(), isPending: false }),
  };
});

vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
  useParams: () => ({ id: '1' }),
}));

describe('ExecutionDrillDown', () => {
  it('renders loading state', () => {
    render(<ExecutionDrillDown budgetId={1} />);
    expect(screen.getByText('جارٍ التحميل...')).toBeInTheDocument();
  });
});
