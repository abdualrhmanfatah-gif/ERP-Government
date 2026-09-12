using ERP_Government.Application.Accounting.EventHandlers;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Payments;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;

namespace ERP_Government.Application.Payments.Commands.Payments.RecordPayment;

[Authorize(Policy = PermissionCodes.PaymentsCreate)]
public class RecordPaymentCommand : IRequest<Result<Common.DTOs.PaymentDto>>
{
    public int PaymentOrderId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}

public class RecordPaymentCommandHandler(
    IApplicationDbContext context,
    IDocumentStatusLogger statusLogger,
    IDocumentSequenceService sequenceService,
    JournalEntryGenerator journalEntryGenerator,
    PostingRuleMatcher postingRuleMatcher,
    IUser user) : IRequestHandler<RecordPaymentCommand, Result<Common.DTOs.PaymentDto>>
{
    public async Task<Result<Common.DTOs.PaymentDto>> Handle(
        RecordPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<Common.DTOs.PaymentDto>.Failure(["User identity is required for this operation."]);

        var paymentOrder = await context.PaymentOrders
            .FindAsync(request.PaymentOrderId, cancellationToken);

        if (paymentOrder is null)
            return Result<Common.DTOs.PaymentDto>.Failure(["Payment order not found."]);

        if (paymentOrder.Status != PaymentOrderStatus.Approved && paymentOrder.Status != PaymentOrderStatus.SentToTreasury)
            return Result<Common.DTOs.PaymentDto>.Failure(["Payment order must be approved before payment execution."]);

        // ADR-001 D-6: one payment per order
        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.PaymentOrderId == request.PaymentOrderId && p.Status == PaymentStatus.Completed, cancellationToken);

        if (existingPayment is not null)
            return Result<Common.DTOs.PaymentDto>.Failure(["A payment has already been recorded for this payment order."]);

        var sequence = await context.DocumentSequences
            .FirstOrDefaultAsync(s => s.DocumentType == "Payment", cancellationToken);

        if (sequence is null)
            return Result<Common.DTOs.PaymentDto>.Failure(["Payment sequence not configured."]);

        var netTotal = paymentOrder.AmountGross - paymentOrder.DeductionAmount;

        var userEntity = await context.Users.FindAsync(userId, cancellationToken);
        string paidByName = userEntity?.Login ?? "Unknown";

        // Check if linked disbursement request has an accrual entry
        int? liabilityAccountId = null;
        if (paymentOrder.DisbursementRequestId.HasValue)
        {
            var disbursementRequest = await context.DisbursementRequests
                .FindAsync(paymentOrder.DisbursementRequestId.Value, cancellationToken);

            if (disbursementRequest?.AccrualJournalEntryId.HasValue == true)
            {
                // Find the liability account from the accrual entry's credit line
                var accrualCreditLine = await context.JournalEntryLines
                    .Where(l => l.JournalEntryId == disbursementRequest.AccrualJournalEntryId.Value && l.Credit > 0)
                    .FirstOrDefaultAsync(cancellationToken);

                if (accrualCreditLine is not null)
                {
                    liabilityAccountId = accrualCreditLine.AccountId;
                }
            }
        }

        JournalEntry journalEntry;
        if (liabilityAccountId.HasValue)
        {
            // Dynamic payment entry: debit liability account, credit bank account
            journalEntry = await CreateDynamicPaymentEntry(
                paymentOrder,
                netTotal,
                liabilityAccountId.Value,
                userId,
                cancellationToken);
        }
        else
        {
            // Standard posting rules
            var eventTypeStr = EventType.PaymentOrderExecuted.ToString();
            var rules = await postingRuleMatcher.MatchAsync(eventTypeStr, cancellationToken);

            if (rules.Count == 0)
                return Result<Common.DTOs.PaymentDto>.Failure(["لا توجد قواعد ترحيل محاسبي معرّفة لأوامر الدفع."]);

            var documentDate = DateOnly.FromDateTime(DateTime.Today);
            journalEntry = null!;
            foreach (var rule in rules)
            {
                journalEntry = await journalEntryGenerator.GenerateJournalEntryAsync(
                    EventType.PaymentOrderExecuted,
                    rule,
                    new PaymentOrderExecuted
                    {
                        SourceEntityId = paymentOrder.Id,
                        PaymentOrderId = paymentOrder.Id,
                        Amount = netTotal,
                        BankAccountId = paymentOrder.BankAccountId,
                        AccountId = paymentOrder.AccountId,
                        CostCenterId = paymentOrder.CostCenterId,
                        CurrencyId = paymentOrder.CurrencyId,
                        OccurredAt = DateTimeOffset.UtcNow
                    },
                    documentDate,
                    cancellationToken);
            }
        }

        paymentOrder.JournalEntryId = journalEntry.Id;

        // Journal entry succeeded — now apply all changes atomically
        var previousStatus = paymentOrder.Status;

        var payment = new Payment
        {
            PaymentNumber = $"PAY-{sequence.CurrentNumber:D6}",
            PaymentOrderId = request.PaymentOrderId,
            DisbursementRequestId = paymentOrder.DisbursementRequestId ?? 0,
            PaymentMethod = request.PaymentMethod,
            Amount = netTotal,
            PaidById = userId,
            PaidByName = paidByName,
            PaidAt = DateTimeOffset.UtcNow,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            Status = PaymentStatus.Completed,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        sequence.CurrentNumber++;

        paymentOrder.Status = PaymentOrderStatus.Paid;
        paymentOrder.PaidAt = DateTimeOffset.UtcNow;
        paymentOrder.LastModified = DateTimeOffset.UtcNow;
        paymentOrder.LastModifiedBy = userId.ToString();

        await statusLogger.LogAsync(
            "paymentorders",
            paymentOrder.Id,
            previousStatus.ToString(),
            PaymentOrderStatus.Paid.ToString(),
            userId,
            null,
            cancellationToken);

        // Update linked disbursement request if exists
        if (paymentOrder.DisbursementRequestId.HasValue)
        {
            var dr = await context.DisbursementRequests.FindAsync(paymentOrder.DisbursementRequestId.Value, cancellationToken);
            if (dr is not null)
            {
                dr.Status = DisbursementRequestStatus.Disbursed;
                dr.PaymentDate = DateTimeOffset.UtcNow;
                dr.LastModified = DateTimeOffset.UtcNow;
                dr.LastModifiedBy = userId.ToString();
            }
        }

        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (CancellationToken ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            try
            {
                context.Payments.Add(payment);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return Result<Common.DTOs.PaymentDto>.Success(new Common.DTOs.PaymentDto(
                    payment.Id,
                    payment.PaymentNumber,
                    payment.DisbursementRequestId,
                    "",
                    payment.PaymentOrderId,
                    paymentOrder.PaymentOrderNumber,
                    payment.PaymentMethod,
                    payment.Amount,
                    payment.PaidById,
                    payment.PaidByName,
                    payment.PaidAt,
                    payment.ReferenceNumber,
                    payment.Notes,
                    payment.Status,
                    paymentOrder.BeneficiaryName));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                await transaction.RollbackAsync(ct);
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                return Result<Common.DTOs.PaymentDto>.Failure([$"فشل الحفظ: {innerMsg}"]);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<Common.DTOs.PaymentDto>.Failure([$"خطأ غير متوقع: {ex.Message}"]);
            }
        }, cancellationToken);
    }

    private async Task<JournalEntry> CreateDynamicPaymentEntry(
        PaymentOrder paymentOrder,
        decimal amount,
        int liabilityAccountId,
        int userId,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var period = await context.FiscalPeriods
            .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
            throw new InvalidOperationException("No active fiscal period found for today.");

        string entryNumber;
        try
        {
            entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = today,
            EntryStatus = EntryStatus.Draft,
            JournalId = null,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = $"دفعة — أمر دفع #{paymentOrder.PaymentOrderNumber}",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.JournalEntries.Add(journalEntry);

        // Line 1: Debit liability account (settle the accrual)
        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 1,
            AccountId = liabilityAccountId,
            Description = $"تسديد — {paymentOrder.BeneficiaryName}",
            Debit = amount,
            Credit = 0,
            CurrencyId = paymentOrder.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = paymentOrder.CostCenterId,
            RowVersion = []
        });

        // Line 2: Credit bank account
        if (!paymentOrder.BankAccountId.HasValue)
            throw new InvalidOperationException("Bank account is required for payment.");

        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 2,
            AccountId = paymentOrder.BankAccountId.Value,
            Description = $"دفعة — {paymentOrder.BeneficiaryName}",
            Debit = 0,
            Credit = amount,
            CurrencyId = paymentOrder.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = paymentOrder.CostCenterId,
            RowVersion = []
        });

        return journalEntry;
    }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentOrderId)
            .GreaterThan(0).WithMessage("Payment order is required.");
    }
}




