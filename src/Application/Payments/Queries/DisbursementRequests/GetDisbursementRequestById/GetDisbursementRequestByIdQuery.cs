using System.Text.Json;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Security.Enums;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequestById;

[Authorize(Policy = PermissionCodes.DisbursementRequestsView)]
public record GetDisbursementRequestByIdQuery(int Id) : IRequest<Result<DisbursementRequestDetailDto>>;

public class GetDisbursementRequestByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementRequestByIdQuery, Result<DisbursementRequestDetailDto>>
{
    public async Task<Result<DisbursementRequestDetailDto>> Handle(
        GetDisbursementRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.DisbursementRequests
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<DisbursementRequestDetailDto>.Failure(ErrorCodes.Payments.DisbursementRequestNotFound, ErrorCategory.NotFound, $"Disbursement request with ID {request.Id} not found.");

        var linkedOrder = entity.AccrualJournalEntryId.HasValue
            ? await context.PaymentOrders
                .FirstOrDefaultAsync(
                    o => o.AccrualJournalEntryId == entity.AccrualJournalEntryId.Value,
                    cancellationToken)
            : null;

        var approvalEntities = await context.ApprovalHistory
            .Where(a => a.DocumentType == "DisbursementRequest" && a.DocumentId == entity.Id)
            .OrderBy(a => a.ApprovalStep)
            .ToListAsync(cancellationToken);

        var approvals = approvalEntities.Select(a =>
        {
            decimal? approvedAmount = null;

            if (a.EvaluationSnapshot is { Length: > 0 })
            {
                var doc = JsonDocument.Parse(a.EvaluationSnapshot);
                if (doc.RootElement.TryGetProperty("ApprovedAmount", out var amt))
                    approvedAmount = amt.GetDecimal();
            }

            return new ApprovalStepDto(
                a.ApprovalStep,
                a.ApproverUserId,
                "",
                a.RequiredRole,
                a.Decision,
                a.DecisionAt,
                approvedAmount);
        }).ToList();

        string? accrualEntryNumber = null;
        if (entity.AccrualJournalEntryId.HasValue)
        {
            var accrualEntry = await context.JournalEntries
                .FirstOrDefaultAsync(j => j.Id == entity.AccrualJournalEntryId.Value, cancellationToken);
            accrualEntryNumber = accrualEntry?.EntryNumber;
        }

        return Result<DisbursementRequestDetailDto>.Success(new DisbursementRequestDetailDto(
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
            linkedOrder?.Id,
            linkedOrder?.PaymentOrderNumber,
            entity.AccrualJournalEntryId,
            accrualEntryNumber,
            approvals));
    }
}
