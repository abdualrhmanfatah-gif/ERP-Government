import type { BudgetClassificationTreeDto } from '../../shared/types';

export function getExcludedDescendantIds(
  tree: BudgetClassificationTreeDto[],
  excludeId: number,
): Set<number> {
  const excluded = new Set<number>([excludeId]);
  const queue = [excludeId];

  const idToNode = new Map<number, BudgetClassificationTreeDto>();
  const collectNodes = (nodes: BudgetClassificationTreeDto[]) => {
    for (const node of nodes) {
      idToNode.set(node.id, node);
      if (node.children?.length) collectNodes(node.children);
    }
  };
  collectNodes(tree);

  while (queue.length > 0) {
    const currentId = queue.shift()!;
    const node = idToNode.get(currentId);
    if (node?.children) {
      for (const child of node.children) {
        if (!excluded.has(child.id)) {
          excluded.add(child.id);
          queue.push(child.id);
        }
      }
    }
  }

  return excluded;
}
