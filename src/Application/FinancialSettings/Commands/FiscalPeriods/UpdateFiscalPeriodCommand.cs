using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalPeriods;

[Authorize(Policy = PermissionCodes.FiscalPeriodsUpdate)]
public class UpdateFiscalPeriodCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
}

public class UpdateFiscalPeriodCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateFiscalPeriodCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFiscalPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalPeriods
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal period not found."]);

        var fiscalYear = await context.FiscalYears
            .FindAsync(entity.FiscalYearId, cancellationToken);

        if (fiscalYear is null)
            return Result.Failure(["Fiscal year not found."]);

        if (fiscalYear.Status is FiscalYearStatus.SoftClosed or FiscalYearStatus.HardClosed)
            return Result.Failure(["Cannot update period in a closed fiscal year."]);

        if (request.StartDate < fiscalYear.StartDate || request.EndDate > fiscalYear.EndDate)
            return Result.Failure(["Period dates must fall within the fiscal year date range."]);

        if (request.EndDate <= request.StartDate)
            return Result.Failure(["End date must be after start date."]);

        var hasOverlap = await context.FiscalPeriods.AnyAsync(
            fp => fp.FiscalYearId == entity.FiscalYearId
                && fp.Id != entity.Id
                && fp.StartDate <= request.EndDate
                && fp.EndDate >= request.StartDate,
            cancellationToken);

        if (hasOverlap)
            return Result.Failure(["Period dates overlap with an existing period."]);

        entity.Name = request.Name;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateFiscalPeriodCommandValidator : AbstractValidator<UpdateFiscalPeriodCommand>
{
    public UpdateFiscalPeriodCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Fiscal period ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}
