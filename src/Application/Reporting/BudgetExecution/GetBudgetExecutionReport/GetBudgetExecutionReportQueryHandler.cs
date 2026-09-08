using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Budgeting.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;

internal class GetBudgetExecutionReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBudgetExecutionReportQuery, BudgetExecutionReportDto>
{
    private static readonly Domain.Budgeting.Enums.EncumbranceStatus[] OpenEncumbranceStatuses =
    [
        Domain.Budgeting.Enums.EncumbranceStatus.Active,
        Domain.Budgeting.Enums.EncumbranceStatus.PartiallyReleased,
        Domain.Budgeting.Enums.EncumbranceStatus.PartiallyLiquidated,
    ];

    private static readonly Domain.Payments.Enums.PaymentOrderStatus[] ExecutedPaymentStatuses =
    [
        Domain.Payments.Enums.PaymentOrderStatus.Approved,
        Domain.Payments.Enums.PaymentOrderStatus.SentToTreasury,
        Domain.Payments.Enums.PaymentOrderStatus.Paid,
        Domain.Payments.Enums.PaymentOrderStatus.PartiallyPaid,
    ];

    public async Task<BudgetExecutionReportDto> Handle(
        GetBudgetExecutionReportQuery request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await dbContext.FiscalYears
            .AsNoTracking()
            .FirstAsync(fy => fy.Id == request.FiscalYearId, cancellationToken);

        var query = dbContext.Appropriations
            .AsNoTracking()
            .Include(a => a.BudgetItem)
            .ThenInclude(bi => bi!.Budget)
            .Where(a => a.BudgetItem != null
                     && a.BudgetItem.Budget != null
                     && a.BudgetItem.Budget.FiscalYearId == request.FiscalYearId
                     && a.Status != Domain.Budgeting.Enums.AppropriationStatus.Cancelled);

        if (request.FundId.HasValue)
            query = query.Where(a => a.BudgetItem!.Budget!.FundId == request.FundId.Value);

        if (request.BudgetItemId.HasValue)
            query = query.Where(a => a.BudgetItemId == request.BudgetItemId.Value);

        var appropriations = await query.ToListAsync(cancellationToken);

        // Classification ancestry (leaf → root) for program/project dimensions and subtree filters.
        // A ProgramId/ProjectId filter matches an item when the item's classification chain
        // contains that node — i.e. the item is classified under the node or any descendant.
        var classifications = await dbContext.BudgetClassifications
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var classificationById = classifications.ToDictionary(c => c.Id);

        List<int> GetChain(int? classificationId)
        {
            var chain = new List<int>();
            var current = classificationId;
            while (current.HasValue && classificationById.TryGetValue(current.Value, out var node))
            {
                chain.Add(node.Id);
                current = node.ParentId;
            }

            return chain;
        }

        if (request.ProgramId.HasValue)
        {
            var programId = request.ProgramId.Value;
            appropriations = appropriations
                .Where(a => GetChain(a.BudgetItem!.BudgetClassificationId).Contains(programId))
                .ToList();
        }

        if (request.ProjectId.HasValue)
        {
            var projectId = request.ProjectId.Value;
            appropriations = appropriations
                .Where(a => GetChain(a.BudgetItem!.BudgetClassificationId).Contains(projectId))
                .ToList();
        }

        var budgetItemIds = appropriations.Select(a => a.BudgetItemId).Distinct().ToList();

        var encumbrances = await dbContext.Encumbrances
            .AsNoTracking()
            .Include(e => e.Appropriation)
            .Where(e => budgetItemIds.Contains(e.Appropriation.BudgetItemId)
                     && OpenEncumbranceStatuses.Contains(e.Status))
            .ToListAsync(cancellationToken);

        var paymentOrders = await dbContext.PaymentOrders
            .AsNoTracking()
            .Where(po => budgetItemIds.Contains(po.AppropriationId)
                      && ExecutedPaymentStatuses.Contains(po.Status))
            .ToListAsync(cancellationToken);

        var funds = await dbContext.Funds
            .AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => (f.FundNumber, f.FundName), cancellationToken);

        var lines = appropriations
            .GroupBy(a => new { a.BudgetItemId, FundId = a.BudgetItem!.Budget!.FundId })
            .Select(g =>
            {
                var budgetItemId = g.Key.BudgetItemId;
                var fundId = g.Key.FundId;
                var appropriated = g.Sum(a => a.Amount);
                var encumbered = encumbrances
                    .Where(e => e.Appropriation.BudgetItemId == budgetItemId)
                    .Sum(e => e.Amount);
                var paid = paymentOrders
                    .Where(po => po.AppropriationId == budgetItemId)
                    .Sum(po => po.AmountGross);

                var fund = funds.GetValueOrDefault(fundId);

                // Program = the item classification's direct parent; Project = the item's own classification.
                var itemClassificationId = g.First().BudgetItem!.BudgetClassificationId;
                int? programId = null;
                string? programCode = null;
                int? projectId = null;
                string? projectCode = null;
                if (itemClassificationId.HasValue && classificationById.TryGetValue(itemClassificationId.Value, out var leaf))
                {
                    projectId = leaf.Id;
                    projectCode = leaf.Code;
                    if (leaf.ParentId.HasValue && classificationById.TryGetValue(leaf.ParentId.Value, out var parent))
                    {
                        programId = parent.Id;
                        programCode = parent.Code;
                    }
                }

                return new BudgetExecutionLineDto
                {
                    BudgetItemId = budgetItemId,
                    ItemCode = g.First().BudgetItem!.ItemCode,
                    ItemName = g.First().BudgetItem!.ItemName,
                    FundId = fundId,
                    FundNumber = fund.FundNumber,
                    FundName = fund.FundName,
                    ProgramId = programId,
                    ProgramCode = programCode,
                    ProjectId = projectId,
                    ProjectCode = projectCode,
                    AppropriatedAmount = appropriated,
                    EncumberedAmount = encumbered,
                    PaidAmount = paid,
                    AvailableAmount = appropriated - encumbered - paid
                };
            })
            .OrderBy(l => l.ItemCode)
            .ToList();

        return new BudgetExecutionReportDto
        {
            FiscalYearId = request.FiscalYearId,
            FiscalYearName = fiscalYear.YearNumber.ToString(),
            Lines = lines,
            Totals = new BudgetExecutionTotalDto
            {
                AppropriatedAmount = lines.Sum(l => l.AppropriatedAmount),
                EncumberedAmount = lines.Sum(l => l.EncumberedAmount),
                PaidAmount = lines.Sum(l => l.PaidAmount),
                AvailableAmount = lines.Sum(l => l.AvailableAmount)
            }
        };
    }
}
