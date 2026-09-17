using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequestAccrual;

[Authorize(Policy = PermissionCodes.DisbursementRequestsView)]
public record GetDisbursementRequestAccrualQuery(int Id) : IRequest<Result<AccrualEntryDto>>;

public record AccrualEntryDto(
    int Id,
    string EntryNumber,
    DateOnly DocumentDate,
    decimal Amount,
    int ExpenseAccountId,
    string ExpenseAccountCode,
    string ExpenseAccountName,
    int LiabilityAccountId,
    string LiabilityAccountCode,
    string LiabilityAccountName,
    string? Narration);

public class GetDisbursementRequestAccrualQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementRequestAccrualQuery, Result<AccrualEntryDto>>
{
    public async Task<Result<AccrualEntryDto>> Handle(
        GetDisbursementRequestAccrualQuery request,
        CancellationToken cancellationToken)
    {
        var disbursementRequest = await context.DisbursementRequests
            .FindAsync(request.Id, cancellationToken);

        if (disbursementRequest?.AccrualJournalEntryId is null)
            return Result<AccrualEntryDto>.Failure(ErrorCodes.Payments.DisbursementRequestNotFound, ErrorCategory.NotFound, $"No accrual found for disbursement request {request.Id}.");

        var journalEntry = await context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == disbursementRequest.AccrualJournalEntryId.Value, cancellationToken);

        if (journalEntry is null)
            return Result<AccrualEntryDto>.Failure(ErrorCodes.Request.NotFound, ErrorCategory.NotFound, "Accrual journal entry not found.");

        var lines = await context.JournalEntryLines
            .Where(l => l.JournalEntryId == journalEntry.Id)
            .ToListAsync(cancellationToken);

        var expenseLine = lines.FirstOrDefault(l => l.Debit > 0);
        var liabilityLine = lines.FirstOrDefault(l => l.Credit > 0);

        if (expenseLine is null || liabilityLine is null)
            return Result<AccrualEntryDto>.Failure(ErrorCodes.Request.NotFound, ErrorCategory.NotFound, "Accrual journal entry lines not found.");

        var expenseAccount = await context.Accounts.FindAsync(expenseLine.AccountId, cancellationToken);
        var liabilityAccount = await context.Accounts.FindAsync(liabilityLine.AccountId, cancellationToken);

        decimal amount = expenseLine.Debit;

        return Result<AccrualEntryDto>.Success(new AccrualEntryDto(
            journalEntry.Id,
            journalEntry.EntryNumber,
            journalEntry.DocumentDate,
            amount,
            expenseLine.AccountId,
            expenseAccount?.Code ?? "",
            expenseAccount?.Name ?? "",
            liabilityLine.AccountId,
            liabilityAccount?.Code ?? "",
            liabilityAccount?.Name ?? "",
            journalEntry.Narration));
    }
}
