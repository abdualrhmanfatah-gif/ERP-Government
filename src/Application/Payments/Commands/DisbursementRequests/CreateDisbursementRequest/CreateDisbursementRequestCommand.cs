using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsCreate)]
public class CreateDisbursementRequestCommand : IRequest<Result<DisbursementRequestDto>>
{
    public string BeneficiaryName { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public int CurrencyId { get; init; }
    public string Purpose { get; init; } = string.Empty;
    public int FinancialYearId { get; init; }
    public string? Notes { get; init; }
}

public class CreateDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateDisbursementRequestCommand, Result<DisbursementRequestDto>>
{
    public async Task<Result<DisbursementRequestDto>> Handle(
        CreateDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<DisbursementRequestDto>.Failure(["User identity is required for this operation."]);

        if (request.RequestedAmount <= 0)
            return Result<DisbursementRequestDto>.Failure(["Requested amount must be greater than zero."]);

        if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
            return Result<DisbursementRequestDto>.Failure(["Beneficiary name is required."]);

        if (string.IsNullOrWhiteSpace(request.Purpose))
            return Result<DisbursementRequestDto>.Failure(["Purpose is required."]);

        // Validate currency exists
        var currency = await context.Currencies.FindAsync(request.CurrencyId, cancellationToken);
        if (currency is null)
            return Result<DisbursementRequestDto>.Failure(["Invalid currency."]);

        // Validate fiscal year exists
        var fiscalYear = await context.FiscalYears.FindAsync(request.FinancialYearId, cancellationToken);
        if (fiscalYear is null)
            return Result<DisbursementRequestDto>.Failure(["Invalid fiscal year."]);

        // Validate requester user exists
        var userEntity = await context.Users.FindAsync(userId, cancellationToken);
        string requestedByName = userEntity?.Login ?? "Unknown";

        string requestNumber;
        try
        {
            requestNumber = await sequenceService.GenerateNextNumberAsync("DisbursementRequest", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<DisbursementRequestDto>.Failure([ex.Message]);
        }

        var entity = new DisbursementRequest
        {
            RequestNumber = requestNumber,
            RequestedById = userId,
            RequestedByName = requestedByName,
            BeneficiaryName = request.BeneficiaryName,
            RequestedAmount = request.RequestedAmount,
            CurrencyId = request.CurrencyId,
            Purpose = request.Purpose,
            FinancialYearId = request.FinancialYearId,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = DisbursementRequestStatus.Draft,
            Notes = request.Notes,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.DisbursementRequests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DisbursementRequestDto>.Success(new DisbursementRequestDto(
            entity.Id,
            entity.RequestNumber,
            entity.RequestedById,
            entity.RequestedByName,
            entity.BeneficiaryName,
            entity.RequestedAmount,
            entity.CurrencyId,
            entity.Purpose,
            entity.FinancialYearId,
            entity.RequestDate,
            entity.Status,
            entity.Notes,
            entity.PaymentDate,
            null,
            null,
            null,
            null,
            new()));
    }
}

public class CreateDisbursementRequestCommandValidator : AbstractValidator<CreateDisbursementRequestCommand>
{
    public CreateDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.BeneficiaryName)
            .NotEmpty().WithMessage("Beneficiary name is required.")
            .MaximumLength(200).WithMessage("Beneficiary name must not exceed 200 characters.");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0).WithMessage("Requested amount must be greater than zero.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required.")
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.");

        RuleFor(x => x.FinancialYearId)
            .GreaterThan(0).WithMessage("Financial year is required.");
    }
}
