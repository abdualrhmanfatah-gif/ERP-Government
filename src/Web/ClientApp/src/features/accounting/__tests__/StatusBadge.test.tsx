import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatusBadge } from '../components/StatusBadge';
import { EntryStatus } from '../types';

describe('StatusBadge', () => {
  it('renders Draft status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Draft} />);
    const badge = screen.getByText('مسودة');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('statusDraft');
  });

  it('renders Submitted status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Submitted} />);
    const badge = screen.getByText('مقدمة');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('statusPending');
  });

  it('renders Approved status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Approved} />);
    const badge = screen.getByText('موافق عليها');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('statusApproved');
  });

  it('renders Posted status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Posted} />);
    const badge = screen.getByText('محاسبة');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('statusActive');
  });

  it('renders Reversed status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Reversed} />);
    const badge = screen.getByText('معكوسة');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('statusClosed');
  });

  it('renders Cancelled status with correct Arabic label', () => {
    render(<StatusBadge status={EntryStatus.Cancelled} />);
    const badge = screen.getByText('ملغاة');
    expect(badge).toBeDefined();
    expect(badge.getAttribute('style')).toContain('errorContainer');
  });

  it('applies custom className', () => {
    render(<StatusBadge status={EntryStatus.Draft} className="test-class" />);
    const badge = screen.getByText('مسودة');
    expect(badge.className).toContain('test-class');
  });
});
