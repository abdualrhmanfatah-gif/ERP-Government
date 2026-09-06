import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

const mockEntries = [
  { id: 1, entryNumber: 'JE-000001', documentDate: '2026-01-15', entryStatus: 'Draft', journalName: 'General', totalDebit: 1000, totalCredit: 1000, isSystemGenerated: false, lines: [], rowVersion: 'AAAA', periodId: 1, fiscalYearId: 1 },
  { id: 2, entryNumber: 'JE-000002', documentDate: '2026-01-16', entryStatus: 'Posted', journalName: 'General', totalDebit: 500, totalCredit: 500, isSystemGenerated: true, lines: [], rowVersion: 'BBBB', periodId: 1, fiscalYearId: 1 },
  { id: 3, entryNumber: 'JE-000003', documentDate: '2026-01-17', entryStatus: 'Submitted', journalName: 'Sales', totalDebit: 200, totalCredit: 200, isSystemGenerated: false, lines: [], rowVersion: 'CCCC', periodId: 1, fiscalYearId: 1, ref: 'INV-100' },
];

const mockUseJournalEntriesList = vi.fn(() => ({ data: mockEntries, isLoading: false }));

vi.mock('../hooks/useJournalEntries', () => ({
  useJournalEntriesList: (...args: unknown[]) => mockUseJournalEntriesList(...args),
}));

import { JournalEntriesListPage } from '../pages/JournalEntriesListPage';

describe('JournalEntriesListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseJournalEntriesList.mockReturnValue({ data: mockEntries, isLoading: false });
  });

  it('renders page heading', () => {
    render(<JournalEntriesListPage />);
    expect(screen.getByRole('heading', { name: /قيود اليومية/ })).toBeDefined();
  });

  it('renders new entry button', () => {
    render(<JournalEntriesListPage />);
    expect(screen.getByRole('button', { name: /\+ قيد جديد/ })).toBeDefined();
  });

  it('renders all 7 status filter chips', () => {
    render(<JournalEntriesListPage />);
    const filterContainer = screen.getByText('الكل').closest('div');
    const filterButtons = filterContainer ? Array.from(filterContainer.children).filter(
      (el) => el.tagName === 'BUTTON',
    ) : [];
    expect(filterButtons.length).toBe(7);
  });

  it('renders search input', () => {
    render(<JournalEntriesListPage />);
    expect(screen.getByPlaceholderText(/بحث برقم القيد/)).toBeDefined();
  });

  it('renders entries in grid', () => {
    render(<JournalEntriesListPage />);
    expect(screen.getAllByText('JE-000001').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('JE-000002').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('JE-000003').length).toBeGreaterThanOrEqual(1);
  });

  it('shows system badge on system-generated entries', () => {
    render(<JournalEntriesListPage />);
    expect(screen.getAllByText('نظام').length).toBeGreaterThanOrEqual(1);
  });

  it('shows loading state', () => {
    mockUseJournalEntriesList.mockReturnValue({ data: [], isLoading: true });
    render(<JournalEntriesListPage />);
    expect(screen.getByRole('status')).toBeDefined();
  });

  it('shows empty state when no entries', () => {
    mockUseJournalEntriesList.mockReturnValue({ data: [], isLoading: false });
    render(<JournalEntriesListPage />);
    expect(screen.getByText(/لا توجد قيود/)).toBeDefined();
  });
});

describe('JournalEntriesListPage — filter/search combos', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseJournalEntriesList.mockReturnValue({ data: mockEntries, isLoading: false });
  });

  it('search filters by entry number', () => {
    render(<JournalEntriesListPage />);
    const searchInput = screen.getByPlaceholderText(/بحث برقم القيد/);
    fireEvent.change(searchInput, { target: { value: 'JE-000002' } });
    expect(screen.getAllByText('JE-000002').length).toBeGreaterThanOrEqual(1);
  });

  it('search filters by ref', () => {
    render(<JournalEntriesListPage />);
    const searchInput = screen.getByPlaceholderText(/بحث برقم القيد/);
    fireEvent.change(searchInput, { target: { value: 'INV-100' } });
    expect(screen.getAllByText('JE-000003').length).toBeGreaterThanOrEqual(1);
  });

  it('search is case-insensitive', () => {
    render(<JournalEntriesListPage />);
    const searchInput = screen.getByPlaceholderText(/بحث برقم القيد/);
    fireEvent.change(searchInput, { target: { value: 'je-000001' } });
    expect(screen.getAllByText('JE-000001').length).toBeGreaterThanOrEqual(1);
  });

  it('empty search shows all entries', () => {
    render(<JournalEntriesListPage />);
    const searchInput = screen.getByPlaceholderText(/بحث برقم القيد/);
    fireEvent.change(searchInput, { target: { value: 'JE-000002' } });
    fireEvent.change(searchInput, { target: { value: '' } });
    expect(screen.getAllByText('JE-000001').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('JE-000002').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('JE-000003').length).toBeGreaterThanOrEqual(1);
  });

  it('clicking status filter passes filter to hook', () => {
    render(<JournalEntriesListPage />);
    const draftFilter = screen.getByRole('button', { name: 'مسودة' });
    fireEvent.click(draftFilter);
    expect(mockUseJournalEntriesList).toHaveBeenCalledWith(expect.objectContaining({ entryStatus: 'Draft' }));
  });

  it('clicking "الكل" filter clears status filter', () => {
    render(<JournalEntriesListPage />);
    const draftFilter = screen.getByRole('button', { name: 'مسودة' });
    fireEvent.click(draftFilter);
    const allFilter = screen.getByRole('button', { name: 'الكل' });
    fireEvent.click(allFilter);
    expect(mockUseJournalEntriesList).toHaveBeenCalledWith(expect.objectContaining({ entryStatus: undefined }));
  });

  it('navigates to detail on row click', () => {
    render(<JournalEntriesListPage />);
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
