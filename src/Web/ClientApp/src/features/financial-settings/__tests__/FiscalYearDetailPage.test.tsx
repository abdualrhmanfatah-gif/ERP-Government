import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import FiscalYearDetailPage from '../fiscal-years/pages/FiscalYearDetailPage';

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

describe('FiscalYearDetailPage', () => {
  it('renders not-found state when no data', () => {
    render(<FiscalYearDetailPage />);
    expect(screen.getByText('السنة المالية غير موجودة')).toBeInTheDocument();
  });
});
