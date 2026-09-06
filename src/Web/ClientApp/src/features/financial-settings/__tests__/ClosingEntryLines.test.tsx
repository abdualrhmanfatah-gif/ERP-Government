import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { ClosingEntryLines } from '../components/ClosingEntryLines';

describe('ClosingEntryLines', () => {
  it('renders lines and totals', () => {
    const lines = [
      { accountCode: '4100', accountName: 'الإيرادات', debit: 0, credit: 1000 },
      { accountCode: '5100', accountName: 'المصروفات', debit: 800, credit: 0 },
    ];
    render(<ClosingEntryLines lines={lines} />);
    expect(screen.getByText('4100')).toBeInTheDocument();
    expect(screen.getByText('الإيرادات')).toBeInTheDocument();
    expect(screen.getByText('الإجمالي')).toBeInTheDocument();
  });

  it('shows unbalanced when totals differ', () => {
    const lines = [
      { accountCode: '4100', accountName: 'الإيرادات', debit: 0, credit: 1000 },
      { accountCode: '5100', accountName: 'المصروفات', debit: 500, credit: 0 },
    ];
    render(<ClosingEntryLines lines={lines} />);
    expect(screen.getByText('غير متوازن')).toBeInTheDocument();
  });
});
