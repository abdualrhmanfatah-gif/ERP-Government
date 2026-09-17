using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Revenue.Common.DTOs;
using ERP_Government.Domain.Revenue.Entities;
using ERP_Government.Domain.Revenue.Enums;
using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Application.Revenue.Commands.DepositSlips.CreateDepositSlip48;

[Authorize(Policy = PermissionCodes.DepositSlipsCreate)]
public class CreateDepositSlip48Command : IRequest<Result<DepositSlip48Dto>>
{
    public DateOnly SlipDate { get; init; }
    public List<int> CheckIds { get; init; } = [];
}

public class CreateDepositSlip48CommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateDepositSlip48Command, Result<DepositSlip48Dto>>
{
    public async Task<Result<DepositSlip48Dto>> Handle(
        CreateDepositSlip48Command request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<DepositSlip48Dto>.Failure(["User identity is required."]);

        if (request.CheckIds.Count == 0)
            return Result<DepositSlip48Dto>.Failure(["At least one check is required for DepositSlip48."]);

        var checks = await context.Checks
            .Where(c => request.CheckIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (checks.Count != request.CheckIds.Count)
            return Result<DepositSlip48Dto>.Failure(["One or more checks were not found."]);

        if (checks.Any(c => c.Status != CheckStatus.Received))
            return Result<DepositSlip48Dto>.Failure(["All checks assigned to DepositSlip48 must be in Received status."]);

        if (checks.Any(c => c.DepositSlip48Id.HasValue))
            return Result<DepositSlip48Dto>.Failure(["One or more checks are already assigned to a DepositSlip48."]);

        var totalAmount = checks.Sum(c => c.Amount);
        var slipNumber = await sequenceService.GenerateNextNumberAsync("DepositSlip48", cancellationToken);

        var slip = new DepositSlip48
        {
            SlipNumber = slipNumber,
            SlipDate = request.SlipDate,
            TotalAmount = totalAmount,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString()
        };

        context.DepositSlips48.Add(slip);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var c in checks)
        {
            c.DepositSlip48Id = slip.Id;
        }

        context.DocumentStatusLogs.Add(new DocumentStatusLog
        {
            EntityName = nameof(DepositSlip48),
            DocumentId = slip.Id,
            FromStatus = "Draft",
            ToStatus = "Draft",
            ChangedById = userId,
            ChangedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<DepositSlip48Dto>.Success(new DepositSlip48Dto
        {
            Id = slip.Id,
            SlipNumber = slip.SlipNumber,
            SlipDate = slip.SlipDate,
            TotalAmount = slip.TotalAmount,
            RowVersion = slip.RowVersion,
            Created = slip.Created,
            CreatedBy = slip.CreatedBy
        });
    }
}

public class CreateDepositSlip48CommandValidator : AbstractValidator<CreateDepositSlip48Command>
{
    public CreateDepositSlip48CommandValidator()
    {
        RuleFor(x => x.SlipDate).NotEmpty().WithMessage("Slip date is required.");
        RuleFor(x => x.CheckIds).NotEmpty().WithMessage("At least one check ID is required.");
    }
}
