using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;

namespace ERP_Government.Application.Payments.Commands.PaymentOrders.ApprovePaymentOrder;

[Authorize(Policy = PermissionCodes.PaymentOrdersApprove)]
public class ApprovePaymentOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class ApprovePaymentOrderCommandHandler(
    IApplicationDbContext context,
    IApprovalRuleEvaluationService evaluationService,
    IIdentityService identityService,
    IDocumentStatusLogger statusLogger,
    IAttachmentGateService attachmentGate,
    IUser user) : IRequestHandler<ApprovePaymentOrderCommand, Result>
{
    public async Task<Result> Handle(
        ApprovePaymentOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result.Failure(["User identity is required for this operation."]);

        var entity = await context.PaymentOrders
            .FindAsync(request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Payment order not found."]);

        if (entity.Status != PaymentOrderStatus.Submitted)
            return Result.Failure(["Only submitted payment orders can be approved."]);

        if (entity.BudgetCheckStatus != BudgetCheckStatus.Passed)
            return Result.Failure(["Cannot approve payment order with failed budget check."]);

        var missingAttachments = await attachmentGate.CheckMandatoryAttachmentsAsync(
            "PaymentOrder", entity.Id, cancellationToken);

        if (missingAttachments.Count > 0)
            return Result.Failure([$"Cannot approve: missing mandatory attachments ({string.Join(", ", missingAttachments)})."]);

        var evaluationResults = await evaluationService.EvaluateAsync(
            "PaymentOrder",
            entity.AmountGross,
            entity.FundId,
            entity.CurrencyId,
            cancellationToken);

        if (evaluationResults.Count == 0)
            return Result.Failure(["No approval rules defined for this document type."]);

        string? matchedRole = null;
        foreach (var rule in evaluationResults)
        {
            if (string.IsNullOrEmpty(rule.RequiredRole)) continue;
            if (await identityService.IsInRoleAsync(userId, rule.RequiredRole))
            {
                matchedRole = rule.RequiredRole;
                break;
            }
        }

        if (matchedRole is null)
        {
            var requiredRoles = evaluationResults
                .Select(r => r.RequiredRole)
                .Where(r => !string.IsNullOrEmpty(r));
            return Result.Failure([$"User does not have any required approval role ({string.Join(", ", requiredRoles)})."]);
        }

        var evaluationSnapshot = JsonSerializer.Serialize(evaluationResults.Select(r => new
        {
            r.Sequence,
            r.RequiredRole,
            r.ApproverRoleId
        }));

        var history = new ApprovalHistory
        {
            DocumentType = "PaymentOrder",
            DocumentId = entity.Id,
            ApprovalStep = 1,
            Action = ApprovalAction.Approve,
            ApproverUserId = userId,
            RequiredRole = matchedRole,
            Decision = "Approved",
            DecisionAt = DateTimeOffset.UtcNow,
            EvaluationSnapshot = evaluationSnapshot,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };
        context.ApprovalHistory.Add(history);

        entity.Status = PaymentOrderStatus.Approved;

        await statusLogger.LogAsync(
            "paymentorders",
            entity.Id,
            PaymentOrderStatus.Submitted.ToString(),
            PaymentOrderStatus.Approved.ToString(),
            userId,
            null,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class ApprovePaymentOrderCommandValidator : AbstractValidator<ApprovePaymentOrderCommand>
{
    public ApprovePaymentOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid payment order ID.");
    }
}
