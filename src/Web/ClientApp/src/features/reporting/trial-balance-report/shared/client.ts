// Trial Balance Report — manual export client.

export class ExportError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ExportError';
  }
}

export interface TrialBalanceExportParams {
  fiscalYearId: number;
  fiscalPeriodId?: number;
}

export async function downloadTrialBalanceExport(
  params: TrialBalanceExportParams,
  format: 'xlsx' | 'pdf',
): Promise<void> {
  const qs = new URLSearchParams();
  qs.set('format', format);
  qs.set('FiscalYearId', String(params.fiscalYearId));
  if (params.fiscalPeriodId !== undefined) qs.set('FiscalPeriodId', String(params.fiscalPeriodId));

  const response = await fetch(`/api/TrialBalanceReports/export?${qs.toString()}`);
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
  anchor.download = `TrialBalance-${new Date().toISOString().slice(0, 10)}.${extension}`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
