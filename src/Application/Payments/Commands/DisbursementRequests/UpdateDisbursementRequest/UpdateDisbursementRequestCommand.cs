using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.UpdateDisbursementRequest;

[Authorize(Policy = PermissionCodes.DisbursementRequestsUpdate)]
public class UpdateDisbursementRequestCommand : IRequest<Result<DisbursementRequestDto>>
{
    public int Id { get; init; }
    public string? BeneficiaryName { get; init; }
    public decimal? RequestedAmount { get; init; }
    public int? CurrencyId { get; init; }
    public string? Purpose { get; init; }
    public int? FinancialYearId { get; init; }
    public string? Notes { get; init; }
    public int? AccrualJournalEntryId { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class UpdateDisbursementRequestCommandHandler(
    IApplicationDbContext context,
    IUser user) : IRequestHandler<UpdateDisbursementRequestCommand, Result<DisbursementRequestDto>>
{
    public async Task<Result<DisbursementRequestDto>> Handle(
        UpdateDisbursementRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<DisbursementRequestDto>.Failure(["User identity is required for this operation."]);

        var entity = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result<DisbursementRequestDto>.Failure(["Disbursement request not found."]);

        if (entity.Status == DisbursementRequestStatus.Approved)
            return Result<DisbursementRequestDto>.Failure(["Cannot edit an approved disbursement request."]);

        if (entity.Status == DisbursementRequestStatus.Cancelled)
            return Result<DisbursementRequestDto>.Failure(["Cannot edit a cancelled disbursement request."]);

        if (entity.Status == DisbursementRequestStatus.Disbursed)
            return Result<DisbursementRequestDto>.Failure(["Cannot edit a paid disbursement request."]);

        bool hasExistingApprovals = await context.ApprovalHistory
            .AnyAsync(a => a.DocumentType == "DisbursementRequest"
                && a.DocumentId == entity.Id
                && a.Action == ApprovalAction.Approve,
                cancellationToken);

        if (hasExistingApprovals)
        {
            if (request.BeneficiaryName is not null)
                return Result<DisbursementRequestDto>.Failure(["Beneficiary name cannot be changed after approval has started."]);

            if (request.RequestedAmount.HasValue)
                return Result<DisbursementRequestDto>.Failure(["Requested amount cannot be changed after approval has started."]);

            if (request.CurrencyId.HasValue)
                return Result<DisbursementRequestDto>.Failure(["Currency cannot be changed after approval has started."]);

            if (request.FinancialYearId.HasValue)
                return Result<DisbursementRequestDto>.Failure(["Financial year cannot be changed after approval has started."]);

            if (request.Notes is not null)
                entity.Notes = request.Notes;

            if (request.Purpose is not null)
            {
                if (string.IsNullOrWhiteSpace(request.Purpose))
                    return Result<DisbursementRequestDto>.Failure(["Purpose is required."]);
                entity.Purpose = request.Purpose;
            }
        }
        else
        {
            if (entity.Status != DisbursementRequestStatus.Draft)
                return Result<DisbursementRequestDto>.Failure(["Only draft requests can be fully edited."]);

            if (request.BeneficiaryName is not null)
            {
                if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
                    return Result<DisbursementRequestDto>.Failure(["Beneficiary name is required."]);
                entity.BeneficiaryName = request.BeneficiaryName;
            }

            if (request.RequestedAmount.HasValue)
            {
                if (request.RequestedAmount.Value <= 0)
                    return Result<DisbursementRequestDto>.Failure(["Requested amount must be greater than zero."]);
                entity.RequestedAmount = request.RequestedAmount.Value;
            }

            if (request.CurrencyId.HasValue)
            {
                var currency = await context.Currencies.FindAsync(request.CurrencyId.Value, cancellationToken);
                if (currency is null)
                    return Result<DisbursementRequestDto>.Failure(["Invalid currency."]);
                entity.CurrencyId = request.CurrencyId.Value;
            }

            if (request.Purpose is not null)
            {
                if (string.IsNullOrWhiteSpace(request.Purpose))
                    return Result<DisbursementRequestDto>.Failure(["Purpose is required."]);
                entity.Purpose = request.Purpose;
            }

            if (request.FinancialYearId.HasValue)
            {
                var fiscalYear = await context.FiscalYears.FindAsync(request.FinancialYearId.Value, cancellationToken);
                if (fiscalYear is null)
                    return Result<DisbursementRequestDto>.Failure(["Invalid fiscal year."]);
                entity.FinancialYearId = request.FinancialYearId.Value;
            }

            if (request.Notes is not null)
                entity.Notes = request.Notes;
        }

        if (request.AccrualJournalEntryId.HasValue)
        {
            var journalEntry = await context.JournalEntries
                .FindAsync(request.AccrualJournalEntryId.Value, cancellationToken);
            if (journalEntry is null)
                return Result<DisbursementRequestDto>.Failure(["Invalid accrual journal entry."]);
            entity.AccrualJournalEntryId = request.AccrualJournalEntryId.Value;
        }

        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        string? accrualEntryNumber = null;
        if (entity.AccrualJournalEntryId.HasValue)
        {
            var accrualEntry = await context.JournalEntries
                .FindAsync(entity.AccrualJournalEntryId.Value, cancellationToken);
            accrualEntryNumber = accrualEntry?.EntryNumber;
        }

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
            entity.AccrualJournalEntryId,
            accrualEntryNumber,
            new()));
    }
}

public class UpdateDisbursementRequestCommandValidator : AbstractValidator<UpdateDisbursementRequestCommand>
{
    public UpdateDisbursementRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");

        RuleFor(x => x.BeneficiaryName)
            .MaximumLength(200).WithMessage("Beneficiary name must not exceed 200 characters.")
            .When(x => x.BeneficiaryName is not null);

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0).WithMessage("Requested amount must be greater than zero.")
            .When(x => x.RequestedAmount.HasValue);

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.")
            .When(x => x.CurrencyId.HasValue);

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.")
            .When(x => x.Purpose is not null);

        RuleFor(x => x.FinancialYearId)
            .GreaterThan(0).WithMessage("Financial year is required.")
            .When(x => x.FinancialYearId.HasValue);
    }
}
