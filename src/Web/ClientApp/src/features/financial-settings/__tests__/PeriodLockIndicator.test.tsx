import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { PeriodLockIndicator } from '../components/PeriodLockIndicator';

describe('PeriodLockIndicator', () => {
  it('renders locked state', () => {
    render(<PeriodLockIndicator isLocked={true} />);
    expect(screen.getByText('مقفل للتقيد')).toBeInTheDocument();
  });

  it('renders unlocked state', () => {
    render(<PeriodLockIndicator isLocked={false} />);
    expect(screen.getByText('غير مقفل')).toBeInTheDocument();
  });
});
