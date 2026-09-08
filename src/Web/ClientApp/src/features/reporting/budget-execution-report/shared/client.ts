// Budget Execution Report — manual export client.
// WHY: the NSwag-generated export method (processExport3) discards the response
// body and cannot deliver the binary file; a typed blob download wrapper is required
// (AGENTS.md API Strategy: manual wrapper only for what the generated client cannot do).

export class ExportError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ExportError';
  }
}

export interface BudgetExecutionExportParams {
  fiscalYearId: number;
  fundId?: number;
  programId?: number;
  projectId?: number;
  budgetItemId?: number;
}

export async function downloadBudgetExecutionExport(
  params: BudgetExecutionExportParams,
  format: 'xlsx' | 'pdf',
): Promise<void> {
  const qs = new URLSearchParams();
  qs.set('format', format);
  qs.set('FiscalYearId', String(params.fiscalYearId));
  if (params.fundId !== undefined) qs.set('FundId', String(params.fundId));
  if (params.programId !== undefined) qs.set('ProgramId', String(params.programId));
  if (params.projectId !== undefined) qs.set('ProjectId', String(params.projectId));
  if (params.budgetItemId !== undefined) qs.set('BudgetItemId', String(params.budgetItemId));

  const response = await fetch(`/api/BudgetExecutionReports/export?${qs.toString()}`);
  if (!response.ok) {
    let detail = response.statusText;
    try {
      const problem = await response.json();
      detail = problem.detail ?? problem.title ?? detail;
    } catch {
      // non-JSON error body
    }
    throw new ExportError(response.status, detail);
  }

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  const extension = format === 'pdf' ? 'pdf' : 'xlsx';
  anchor.download = `BudgetExecution-${new Date().toISOString().slice(0, 10)}.${extension}`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
