using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateAccrualEntry;

[Authorize(Policy = PermissionCodes.DisbursementRequestsCreateAccrual)]
public class CreateAccrualEntryCommand : IRequest<Result<int>>
{
    public int DisbursementRequestId { get; init; }
    public int ExpenseAccountId { get; init; }
    public int LiabilityAccountId { get; init; }
    public decimal Amount { get; init; }
    public int CurrencyId { get; init; }
    public int? CostCenterId { get; init; }
    public string? Narration { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class CreateAccrualEntryCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateAccrualEntryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateAccrualEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(["User identity is required for this operation."]);

        var entity = await context.DisbursementRequests
            .FindAsync(request.DisbursementRequestId, cancellationToken);

        if (entity is null)
            return Result<int>.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.Approved)
            return Result<int>.Failure(["Only approved disbursement requests can have accrual entries."]);

        if (entity.AccrualJournalEntryId.HasValue)
            return Result<int>.Failure(["An accrual entry already exists for this disbursement request."]);

        // Validate accounts exist, are postable, and active
        var expenseAccount = await context.Accounts
            .FindAsync(request.ExpenseAccountId, cancellationToken);
        if (expenseAccount is null || !expenseAccount.IsPostable || !expenseAccount.IsActive)
            return Result<int>.Failure(["Invalid expense account."]);

        var liabilityAccount = await context.Accounts
            .FindAsync(request.LiabilityAccountId, cancellationToken);
        if (liabilityAccount is null || !liabilityAccount.IsPostable || !liabilityAccount.IsActive)
            return Result<int>.Failure(["Invalid liability account."]);

        if (request.Amount <= 0)
            return Result<int>.Failure(["Amount must be greater than zero."]);

        if (request.Amount > entity.RequestedAmount)
            return Result<int>.Failure(["Amount cannot exceed the requested amount."]);

        if (request.ExpenseAccountId == request.LiabilityAccountId)
            return Result<int>.Failure(["Expense and liability accounts must be different."]);

        // Get active fiscal period for today
        var today = DateOnly.FromDateTime(DateTime.Today);
        var period = await context.FiscalPeriods
            .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
            return Result<int>.Failure(["No active fiscal period found for today."]);

        // Generate entry number
        string entryNumber;
        try
        {
            entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure([ex.Message]);
        }

        // Create journal entry
        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = today,
            EntryStatus = EntryStatus.Draft,
            EntryType = MoveEntryType.Accrual,
            Ref = $"طلب صرف #{entity.RequestNumber}",
            JournalId = null,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = request.Narration ?? $"قيد استحقاق — طلب صرف #{entity.RequestNumber}",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.JournalEntries.Add(journalEntry);

        // Line 1: Debit expense account
        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 1,
            AccountId = request.ExpenseAccountId,
            Description = $"استحقاق — {entity.BeneficiaryName}",
            Debit = request.Amount,
            Credit = 0,
            CurrencyId = request.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = request.CostCenterId,
            RowVersion = []
        });

        // Line 2: Credit liability account
        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 2,
            AccountId = request.LiabilityAccountId,
            Description = $"استحقاق — {entity.BeneficiaryName}",
            Debit = 0,
            Credit = request.Amount,
            CurrencyId = request.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = request.CostCenterId,
            RowVersion = []
        });

        await context.SaveChangesAsync(cancellationToken);

        // Link to disbursement request (JournalEntry.Id now populated)
        entity.AccrualJournalEntryId = journalEntry.Id;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(journalEntry.Id);
    }
}

public class CreateAccrualEntryCommandValidator : AbstractValidator<CreateAccrualEntryCommand>
{
    public CreateAccrualEntryCommandValidator()
    {
        RuleFor(x => x.DisbursementRequestId)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");

        RuleFor(x => x.ExpenseAccountId)
            .GreaterThan(0).WithMessage("Expense account is required.");

        RuleFor(x => x.LiabilityAccountId)
            .GreaterThan(0).WithMessage("Liability account is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.ExpenseAccountId)
            .NotEqual(x => x.LiabilityAccountId)
            .WithMessage("Expense and liability accounts must be different.");
    }
}
