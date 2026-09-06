import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import FiscalYearCreatePage from '../fiscal-years/pages/FiscalYearCreatePage';

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

describe('FiscalYearCreatePage', () => {
  it('renders create form with required fields', () => {
    render(<FiscalYearCreatePage />);
    expect(screen.getByLabelText(/اسم السنة المالية/)).toBeInTheDocument();
    expect(screen.getByLabelText(/تاريخ البداية/)).toBeInTheDocument();
    expect(screen.getByLabelText(/تاريخ النهاية/)).toBeInTheDocument();
  });

  it('shows submit button', () => {
    render(<FiscalYearCreatePage />);
    expect(screen.getByText('إنشاء')).toBeInTheDocument();
  });
});
