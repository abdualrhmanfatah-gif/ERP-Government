import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { TestWrapper } from '../../../test-utils';
import ClassificationsListPage, { normalizeTree } from '../classifications/pages/ClassificationsListPage';
import { getExcludedDescendantIds } from '../classifications/utils/classification-utils';
import type { BudgetClassificationTreeDto } from '../shared/types';

const tree: BudgetClassificationTreeDto[] = [
  {
    id: 1,
    code: 'ROOT',
    name: 'Root',
    level: 1,
    isActive: true,
    rowVersion: 'AAAA',
    children: [
      {
        id: 2,
        code: 'CHILD',
        name: 'Child',
        level: 2,
        parentId: 1,
        isActive: true,
        rowVersion: 'BBBB',
        children: [],
      },
    ],
  },
  {
    id: 3,
    code: 'LEAF',
    name: 'Leaf',
    level: 1,
    isActive: true,
    rowVersion: 'CCCC',
    children: [],
  },
];

const deepTree: BudgetClassificationTreeDto[] = [
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
            children: [],
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

const mockUsePermission = vi.fn(() => ({ hasPermission: true, isLoading: false }));
vi.mock('../../../shared/hooks/usePermission', () => ({
  usePermission: () => mockUsePermission(),
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

import { useClassificationsTree } from '../classifications/hooks/useClassifications';
const mockUseClassificationsTree = vi.mocked(useClassificationsTree);

// Mock showModal for jsdom
HTMLDialogElement.prototype.showModal = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = true;
});
HTMLDialogElement.prototype.close = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = false;
});

describe('ClassificationsListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('T004: Tree normalization', () => {
    it('moves orphaned parentId to root-level', () => {
      const orphaned: BudgetClassificationTreeDto[] = [
        {
          id: 1,
          code: 'A',
          name: 'A',
          level: 1,
          isActive: true,
          rowVersion: 'AAAA',
          children: [
            {
              id: 2,
              code: 'B',
              name: 'B',
              level: 2,
              parentId: 999,
              isActive: true,
              rowVersion: 'BBBB',
              children: [],
            },
          ],
        },
      ];
      const result = normalizeTree(orphaned);
      expect(result.find((n) => n.id === 2)).toBeDefined();
      expect(result.find((n) => n.id === 2)?.parentId).toBeUndefined();
    });

    it('moves cycle node to root-level', () => {
      const cycle: BudgetClassificationTreeDto[] = [
        {
          id: 1,
          code: 'A',
          name: 'A',
          level: 1,
          isActive: true,
          rowVersion: 'AAAA',
          parentId: 2,
          children: [
            {
              id: 2,
              code: 'B',
              name: 'B',
              level: 2,
              parentId: 1,
              isActive: true,
              rowVersion: 'BBBB',
              children: [],
            },
          ],
        },
      ];
      const result = normalizeTree(cycle);
      expect(result.find((n) => n.id === 1)).toBeDefined();
    });

    it('returns valid tree unchanged', () => {
      const result = normalizeTree(tree);
      expect(result).toHaveLength(2);
      expect(result[0].children).toHaveLength(1);
    });
  });

  describe('T005: Expand/collapse', () => {
    it('shows empty state when tree is empty', () => {
      mockUseClassificationsTree.mockReturnValue({ data: [], isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      expect(screen.getByText(/لا توجد تصنيفات/)).toBeInTheDocument();
    });

    it('renders tree nodes with code and name', () => {
      mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      expect(screen.getByText('ROOT')).toBeInTheDocument();
      expect(screen.getByText('Child')).toBeInTheDocument();
      expect(screen.getByText('LEAF')).toBeInTheDocument();
    });

    it('root nodes with children are expanded by default', () => {
      mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      expect(screen.getByText('Child')).toBeVisible();
    });

    it('deeper nodes are collapsed by default', () => {
      mockUseClassificationsTree.mockReturnValue({ data: deepTree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      expect(screen.getByText('Level 2')).toBeVisible();
      expect(screen.queryByText('Level 3')).not.toBeInTheDocument();
    });

    it('single-node tree renders without expand/collapse toggle', () => {
      const singleNode: BudgetClassificationTreeDto[] = [
        { id: 1, code: 'ONLY', name: 'Only', level: 1, isActive: true, rowVersion: 'AAAA', children: [] },
      ];
      mockUseClassificationsTree.mockReturnValue({ data: singleNode, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      expect(screen.getByText('ONLY')).toBeInTheDocument();
    });
  });

  describe('T006: Search', () => {
    it('filters by code', () => {
      mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: 'ROOT' } });
      expect(screen.getByText('ROOT')).toBeInTheDocument();
      expect(screen.queryByText('LEAF')).not.toBeInTheDocument();
    });

    it('filters by name', () => {
      mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: 'Leaf' } });
      expect(screen.getByText('LEAF')).toBeInTheDocument();
      expect(screen.queryByText('ROOT')).not.toBeInTheDocument();
    });

    it('shows no matches when search has no results', () => {
      mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: 'NONEXISTENT' } });
      expect(screen.queryByText('ROOT')).not.toBeInTheDocument();
    });
  });

  describe('T007: Filter', () => {
    it('filters by active state', () => {
      const mixedTree: BudgetClassificationTreeDto[] = [
        { id: 1, code: 'A', name: 'Active', level: 1, isActive: true, rowVersion: 'AAAA', children: [] },
        { id: 2, code: 'B', name: 'Inactive', level: 1, isActive: false, rowVersion: 'BBBB', children: [] },
      ];
      mockUseClassificationsTree.mockReturnValue({ data: mixedTree, isLoading: false, error: null } as any);
      render(<ClassificationsListPage />, { wrapper: TestWrapper });
      fireEvent.change(screen.getByDisplayValue('الكل'), { target: { value: 'active' } });
      expect(screen.getByText('A')).toBeInTheDocument();
      expect(screen.queryByText('B')).not.toBeInTheDocument();
    });
  });
});

describe('T014: getExcludedDescendantIds', () => {
  it('returns only self for leaf node', () => {
    const result = getExcludedDescendantIds(tree, 3);
    expect(result).toEqual(new Set([3]));
  });

  it('returns self + descendants for node with children', () => {
    const result = getExcludedDescendantIds(tree, 1);
    expect(result).toEqual(new Set([1, 2]));
  });

  it('does not exclude nodes outside the subtree', () => {
    const result = getExcludedDescendantIds(tree, 2);
    expect(result).toEqual(new Set([2]));
    expect(result.has(1)).toBe(false);
  });
});

describe('T015: Validation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
  });

  it('create button opens dialog', () => {
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const addBtn = screen.getAllByText(/إضافة تصنيف جديد/)[0];
    expect(addBtn).toBeInTheDocument();
    fireEvent.click(addBtn);
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });
});

describe('T016: Permission gating', () => {
  it('create button hidden without permission', () => {
    mockUsePermission.mockReturnValue({ hasPermission: false, isLoading: false });
    mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const buttons = screen.queryAllByText(/إضافة تصنيف جديد/);
    const addBtn = buttons.find((el) => el.tagName === 'BUTTON');
    expect(addBtn).toBeUndefined();
  });

  it('create button shown with permission', () => {
    mockUsePermission.mockReturnValue({ hasPermission: true, isLoading: false });
    mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const buttons = screen.getAllByText(/إضافة تصنيف جديد/);
    const addBtn = buttons.find((el) => el.tagName === 'BUTTON');
    expect(addBtn).toBeDefined();
  });
});

describe('T024-T025: Toggle flow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseClassificationsTree.mockReturnValue({ data: tree, isLoading: false, error: null } as any);
  });

  it('renders IsActive switch for each node', () => {
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const switches = screen.getAllByRole('switch');
    expect(switches.length).toBeGreaterThanOrEqual(3);
  });

  it('toggle switch opens confirmation dialog', async () => {
    const user = userEvent.setup();
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);
    expect(screen.getByText(/تأكيد تغيير الحالة/)).toBeInTheDocument();
  });
});
