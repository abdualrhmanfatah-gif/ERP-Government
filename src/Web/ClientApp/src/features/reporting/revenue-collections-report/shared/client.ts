// Revenue Collections Report — manual export client.
// WHY: the NSwag-generated export method discards the response body;
// a typed blob download wrapper is required (AGENTS.md API Strategy).

export class ExportError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ExportError';
  }
}

export interface RevenueCollectionsExportParams {
  fiscalYearId: number;
  fiscalPeriodId?: number;
  fundId?: number;
  revenueAccountId?: number;
  partyId?: number;
  paymentMethod?: string;
}

export async function downloadRevenueCollectionsExport(
  params: RevenueCollectionsExportParams,
  format: 'xlsx' | 'pdf',
): Promise<void> {
  const qs = new URLSearchParams();
  qs.set('format', format);
  qs.set('FiscalYearId', String(params.fiscalYearId));
  if (params.fiscalPeriodId !== undefined) qs.set('FiscalPeriodId', String(params.fiscalPeriodId));
  if (params.fundId !== undefined) qs.set('FundId', String(params.fundId));
  if (params.revenueAccountId !== undefined) qs.set('RevenueAccountId', String(params.revenueAccountId));
  if (params.partyId !== undefined) qs.set('PartyId', String(params.partyId));
  if (params.paymentMethod !== undefined) qs.set('PaymentMethod', params.paymentMethod);

  const response = await fetch(`/api/RevenueCollectionsReports/export?${qs.toString()}`);
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
  anchor.download = `RevenueCollections-${new Date().toISOString().slice(0, 10)}.${extension}`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}
