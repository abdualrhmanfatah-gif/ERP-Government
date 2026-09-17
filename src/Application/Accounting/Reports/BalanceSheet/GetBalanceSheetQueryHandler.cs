using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.Reports.BalanceSheet;

public class GetBalanceSheetQueryHandler(
    IApplicationDbContext context,
    ReportAuditService auditService) : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    public async Task<BalanceSheetDto> Handle(
        GetBalanceSheetQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var asOfDate = DateOnly.TryParse(request.AsOfDate, out var parsed)
                ? parsed
                : DateOnly.FromDateTime(DateTime.Today);

            if (request.FiscalPeriodId is int fiscalPeriodId)
            {
                var periodEndDate = await context.FiscalPeriods
                    .Where(x => x.Id == fiscalPeriodId)
                    .Select(x => (DateOnly?)x.EndDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (periodEndDate.HasValue && asOfDate > periodEndDate.Value)
                {
                    asOfDate = periodEndDate.Value;
                }
            }

            var currency = await context.Currencies
                .Where(x => x.IsBase && x.IsActive)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken) ?? "YER";

            var balances = await context.JournalEntryLines
                .Where(x => x.JournalEntry.EntryStatus == EntryStatus.Posted
                         && x.JournalEntry.DocumentDate <= asOfDate)
                .GroupBy(x => new
                {
                    x.AccountId,
                    AccountCode = x.Account.Code ?? string.Empty,
                    AccountName = x.Account.Name ?? string.Empty,
                    AccountGroupCode = x.Account.AccountGroup.Code ?? string.Empty,
                    AccountGroupName = x.Account.AccountGroup.Name ?? string.Empty,
                    AccountGroupType = x.Account.AccountGroup.Type,
                })
                .Select(g => new BalanceSheetRow
                {
                    AccountCode = g.Key.AccountCode,
                    AccountName = g.Key.AccountName,
                    AccountGroupCode = g.Key.AccountGroupCode,
                    AccountGroupName = g.Key.AccountGroupName,
                    AccountGroupType = g.Key.AccountGroupType,
                    Debit = g.Sum(x => x.Debit * x.ExchangeRate),
                    Credit = g.Sum(x => x.Credit * x.ExchangeRate),
                })
                .ToListAsync(cancellationToken);

            var assets = BuildGroup(balances, AccountGroupType.Asset);
            var currentAssets = BuildCategorizedGroup(balances, CurrentAssetCategories, GetAssetCategory);
            var nonCurrentAssets = BuildCategorizedGroup(balances, NonCurrentAssetCategories, GetAssetCategory);
            var fixedAssets = BuildCategorizedGroup(
                balances,
                NonCurrentAssetCategories.Where(x => x.Key == BalanceSheetCategoryKeys.PropertyPlantEquipment).ToArray(),
                GetAssetCategory);

            var liabilities = BuildGroup(balances, AccountGroupType.Liability);
            var currentLiabilities = BuildCategorizedGroup(balances, CurrentLiabilityCategories, GetLiabilityCategory);
            var nonCurrentLiabilities = BuildCategorizedGroup(balances, NonCurrentLiabilityCategories, GetLiabilityCategory);
            var equityBase = BuildGroup(balances, AccountGroupType.Equity);

            var netIncome = balances
                .Where(x => x.AccountGroupType == AccountGroupType.Revenue)
                .Sum(x => x.Credit - x.Debit)
                - balances
                .Where(x => x.AccountGroupType == AccountGroupType.Expense)
                .Sum(x => x.Debit - x.Credit);

            var equitySections = new List<ReportSection>(equityBase.Sections);
            if (netIncome != 0)
            {
                equitySections.Add(new ReportSection
                {
                    Title = "صافي الدخل / الخسارة",
                    TitleEn = "Net Income / Loss",
                    Lines =
                    [
                        new ReportLine
                        {
                            AccountCode = "NI",
                            AccountName = "صافي الدخل",
                            Balance = netIncome,
                        }
                    ],
                    Total = netIncome,
                });
            }

            var totalEquity = equityBase.Total + netIncome;
            var liabilitiesAndEquity = liabilities.Total + totalEquity;

            var result = new BalanceSheetDto
            {
                AsOfDate = asOfDate,
                Currency = currency,
                Assets = assets,
                FixedAssets = fixedAssets,
                CurrentAssets = currentAssets,
                NonCurrentAssets = nonCurrentAssets,
                Liabilities = liabilities,
                CurrentLiabilities = currentLiabilities,
                NonCurrentLiabilities = nonCurrentLiabilities,
                Equity = new BalanceSheetGroup
                {
                    Sections = equitySections,
                    Total = totalEquity,
                },
                LiabilitiesAndEquity = liabilitiesAndEquity,
                Balanced = assets.Total == liabilitiesAndEquity,
                GeneratedAt = DateTimeOffset.UtcNow,
            };

            await auditService.LogAsync("BalanceSheet", request, "Screen", true, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await auditService.LogAsync("BalanceSheet", request, "Screen", false, ex.Message, cancellationToken);
            throw;
        }
    }

    private static BalanceSheetGroup BuildGroup(
        IReadOnlyCollection<BalanceSheetRow> rows,
        AccountGroupType type)
    {
        var sections = rows
            .Where(x => x.AccountGroupType == type)
            .GroupBy(x => new { x.AccountGroupCode, x.AccountGroupName })
            .OrderBy(g => g.Key.AccountGroupCode)
            .Select(g => new ReportSection
            {
                Title = g.Key.AccountGroupName,
                TitleEn = g.Key.AccountGroupName,
                Lines = g
                    .OrderBy(x => x.AccountCode)
                    .Select(ToReportLine)
                    .ToList(),
                Total = g.Sum(BalanceFor),
            })
            .ToList();

        return new BalanceSheetGroup
        {
            Sections = sections,
            Total = sections.Sum(s => s.Total),
        };
    }

    private static BalanceSheetGroup BuildCategorizedGroup(
        IReadOnlyCollection<BalanceSheetRow> balances,
        IReadOnlyCollection<BalanceSheetCategory> categories,
        Func<BalanceSheetRow, string> categorySelector)
    {
        var categoryKeys = categories.Select(x => x.Key).ToHashSet(StringComparer.Ordinal);
        var rows = balances
            .Where(row => categoryKeys.Contains(categorySelector(row)))
            .ToList();

        var groupedRows = rows
            .GroupBy(categorySelector)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.AccountCode).ToList(), StringComparer.Ordinal);

        var sections = categories
            .Select(category =>
            {
                var lines = groupedRows.TryGetValue(category.Key, out var categoryRows)
                    ? categoryRows.Select(ToReportLine).ToList()
                    : [];

                return new ReportSection
                {
                    Title = category.Title,
                    TitleEn = category.TitleEn,
                    Lines = lines,
                    Total = lines.Sum(x => x.Balance),
                };
            })
            .ToList();

        return new BalanceSheetGroup
        {
            Sections = sections,
            Total = sections.Sum(x => x.Total),
        };
    }

    private static ReportLine ToReportLine(BalanceSheetRow row) =>
        new()
        {
            AccountCode = row.AccountCode,
            AccountName = row.AccountName,
            Debit = row.Debit,
            Credit = row.Credit,
            Balance = BalanceFor(row),
        };

    private static decimal BalanceFor(BalanceSheetRow row) =>
        row.AccountGroupType == AccountGroupType.Asset
            ? row.Debit - row.Credit
            : row.Credit - row.Debit;

    private static string GetAssetCategory(BalanceSheetRow row)
    {
        var code = row.AccountCode;
        var groupCode = row.AccountGroupCode;
        var name = row.AccountName;

        if (row.AccountGroupType != AccountGroupType.Asset)
        {
            return string.Empty;
        }

        if (HasAny(name, "ضريبة مؤجلة", "ضريبي مؤجل", "Deferred tax"))
        {
            return BalanceSheetCategoryKeys.DeferredTaxAssets;
        }

        if (HasAny(name, "غير ملموس", "برمج", "رخص", "Intangible"))
        {
            return BalanceSheetCategoryKeys.IntangibleAssets;
        }

        if (StartsWithAny(code, "181", "182") || StartsWithAny(groupCode, "18"))
        {
            return BalanceSheetCategoryKeys.CashAndCashEquivalents;
        }

        if (HasAny(name, "ذمم مدينة", "مدينون", "مستحق", "Receivable"))
        {
            return BalanceSheetCategoryKeys.Receivables;
        }

        if (HasAny(name, "مخزون", "Inventory"))
        {
            return BalanceSheetCategoryKeys.Inventory;
        }

        if (HasAny(name, "مدفوع مقدم", "مصروفات مقدمة", "Prepaid"))
        {
            return BalanceSheetCategoryKeys.PrepaidExpenses;
        }

        if (StartsWithAny(groupCode, "13"))
        {
            return HasAny(name, "قصير", "متداول", "Short")
                ? BalanceSheetCategoryKeys.ShortTermInvestments
                : BalanceSheetCategoryKeys.LongTermInvestments;
        }

        if (StartsWithAny(groupCode, "11") && HasAny(name, "استثمارية", "استثماري", "Investment property"))
        {
            return BalanceSheetCategoryKeys.InvestmentProperty;
        }

        if (StartsWithAny(groupCode, "11"))
        {
            return BalanceSheetCategoryKeys.PropertyPlantEquipment;
        }

        if (StartsWithAny(groupCode, "12"))
        {
            return BalanceSheetCategoryKeys.OtherNonCurrentAssets;
        }

        return BalanceSheetCategoryKeys.OtherCurrentAssets;
    }

    private static string GetLiabilityCategory(BalanceSheetRow row)
    {
        var code = row.AccountCode;
        var groupCode = row.AccountGroupCode;
        var name = row.AccountName;

        if (row.AccountGroupType != AccountGroupType.Liability)
        {
            return string.Empty;
        }

        if (HasAny(name, "ضريبة مؤجلة", "ضريبي مؤجل", "Deferred tax"))
        {
            return BalanceSheetCategoryKeys.DeferredTaxLiabilities;
        }

        if (HasAny(name, "طويل", "Long-term", "Long term"))
        {
            return HasAny(name, "قرض", "اقتراض", "Borrow")
                ? BalanceSheetCategoryKeys.LongTermBorrowings
                : BalanceSheetCategoryKeys.LongTermFinancialLiabilities;
        }

        if (HasAny(name, "نهاية الخدمة", "حقوق العاملين", "منافع الموظفين", "Employee benefit"))
        {
            return BalanceSheetCategoryKeys.EmployeeBenefitLiabilities;
        }

        if (StartsWithAny(code, "251", "253", "254") || StartsWithAny(groupCode, "25"))
        {
            if (HasAny(name, "ضريبة", "ضرائب", "Tax"))
            {
                return BalanceSheetCategoryKeys.CurrentTaxLiabilities;
            }

            return BalanceSheetCategoryKeys.Payables;
        }

        if (StartsWithAny(code, "272"))
        {
            return BalanceSheetCategoryKeys.AccruedExpenses;
        }

        if (HasAny(name, "قرض", "اقتراض", "Borrow"))
        {
            return BalanceSheetCategoryKeys.ShortTermBorrowings;
        }

        if (HasAny(name, "مخصص", "Provision"))
        {
            return BalanceSheetCategoryKeys.ShortTermProvisions;
        }

        return BalanceSheetCategoryKeys.OtherCurrentLiabilities;
    }

    private static bool StartsWithAny(string value, params string[] prefixes) =>
        prefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static bool HasAny(string value, params string[] fragments) =>
        fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    private static readonly BalanceSheetCategory[] CurrentAssetCategories =
    [
        new(BalanceSheetCategoryKeys.CashAndCashEquivalents, "النقد وما في حكمه", "Cash and cash equivalents"),
        new(BalanceSheetCategoryKeys.Receivables, "الحسابات المدينة / الذمم المدينة", "Trade and other receivables"),
        new(BalanceSheetCategoryKeys.Inventory, "المخزون", "Inventories"),
        new(BalanceSheetCategoryKeys.PrepaidExpenses, "المصروفات المدفوعة مقدما", "Prepaid expenses"),
        new(BalanceSheetCategoryKeys.ShortTermInvestments, "الاستثمارات قصيرة الأجل", "Short-term investments"),
        new(BalanceSheetCategoryKeys.OtherCurrentAssets, "أصول أخرى متداولة", "Other current assets"),
    ];

    private static readonly BalanceSheetCategory[] NonCurrentAssetCategories =
    [
        new(BalanceSheetCategoryKeys.PropertyPlantEquipment, "الممتلكات والآلات والمعدات", "Property, plant and equipment"),
        new(BalanceSheetCategoryKeys.InvestmentProperty, "العقارات الاستثمارية", "Investment property"),
        new(BalanceSheetCategoryKeys.IntangibleAssets, "الأصول غير الملموسة", "Intangible assets"),
        new(BalanceSheetCategoryKeys.LongTermInvestments, "الاستثمارات طويلة الأجل", "Long-term investments"),
        new(BalanceSheetCategoryKeys.FinancialAssets, "الأصول المالية", "Financial assets"),
        new(BalanceSheetCategoryKeys.DeferredTaxAssets, "الأصول الضريبية المؤجلة", "Deferred tax assets"),
        new(BalanceSheetCategoryKeys.OtherNonCurrentAssets, "أصول أخرى غير متداولة", "Other non-current assets"),
    ];

    private static readonly BalanceSheetCategory[] CurrentLiabilityCategories =
    [
        new(BalanceSheetCategoryKeys.Payables, "الحسابات الدائنة / الذمم الدائنة", "Trade and other payables"),
        new(BalanceSheetCategoryKeys.AccruedExpenses, "المصروفات المستحقة", "Accrued expenses"),
        new(BalanceSheetCategoryKeys.ShortTermBorrowings, "القروض قصيرة الأجل", "Short-term borrowings"),
        new(BalanceSheetCategoryKeys.CurrentPortionLongTermBorrowings, "الجزء المتداول من القروض طويلة الأجل", "Current portion of long-term borrowings"),
        new(BalanceSheetCategoryKeys.CurrentTaxLiabilities, "الضرائب المستحقة", "Current tax liabilities"),
        new(BalanceSheetCategoryKeys.ShortTermProvisions, "المخصصات قصيرة الأجل", "Short-term provisions"),
        new(BalanceSheetCategoryKeys.OtherCurrentLiabilities, "خصوم أخرى متداولة", "Other current liabilities"),
    ];

    private static readonly BalanceSheetCategory[] NonCurrentLiabilityCategories =
    [
        new(BalanceSheetCategoryKeys.LongTermBorrowings, "القروض طويلة الأجل", "Long-term borrowings"),
        new(BalanceSheetCategoryKeys.LongTermFinancialLiabilities, "الالتزامات المالية طويلة الأجل", "Long-term financial liabilities"),
        new(BalanceSheetCategoryKeys.EmployeeBenefitLiabilities, "مخصصات نهاية الخدمة أو منافع الموظفين", "Employee benefit liabilities"),
        new(BalanceSheetCategoryKeys.DeferredTaxLiabilities, "الالتزامات الضريبية المؤجلة", "Deferred tax liabilities"),
        new(BalanceSheetCategoryKeys.LongTermProvisions, "مخصصات طويلة الأجل", "Long-term provisions"),
        new(BalanceSheetCategoryKeys.OtherNonCurrentLiabilities, "خصوم أخرى غير متداولة", "Other non-current liabilities"),
    ];

    private sealed class BalanceSheetRow
    {
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string AccountGroupCode { get; init; } = string.Empty;
        public string AccountGroupName { get; init; } = string.Empty;
        public AccountGroupType AccountGroupType { get; init; }
        public decimal Debit { get; init; }
        public decimal Credit { get; init; }
    }

    private sealed record BalanceSheetCategory(string Key, string Title, string TitleEn);

    private static class BalanceSheetCategoryKeys
    {
        public const string CashAndCashEquivalents = "current-assets.cash";
        public const string Receivables = "current-assets.receivables";
        public const string Inventory = "current-assets.inventory";
        public const string PrepaidExpenses = "current-assets.prepaid";
        public const string ShortTermInvestments = "current-assets.short-term-investments";
        public const string OtherCurrentAssets = "current-assets.other";
        public const string PropertyPlantEquipment = "non-current-assets.ppe";
        public const string InvestmentProperty = "non-current-assets.investment-property";
        public const string IntangibleAssets = "non-current-assets.intangible";
        public const string LongTermInvestments = "non-current-assets.long-term-investments";
        public const string FinancialAssets = "non-current-assets.financial-assets";
        public const string DeferredTaxAssets = "non-current-assets.deferred-tax";
        public const string OtherNonCurrentAssets = "non-current-assets.other";
        public const string Payables = "current-liabilities.payables";
        public const string AccruedExpenses = "current-liabilities.accrued-expenses";
        public const string ShortTermBorrowings = "current-liabilities.short-term-borrowings";
        public const string CurrentPortionLongTermBorrowings = "current-liabilities.current-portion-long-term-borrowings";
        public const string CurrentTaxLiabilities = "current-liabilities.current-tax";
        public const string ShortTermProvisions = "current-liabilities.short-term-provisions";
        public const string OtherCurrentLiabilities = "current-liabilities.other";
        public const string LongTermBorrowings = "non-current-liabilities.long-term-borrowings";
        public const string LongTermFinancialLiabilities = "non-current-liabilities.financial-liabilities";
        public const string EmployeeBenefitLiabilities = "non-current-liabilities.employee-benefits";
        public const string DeferredTaxLiabilities = "non-current-liabilities.deferred-tax";
        public const string LongTermProvisions = "non-current-liabilities.long-term-provisions";
        public const string OtherNonCurrentLiabilities = "non-current-liabilities.other";
    }
}
