using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetAttributes.Definitions.Commands;

[Authorize(Policy = PermissionCodes.AssetGroupsUpdate)]
public record UpdateAssetAttributeDefinitionCommand(
    int Id,
    string Name,
    string? Description,
    string? Unit,
    int SortOrder,
    AssetAttributeDataType AttributeDataType,
    bool IsActive,
    byte[] RowVersion) : IRequest<Result<int>>;

public class UpdateAssetAttributeDefinitionCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssetAttributeDefinitionCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateAssetAttributeDefinitionCommand request, CancellationToken ct)
    {
        var entity = await context.AssetAttributeDefinitions.FindAsync([request.Id], ct);
        if (entity is null)
            return Result<int>.Failure(ErrorCodes.Assets.AttributeDefinitionNotFound, ErrorCategory.NotFound, "المواصفة غير موجودة");

        if (entity.AttributeDataType != request.AttributeDataType)
        {
            var hasValues = await context.AssetAttributeValues.AnyAsync(
                v => v.AssetAttributeDefinitionId == request.Id, ct);
            if (hasValues)
                return Result<int>.Failure(ErrorCodes.Assets.InvalidAttributeValue, ErrorCategory.Validation,
                    "لا يمكن تغيير نوع البيانات لمواصفة لها قيم مسجلة");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Unit = request.Unit;
        entity.SortOrder = request.SortOrder;
        entity.AttributeDataType = request.AttributeDataType;
        entity.IsActive = request.IsActive;
        entity.LastModified = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}

public class UpdateAssetAttributeDefinitionCommandValidator : AbstractValidator<UpdateAssetAttributeDefinitionCommand>
{
    public UpdateAssetAttributeDefinitionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المواصفة مطلوب")
            .MaximumLength(200).WithMessage("اسم المواصفة يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب أن لا يتجاوز 500 حرف");

        RuleFor(x => x.Unit)
            .MaximumLength(50).WithMessage("الوحدة يجب أن لا تتجاوز 50 حرف");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("إصدار الصف مطلوب");
    }
}
