# Frontend Contracts: BudgetTransactions

**Date**: 2026-09-10

## Route Structure

| Route | Component | Permission |
|-------|-----------|------------|
| /budgeting/transactions | BudgetTransactionsListPage | BudgetTransactionsView |
| /budgeting/transactions/new | BudgetTransactionCreatePage | BudgetTransactionsCreate |
| /budgeting/transactions/:id | BudgetTransactionDetailPage | BudgetTransactionsView |
| /budgeting/transactions/:id/edit | BudgetTransactionEditPage | BudgetTransactionsUpdate |

**REMOVED**: /budgeting/appropriations/* (all routes)

## Navigation Update

```text
src/layouts/navigation.ts
├── ...
├── Budgeting
│   ├── Budgets
│   ├── Budget Items
│   ├── Transactions (NEW — replaces Appropriations)
│   ├── Encumbrances
│   ├── Year Closing
│   └── Final Accounts
└── ...
```

## Component Structure

```text
src/features/budgeting/budget-transactions/
├── pages/
│   ├── BudgetTransactionsListPage.tsx    # List with filters
│   ├── BudgetTransactionCreatePage.tsx   # Create with lines
│   ├── BudgetTransactionDetailPage.tsx   # Detail view
│   └── BudgetTransactionEditPage.tsx     # Edit (Draft only)
├── hooks/
│   └── useBudgetTransactions.ts          # Query/mutation hooks
└── shared/
    ├── client.ts                          # API client wrapper
    ├── types.ts                           # TypeScript types
    └── schemas.ts                         # Zod validation schemas
```

## Zod Schemas

```typescript
// BudgetTransactionLine schema
const BudgetTransactionLineSchema = z.object({
  budgetItemId: z.number().int().positive(),
  direction: z.enum(['Increase', 'Decrease']),
  amount: z.number().positive(),
  remarks: z.string().max(1000).optional(),
});

// Create BudgetTransaction schema
const CreateBudgetTransactionSchema = z.object({
  budgetId: z.number().int().positive(),
  transactionType: z.enum([
    'InitialAppropriation', 'Supplement', 'Reduction',
    'Transfer', 'CarryForward', 'Adjustment', 'Lapse', 'Reversal'
  ]),
  transactionDate: z.string(),
  documentType: z.string().max(100).optional(),
  documentId: z.number().int().positive().optional(),
  description: z.string().max(1000).optional(),
  lines: z.array(BudgetTransactionLineSchema).min(1),
}).refine(
  (data) => {
    if (data.transactionType === 'Transfer') {
      const increases = data.lines
        .filter(l => l.direction === 'Increase')
        .reduce((sum, l) => sum + l.amount, 0);
      const decreases = data.lines
        .filter(l => l.direction === 'Decrease')
        .reduce((sum, l) => sum + l.amount, 0);
      return Math.abs(increases - decreases) < 0.01;
    }
    return true;
  },
  { message: 'Transfers must have balanced lines' }
);
```

## List Page Columns

| Column | Field | Width |
|--------|-------|-------|
| رقم المعاملة | transactionNumber | 120px |
| النوع | transactionTypeLabel | 150px |
| التاريخ | transactionDate | 100px |
| الحالة | status | 100px |
| الوصف | description | 200px |
| تاريخ الإنشاء | createdAt | 120px |

## Detail Page Sections

1. **Header**: Transaction number, type, date, status badge, budget name
2. **Lines Table**: Budget item code/name, direction, amount, remarks
3. **Audit**: Created by/at, approved by/at, posted by/at
4. **Actions**: Submit/Approve/Post/Cancel/Reverse based on status and permissions

## Shared Hooks

```typescript
// useBudgetTransactions.ts
export function useBudgetTransactions(filters: TransactionFilters) {
  return useQuery({
    queryKey: ['budgetTransactions', filters],
    queryFn: () => client.getBudgetTransactions(filters),
  });
}

export function useBudgetTransaction(id: number) {
  return useQuery({
    queryKey: ['budgetTransaction', id],
    queryFn: () => client.getBudgetTransaction(id),
  });
}

export function useCreateBudgetTransaction() {
  return useMutation({
    mutationFn: (data: CreateBudgetTransaction) =>
      client.createBudgetTransaction(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['budgetTransactions'] });
    },
  });
}
```
