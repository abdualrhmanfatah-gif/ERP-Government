import { useFundsList } from '../funds/hooks/useFunds';
import { useCostCenters } from '../../organization/hooks/useCostCenters';
import { useAccountsList } from '../../accounting/hooks/useAccountsList';
import { useClassificationsTree } from '../classifications/hooks/useClassifications';

interface ClassificationNode {
  id: number;
  code: string;
  name: string;
  children?: ClassificationNode[];
}

function flattenClassifications(nodes: ClassificationNode[]): { id: number; label: string }[] {
  const result: { id: number; label: string }[] = [];
  for (const node of nodes) {
    result.push({ id: node.id, label: `${node.code} — ${node.name}` });
    if (node.children?.length) {
      result.push(...flattenClassifications(node.children));
    }
  }
  return result;
}

export function useBudgetItemFormFields() {
  const { data: funds = [] } = useFundsList();
  const { data: costCenters = [] } = useCostCenters();
  const { data: accounts = [] } = useAccountsList({ isPostable: true });
  const { data: rawClassifications = [] } = useClassificationsTree();

  const fundOptions = (funds as { id: number; isActive: boolean; fundName: string }[])
    .filter((f) => f.isActive)
    .map((f) => ({ id: f.id, label: f.fundName }));

  const costCenterOptions = (costCenters as { id: number; isActive: boolean; code: string; name: string }[])
    .filter((cc) => cc.isActive)
    .map((cc) => ({ id: cc.id, label: `${cc.code} — ${cc.name}` }));

  const accountOptions = (accounts as { id: number; isActive: boolean; isPostable: boolean; code: string; name: string }[])
    .filter((a) => a.isActive && a.isPostable)
    .map((a) => ({ id: a.id, label: `${a.code} — ${a.name}` }));

  const classificationOptions = flattenClassifications(rawClassifications as ClassificationNode[]);

  return { fundOptions, costCenterOptions, accountOptions, classificationOptions };
}
