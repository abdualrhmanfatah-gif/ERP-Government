import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import EncumbranceCreatePage from '../encumbrances/pages/EncumbranceCreatePage';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: undefined, isLoading: false, error: null }),
    useQueryClient: () => ({ invalidateQueries: vi.fn() }),
    useMutation: () => ({ mutateAsync: vi.fn(), isPending: false }),
  };
});

vi.mock('react-router-dom', () => ({
  useParams: () => ({}),
  useSearchParams: () => [new URLSearchParams({ appropriationId: '5' }), vi.fn()],
  useNavigate: () => vi.fn(),
}));

describe('EncumbranceCreatePage', () => {
  it('renders the create form with encumbrance type dropdown', () => {
    render(<EncumbranceCreatePage />);
    expect(screen.getByText('التزام جديد')).toBeInTheDocument();
    expect(screen.getByText('نوع الالتزام *')).toBeInTheDocument();
  });
});
