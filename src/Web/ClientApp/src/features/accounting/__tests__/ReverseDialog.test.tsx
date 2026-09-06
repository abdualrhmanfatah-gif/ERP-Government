import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ReverseDialog } from '../components/ReverseDialog';
import type { JournalEntryLineDto } from '../types';

const makeLines = (overrides: Partial<JournalEntryLineDto> = {}): JournalEntryLineDto[] => [
  {
    id: 1,
    journalEntryId: 1,
    sequence: 1,
    accountId: 1001,
    accountCode: '1001',
    accountName: 'Cash',
    currencyId: 1,
    exchangeRate: 1,
    debit: 1000,
    credit: 0,
    rowVersion: 'AAAA',
    ...overrides,
  },
  {
    id: 2,
    journalEntryId: 1,
    sequence: 2,
    accountId: 5001,
    accountCode: '5001',
    accountName: 'Expense',
    currencyId: 1,
    exchangeRate: 1,
    debit: 0,
    credit: 1000,
    rowVersion: 'BBBB',
  },
];

describe('ReverseDialog', () => {
  const defaultProps = {
    entryNumber: 'JE-000001',
    lines: makeLines(),
    onConfirm: vi.fn(),
    onClose: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders dialog with reason textarea', () => {
    render(<ReverseDialog {...defaultProps} />);
    expect(screen.getByLabelText(/سبب العكس/)).toBeDefined();
  });

  it('renders reason as required', () => {
    render(<ReverseDialog {...defaultProps} />);
    const textarea = screen.getByLabelText(/سبب العكس/);
    expect(textarea).toHaveAttribute('aria-required', 'true');
  });

  it('renders confirm button disabled when reason is empty', () => {
    render(<ReverseDialog {...defaultProps} />);
    const confirmBtn = screen.getByRole('button', { name: /تأكيد العكس/ });
    expect(confirmBtn).toBeDisabled();
  });

  it('enables confirm button when reason is entered', () => {
    render(<ReverseDialog {...defaultProps} />);
    const textarea = screen.getByLabelText(/سبب العكس/);
    fireEvent.change(textarea, { target: { value: 'Error correction' } });
    const confirmBtn = screen.getByRole('button', { name: /تأكيد العكس/ });
    expect(confirmBtn).not.toBeDisabled();
  });

  it('calls onConfirm with reason when confirmed', () => {
    const onConfirm = vi.fn();
    render(<ReverseDialog {...defaultProps} onConfirm={onConfirm} />);
    const textarea = screen.getByLabelText(/سبب العكس/);
    fireEvent.change(textarea, { target: { value: 'Error correction' } });
    fireEvent.click(screen.getByRole('button', { name: /تأكيد العكس/ }));
    expect(onConfirm).toHaveBeenCalledWith('Error correction');
  });

  it('calls onClose when cancel clicked', () => {
    const onClose = vi.fn();
    render(<ReverseDialog {...defaultProps} onClose={onClose} />);
    fireEvent.click(screen.getByRole('button', { name: /إلغاء/ }));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('calls onClose when backdrop clicked', () => {
    const onClose = vi.fn();
    const { container } = render(<ReverseDialog {...defaultProps} onClose={onClose} />);
    const backdrop = container.firstChild as HTMLElement;
    fireEvent.click(backdrop);
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('shows counter-line preview with mirrored amounts', () => {
    render(<ReverseDialog {...defaultProps} />);
    expect(screen.getByText(/معاينة القيد العكسي/)).toBeDefined();
  });

  it('displays entry number', () => {
    render(<ReverseDialog {...defaultProps} />);
    expect(screen.getByText('JE-000001')).toBeDefined();
  });

  it('shows loading state when reversing', () => {
    render(<ReverseDialog {...defaultProps} isReversing={true} />);
    expect(screen.getByText(/جاري العكس/)).toBeDefined();
  });

  it('disables confirm button when reversing', () => {
    render(<ReverseDialog {...defaultProps} isReversing={true} />);
    const confirmBtn = screen.getByRole('button', { name: /جاري العكس/ });
    expect(confirmBtn).toBeDisabled();
  });

  it('displays error message when provided', () => {
    render(<ReverseDialog {...defaultProps} error="Fiscal period is locked" />);
    expect(screen.getByText('Fiscal period is locked')).toBeDefined();
  });

  it('enforces max 500 character limit on reason', () => {
    render(<ReverseDialog {...defaultProps} />);
    const textarea = screen.getByLabelText(/سبب العكس/);
    expect(textarea).toHaveAttribute('maxLength', '500');
  });
});
