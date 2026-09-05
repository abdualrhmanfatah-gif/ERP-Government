import { describe, expect, it } from 'vitest';
import { moduleGroups } from '../../../layouts/navigation';
import { AppRoutes } from '../../../app/routes';

describe('T029: Route and Sidebar', () => {
  it('route is registered in app routes', () => {
    const route = AppRoutes.find((r) => r.path === '/budgeting/budget-classifications');
    expect(route).toBeDefined();
    expect(route?.label).toBe('التصنيفات المالية');
  });

  it('sidebar includes link in الموازنة group', () => {
    const budgetGroup = moduleGroups.find((g) => g.label === 'الموازنة');
    expect(budgetGroup).toBeDefined();
    const link = budgetGroup?.items.find((i) => i.path === '/budgeting/budget-classifications');
    expect(link).toBeDefined();
    expect(link?.label).toBe('التصنيفات المالية');
    expect(link?.permission).toBe('BudgetClassifications.View');
  });

  it('sidebar link has correct permission', () => {
    const budgetGroup = moduleGroups.find((g) => g.label === 'الموازنة');
    const link = budgetGroup?.items.find((i) => i.path === '/budgeting/budget-classifications');
    expect(link?.permission).toBe('BudgetClassifications.View');
  });
});
