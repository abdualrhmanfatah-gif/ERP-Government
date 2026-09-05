import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { TestWrapper } from '../../../test-utils';
import { AvailabilityIndicator } from '../components/AvailabilityIndicator';
import { resolveAvailabilityTone } from '../utils/budgeting-utils';

describe('resolveAvailabilityTone', () => {
  it('returns green for non-negative availability', () => {
    expect(resolveAvailabilityTone(100, 'Blocking')).toBe('green');
    expect(resolveAvailabilityTone(0, 'Warning')).toBe('green');
  });

  it('returns red for negative availability under Blocking', () => {
    expect(resolveAvailabilityTone(-1, 'Blocking')).toBe('red');
  });

  it('returns amber for negative availability under Warning', () => {
    expect(resolveAvailabilityTone(-1, 'Warning')).toBe('amber');
  });

  it('returns neutral when availability is unknown', () => {
    expect(resolveAvailabilityTone(undefined, 'Blocking')).toBe('neutral');
  });
});

describe('AvailabilityIndicator', () => {
  it('renders the formatted available amount', () => {
    render(<AvailabilityIndicator available={1234.5} controlMethod="Blocking" />, { wrapper: TestWrapper });
    expect(screen.getByRole('status')).toBeInTheDocument();
  });

  it('surfaces the warning text when provided', () => {
    render(
      <AvailabilityIndicator available={-10} controlMethod="Warning" warning="تجاوز بسيط" />,
      { wrapper: TestWrapper },
    );
    expect(screen.getByText(/تجاوز بسيط/)).toBeInTheDocument();
  });

  it('renders a placeholder when availability is unknown', () => {
    render(<AvailabilityIndicator controlMethod="None" />, { wrapper: TestWrapper });
    expect(screen.getByText(/—/)).toBeInTheDocument();
  });
});
