using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;
using ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;
using ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;
using ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;
using ERP_Government.Application.Accounting.Reports.Common;

namespace ERP_Government.Application.Reporting.Common;

public static class ReportResultMapper
{
    private const string PartialDataWarning = "بيانات جزئية — الفترة الحالية جارية";

    public static ReportResult ToReportResult(this BudgetExecutionReportDto report, string? currencyCode, bool includePartialDataWarning = false) =>
        new()
        {
            Currency = currencyCode ?? string.Empty,
            GeneratedAt = DateTimeOffset.UtcNow,
            DataWarning = includePartialDataWarning ? PartialDataWarning : null,
            Sections =
            [
                new ReportSection
                {
                    Title = "تقرير تنفيذ الموازنة",
                    TitleEn = "Budget Execution",
                    Lines = report.Lines.Select(l => new ReportLine
                    {
                        AccountCode = l.ItemCode,
                        AccountName = $"{l.ItemName} - {l.FundNumber}",
                        Values =
                        [
                            l.AppropriatedAmount,
                            l.EncumberedAmount,
                            l.PaidAmount,
                            l.AvailableAmount
                        ]
                    }).ToList(),
                    Total = report.Totals.AvailableAmount,
                    ColumnHeaders =
                    [
                        "كود البند",
                        "اسم البند",
                        "المخصص",
                        "الالتزامات",
                        "المدفوعات",
                        "المتاح"
                    ],
                    ColumnTotals =
                    [
                        report.Totals.AppropriatedAmount,
                        report.Totals.EncumberedAmount,
                        report.Totals.PaidAmount,
                        report.Totals.AvailableAmount
                    ]
                }
            ]
        };

    public static ReportResult ToReportResult(this RevenueCollectionsReportDto report, string? currencyCode) =>
        new()
        {
            Currency = currencyCode ?? string.Empty,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections =
            [
                new ReportSection
                {
                    Title = "Revenue Collections",
                    Lines = report.Lines.Select(l => new ReportLine
                    {
                        AccountCode = l.AccountCode,
                        AccountName = $"{l.AccountName} - {l.PartyName}",
                        Debit = l.Amount,
                        Credit = 0,
                        Balance = l.Amount
                    }).ToList(),
                    Total = report.Totals.TotalAmount
                }
            ]
        };

    public static ReportResult ToReportResult(this DisbursementRegisterDto report, string? currencyCode) =>
        new()
        {
            Currency = currencyCode ?? string.Empty,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections =
            [
                new ReportSection
                {
                    Title = "Disbursement Register",
                    Lines = report.Lines.Select(l => new ReportLine
                    {
                        AccountCode = l.OrderNumber,
                        AccountName = $"{l.PayeeName} - {l.FundCode}",
                        Debit = l.Amount,
                        Credit = 0,
                        Balance = l.Amount
                    }).ToList(),
                    Total = report.Totals.TotalAmount
                }
            ]
        };

    public static ReportResult ToReportResult(this AvailabilitySnapshotDto report, string? currencyCode) =>
        new()
        {
            Currency = currencyCode ?? string.Empty,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections =
            [
                new ReportSection
                {
                    Title = "Availability Snapshot",
                    Lines = report.Breakdown.Select(l => new ReportLine
                    {
                        AccountCode = report.ItemCode,
                        AccountName = $"{report.ItemName} - {l.FundCode}",
                        Debit = l.PaidAmount,
                        Credit = 0,
                        Balance = l.AvailableAmount
                    }).ToList(),
                    Total = report.Totals.AvailableAmount
                }
            ]
        };

    public static ReportResult ToReportResult(this TrialBalanceReportDto report, string? currencyCode) =>
        new()
        {
            Currency = currencyCode ?? string.Empty,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections =
            [
                new ReportSection
                {
                    Title = $"Trial Balance — {report.FiscalYearName}",
                    Lines = report.Lines.Select(l => new ReportLine
                    {
                        AccountCode = l.AccountCode,
                        AccountName = $"{l.AccountName} [{l.AccountType}]",
                        Debit = l.DebitTotal,
                        Credit = l.CreditTotal,
                        Balance = l.ClosingBalance
                    }).ToList(),
                    Total = report.Totals.TotalClosingBalance
                }
            ]
        };
}
