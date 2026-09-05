import '@testing-library/jest-dom/vitest';
import { describe, expect, it } from 'vitest';
import { normalizeTree } from '../classifications/pages/ClassificationsListPage';
import type { BudgetClassificationTreeDto } from '../../shared/types';

const makeNode = (
  id: number,
  code: string,
  parentId?: number,
  children: BudgetClassificationTreeDto[] = [],
): BudgetClassificationTreeDto => ({
  id,
  code,
  name: `Name ${code}`,
  level: 1,
  isActive: true,
  rowVersion: 'AAAA',
  parentId,
  children,
});

describe('normalizeTree', () => {
  it('moves a node with orphaned parentId to root level', () => {
    const tree: BudgetClassificationTreeDto[] = [
      makeNode(1, 'A'),
      makeNode(2, 'B', 999),
    ];
    const result = normalizeTree(tree);
    expect(result).toHaveLength(2);
    expect(result.find((n) => n.id === 2)?.parentId).toBeUndefined();
  });

  it('moves a node forming a cycle (parentId is descendant) to root level', () => {
    const child = makeNode(2, 'B', 1);
    const parent = makeNode(1, 'A', 2, [child]);
    const tree: BudgetClassificationTreeDto[] = [parent];
    const result = normalizeTree(tree);
    expect(result).toHaveLength(2);
    expect(result.find((n) => n.id === 1)?.parentId).toBeUndefined();
    expect(result.find((n) => n.id === 2)?.parentId).toBeUndefined();
  });

  it('returns a valid tree unchanged', () => {
    const child = makeNode(2, 'B', 1);
    const parent = makeNode(1, 'A', undefined, [child]);
    const tree: BudgetClassificationTreeDto[] = [parent];
    const result = normalizeTree(tree);
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe(1);
    expect(result[0].parentId).toBeUndefined();
    expect(result[0].children).toHaveLength(1);
    expect(result[0].children![0].id).toBe(2);
    expect(result[0].children![0].parentId).toBe(1);
  });
});
