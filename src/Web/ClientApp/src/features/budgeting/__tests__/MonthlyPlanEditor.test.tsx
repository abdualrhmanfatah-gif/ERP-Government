import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MonthlyPlanEditor } from '../monthly-plan/components/MonthlyPlanEditor';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: undefined, isLoading: false, error: null }),
    useQueryClient: () => ({ invalidateQueries: vi.fn() }),
    useMutation: () => ({ mutateAsync: vi.fn(), isPending: false }),
  };
});

describe('MonthlyPlanEditor', () => {
  it('renders 12 month columns', () => {
    render(<MonthlyPlanEditor budgetItemId={1} appropriatedTotal={0} />);
    expect(screen.getByText('يناير')).toBeInTheDocument();
    expect(screen.getByText('ديسمبر')).toBeInTheDocument();
  });

  it('displays total alongside appropriated total', () => {
    render(<MonthlyPlanEditor budgetItemId={1} appropriatedTotal={100000} />);
    expect(screen.getByText('المجموع')).toBeInTheDocument();
  });
});
