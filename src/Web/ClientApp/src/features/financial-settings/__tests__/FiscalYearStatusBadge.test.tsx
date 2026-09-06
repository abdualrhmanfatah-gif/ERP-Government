import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { FiscalYearStatusBadge } from '../components/FiscalYearStatusBadge';

describe('FiscalYearStatusBadge', () => {
  it('renders Draft status in Arabic', () => {
    render(<FiscalYearStatusBadge status="Draft" />);
    expect(screen.getByText('مسودة')).toBeInTheDocument();
  });

  it('renders Open status in Arabic', () => {
    render(<FiscalYearStatusBadge status="Open" />);
    expect(screen.getByText('مفتوح')).toBeInTheDocument();
  });

  it('renders SoftClosed status in Arabic', () => {
    render(<FiscalYearStatusBadge status="SoftClosed" />);
    expect(screen.getByText('مغلق ( مؤقت )')).toBeInTheDocument();
  });

  it('renders HardClosed status in Arabic', () => {
    render(<FiscalYearStatusBadge status="HardClosed" />);
    expect(screen.getByText('مغلق')).toBeInTheDocument();
  });
});
