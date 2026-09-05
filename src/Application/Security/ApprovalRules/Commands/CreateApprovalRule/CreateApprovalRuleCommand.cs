using ERP_Government.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Security.ApprovalRules.Commands.CreateApprovalRule;

public class CreateApprovalRuleCommand : IRequest<int>
{
    public string DocumentType { get; init; } = string.Empty;
    public int? FundId { get; init; }
    public decimal? AmountThreshold { get; init; }
    public int? CurrencyId { get; init; }
    public string? ApproverRole { get; init; }
    public int Sequence { get; init; }
}

public class CreateApprovalRuleCommandValidator : AbstractValidator<CreateApprovalRuleCommand>
{
    public CreateApprovalRuleCommandValidator()
    {
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

public class CreateApprovalRuleCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateApprovalRuleCommand, int>
{
    public async Task<int> Handle(
        CreateApprovalRuleCommand request,
        CancellationToken cancellationToken)
    {
        // Validate SecurityRole.Code exists
        var roleExists = await context.SecurityRoles
            .AnyAsync(r => r.Code == request.ApproverRole, cancellationToken);

        if (!roleExists)
            throw new InvalidOperationException($"Security role with code '{request.ApproverRole}' does not exist.");

        // Validate unique constraint (DocumentType, FundId, Sequence)
        var duplicateExists = await context.ApprovalRules
            .AnyAsync(r => r.DocumentType == request.DocumentType
                && r.FundId == request.FundId
                && r.Sequence == request.Sequence,
                cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException(
                $"An approval rule with sequence {request.Sequence} already exists for document type '{request.DocumentType}' and the same fund.");

        var entity = new Domain.Security.Entities.ApprovalRule
        {
            DocumentType = request.DocumentType,
            FundId = request.FundId,
            AmountThreshold = request.AmountThreshold,
            CurrencyId = request.CurrencyId,
            ApproverRole = request.ApproverRole,
            Sequence = request.Sequence,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.ApprovalRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
