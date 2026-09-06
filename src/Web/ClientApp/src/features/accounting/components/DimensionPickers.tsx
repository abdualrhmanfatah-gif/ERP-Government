import { useFundsList } from '../../budgeting/hooks/useFunds';
import { useProjects } from '../../organization/hooks/useProjects';
import { useEncumbrancesList } from '../../budgeting/hooks/useEncumbrances';
import { useQuery } from '@tanstack/react-query';
import {
  BudgetsClient,
  PaymentOrdersClient,
  type FundDto,
  type ProjectDto,
  type EncumbranceListItemDto,
  type PaymentOrderDto,
  type BudgetItemDto,
} from '../../../web-api-client';

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

const selectClass =
  'w-full px-2 py-1.5 rounded border text-xs focus:outline-none focus:ring-1 transition-colors duration-200';
const selectStyle = {
  backgroundColor: 'var(--color-surface)',
  color: 'var(--color-onSurface)',
  borderColor: 'var(--color-outlineVariant)',
};

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
      <div>
        <label className="block text-[10px] font-bold mb-0.5" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          الصندوق
        </label>
        <select
          value={value.fundId ?? ''}
          onChange={(e) => update('fundId', e.target.value ? Number(e.target.value) : null)}
          className={selectClass}
          style={selectStyle}
        >
          <option value="">—</option>
          {funds.map((f: FundDto) => (
            <option key={f.id} value={f.id}>{f.fundNumber} - {f.fundName}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-[10px] font-bold mb-0.5" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          المشروع
        </label>
        <select
          value={value.projectId ?? ''}
          onChange={(e) => update('projectId', e.target.value ? Number(e.target.value) : null)}
          className={selectClass}
          style={selectStyle}
        >
          <option value="">—</option>
          {projects.map((p: ProjectDto) => (
            <option key={p.id} value={p.id}>{p.code} - {p.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-[10px] font-bold mb-0.5" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          البند
        </label>
        <select
          value={value.budgetItemId ?? ''}
          onChange={(e) => update('budgetItemId', e.target.value ? Number(e.target.value) : null)}
          className={selectClass}
          style={selectStyle}
        >
          <option value="">—</option>
          {budgetItems.map((bi: BudgetItemDto) => (
            <option key={bi.id} value={bi.id}>{bi.code} - {bi.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-[10px] font-bold mb-0.5" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          الالتزام
        </label>
        <select
          value={value.encumbranceId ?? ''}
          onChange={(e) => update('encumbranceId', e.target.value ? Number(e.target.value) : null)}
          className={selectClass}
          style={selectStyle}
        >
          <option value="">—</option>
          {encumbrances.map((en: EncumbranceListItemDto) => (
            <option key={en.id} value={en.id}>{en.encumbranceNumber}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="block text-[10px] font-bold mb-0.5" style={{ color: 'var(--color-onSurfaceVariant)' }}>
          أمر الدفع
        </label>
        <select
          value={value.paymentOrderId ?? ''}
          onChange={(e) => update('paymentOrderId', e.target.value ? Number(e.target.value) : null)}
          className={selectClass}
          style={selectStyle}
        >
          <option value="">—</option>
          {paymentOrders.map((po: PaymentOrderDto) => (
            <option key={po.id} value={po.id}>{po.paymentOrderNumber}</option>
          ))}
        </select>
      </div>
    </div>
  );
}
