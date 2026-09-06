import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { DimensionPickers } from '../components/DimensionPickers';

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });

vi.mock('../../budgeting/hooks/useFunds', () => ({
  useFundsList: () => ({
    data: [
      { id: 1, code: 'F001', name: 'General Fund' },
      { id: 2, code: 'F002', name: 'Special Fund' },
    ],
  }),
}));

vi.mock('../../organization/hooks/useProjects', () => ({
  useProjects: () => ({
    data: [
      { id: 10, name: 'Project Alpha' },
      { id: 20, name: 'Project Beta' },
    ],
  }),
}));

vi.mock('../../budgeting/hooks/useEncumbrances', () => ({
  useEncumbrancesList: () => ({
    data: [
      { id: 100, encumbranceNumber: 'ENC-001' },
      { id: 200, encumbranceNumber: 'ENC-002' },
    ],
  }),
}));

vi.mock('../../../web-api-client', () => ({
  BudgetsClient: class {
    budgetsAll = vi.fn().mockResolvedValue([]);
    tree2 = vi.fn().mockResolvedValue([]);
  },
  PaymentOrdersClient: class {
    paymentOrdersAll = vi.fn().mockResolvedValue([
      { id: 500, paymentOrderNumber: 'PO-001' },
      { id: 600, paymentOrderNumber: 'PO-002' },
    ]);
  },
}));

function renderPickers(props: { value?: Record<string, number | null>; onChange?: ReturnType<typeof vi.fn> } = {}) {
  const defaultProps = {
    value: { fundId: null, projectId: null, budgetItemId: null, encumbranceId: null, paymentOrderId: null },
    onChange: vi.fn(),
    ...props,
  };
  return {
    ...render(
      <QueryClientProvider client={queryClient}>
        <DimensionPickers value={defaultProps.value as never} onChange={defaultProps.onChange} />
      </QueryClientProvider>,
    ),
    onChange: defaultProps.onChange,
  };
}

describe('DimensionPickers', () => {
  it('renders all five dimension labels', () => {
    renderPickers();
    expect(screen.getByText('الصندوق')).toBeDefined();
    expect(screen.getByText('المشروع')).toBeDefined();
    expect(screen.getByText('البند')).toBeDefined();
    expect(screen.getByText('الالتزام')).toBeDefined();
    expect(screen.getByText('أمر الدفع')).toBeDefined();
  });

  it('renders five select elements', () => {
    renderPickers();
    const selects = document.querySelectorAll('select');
    expect(selects.length).toBe(5);
  });

  it('renders fund options from API', () => {
    renderPickers();
    const fundSelect = document.querySelectorAll('select')[0];
    expect((fundSelect as HTMLSelectElement).options.length).toBeGreaterThanOrEqual(3);
  });

  it('renders project options from API', () => {
    renderPickers();
    const projectSelect = document.querySelectorAll('select')[1];
    expect((projectSelect as HTMLSelectElement).options.length).toBeGreaterThanOrEqual(3);
  });

  it('calls onChange when fund is selected', () => {
    const { onChange } = renderPickers();
    const fundSelect = document.querySelectorAll('select')[0];
    fireEvent.change(fundSelect, { target: { value: '1' } });
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ fundId: 1 }),
    );
  });

  it('calls onChange with null when fund is cleared', () => {
    const { onChange } = renderPickers({ value: { fundId: 1, projectId: null, budgetItemId: null, encumbranceId: null, paymentOrderId: null } });
    const fundSelect = document.querySelectorAll('select')[0];
    fireEvent.change(fundSelect, { target: { value: '' } });
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({ fundId: null }),
    );
  });

  it('renders all selects as optional (blank option first)', () => {
    renderPickers();
    const selects = document.querySelectorAll('select');
    selects.forEach((sel) => {
      expect((sel as HTMLSelectElement).options[0].value).toBe('');
    });
  });
});
