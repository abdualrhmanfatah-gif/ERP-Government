using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.ApprovalRules.Commands.UpdateApprovalRule;

[Authorize(Policy = PermissionCodes.ApprovalRulesManage)]
public class UpdateApprovalRuleCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public decimal? AmountThreshold { get; init; }
    public int? CurrencyId { get; init; }
    public string? ApproverRole { get; init; }
    public int Sequence { get; init; }
}

public class UpdateApprovalRuleCommandValidator : AbstractValidator<UpdateApprovalRuleCommand>
{
    public UpdateApprovalRuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid approval rule ID.");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.")
            .MaximumLength(50).WithMessage("Document type must not exceed 50 characters.");

        RuleFor(x => x.ApproverRole)
            .NotEmpty().WithMessage("Approver role is required.")
            .MaximumLength(50).WithMessage("Approver role must not exceed 50 characters.");

        RuleFor(x => x.Sequence)
            .GreaterThanOrEqualTo(0).WithMessage("Sequence must be non-negative.");
    }
}

public class UpdateApprovalRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateApprovalRuleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateApprovalRuleCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ApprovalRules.FindAsync(request.Id, cancellationToken);

        if (entity == null)
            return Result.Failure(ErrorCodes.SecurityApprovalRules.NotFound, ErrorCategory.NotFound, $"Approval rule with ID {request.Id} not found.");

        // Validate SecurityRole.Code exists
        var roleExists = await context.SecurityRoles
            .AnyAsync(r => r.Code == request.ApproverRole, cancellationToken);

        if (!roleExists)
            return Result.Failure(ErrorCodes.SecurityApprovalRules.RoleNotFound, ErrorCategory.NotFound, $"Security role with code '{request.ApproverRole}' does not exist.");

        // Validate unique constraint if sequence changed
        if (entity.Sequence != request.Sequence || entity.FundId != request.FundId)
        {
            var duplicateExists = await context.ApprovalRules
                .AnyAsync(r => r.Id != request.Id
                    && r.DocumentType == request.DocumentType
                    && r.FundId == request.FundId
                    && r.Sequence == request.Sequence,
                    cancellationToken);

            if (duplicateExists)
                return Result.Failure(ErrorCodes.SecurityApprovalRules.DuplicateRule, ErrorCategory.Conflict,
                    $"An approval rule with sequence {request.Sequence} already exists for document type '{request.DocumentType}' and the same fund.");
        }

        entity.DocumentType = request.DocumentType;
        entity.FundId = request.FundId;
        entity.AmountThreshold = request.AmountThreshold;
        entity.CurrencyId = request.CurrencyId;
        entity.ApproverRole = request.ApproverRole;
        entity.Sequence = request.Sequence;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
