using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetAttributes.Bindings.Commands;

[Authorize(Policy = PermissionCodes.AssetGroupsUpdate)]
public record SetGroupAttributeBindingsCommand(
    int AssetGroupId,
    List<AttributeBindingDto> Bindings) : IRequest<Result<int>>;

public record AttributeBindingDto(
    int AssetAttributeDefinitionId,
    bool IsRequired,
    int? SortOrder);

public class SetGroupAttributeBindingsCommandHandler(
    IApplicationDbContext context) : IRequestHandler<SetGroupAttributeBindingsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SetGroupAttributeBindingsCommand request, CancellationToken ct)
    {
        var group = await context.AssetGroups.FindAsync([request.AssetGroupId], ct);
        if (group is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة غير موجودة");

        var existingBindings = await context.AssetGroupAttributes
            .Where(b => b.AssetGroupId == request.AssetGroupId)
            .ToListAsync(ct);

        context.AssetGroupAttributes.RemoveRange(existingBindings);

        foreach (var binding in request.Bindings)
        {
            var definition = await context.AssetAttributeDefinitions.FindAsync([binding.AssetAttributeDefinitionId], ct);
            if (definition is null)
                return Result<int>.Failure(ErrorCodes.Assets.AttributeDefinitionNotFound, ErrorCategory.NotFound, "المواصفة غير موجودة");

            context.AssetGroupAttributes.Add(new AssetGroupAttribute
            {
                AssetGroupId = request.AssetGroupId,
                AssetAttributeDefinitionId = binding.AssetAttributeDefinitionId,
                IsRequired = binding.IsRequired,
                SortOrder = binding.SortOrder
            });
        }

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(request.AssetGroupId);
    }
}

public class SetGroupAttributeBindingsCommandValidator : AbstractValidator<SetGroupAttributeBindingsCommand>
{
    public SetGroupAttributeBindingsCommandValidator()
    {
        RuleFor(x => x.AssetGroupId)
            .GreaterThan(0).WithMessage("معرف المجموعة مطلوب");

        RuleFor(x => x.Bindings)
            .NotEmpty().WithMessage("يجب تحديد مواصفة واحدة على الأقل");
    }
}
