using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.FinancialSettings.Enums;

namespace ERP_Government.Application.FinancialSettings.Commands.FiscalYears;

// C-F002b — UpdateFiscalYearCommand
[Authorize(Policy = PermissionCodes.FiscalYearsUpdate)]
public class UpdateFiscalYearCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateFiscalYearCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateFiscalYearCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFiscalYearCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.FiscalYears
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fiscal year not found."]);

        if (entity.Status != FiscalYearStatus.Draft)
            return Result.Failure(["Only draft fiscal years can be edited."]);

        if (!entity.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Failure(["Fiscal year has been modified by another user. Please reload and try again."]);

        entity.Name = request.Name;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateFiscalYearCommandValidator : AbstractValidator<UpdateFiscalYearCommand>
{
    public UpdateFiscalYearCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");
    }
}
