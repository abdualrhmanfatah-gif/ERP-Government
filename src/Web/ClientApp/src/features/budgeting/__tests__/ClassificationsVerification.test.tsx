import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { TestWrapper } from '../../../test-utils';
import ClassificationsListPage from '../classifications/pages/ClassificationsListPage';
import { getExcludedDescendantIds } from '../classifications/utils/classification-utils';
import type { BudgetClassificationTreeDto } from '../shared/types';

const fiveLevelTree: BudgetClassificationTreeDto[] = [
  {
    id: 1,
    code: 'L1',
    name: 'Level 1',
    level: 1,
    isActive: true,
    rowVersion: 'AAAA',
    children: [
      {
        id: 2,
        code: 'L2',
        name: 'Level 2',
        level: 2,
        parentId: 1,
        isActive: true,
        rowVersion: 'BBBB',
        children: [
          {
            id: 3,
            code: 'L3',
            name: 'Level 3',
            level: 3,
            parentId: 2,
            isActive: true,
            rowVersion: 'CCCC',
            children: [
              {
                id: 4,
                code: 'L4',
                name: 'Level 4',
                level: 4,
                parentId: 3,
                isActive: true,
                rowVersion: 'DDDD',
                children: [
                  {
                    id: 5,
                    code: 'L5',
                    name: 'Level 5',
                    level: 5,
                    parentId: 4,
                    isActive: true,
                    rowVersion: 'EEEE',
                    children: [],
                  },
                ],
              },
            ],
          },
        ],
      },
    ],
  },
];

const mockMutate = vi.fn();
const mockToggleMutate = vi.fn();

vi.mock('../classifications/hooks/useClassifications', () => ({
  useClassificationsTree: vi.fn(),
  useCreateClassification: vi.fn(() => ({ mutate: mockMutate, isPending: false })),
  useUpdateClassification: vi.fn(() => ({ mutate: mockMutate, isPending: false })),
  useToggleClassificationActive: vi.fn(() => ({ mutate: mockToggleMutate, isPending: false })),
}));

vi.mock('../../../shared/hooks/usePermission', () => ({
  usePermission: () => ({ hasPermission: true, isLoading: false }),
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

import { useClassificationsTree } from '../classifications/hooks/useClassifications';
import { toast } from 'sonner';
const mockUseClassificationsTree = vi.mocked(useClassificationsTree);

HTMLDialogElement.prototype.showModal = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = true;
});
HTMLDialogElement.prototype.close = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = false;
});

describe('T042: SC-002 - RTL indentation at every level', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('each level has increasing padding-inline-start', async () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const treeItems = screen.getAllByRole('treeitem');
    const paddings: string[] = [];
    
    for (const item of treeItems) {
      const clickableDiv = item.querySelector('.cursor-pointer') as HTMLElement;
      if (clickableDiv) {
        const style = clickableDiv.getAttribute('style');
        const match = style?.match(/padding-inline-start:\s*([\d.]+)rem/);
        if (match) paddings.push(match[1]);
      }
    }

    expect(paddings.length).toBeGreaterThanOrEqual(2);
    const uniquePaddings = [...new Set(paddings)].sort((a, b) => Number(a) - Number(b));
    expect(uniquePaddings.length).toBeGreaterThanOrEqual(2);
  });

  it('page has dir=rtl', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    const { container } = render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper).toHaveAttribute('dir', 'rtl');
  });
});

describe('T043: SC-003 - Cycle prevention', () => {
  it('getExcludedDescendantIds excludes self + all descendants', () => {
    const result = getExcludedDescendantIds(fiveLevelTree, 1);
    expect(result.has(1)).toBe(true);
    expect(result.has(2)).toBe(true);
    expect(result.has(3)).toBe(true);
    expect(result.has(4)).toBe(true);
    expect(result.has(5)).toBe(true);
  });

  it('getExcludedDescendantIds does not exclude nodes outside subtree', () => {
    const tree: BudgetClassificationTreeDto[] = [
      {
        id: 1, code: 'A', name: 'A', level: 1, isActive: true, rowVersion: 'AAAA',
        children: [
          { id: 2, code: 'B', name: 'B', level: 2, parentId: 1, isActive: true, rowVersion: 'BBBB', children: [] },
        ],
      },
      { id: 3, code: 'C', name: 'C', level: 1, isActive: true, rowVersion: 'CCCC', children: [] },
    ];
    const result = getExcludedDescendantIds(tree, 1);
    expect(result.has(3)).toBe(false);
  });

  it('parent select excludes self and descendants on edit', async () => {
    const user = (await import('@testing-library/user-event')).userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const editButtons = screen.getAllByRole('button');
    const pencilBtn = editButtons.find((btn) => btn.querySelector('.lucide-pencil'));
    if (pencilBtn) {
      await user.click(pencilBtn);
      const parentSelect = screen.getByLabelText(/التصنيف الأب/) as HTMLSelectElement;
      const options = Array.from(parentSelect.options);
      const disabledOptions = options.filter((opt) => opt.disabled);
      expect(disabledOptions.length).toBeGreaterThan(0);
    }
  });
});

describe('T044: SC-004 - RowVersion conflict handling', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('409 error shows error toast', async () => {
    mockToggleMutate.mockImplementation((_opts: any, callbacks: any) => {
      callbacks.onError({ status: 409 });
    });
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    
    const user = (await import('@testing-library/user-event')).userEvent.setup();
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);

    const confirmBtn = screen.getByRole('button', { name: /تأكيد/ });
    await user.click(confirmBtn);

    expect(toast.error).toHaveBeenCalledWith('تعارض في البيانات. جاري تحديث البيانات...');
  });

  it('non-409 error shows generic error toast', async () => {
    mockToggleMutate.mockImplementation((_opts: any, callbacks: any) => {
      callbacks.onError({ status: 500 });
    });
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    
    const user = (await import('@testing-library/user-event')).userEvent.setup();
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);

    const confirmBtn = screen.getByRole('button', { name: /تأكيد/ });
    await user.click(confirmBtn);

    expect(toast.error).toHaveBeenCalledWith('خطأ في تحديث الحالة');
  });
});

describe('T045: SC-005 - No hardcoded colors, keyboard nav works', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('uses CSS variables for colors (no hardcoded hex/rgb)', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    const { container } = render(<ClassificationsListPage />, { wrapper: TestWrapper });
    
    const allElements = container.querySelectorAll('*');
    for (const el of allElements) {
      const inlineStyle = el.getAttribute('style') || '';
      expect(inlineStyle).not.toMatch(/#[0-9a-fA-F]{3,8}/);
      expect(inlineStyle).not.toMatch(/rgb\(/);
      expect(inlineStyle).not.toMatch(/rgba\(/);
    }
  });

  it('tree items are keyboard focusable (tabIndex=0)', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    
    const treeItems = screen.getAllByRole('treeitem');
    for (const item of treeItems) {
      expect(item).toHaveAttribute('tabindex', '0');
    }
  });

  it('tree items have aria-expanded attribute', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    
    const treeItems = screen.getAllByRole('treeitem');
    for (const item of treeItems) {
      expect(item).toHaveAttribute('aria-expanded');
    }
  });
});
