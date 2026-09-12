using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.PaymentOrders.GetPaymentOrderPrint;

internal class GetPaymentOrderPrintQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetPaymentOrderPrintQuery, Common.DTOs.PaymentOrderPrintDto?>
{
    private static readonly Dictionary<PaymentOrderStatus, string> StatusLabels = new()
    {
        [PaymentOrderStatus.Draft] = "مسودة",
        [PaymentOrderStatus.Submitted] = "مرسلة",
        [PaymentOrderStatus.Approved] = "موافق عليها",
        [PaymentOrderStatus.SentToTreasury] = "مرسلة للخزينة",
        [PaymentOrderStatus.Paid] = "مدفوعة",
        [PaymentOrderStatus.Cancelled] = "ملغاة",
        [PaymentOrderStatus.Rejected] = "مرفوضة",
        [PaymentOrderStatus.Voided] = "ملغاة نهائياً"
    };

    public async Task<Common.DTOs.PaymentOrderPrintDto?> Handle(
        GetPaymentOrderPrintQuery request,
        CancellationToken cancellationToken)
    {
        var order = await context.PaymentOrders
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (order is null) return null;

        var deductions = await context.PaymentOrderDeductions
            .Where(d => d.PaymentOrderId == request.Id)
            .ToListAsync(cancellationToken);

        var fund = await context.Funds.FindAsync(new object[] { order.FundId }, cancellationToken);
        var currency = await context.Currencies.FindAsync(new object[] { order.CurrencyId }, cancellationToken);
        var classification = order.BudgetClassificationId.HasValue
            ? await context.BudgetClassifications.FindAsync(new object[] { order.BudgetClassificationId.Value }, cancellationToken)
            : null;
        var account = order.AccountId.HasValue
            ? await context.Accounts.FindAsync(new object[] { order.AccountId.Value }, cancellationToken)
            : null;
        var costCenter = order.CostCenterId.HasValue
            ? await context.CostCenters.FindAsync(new object[] { order.CostCenterId.Value }, cancellationToken)
            : null;
        var fiscalYear = await context.FiscalYears.FindAsync(new object[] { order.FiscalYearId }, cancellationToken);

        DisbursementRequest? dr = null;
        ERP_Government.Domain.Accounting.Entities.JournalEntry? accrualEntry = null;
        List<ERP_Government.Domain.Accounting.Entities.JournalEntryLine> accrualLines = [];
        if (order.AccrualJournalEntryId.HasValue)
        {
            accrualEntry = await context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == order.AccrualJournalEntryId.Value,
                    cancellationToken);
            if (accrualEntry is not null)
            {
                accrualLines = await context.JournalEntryLines
                    .Where(l => l.JournalEntryId == accrualEntry.Id)
                    .ToListAsync(cancellationToken);

                dr = await context.DisbursementRequests
                    .FirstOrDefaultAsync(x => x.AccrualJournalEntryId == accrualEntry.Id,
                        cancellationToken);
            }
        }

        Payment? payment = null;
        if (order.Status is PaymentOrderStatus.Paid or PaymentOrderStatus.SentToTreasury)
        {
            payment = await context.Payments
                .Where(p => p.PaymentOrderId == request.Id
                         && p.Status == PaymentStatus.Completed)
                .OrderByDescending(p => p.PaidAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var latestApproval = await context.ApprovalHistory
            .Where(a => a.DocumentType == "PaymentOrder"
                     && a.DocumentId == request.Id
                     && a.Action == ApprovalAction.Approve)
            .OrderByDescending(a => a.DecisionAt)
            .Select(a => new
            {
                ApproverName = a.ApproverUser.Login,
                a.RequiredRole,
                a.DecisionAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        string? createdByName = null;
        if (int.TryParse(order.CreatedBy, out var createdByUserId))
        {
            var creator = await context.Users.FindAsync(new object[] { createdByUserId }, cancellationToken);
            createdByName = creator?.Login;
        }

        var attachmentsCount = await context.Attachments
            .CountAsync(a => a.DocumentType == "PaymentOrder"
                          && a.DocumentId == request.Id,
                    cancellationToken);

        ERP_Government.Domain.Budgeting.Entities.BudgetItemAllocation? allocation = null;
        ERP_Government.Domain.Budgeting.Entities.BudgetItem? budgetItem = null;
        if (order.BudgetItemAllocationId.HasValue)
        {
            allocation = await context.BudgetItemAllocations
                .FirstOrDefaultAsync(a => a.Id == order.BudgetItemAllocationId.Value,
                    cancellationToken);
            if (allocation is not null)
            {
                budgetItem = await context.BudgetItems
                    .FirstOrDefaultAsync(b => b.Id == allocation.BudgetItemId,
                        cancellationToken);
            }
        }

        var taxDeductions = deductions.Where(d => d.IsTaxDeduction).Sum(d => d.Amount);
        var otherDeductions = deductions.Where(d => !d.IsTaxDeduction).Sum(d => d.Amount);
        var totalDeductions = deductions.Sum(d => d.Amount);
        var netAmount = order.AmountGross - totalDeductions;

        var paymentMethod = payment?.PaymentMethod ?? order.PaymentMethod;
        var paymentMethodName = paymentMethod switch
        {
            PaymentMethod.Cash => "نقدي",
            PaymentMethod.Check => "شيك",
            _ => "—"
        };

        var purpose = dr?.Purpose ?? order.Notes;
        if (string.IsNullOrWhiteSpace(purpose)) purpose = "—";

        var expenseLine = accrualLines.FirstOrDefault(l => l.Debit > 0);
        var liabilityLine = accrualLines.FirstOrDefault(l => l.Credit > 0);
        string? expenseAccountCode = null, expenseAccountName = null;
        string? liabilityAccountCode = null, liabilityAccountName = null;
        if (expenseLine is not null)
        {
            var expAccount = await context.Accounts.FindAsync(new object[] { expenseLine.AccountId }, cancellationToken);
            expenseAccountCode = expAccount?.Code;
            expenseAccountName = expAccount?.Name;
        }
        if (liabilityLine is not null)
        {
            var liaAccount = await context.Accounts.FindAsync(new object[] { liabilityLine.AccountId }, cancellationToken);
            liabilityAccountCode = liaAccount?.Code;
            liabilityAccountName = liaAccount?.Name;
        }

        var isUnapproved = order.Status is not
            (PaymentOrderStatus.Approved or PaymentOrderStatus.SentToTreasury or PaymentOrderStatus.Paid);

        return new Common.DTOs.PaymentOrderPrintDto
        {
            OrderNumber = order.PaymentOrderNumber,
            OrderDate = order.PaymentOrderDate,
            DueDate = order.DueDate,
            OrderType = order.PaymentOrderType,
            Status = order.Status,
            StatusLabel = StatusLabels.GetValueOrDefault(order.Status, order.Status.ToString()),
            DisbursementRequestNumber = dr?.RequestNumber,
            AccrualJournalEntryId = order.AccrualJournalEntryId,
            AccrualEntryNumber = accrualEntry?.EntryNumber,
            AccrualAmount = expenseLine?.Debit,
            AccrualExpenseAccountCode = expenseAccountCode,
            AccrualExpenseAccountName = expenseAccountName,
            AccrualLiabilityAccountCode = liabilityAccountCode,
            AccrualLiabilityAccountName = liabilityAccountName,
            FiscalYearName = fiscalYear?.Name ?? "—",
            FiscalYearNumber = fiscalYear?.YearNumber ?? 0,
            AmountGross = order.AmountGross,
            TaxDeductions = taxDeductions,
            OtherDeductions = otherDeductions,
            TotalDeductions = totalDeductions,
            NetAmount = netAmount,
            CurrencyCode = currency?.Code ?? "YER",
            CurrencyName = currency?.Name ?? "ريال يمني",
            BeneficiaryName = order.BeneficiaryName,
            BeneficiaryAccountNumber = order.BeneficiaryAccountNumber,
            BeneficiaryBankName = order.BeneficiaryBankName,
            Purpose = purpose,
            AttachmentsCount = attachmentsCount,
            FundCode = fund?.FundNumber ?? "—",
            FundName = fund?.FundName ?? "—",
            ClassificationCode = classification?.Code,
            ClassificationName = classification?.Name,
            AccountCode = account?.Code,
            AccountName = account?.Name,
            CostCenterCode = costCenter?.Code,
            CostCenterName = costCenter?.Name,
            BudgetItemCode = budgetItem?.ItemCode,
            BudgetItemName = budgetItem?.ItemName,
            AllocationProposedAmount = allocation?.ProposedAmount,
            AllocationApprovedAmount = allocation?.ApprovedAmount,
            PaymentMethodName = paymentMethodName,
            PaymentReferenceNumber = payment?.ReferenceNumber,
            PaymentNumber = payment?.PaymentNumber,
            PaidAt = payment?.PaidAt,
            CreatedByName = createdByName,
            Created = order.Created,
            ApproverName = latestApproval?.ApproverName,
            RequiredRole = latestApproval?.RequiredRole,
            ApprovedAt = latestApproval?.DecisionAt,
            PaidByName = payment?.PaidByName,
            IsUnapproved = isUnapproved
        };
    }
}
