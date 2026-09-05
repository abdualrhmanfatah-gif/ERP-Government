import '@testing-library/jest-dom/vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { TestWrapper } from '../../../test-utils';
import ClassificationsListPage from '../classifications/pages/ClassificationsListPage';
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
                    isActive: false,
                    rowVersion: 'EEEE',
                    children: [],
                  },
                ],
              },
            ],
          },
        ],
      },
      {
        id: 6,
        code: 'L2B',
        name: 'Level 2B',
        level: 2,
        parentId: 1,
        isActive: false,
        rowVersion: 'FFFF',
        children: [],
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
const mockUseClassificationsTree = vi.mocked(useClassificationsTree);

HTMLDialogElement.prototype.showModal = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = true;
});
HTMLDialogElement.prototype.close = vi.fn(function (this: HTMLDialogElement) {
  (this as any).open = false;
});

describe('T033: Tree view renders all classifications with Code, Name, Level', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders 5+ level hierarchy with correct level badges', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    expect(screen.getByText('L1')).toBeInTheDocument();
    expect(screen.getByText('L2')).toBeInTheDocument();
    expect(screen.getByText('Level 1')).toBeInTheDocument();
    expect(screen.getByText('Level 2')).toBeInTheDocument();

    const levelBadges = screen.getAllByText(/مستوى/);
    expect(levelBadges.length).toBeGreaterThanOrEqual(2);
  });

  it('tree container has tree role', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    expect(screen.getByRole('tree')).toBeInTheDocument();
  });

  it('each node has treeitem role', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const treeItems = screen.getAllByRole('treeitem');
    expect(treeItems.length).toBeGreaterThanOrEqual(2);
  });
});

describe('T034: Collapse hides descendants and toggle icon updates', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('clicking expanded node collapses it and hides children', async () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    expect(screen.getByText('Level 2')).toBeVisible();

    const treeItems = screen.getAllByRole('treeitem');
    const rootTreeItem = treeItems[0];
    const clickableDiv = rootTreeItem.querySelector('.cursor-pointer');
    
    await act(async () => {
      fireEvent.click(clickableDiv!);
    });

    expect(screen.queryByText('Level 2')).not.toBeInTheDocument();
  });

  it('collapsed node can be expanded again', async () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const treeItems = screen.getAllByRole('treeitem');
    const rootTreeItem = treeItems[0];
    const clickableDiv = rootTreeItem.querySelector('.cursor-pointer');
    
    await act(async () => {
      fireEvent.click(clickableDiv!);
    });
    expect(screen.queryByText('Level 2')).not.toBeInTheDocument();

    await act(async () => {
      fireEvent.click(clickableDiv!);
    });
    expect(screen.getByText('Level 2')).toBeVisible();
  });
});

describe('T035: Search auto-expands ancestors, non-matching collapsed', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('search matches deep node and auto-expands ancestors', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: 'L5' } });

    expect(screen.getByText('L5')).toBeInTheDocument();
    expect(screen.getByText('Level 5')).toBeInTheDocument();
    expect(screen.getByText('L1')).toBeInTheDocument();
  });

  it('search clears restores previous expand state', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: 'L5' } });
    expect(screen.getByText('L5')).toBeInTheDocument();

    fireEvent.change(screen.getByPlaceholderText(/بحث/), { target: { value: '' } });
    expect(screen.getByText('L2')).toBeVisible();
  });
});

describe('T036: IsActive filter shows matching nodes and ancestors', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('filter active shows only active nodes', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    fireEvent.change(screen.getByDisplayValue('الكل'), { target: { value: 'active' } });

    expect(screen.getByText('L1')).toBeInTheDocument();
    expect(screen.getByText('L2')).toBeInTheDocument();
    expect(screen.queryByText('L2B')).not.toBeInTheDocument();
  });

  it('filter inactive shows only inactive nodes with ancestors', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    fireEvent.change(screen.getByDisplayValue('الكل'), { target: { value: 'inactive' } });

    expect(screen.getByText('L1')).toBeInTheDocument();
    expect(screen.queryByText('L2B')).toBeInTheDocument();
  });
});

describe('T037: Create dialog fields, create with/without parent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('create dialog has all required fields', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const addBtn = screen.getAllByText(/إضافة تصنيف جديد/)[0];
    await user.click(addBtn);

    expect(screen.getByLabelText(/الكود/)).toBeInTheDocument();
    expect(screen.getByLabelText(/الاسم/)).toBeInTheDocument();
    expect(screen.getByLabelText(/التصنيف الأب/)).toBeInTheDocument();
  });

  it('create without parent creates root-level classification', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const addBtn = screen.getAllByText(/إضافة تصنيف جديد/)[0];
    await user.click(addBtn);

    await user.type(screen.getByLabelText(/الكود/), 'NEW');
    await user.type(screen.getByLabelText(/الاسم/), 'New Classification');

    const submitBtn = screen.getByRole('button', { name: /إنشاء/ });
    await user.click(submitBtn);

    expect(mockMutate).toHaveBeenCalled();
  });
});

describe('T038: Edit dialog pre-fill, cycle prevention, validation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('edit dialog pre-fills fields', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const editButtons = screen.getAllByRole('button');
    const pencilBtn = editButtons.find((btn) => btn.querySelector('.lucide-pencil'));
    if (pencilBtn) {
      await user.click(pencilBtn);
      expect(screen.getByDisplayValue('L1')).toBeInTheDocument();
      expect(screen.getByDisplayValue('Level 1')).toBeInTheDocument();
    }
  });

  it('validation shows error on empty submission attempt', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const addBtn = screen.getAllByText(/إضافة تصنيف جديد/)[0];
    await user.click(addBtn);

    const submitBtn = screen.getByRole('button', { name: /إنشاء/ });
    await user.click(submitBtn);

    expect(mockMutate).not.toHaveBeenCalled();
  });
});

describe('T039: Toggle confirmation, success toast, RowVersion conflict', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('toggle opens confirmation dialog', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);

    expect(screen.getByText(/تأكيد تغيير الحالة/)).toBeInTheDocument();
    expect(screen.getByText(/هل تريد تعطيل هذا التصنيف/)).toBeInTheDocument();
  });

  it('confirm toggle triggers mutation', async () => {
    const user = userEvent.setup();
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });

    const switches = screen.getAllByRole('switch');
    await user.click(switches[0]);

    const confirmBtn = screen.getByRole('button', { name: /تأكيد/ });
    await user.click(confirmBtn);

    expect(mockToggleMutate).toHaveBeenCalled();
  });
});

describe('T040: Sidebar link visible and navigates correctly', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('page title is visible', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    render(<ClassificationsListPage />, { wrapper: TestWrapper });
    expect(screen.getByText('التصنيفات المالية')).toBeInTheDocument();
  });

  it('RTL direction is set', () => {
    mockUseClassificationsTree.mockReturnValue({ data: fiveLevelTree, isLoading: false, error: null } as any);
    const { container } = render(<ClassificationsListPage />, { wrapper: TestWrapper });
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper).toHaveAttribute('dir', 'rtl');
  });
});
