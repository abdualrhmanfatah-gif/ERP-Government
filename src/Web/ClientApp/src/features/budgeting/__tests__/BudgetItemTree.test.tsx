import '@testing-library/jest-dom/vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { BudgetItemTree } from '../components/BudgetItemTree';
import type { BudgetItemTreeDto } from '../../../web-api-client';

const nodes = [
  {
    id: 1,
    itemCode: 'ROOT',
    itemName: 'Root',
    budgetId: 1,
    level: 1,
    isActive: true,
    children: [
      {
        id: 2,
        itemCode: 'CHILD',
        itemName: 'Child',
        budgetId: 1,
        parentId: 1,
        level: 2,
        isActive: true,
        children: [],
      },
    ],
  },
] as unknown as BudgetItemTreeDto[];

describe('BudgetItemTree', () => {
  it('renders nodes with codes and computed levels', () => {
    render(<BudgetItemTree nodes={nodes} />);
    expect(screen.getByText('ROOT')).toBeInTheDocument();
    expect(screen.getByText('CHILD')).toBeInTheDocument();
    expect(screen.getByText('مستوى 2')).toBeInTheDocument();
  });

  it('shows the empty state with an add action when permitted', () => {
    const onAddChild = vi.fn();
    render(<BudgetItemTree nodes={[]} onAddChild={onAddChild} canEdit />);
    expect(screen.getByText('لا توجد بنود بعد')).toBeInTheDocument();
    fireEvent.click(screen.getByText('إضافة بند رئيسي'));
    expect(onAddChild).toHaveBeenCalledWith(undefined);
  });

  it('selects a node with keyboard Enter', () => {
    const onSelect = vi.fn();
    render(<BudgetItemTree nodes={nodes} onSelect={onSelect} />);
    const item = screen.getByRole('treeitem', { name: /ROOT/ });
    item.focus();
    fireEvent.keyDown(item, { key: 'Enter' });
    expect(onSelect).toHaveBeenCalledWith(1);
  });
});
