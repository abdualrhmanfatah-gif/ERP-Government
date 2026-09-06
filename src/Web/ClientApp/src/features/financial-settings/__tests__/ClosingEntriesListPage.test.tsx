import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ClosingEntriesListPage from '../closing-entries/pages/ClosingEntriesListPage';

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
  useParams: () => ({ fiscalYearId: '1' }),
}));

describe('ClosingEntriesListPage', () => {
  it('renders empty state', () => {
    render(<ClosingEntriesListPage />);
    expect(screen.getByText('لا توجد قيود إغلاق بعد')).toBeInTheDocument();
  });
});
