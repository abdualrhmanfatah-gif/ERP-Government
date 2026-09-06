import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import PartyCreatePage from '../pages/PartyCreatePage';
import { partiesClient } from '../shared/client';

vi.mock('../shared/client', () => ({
  partiesClient: {
    create: vi.fn(),
    checkDuplicateTaxNumber: vi.fn(),
  },
}));

vi.mock('../hooks/useParties', () => ({
  useCreateParty: () => ({
    mutateAsync: vi.fn().mockResolvedValue(1),
    isPending: false,
  }),
}));

vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
}));

vi.mock('@/shared/hooks/usePermission', () => ({
  usePermission: () => ({ hasPermission: () => true }),
}));

describe('DuplicateTaxWarning', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows warning when duplicate tax number exists on blur', async () => {
    vi.mocked(partiesClient.checkDuplicateTaxNumber).mockResolvedValue(true);

    render(<PartyCreatePage />);
    const taxInput = screen.getByLabelText(/الرقم الضريبي/i);
    fireEvent.change(taxInput, { target: { value: '12345' } });
    fireEvent.blur(taxInput);

    await waitFor(() => {
      expect(screen.getByText(/رقم ضريبي مسجل مسبقاً/i)).toBeInTheDocument();
    });
  });

  it('hides warning when tax number is cleared', async () => {
    vi.mocked(partiesClient.checkDuplicateTaxNumber).mockResolvedValue(false);

    render(<PartyCreatePage />);
    const taxInput = screen.getByLabelText(/الرقم الضريبي/i);
    fireEvent.change(taxInput, { target: { value: '12345' } });
    fireEvent.blur(taxInput);

    await waitFor(() => {
      expect(partiesClient.checkDuplicateTaxNumber).toHaveBeenCalledWith('12345');
    });

    fireEvent.change(taxInput, { target: { value: '' } });

    await waitFor(() => {
      expect(screen.queryByText(/رقم ضريبي مسجل مسبقاً/i)).not.toBeInTheDocument();
    });
  });

  it('does not show warning for unique tax number', async () => {
    vi.mocked(partiesClient.checkDuplicateTaxNumber).mockResolvedValue(false);

    render(<PartyCreatePage />);
    const taxInput = screen.getByLabelText(/الرقم الضريبي/i);
    fireEvent.change(taxInput, { target: { value: 'UNIQUE-999' } });
    fireEvent.blur(taxInput);

    await waitFor(() => {
      expect(partiesClient.checkDuplicateTaxNumber).toHaveBeenCalledWith('UNIQUE-999');
    });

    expect(screen.queryByText(/رقم ضريبي مسجل مسبقاً/i)).not.toBeInTheDocument();
  });
});
