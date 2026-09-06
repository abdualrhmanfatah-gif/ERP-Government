import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import DocumentSequencesListPage from '../document-sequences/pages/DocumentSequencesListPage';

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

describe('DocumentSequencesListPage', () => {
  it('renders page title', () => {
    render(<DocumentSequencesListPage />);
    expect(screen.getByText('تسلسل الوثائق')).toBeInTheDocument();
  });
});
