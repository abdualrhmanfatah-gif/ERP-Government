using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.Extensions.Logging;

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
    IUser user,
    ILogger<RecordPaymentCommandHandler> logger) : IRequestHandler<RecordPaymentCommand, Result<Common.DTOs.PaymentDto>>
{
    public async Task<Result<Common.DTOs.PaymentDto>> Handle(
        RecordPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Request.Unauthorized, ErrorCategory.Authorization, "هوية المستخدم مطلوبة لهذه العملية.");

        var paymentOrder = await context.PaymentOrders
            .FindAsync(request.PaymentOrderId, cancellationToken);

        if (paymentOrder is null)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.PaymentNotFound, ErrorCategory.NotFound, "أمر الدفع غير موجود.");

        if (paymentOrder.Status != PaymentOrderStatus.Approved && paymentOrder.Status != PaymentOrderStatus.SentToTreasury)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.ApprovalRequired, ErrorCategory.BusinessRule, "يجب أن يكون أمر الدفع معتمداً قبل تنفيذ الدفع.");

        // ADR-001 D-6: one payment per order
        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.PaymentOrderId == request.PaymentOrderId && p.Status == PaymentStatus.Completed, cancellationToken);

        if (existingPayment is not null)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.PaymentNotFound, ErrorCategory.Conflict, "تم تسجيل دفع مسبقاً لهذا أمر الدفع.");

        var sequence = await context.DocumentSequences
            .FirstOrDefaultAsync(s => s.DocumentType == "Payment", cancellationToken);

        if (sequence is null)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.FinancialSettings.SequenceNotFound, ErrorCategory.Internal, "تسلسل الدفع غير مُعد.");

        var netTotal = paymentOrder.AmountGross - paymentOrder.DeductionAmount;

        var userEntity = await context.Users.FindAsync(userId, cancellationToken);
        string paidByName = userEntity?.Login ?? "Unknown";

        if (!paymentOrder.AccrualJournalEntryId.HasValue)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.ApprovalRequired, ErrorCategory.BusinessRule, "يجب ربط أمر الدفع بقيد احتسابي قبل تنفيذ الدفع.");

        var accrualEntry = await context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == paymentOrder.AccrualJournalEntryId.Value, cancellationToken);
        if (accrualEntry is null || accrualEntry.EntryType != MoveEntryType.Accrual)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Accounting.EntryNotBalanced, ErrorCategory.NotFound, "أمر الدفع غير مرتبط بقيد احتسابي صالح.");

        var accrualCreditLines = await context.JournalEntryLines
            .Where(l => l.JournalEntryId == accrualEntry.Id && l.Credit > 0)
            .ToListAsync(cancellationToken);
        if (accrualCreditLines.Count != 1)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Accounting.EntryNotBalanced, ErrorCategory.BusinessRule, "يجب أن يحتوي القيد الاحتسابي على سطر دائن للالتزام واحد فقط.");

        if (!paymentOrder.BankAccountId.HasValue)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.InvalidAccount, ErrorCategory.Validation, "الحساب البنكي مطلوب للدفع.");

        var bankAccount = await context.BankAccounts
            .FindAsync(paymentOrder.BankAccountId.Value, cancellationToken);
        if (bankAccount is null || !bankAccount.IsActive)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.InvalidAccount, ErrorCategory.NotFound, "الحساب البنكي غير صالح أو غير نشط.");

        if (!bankAccount.GlAccountId.HasValue)
            return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Payments.InvalidAccount, ErrorCategory.BusinessRule, "يجب ربط الحساب البنكي بحساب دفتر عام قبل تنفيذ الدفع.");

        var linkedDisbursementRequest = await context.DisbursementRequests
            .FirstOrDefaultAsync(d => d.AccrualJournalEntryId == accrualEntry.Id, cancellationToken);

        var journalEntry = await CreateDynamicPaymentEntry(
            paymentOrder,
            netTotal,
            accrualCreditLines[0].AccountId,
            bankAccount.GlAccountId.Value,
            userId,
            cancellationToken);

        paymentOrder.JournalEntryId = journalEntry.Id;

        // Journal entry succeeded — now apply all changes atomically
        var previousStatus = paymentOrder.Status;

        var payment = new Payment
        {
            PaymentNumber = $"PAY-{sequence.CurrentNumber:D6}",
            PaymentOrderId = request.PaymentOrderId,
            DisbursementRequestId = linkedDisbursementRequest?.Id ?? paymentOrder.DisbursementRequestId ?? 0,
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

        if (linkedDisbursementRequest is not null)
        {
            linkedDisbursementRequest.Status = DisbursementRequestStatus.Disbursed;
            linkedDisbursementRequest.PaymentDate = DateTimeOffset.UtcNow;
            linkedDisbursementRequest.LastModified = DateTimeOffset.UtcNow;
            linkedDisbursementRequest.LastModifiedBy = userId.ToString();
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
                logger.LogError(ex, "Database update failure recording payment for order {PaymentOrderId}", request.PaymentOrderId);
                return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "فشل الحفظ بسبب خطأ في قاعدة البيانات.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex, "Unexpected error recording payment for order {PaymentOrderId}", request.PaymentOrderId);
                return Result<Common.DTOs.PaymentDto>.Failure(ErrorCodes.Request.InternalError, ErrorCategory.Internal, "خطأ غير متوقع أثناء تسجيل الدفع.");
            }
        }, cancellationToken);
    }

    private async Task<JournalEntry> CreateDynamicPaymentEntry(
        PaymentOrder paymentOrder,
        decimal amount,
        int liabilityAccountId,
        int bankGlAccountId,
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
            EntryType = MoveEntryType.SystemGenerated,
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
            PaymentOrderId = paymentOrder.Id,
            RowVersion = []
        });

        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 2,
            AccountId = bankGlAccountId,
            Description = $"دفعة — {paymentOrder.BeneficiaryName}",
            Debit = 0,
            Credit = amount,
            CurrencyId = paymentOrder.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = paymentOrder.CostCenterId,
            PaymentOrderId = paymentOrder.Id,
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




