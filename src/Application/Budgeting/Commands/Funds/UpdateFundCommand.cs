using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Funds;

[Authorize(Policy = PermissionCodes.FundsUpdate)]
public record UpdateFundCommand(
    int Id,
    string FundNumber,
    string FundName,
    FundType FundType,
    FundCategory FundCategory,
    int? FiscalYearId,
    string LegalAuthority,
    string? Description,
    int? DefaultRevenueDebitAccountId,
    byte[] RowVersion) : IRequest<Result>;

public class UpdateFundCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateFundCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFundCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Funds
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Fund not found."]);

        if (!request.RowVersion.SequenceEqual(entity.RowVersion))
            return Result.Failure(["Concurrency conflict. The record has been modified by another user."]);

        var numberExists = await context.Funds
            .AnyAsync(x => x.FundNumber == request.FundNumber && x.Id != request.Id, cancellationToken);

        if (numberExists)
            return Result.Failure(["Fund number already exists."]);

        if (request.FiscalYearId.HasValue)
        {
            var fiscalYearExists = await context.FiscalYears
                .AnyAsync(x => x.Id == request.FiscalYearId.Value, cancellationToken);

            if (!fiscalYearExists)
                return Result.Failure(["Fiscal year not found."]);
        }

        entity.FundNumber = request.FundNumber;
        entity.FundName = request.FundName;
        entity.FundType = request.FundType;
        entity.FundCategory = request.FundCategory;
        entity.FiscalYearId = request.FiscalYearId;
        entity.LegalAuthority = request.LegalAuthority;
        entity.Description = request.Description;
        entity.DefaultRevenueDebitAccountId = request.DefaultRevenueDebitAccountId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class UpdateFundCommandValidator : AbstractValidator<UpdateFundCommand>
{
    public UpdateFundCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid fund ID.");

        RuleFor(x => x.FundNumber)
            .NotEmpty().WithMessage("Fund number is required.");

        RuleFor(x => x.FundName)
            .NotEmpty().WithMessage("Fund name is required.");

        RuleFor(x => x.LegalAuthority)
            .NotEmpty().WithMessage("Legal authority is required.");
    }
}
