using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;
using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;
using ERP_Government.Application.FunctionalTests.Infrastructure;
using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Reporting;

[TestFixture]
public class BudgetExecutionReportTests : TestBase
{
    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();
    }

    private static async Task<int> CreateActiveBudgetAsync(string name, int fiscalYearId, int fundId)
    {
        var budgetResult = await TestApp.SendAsync(new CreateBudgetCommand(name, fiscalYearId, fundId, 1));
        budgetResult.Succeeded.ShouldBeTrue(budgetResult.Errors != null ? string.Join("; ", budgetResult.Errors) : string.Empty);
        var budget = await TestApp.FindAsync<Budget>(budgetResult.Value);
        budget!.Status = BudgetStatus.Active;
        await TestApp.AddAsync(budget);
        return budgetResult.Value;
    }

    private static async Task<int> CreateActiveAppropriationAsync(int budgetId, int itemId, decimal amount, string number)
    {
        var appropriation = new Appropriation
        {
            AppropriationNumber = number,
            BudgetId = budgetId,
            BudgetItemId = itemId,
            AppropriationType = AppropriationType.Original,
            DocumentType = "Appropriation",
            DocumentId = 1,
            Amount = amount,
            Status = AppropriationStatus.Active,
        };
        await TestApp.AddAsync(appropriation);
        return appropriation.Id;
    }

    private static async Task<int> CreateEncumbranceAsync(int appropriationId, decimal amount, EncumbranceStatus status, string number)
    {
        var encumbrance = new Encumbrance
        {
            EncumbranceNumber = number,
            EncumbranceType = EncumbranceType.Commitment,
            AppropriationId = appropriationId,
            DocumentType = "PO",
            DocumentId = 1,
            EncumbranceDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = amount,
            Status = status,
        };
        await TestApp.AddAsync(encumbrance);
        return encumbrance.Id;
    }

    private static async Task CreatePaymentOrderAsync(int appropriationId, decimal amount, PaymentOrderStatus status, string number)
    {
        await TestApp.AddAsync(new PaymentOrder
        {
            PaymentOrderNumber = number,
            PaymentOrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentOrderType = "Standard",
            VendorId = 1,
            FundId = 1,
            FiscalYearId = 2,
            AppropriationId = appropriationId,
            CurrencyId = 1,
            AmountGross = amount,
            BeneficiaryName = "Test",
            Status = status,
        });
    }

    [Test]
    public async Task GetBudgetExecutionReport_ShouldReturnLines()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Lines.ShouldNotBeNull();
    }

    [Test]
    public async Task GetBudgetExecutionReport_Reconciliation_ShouldBalance()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        foreach (var line in result.Lines)
        {
            line.AvailableAmount.ShouldBe(
                line.AppropriatedAmount - line.EncumberedAmount - line.PaidAmount,
                $"Budget line {line.ItemCode}: Available should equal Appropriated - Encumbered - Paid");
        }
    }

    [Test]
    public async Task GetBudgetExecutionReport_Totals_ShouldMatchLineSums()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);

        result.Totals.AppropriatedAmount.ShouldBe(result.Lines.Sum(l => l.AppropriatedAmount));
        result.Totals.EncumberedAmount.ShouldBe(result.Lines.Sum(l => l.EncumberedAmount));
        result.Totals.PaidAmount.ShouldBe(result.Lines.Sum(l => l.PaidAmount));
        result.Totals.AvailableAmount.ShouldBe(result.Lines.Sum(l => l.AvailableAmount));
    }

    [Test]
    public async Task GetBudgetExecutionReport_FilterByFund_ShouldReturnFilteredResults()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1, FundId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.FundId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetBudgetExecutionReport_FilterByBudgetItem_ShouldReturnFilteredResults()
    {
        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 1, BudgetItemId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        foreach (var line in result.Lines)
        {
            line.BudgetItemId.ShouldBe(1);
        }
    }

    [Test]
    public async Task GetBudgetExecutionDetail_ShouldReturnEncumbrancesAndPayments()
    {
        var query = new GetBudgetExecutionDetailQuery { BudgetItemId = 1, FiscalYearId = 1 };
        var result = await TestApp.SendAsync(query);
        result.ShouldNotBeNull();
        result.Encumbrances.ShouldNotBeNull();
        result.Payments.ShouldNotBeNull();
    }

    [Test]
    public async Task GetBudgetExecutionDetail_EncumbranceSum_ShouldMatchReport()
    {
        var reportQuery = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var report = await TestApp.SendAsync(reportQuery);

        if (report.Lines.Count > 0)
        {
            var firstLine = report.Lines.First();
            var detailQuery = new GetBudgetExecutionDetailQuery
            {
                BudgetItemId = firstLine.BudgetItemId,
                FiscalYearId = 1
            };
            var detail = await TestApp.SendAsync(detailQuery);

            detail.Encumbrances.Sum(e => e.Amount).ShouldBe(firstLine.EncumberedAmount,
                "Detail encumbrance sum should match report line");
        }
    }

    [Test]
    public async Task GetBudgetExecutionDetail_PaymentSum_ShouldMatchReport()
    {
        var reportQuery = new GetBudgetExecutionReportQuery { FiscalYearId = 1 };
        var report = await TestApp.SendAsync(reportQuery);

        if (report.Lines.Count > 0)
        {
            var firstLine = report.Lines.First();
            var detailQuery = new GetBudgetExecutionDetailQuery
            {
                BudgetItemId = firstLine.BudgetItemId,
                FiscalYearId = 1
            };
            var detail = await TestApp.SendAsync(detailQuery);

            detail.Payments.Sum(p => p.Amount).ShouldBe(firstLine.PaidAmount,
                "Detail payment sum should match report line");
        }
    }

    // ─── T005: Program/Project subtree filters + dimension population ───

    [Test]
    public async Task GetBudgetExecutionReport_ProgramFilter_ShouldIncludeSubtreeItemsAndPopulateDimensions()
    {
        // Program (root-level child) → Project (its child)
        var program = new BudgetClassification { Code = "P-3000", Name = "Program Three", IsActive = true };
        await TestApp.AddAsync(program);
        var project = new BudgetClassification { Code = "P-3100", Name = "Project Three-One", ParentId = program.Id, IsActive = true };
        await TestApp.AddAsync(project);
        var otherProgram = new BudgetClassification { Code = "P-9000", Name = "Other Program", IsActive = true };
        await TestApp.AddAsync(otherProgram);

        var budget = await CreateActiveBudgetAsync("Program Filter Budget", 2, 1);

        var itemInSubtree = new BudgetItem { BudgetId = budget, ItemCode = "PF-001", ItemName = "In Subtree", BudgetClassificationId = project.Id };
        await TestApp.AddAsync(itemInSubtree);
        var itemOutside = new BudgetItem { BudgetId = budget, ItemCode = "PF-002", ItemName = "Outside", BudgetClassificationId = otherProgram.Id };
        await TestApp.AddAsync(itemOutside);

        await CreateActiveAppropriationAsync(budget, itemInSubtree.Id, 10000m, "APF-001");
        await CreateActiveAppropriationAsync(budget, itemOutside.Id, 20000m, "APF-002");

        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 2, ProgramId = program.Id };
        var result = await TestApp.SendAsync(query);

        result.Lines.Count.ShouldBe(1, "Program filter must include items classified under the program's subtree (descendants) and exclude others");
        result.Lines.Single().BudgetItemId.ShouldBe(itemInSubtree.Id);
        result.Lines.Single().ProgramId.ShouldBe(program.Id);
        result.Lines.Single().ProgramCode.ShouldBe("P-3000");
        result.Lines.Single().ProjectId.ShouldBe(project.Id);
        result.Lines.Single().ProjectCode.ShouldBe("P-3100");
        result.Totals.AppropriatedAmount.ShouldBe(10000m, "Totals must be computed over the filtered set only");
    }

    [Test]
    public async Task GetBudgetExecutionReport_ProjectFilter_ShouldReturnOnlyProjectSubtreeItems()
    {
        var program = new BudgetClassification { Code = "P-4000", Name = "Program Four", IsActive = true };
        await TestApp.AddAsync(program);
        var projectA = new BudgetClassification { Code = "P-4100", Name = "Project A", ParentId = program.Id, IsActive = true };
        await TestApp.AddAsync(projectA);
        var projectB = new BudgetClassification { Code = "P-4200", Name = "Project B", ParentId = program.Id, IsActive = true };
        await TestApp.AddAsync(projectB);

        var budget = await CreateActiveBudgetAsync("Project Filter Budget", 2, 1);

        var itemA = new BudgetItem { BudgetId = budget, ItemCode = "PJ-001", ItemName = "Item A", BudgetClassificationId = projectA.Id };
        await TestApp.AddAsync(itemA);
        var itemB = new BudgetItem { BudgetId = budget, ItemCode = "PJ-002", ItemName = "Item B", BudgetClassificationId = projectB.Id };
        await TestApp.AddAsync(itemB);

        await CreateActiveAppropriationAsync(budget, itemA.Id, 5000m, "APJ-001");
        await CreateActiveAppropriationAsync(budget, itemB.Id, 6000m, "APJ-002");

        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 2, ProjectId = projectA.Id };
        var result = await TestApp.SendAsync(query);

        result.Lines.Count.ShouldBe(1, "Project filter must return only items under that project subtree");
        result.Lines.Single().BudgetItemId.ShouldBe(itemA.Id);
        result.Lines.Single().ProjectCode.ShouldBe("P-4100");
    }

    // ─── T006: Status semantics — executed amounts only ───

    [Test]
    public async Task GetBudgetExecutionReport_PaidAmount_ShouldExcludeNonExecutedPaymentOrders()
    {
        var budget = await CreateActiveBudgetAsync("Status Budget", 2, 1);
        var item = new BudgetItem { BudgetId = budget, ItemCode = "ST-001", ItemName = "Status Item" };
        await TestApp.AddAsync(item);
        var appropriation = await CreateActiveAppropriationAsync(budget, item.Id, 10000m, "AST-001");

        await CreateEncumbranceAsync(appropriation, 1000m, EncumbranceStatus.Active, "ENC-ST-001");
        await CreateEncumbranceAsync(appropriation, 500m, EncumbranceStatus.Draft, "ENC-ST-002");

        await CreatePaymentOrderAsync(appropriation, 300m, PaymentOrderStatus.Paid, "PO-ST-001");
        await CreatePaymentOrderAsync(appropriation, 200m, PaymentOrderStatus.Approved, "PO-ST-002");
        await CreatePaymentOrderAsync(appropriation, 250m, PaymentOrderStatus.Draft, "PO-ST-003");
        await CreatePaymentOrderAsync(appropriation, 150m, PaymentOrderStatus.Submitted, "PO-ST-004");
        await CreatePaymentOrderAsync(appropriation, 100m, PaymentOrderStatus.Rejected, "PO-ST-005");

        var query = new GetBudgetExecutionReportQuery { FiscalYearId = 2 };
        var result = await TestApp.SendAsync(query);

        var line = result.Lines.Single(l => l.BudgetItemId == item.Id);
        line.PaidAmount.ShouldBe(500m, "Paid must include only Approved/SentToTreasury/Paid/PartiallyPaid orders — drafts, submitted, and rejected excluded");
        line.EncumberedAmount.ShouldBe(1000m, "Encumbered must include only open commitments (Active/PartiallyReleased/PartiallyLiquidated) — drafts excluded");
        line.AvailableAmount.ShouldBe(8500m, "Available = Appropriated - Encumbered - Paid");
    }

    // ─── T007: Totals over the filtered set (not the full year) ───

    [Test]
    public async Task GetBudgetExecutionReport_TotalsOverFilteredSet_ShouldEqualSumOfDisplayedLines()
    {
        var budgetFund1 = await CreateActiveBudgetAsync("Totals Budget F1", 2, 1);
        var budgetFund2 = await CreateActiveBudgetAsync("Totals Budget F2", 2, 2);

        var itemF1 = new BudgetItem { BudgetId = budgetFund1, ItemCode = "TT-001", ItemName = "Totals F1" };
        await TestApp.AddAsync(itemF1);
        var itemF2 = new BudgetItem { BudgetId = budgetFund2, ItemCode = "TT-002", ItemName = "Totals F2" };
        await TestApp.AddAsync(itemF2);

        await CreateActiveAppropriationAsync(budgetFund1, itemF1.Id, 7000m, "ATT-001");
        await CreateActiveAppropriationAsync(budgetFund2, itemF2.Id, 3000m, "ATT-002");

        var filtered = await TestApp.SendAsync(new GetBudgetExecutionReportQuery { FiscalYearId = 2, FundId = 1 });
        var unfiltered = await TestApp.SendAsync(new GetBudgetExecutionReportQuery { FiscalYearId = 2 });

        filtered.Lines.Select(l => l.BudgetItemId).ShouldNotContain(itemF2.Id, "Fund filter must exclude other funds' lines");
        filtered.Totals.AppropriatedAmount.ShouldBe(filtered.Lines.Sum(l => l.AppropriatedAmount), "Totals must equal the sum of the DISPLAYED (filtered) lines");
        filtered.Totals.AppropriatedAmount.ShouldBe(7000m);
        unfiltered.Totals.AppropriatedAmount.ShouldBe(unfiltered.Lines.Sum(l => l.AppropriatedAmount));
        unfiltered.Totals.AppropriatedAmount.ShouldBe(10000m);
    }
}
