import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { ExchangeRateLookup } from '../components/ExchangeRateLookup';

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const orig = await importOriginal<typeof import('@tanstack/react-query')>();
  return {
    ...orig,
    useQuery: () => ({ data: null, isLoading: false, error: new Error('not found') }),
  };
});

describe('ExchangeRateLookup', () => {
  it('shows no rate message on error', () => {
    render(<ExchangeRateLookup baseCurrencyId={1} currencyId={2} date="2026-01-01" />);
    expect(screen.getByText('لا يوجد سعر صرف لهذا التاريخ')).toBeInTheDocument();
  });
});
