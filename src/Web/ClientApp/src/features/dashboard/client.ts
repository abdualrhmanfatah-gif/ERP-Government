import {
  BudgetDto,
  PendingApprovalDto,
  BankAccountDto,
} from '../../web-api-client';

const BASE_BUDGETS = '/api/Budgets';
const BASE_APPROVALS = '/api/ApprovalRules';
const BASE_BANK_ACCOUNTS = '/api/BankAccounts';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

function buildQueryString(params: Record<string, string | number | boolean | undefined>): string {
  const entries = Object.entries(params).filter(
    ([, v]) => v !== undefined && v !== null
  );
  if (entries.length === 0) return '';
  return '?' + entries.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(String(v))}`).join('&');
}

export const dashboardClient = {
  async getBudgets(): Promise<BudgetDto[]> {
    const qs = buildQueryString({});
    return handleResponse<BudgetDto[]>(
      await fetch(`${BASE_BUDGETS}${qs}`, { headers: { Accept: 'application/json' } })
    );
  },

  async getPendingApprovals(): Promise<PendingApprovalDto[]> {
    return handleResponse<PendingApprovalDto[]>(
      await fetch(`${BASE_APPROVALS}/pending`, { headers: { Accept: 'application/json' } })
    );
  },

  async getBankAccounts(): Promise<BankAccountDto[]> {
    return handleResponse<BankAccountDto[]>(
      await fetch(BASE_BANK_ACCOUNTS, { headers: { Accept: 'application/json' } })
    );
  },
};
