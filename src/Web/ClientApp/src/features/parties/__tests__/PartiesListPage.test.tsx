import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import PartiesListPage from '../pages/PartiesListPage';
import { usePartiesList, useTogglePartyActive } from '../hooks/useParties';
import { usePermission } from '@/shared/hooks/usePermission';
import { PartyType } from '../shared/types';

vi.mock('../hooks/useParties');
vi.mock('@/shared/hooks/usePermission');
vi.mock('react-router-dom', () => ({
  useNavigate: () => vi.fn(),
  useSearchParams: () => [new URLSearchParams(), vi.fn()],
}));

const mockParties = [
  { id: 1, partyCode: 'P-000001', partyType: PartyType.Supplier, nameAr: 'شركة أحمد', nameEn: 'Ahmed Co', taxNumber: '12345', nationalId: null, phone: null, email: null, address: null, notes: null, isActive: true },
  { id: 2, partyCode: 'P-000002', partyType: PartyType.Customer, nameAr: 'عميل خالد', nameEn: null, taxNumber: '67890', nationalId: null, phone: null, email: null, address: null, notes: null, isActive: false },
];

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(usePartiesList).mockReturnValue({ data: mockParties, isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
  vi.mocked(useTogglePartyActive).mockReturnValue({ mutateAsync: vi.fn().mockResolvedValue(undefined), isPending: false } as any);
  vi.mocked(usePermission).mockReturnValue({ hasPermission: true, isLoading: false });
});

describe('PartiesListPage', () => {
  it('renders party list', () => {
    render(<PartiesListPage />);
    expect(screen.getAllByText('شركة أحمد').length).toBeGreaterThan(0);
    expect(screen.getAllByText('عميل خالد').length).toBeGreaterThan(0);
  });

  it('renders search input', () => {
    render(<PartiesListPage />);
    expect(screen.getByPlaceholderText(/بحث/i)).toBeInTheDocument();
  });

  it('shows New Party button when permitted', () => {
    vi.mocked(usePermission).mockReturnValue({ hasPermission: true, isLoading: false });
    render(<PartiesListPage />);
    expect(screen.getByRole('button', { name: /جديد/i })).toBeInTheDocument();
  });

  it('hides New Party button when user lacks Parties.Create permission', () => {
    vi.mocked(usePermission).mockImplementation((policy?: string) => ({
      hasPermission: policy !== 'Parties.Create',
      isLoading: false,
    }));
    render(<PartiesListPage />);
    expect(screen.queryByRole('button', { name: /جديد/i })).not.toBeInTheDocument();
  });

  it('hides toggle active when user lacks Parties.Update permission', () => {
    vi.mocked(usePermission).mockImplementation((policy?: string) => ({
      hasPermission: policy !== 'Parties.Update',
      isLoading: false,
    }));
    render(<PartiesListPage />);
    expect(screen.queryByRole('button', { name: /تعطيل/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /تفعيل/i })).not.toBeInTheDocument();
  });

  it('shows unauthorized state when user lacks Parties.View permission', () => {
    vi.mocked(usePermission).mockReturnValue({ hasPermission: false, isLoading: false });
    render(<PartiesListPage />);
    expect(screen.getByText(/غير مصرح بالوصول/i)).toBeInTheDocument();
  });

  it('shows empty state when no parties', () => {
    vi.mocked(usePartiesList).mockReturnValue({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() } as any);
    render(<PartiesListPage />);
    expect(screen.getByText(/لا توجد أطراف/i)).toBeInTheDocument();
  });

  it('shows error state with retry when query fails', () => {
    vi.mocked(usePartiesList).mockReturnValue({ data: undefined, isLoading: false, isError: true, error: new Error('fail'), refetch: vi.fn() } as any);
    render(<PartiesListPage />);
    expect(screen.getByText(/حدث خطأ/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /إعادة المحاولة/i })).toBeInTheDocument();
  });
});
