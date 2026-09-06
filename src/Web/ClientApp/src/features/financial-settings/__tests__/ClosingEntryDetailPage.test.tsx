import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import ClosingEntryDetailPage from '../closing-entries/pages/ClosingEntryDetailPage';

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
  useParams: () => ({ id: '1' }),
}));

describe('ClosingEntryDetailPage', () => {
  it('renders not-found state when no data', () => {
    render(<ClosingEntryDetailPage />);
    expect(screen.getByText('قيد الإغلاق غير موجود')).toBeInTheDocument();
  });
});
