import '@testing-library/jest-dom/vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import PartyDetailPage from '../pages/PartyDetailPage';
import { useParty, useUpdateParty, useTogglePartyActive, usePartyDocuments } from '../hooks/useParties';
import { usePermission } from '@/shared/hooks/usePermission';
import { PartyType } from '../shared/types';

vi.mock('../hooks/useParties');
vi.mock('@/shared/hooks/usePermission');
vi.mock('@/features/documents/hooks/useDocuments', () => ({
  useApprovals: () => ({ data: [], isLoading: false, isError: false, error: null }),
  useStatusLog: () => ({ data: [], isLoading: false, isError: false, error: null }),
  useAttachments: () => ({ data: [], isLoading: false, isError: false, error: null }),
  useAttachmentRequirements: () => ({ data: [], isLoading: false, isError: false, error: null }),
  useAttachmentGateCheck: () => ({ data: [], isLoading: false, isError: false, error: null }),
  useUploadAttachment: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useDeleteAttachment: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));
vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
  useParams: () => ({ id: '1' }),
}));

const mockParty = {
  id: 1,
  partyCode: 'P-000001',
  partyType: PartyType.Supplier,
  nameAr: 'شركة أحمد',
  nameEn: 'Ahmed Co',
  taxNumber: '12345',
  nationalId: 'ID-001',
  phone: '+967-777-000000',
  email: 'ahmed@example.com',
  address: 'صنعاء',
  notes: null,
  isActive: true,
};

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(useParty).mockReturnValue({ data: mockParty, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
  vi.mocked(useUpdateParty).mockReturnValue({
    mutateAsync: vi.fn(),
    isPending: false,
    error: null,
    isError: false,
  } as any);
  vi.mocked(useTogglePartyActive).mockReturnValue({ mutateAsync: vi.fn().mockResolvedValue(undefined), isPending: false } as any);
  vi.mocked(usePartyDocuments).mockReturnValue({ data: [], isLoading: false, isError: false, error: null } as any);
  vi.mocked(usePermission).mockReturnValue({ hasPermission: true, isLoading: false });
});

describe('PartyDetailPage', () => {
  it('renders party profile', () => {
    render(<PartyDetailPage />);
    expect(screen.getByText('P-000001')).toBeInTheDocument();
  });

  it('shows Edit button when permitted', () => {
    vi.mocked(usePermission).mockReturnValue({ hasPermission: true, isLoading: false });
    render(<PartyDetailPage />);
    expect(screen.getByRole('button', { name: /تعديل/i })).toBeInTheDocument();
  });

  it('hides Edit button when user lacks Parties.Update permission', () => {
    vi.mocked(usePermission).mockReturnValue({ hasPermission: false, isLoading: false });
    render(<PartyDetailPage />);
    expect(screen.queryByRole('button', { name: /تعديل/i })).not.toBeInTheDocument();
  });

  it('shows not-found state when party is null', () => {
    vi.mocked(useParty).mockReturnValue({ data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    render(<PartyDetailPage />);
    expect(screen.getByText(/لم يتم العثور على الطرف/i)).toBeInTheDocument();
  });

  it('shows not-found state when party query errors', () => {
    vi.mocked(useParty).mockReturnValue({ data: undefined, isLoading: false, isError: true, error: new Error('not found'), refetch: vi.fn() } as any);
    render(<PartyDetailPage />);
    expect(screen.getByText(/لم يتم العثور على الطرف/i)).toBeInTheDocument();
  });

  it('shows conflict dialog when update fails with 409', async () => {
    const mockMutate = vi.fn().mockRejectedValue({ status: 409 });
    vi.mocked(useUpdateParty).mockReturnValue({
      mutateAsync: mockMutate,
      isPending: false,
      error: { status: 409 },
      isError: true,
    } as any);
    render(<PartyDetailPage />);

    await userEvent.click(screen.getByRole('button', { name: /تعديل/i }));
    await userEvent.click(screen.getByRole('button', { name: /حفظ/i }));

    await waitFor(() => {
      expect(screen.getByText(/تعارض/i)).toBeInTheDocument();
    });
  });

  it('shows inline validation error on server 400 response', async () => {
    const mockMutate = vi.fn().mockRejectedValue({
      status: 400,
      errors: { nameAr: ['الاسم بالعربية مطلوب'] },
    });
    vi.mocked(useUpdateParty).mockReturnValue({
      mutateAsync: mockMutate,
      isPending: false,
      error: { status: 400, errors: { nameAr: ['الاسم بالعربية مطلوب'] } },
      isError: true,
    } as any);
    render(<PartyDetailPage />);

    await userEvent.click(screen.getByRole('button', { name: /تعديل/i }));
    await userEvent.click(screen.getByRole('button', { name: /حفظ/i }));

    await waitFor(() => {
      expect(screen.getByText('الاسم بالعربية مطلوب')).toBeInTheDocument();
    });
  });
});
