import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateBudget } from '../../hooks/useBudgets';
import { useBudgetTypesList } from '../../hooks/useBudgetTypes';
import { useFundsList } from '../../funds/hooks/useFunds';
import { useFiscalYearsList } from '../../../financial-settings/hooks/useFiscalYears';
import { BudgetForm } from '../components/BudgetForm';

export default function BudgetCreatePage() {
  const navigate = useNavigate();
  const createBudget = useCreateBudget();
  const { data: budgetTypes = [] } = useBudgetTypesList();
  const { data: fiscalYears = [] } = useFiscalYearsList(true);
  const { data: funds = [] } = useFundsList();

  const budgetTypeOptions = budgetTypes.map((bt) => ({ value: String(bt.id), label: bt.name }));
  const fiscalYearOptions = fiscalYears.map((fy) => ({ value: String(fy.id), label: fy.name }));
  const fundOptions = funds.map((f) => ({ value: String(f.id), label: f.fundName }));

  function onSubmit(data: { budgetName: string; budgetTypeId: number; fiscalYearId: number; fundId: number; description?: string }) {
    createBudget.mutate(data, {
      onSuccess: (id) => navigate(`/budgeting/budgets/${id}`),
      onError: () => {},
    });
  }

  return (
    <Page title="موازنة جديدة" maxWidth="sm">
      <BudgetForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/budgeting/budgets')}
        isPending={createBudget.isPending}
        budgetTypeOptions={budgetTypeOptions}
        fiscalYearOptions={fiscalYearOptions}
        fundOptions={fundOptions}
      />
    </Page>
  );
}
