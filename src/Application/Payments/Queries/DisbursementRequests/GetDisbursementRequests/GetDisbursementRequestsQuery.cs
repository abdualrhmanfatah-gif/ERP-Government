using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Common.DTOs;
using ERP_Government.Domain.Payments.Enums;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Queries.DisbursementRequests.GetDisbursementRequests;

public record GetDisbursementRequestsQuery(
    DisbursementRequestStatus? Status = null,
    int? RequestedById = null) : IRequest<List<DisbursementRequestDto>>;

public class GetDisbursementRequestsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetDisbursementRequestsQuery, List<DisbursementRequestDto>>
{
    public async Task<List<DisbursementRequestDto>> Handle(
        GetDisbursementRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.DisbursementRequests.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(d => d.Status == request.Status.Value);

        if (request.RequestedById.HasValue)
            query = query.Where(d => d.RequestedById == request.RequestedById.Value);

        var entities = await query
            .OrderByDescending(d => d.RequestDate)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        var ids = entities.Select(d => d.Id).ToList();

        var approvalsByDoc = await context.ApprovalHistory
            .Where(a => a.DocumentType == "DisbursementRequest" && ids.Contains(a.DocumentId))
            .OrderBy(a => a.ApprovalStep)
            .GroupBy(a => a.DocumentId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

        var orders = await context.PaymentOrders
            .Where(o => o.DisbursementRequestId.HasValue && ids.Contains(o.DisbursementRequestId.Value))
            .ToDictionaryAsync(o => o.DisbursementRequestId!.Value, o => o, cancellationToken);

        var accrualJournalEntryIds = entities
            .Where(d => d.AccrualJournalEntryId.HasValue)
            .Select(d => d.AccrualJournalEntryId!.Value)
            .ToList();

        var journalEntries = await context.JournalEntries
            .Where(j => accrualJournalEntryIds.Contains(j.Id))
            .ToDictionaryAsync(j => j.Id, j => j.EntryNumber, cancellationToken);

        return entities.Select(d =>
        {
            approvalsByDoc.TryGetValue(d.Id, out var approvalEntities);
            var approvals = (approvalEntities ?? new()).Select(a =>
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

            orders.TryGetValue(d.Id, out var linkedOrder);

            string? accrualNumber = null;
            if (d.AccrualJournalEntryId.HasValue)
                journalEntries.TryGetValue(d.AccrualJournalEntryId.Value, out accrualNumber);

            return new DisbursementRequestDto(
                d.Id,
                d.RequestNumber,
                d.RequestedById,
                d.RequestedByName,
                d.BeneficiaryName,
                d.RequestedAmount,
                d.CurrencyId,
                d.Purpose,
                d.FinancialYearId,
                d.RequestDate,
                d.Status,
                d.Notes,
                d.PaymentDate,
                linkedOrder?.Id,
                linkedOrder?.PaymentOrderNumber,
                d.AccrualJournalEntryId,
                accrualNumber,
                approvals);
        }).ToList();
    }
}
