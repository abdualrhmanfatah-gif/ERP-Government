// Disbursement Register Report — manual export client.
// WHY: the NSwag-generated export method discards the response body.

export class ExportError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ExportError';
  }
}

export interface DisbursementRegisterExportParams {
  fiscalYearId: number;
  fiscalPeriodId?: number;
  fundId?: number;
  status?: string;
  approverId?: number;
}

export async function downloadDisbursementRegisterExport(
  params: DisbursementRegisterExportParams,
  format: 'xlsx' | 'pdf',
): Promise<void> {
  const qs = new URLSearchParams();
  qs.set('format', format);
  qs.set('FiscalYearId', String(params.fiscalYearId));
  if (params.fiscalPeriodId !== undefined) qs.set('FiscalPeriodId', String(params.fiscalPeriodId));
  if (params.fundId !== undefined) qs.set('FundId', String(params.fundId));
  if (params.status !== undefined) qs.set('Status', params.status);
  if (params.approverId !== undefined) qs.set('ApproverId', String(params.approverId));

  const response = await fetch(`/api/DisbursementRegisterReports/export?${qs.toString()}`);
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
  anchor.download = `DisbursementRegister-${new Date().toISOString().slice(0, 10)}.${extension}`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
