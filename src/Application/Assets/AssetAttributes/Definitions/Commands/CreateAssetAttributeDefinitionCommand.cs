using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetAttributes.Definitions.Commands;

[Authorize(Policy = PermissionCodes.AssetGroupsCreate)]
public record CreateAssetAttributeDefinitionCommand(
    string Code,
    string Name,
    string? Description,
    string? Unit,
    int SortOrder,
    AssetAttributeDataType AttributeDataType) : IRequest<Result<int>>;

public class CreateAssetAttributeDefinitionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateAssetAttributeDefinitionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetAttributeDefinitionCommand request, CancellationToken ct)
    {
        if (await context.AssetAttributeDefinitions.AnyAsync(d => d.Code == request.Code, ct))
            return Result<int>.Failure(ErrorCodes.Assets.DuplicateAttributeCode, ErrorCategory.Validation, "كود المواصفة مستخدم بالفعل");

        var entity = new AssetAttributeDefinition
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Unit = request.Unit,
            SortOrder = request.SortOrder,
            AttributeDataType = request.AttributeDataType,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetAttributeDefinitions.Add(entity);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateAssetAttributeDefinitionCommandValidator : AbstractValidator<CreateAssetAttributeDefinitionCommand>
{
    public CreateAssetAttributeDefinitionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود المواصفة مطلوب")
            .MaximumLength(50).WithMessage("كود المواصفة يجب أن لا يتجاوز 50 حرف");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المواصفة مطلوب")
            .MaximumLength(200).WithMessage("اسم المواصفة يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب أن لا يتجاوز 500 حرف");

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("الوحدة يجب أن لا تتجاوز 50 حرف");
    }
}
