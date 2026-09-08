import { useFundsList } from '@/features/budgeting/hooks/useFunds';
import { useProjects } from '@/features/organization/hooks/useProjects';
import { useEncumbrancesList } from '@/features/budgeting/hooks/useEncumbrances';
import { useQuery } from '@tanstack/react-query';
import { Select } from '@/components/ui/Select';
import {
  BudgetsClient,
  PaymentOrdersClient,
  type FundDto,
  type ProjectDto,
  type EncumbranceListItemDto,
  type PaymentOrderDto,
  type BudgetItemDto,
} from '../web-api-client';

const budgetsClient = new BudgetsClient();
const paymentOrdersClient = new PaymentOrdersClient();

export interface DimensionValues {
  fundId?: number | null;
  projectId?: number | null;
  budgetItemId?: number | null;
  encumbranceId?: number | null;
  paymentOrderId?: number | null;
}

interface DimensionPickersProps {
  value: DimensionValues;
  onChange: (dimensions: DimensionValues) => void;
  className?: string;
}

function useAllBudgetItems() {
  return useQuery({
    queryKey: ['budget-items-all'],
    queryFn: async (): Promise<BudgetItemDto[]> => {
      const budgets = await budgetsClient.budgetsAll();
      const allItems: BudgetItemDto[] = [];
      for (const b of budgets.slice(0, 5)) {
        try {
          const tree = await budgetsClient.tree2(b.id!);
          const flatten = (items: BudgetItemDto[]): void => {
            for (const item of items) {
              allItems.push(item);
              if (item.children?.length) flatten(item.children);
            }
          };
          flatten(tree);
        } catch {
          /* skip failed budget */
        }
      }
      return allItems;
    },
  });
}

function useAllPaymentOrders() {
  return useQuery({
    queryKey: ['payment-orders-all'],
    queryFn: () => paymentOrdersClient.paymentOrdersAll(undefined, undefined, undefined),
  });
}

export function DimensionPickers({ value, onChange, className = '' }: DimensionPickersProps) {
  const { data: funds = [] } = useFundsList();
  const { data: projects = [] } = useProjects();
  const { data: budgetItems = [] } = useAllBudgetItems();
  const { data: encumbrances = [] } = useEncumbrancesList();
  const { data: paymentOrders = [] } = useAllPaymentOrders();

  const update = (field: keyof DimensionValues, fieldValue: number | null) => {
    onChange({ ...value, [field]: fieldValue || null });
  };

  return (
    <div className={`grid grid-cols-5 gap-2 ${className}`}>
      <Select
        label="الصندوق"
        value={value.fundId ?? ''}
        onChange={(e) => update('fundId', e.target.value ? Number(e.target.value) : null)}
        options={[
          { value: '', label: '—' },
          ...funds.map((f: FundDto) => ({ value: String(f.id), label: `${f.fundNumber} - ${f.fundName}` })),
        ]}
      />
      <Select
        label="المشروع"
        value={value.projectId ?? ''}
        onChange={(e) => update('projectId', e.target.value ? Number(e.target.value) : null)}
        options={[
          { value: '', label: '—' },
          ...projects.map((p: ProjectDto) => ({ value: String(p.id), label: `${p.code} - ${p.name}` })),
        ]}
      />
      <Select
        label="البند"
        value={value.budgetItemId ?? ''}
        onChange={(e) => update('budgetItemId', e.target.value ? Number(e.target.value) : null)}
        options={[
          { value: '', label: '—' },
          ...budgetItems.map((bi: BudgetItemDto) => ({ value: String(bi.id), label: `${bi.code} - ${bi.name}` })),
        ]}
      />
      <Select
        label="الالتزام"
        value={value.encumbranceId ?? ''}
        onChange={(e) => update('encumbranceId', e.target.value ? Number(e.target.value) : null)}
        options={[
          { value: '', label: '—' },
          ...encumbrances.map((en: EncumbranceListItemDto) => ({ value: String(en.id), label: en.encumbranceNumber })),
        ]}
      />
      <Select
        label="أمر الدفع"
        value={value.paymentOrderId ?? ''}
        onChange={(e) => update('paymentOrderId', e.target.value ? Number(e.target.value) : null)}
        options={[
          { value: '', label: '—' },
          ...paymentOrders.map((po: PaymentOrderDto) => ({ value: String(po.id), label: po.paymentOrderNumber })),
        ]}
      />
    </div>
  );
}
