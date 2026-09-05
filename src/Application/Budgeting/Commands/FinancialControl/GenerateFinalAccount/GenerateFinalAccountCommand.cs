using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using ERP_Government.Domain.Events.Budgeting;
using MediatR;

namespace ERP_Government.Application.Budgeting.Commands.FinancialControl.GenerateFinalAccount;

[Authorize(Policy = PermissionCodes.FinancialControlApproveFinalAccount)]
public record GenerateFinalAccountCommand(int FiscalYearId) : IRequest<Result<GenerateFinalAccountResult>>;

public record GenerateFinalAccountResult(
    int FinalAccountId,
    int FiscalYearId,
    string FiscalYearName,
    int LineCount);

public class GenerateFinalAccountCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<GenerateFinalAccountCommand, Result<GenerateFinalAccountResult>>
{
    public async Task<Result<GenerateFinalAccountResult>> Handle(
        GenerateFinalAccountCommand request,
        CancellationToken cancellationToken)
    {
        var fiscalYear = await context.FiscalYears.FindAsync(request.FiscalYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<GenerateFinalAccountResult>.Failure(new[] { "Fiscal year not found." });

        if (!fiscalYear.IsClosed)
            return Result<GenerateFinalAccountResult>.Failure(new[]
                { $"Fiscal year {fiscalYear.Name} must be closed before generating the final account." });

        var existingAccount = await context.FinalAccounts
            .Where(f => f.FiscalYearId == request.FiscalYearId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAccount is not null)
            return Result<GenerateFinalAccountResult>.Failure(new[]
                { $"Final account already exists for fiscal year {fiscalYear.Name}." });

        var finalAccount = new FinalAccount
        {
            FiscalYearId = request.FiscalYearId,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedById = user.Id ?? 0,
            Status = FinalAccountStatus.Draft
        };

        var appropriations = await context.Appropriations
            .Where(a => a.BudgetItem.Budget.FiscalYearId == request.FiscalYearId)
            .ToListAsync(cancellationToken);

        var appropriationIds = appropriations.Select(a => a.Id).ToList();

        var actualAmounts = await context.PaymentOrders
            .Where(po => appropriationIds.Contains(po.AppropriationId)
                && po.Status == Domain.Payments.Enums.PaymentOrderStatus.Paid)
            .GroupBy(po => po.AppropriationId)
            .Select(g => new { AppropriationId = g.Key, Amount = g.Sum(po => po.AmountGross - po.DeductionAmount) })
            .ToDictionaryAsync(g => g.AppropriationId, g => g.Amount, cancellationToken);

        var lines = appropriations
            .GroupBy(a => new { a.BudgetItemId, a.BudgetItem.ItemCode })
            .Select(g =>
            {
                var budgetedAmount = g.Sum(a => a.AppropriationType == AppropriationType.Original ? a.Amount :
                    a.AppropriationType == AppropriationType.Supplement ? a.Amount :
                    a.AppropriationType == AppropriationType.Reduction ? -a.Amount :
                    a.AppropriationType == AppropriationType.Adjustment ? a.Amount : 0m);

                var actualAmount = g.Sum(a => actualAmounts.GetValueOrDefault(a.Id, 0m));

                return new FinalAccountLine
                {
                    Dimension = FinalAccountLineDimension.Item,
                    DimensionId = g.Key.BudgetItemId,
                    DimensionCode = g.Key.ItemCode,
                    DimensionName = g.Key.ItemCode,
                    BudgetedAmount = budgetedAmount,
                    ActualAmount = actualAmount,
                    Variance = budgetedAmount - actualAmount
                };
            })
            .ToList();

        foreach (var line in lines)
        {
            finalAccount.AddLine(line);
        }

        finalAccount.AddDomainEvent(new ClosingEntryGenerated
        {
            SourceEntityId = finalAccount.Id,
            FinalAccountId = finalAccount.Id,
            FiscalYearId = request.FiscalYearId,
            OccurredAt = DateTimeOffset.UtcNow
        });

        context.FinalAccounts.Add(finalAccount);
        await context.SaveChangesAsync(cancellationToken);

        finalAccount.ClearDomainEvents();

        return Result<GenerateFinalAccountResult>.Success(new GenerateFinalAccountResult(
            finalAccount.Id,
            request.FiscalYearId,
            fiscalYear.Name,
            lines.Count));
    }
}
