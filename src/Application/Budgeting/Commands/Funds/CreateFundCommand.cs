using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;

namespace ERP_Government.Application.Budgeting.Commands.Funds;

[Authorize(Policy = PermissionCodes.FundsCreate)]
public record CreateFundCommand(
    string FundNumber,
    string FundName,
    FundType FundType,
    FundCategory FundCategory,
    string LegalAuthority,
    string? Description,
    int? DefaultRevenueAccountId,
    int? CurrencyId) : IRequest<Result<int>>;

public class CreateFundCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateFundCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateFundCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Funds
            .AnyAsync(x => x.FundNumber == request.FundNumber, cancellationToken);

        if (exists)
            return Result<int>.Failure(["Fund number already exists."]);

        var entity = new Domain.Budgeting.Entities.Fund
        {
            FundNumber = request.FundNumber,
            FundName = request.FundName,
            FundType = request.FundType,
            FundCategory = request.FundCategory,
            LegalAuthority = request.LegalAuthority,
            Description = request.Description,
            DefaultRevenueAccountId = request.DefaultRevenueAccountId,
            CurrencyId = request.CurrencyId,
            IsActive = true
        };

        context.Funds.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateFundCommandValidator : AbstractValidator<CreateFundCommand>
{
    public CreateFundCommandValidator()
    {
        RuleFor(x => x.FundNumber)
            .NotEmpty().WithMessage("Fund number is required.");

        RuleFor(x => x.FundName)
            .NotEmpty().WithMessage("Fund name is required.");

        RuleFor(x => x.LegalAuthority)
            .NotEmpty().WithMessage("Legal authority is required.");
    }
}
