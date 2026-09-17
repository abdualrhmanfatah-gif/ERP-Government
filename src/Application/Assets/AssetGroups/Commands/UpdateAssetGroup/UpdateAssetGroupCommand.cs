using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Commands.UpdateAssetGroup;

[Authorize(Policy = PermissionCodes.AssetGroupsUpdate)]
public record UpdateAssetGroupCommand(
    int Id,
    string Name,
    string? Description,
    int? ParentAssetGroupId,
    string AssetCategory,
    bool IsDepreciable,
    string DepreciationMethod,
    decimal? DepreciationRate,
    int? DefaultUsefulLifeYears,
    decimal? ResidualValuePercentage,
    int? AssetAccountId,
    int? AccumulatedDepreciationAccountId,
    int? DepreciationExpenseAccountId,
    int? DisposalAccountId,
    byte[] RowVersion) : IRequest<Result<int>>;

public class UpdateAssetGroupCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateAssetGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(UpdateAssetGroupCommand request, CancellationToken ct)
    {
        var entity = await context.AssetGroups.FindAsync([request.Id], ct);
        if (entity is null)
            return Result<int>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة غير موجودة");

        if (!entity.IsActive)
            return Result<int>.Failure(ErrorCodes.Assets.InactiveGroup, ErrorCategory.Validation, "يجب تفعيل المجموعة قبل التعديل");

        if (request.ParentAssetGroupId.HasValue && request.ParentAssetGroupId.Value == request.Id)
            return Result<int>.Failure(ErrorCodes.Assets.SelfParent, ErrorCategory.Validation, "لا يمكن أن تكون المجموعة والدتها");

        if (request.ParentAssetGroupId.HasValue)
        {
            if (await AssetGroupValidationHelper.WouldCreateCycleAsync(context, request.ParentAssetGroupId.Value, request.Id, ct))
                return Result<int>.Failure(ErrorCodes.Assets.CycleDetected, ErrorCategory.Validation, "لا يمكن إنشاء حلقة في التسلسل الهرمي");

            if (await AssetGroupValidationHelper.GetDepthAsync(context, request.ParentAssetGroupId.Value, ct) >= 3)
                return Result<int>.Failure(ErrorCodes.Assets.MaxDepthExceeded, ErrorCategory.Validation, "تم تجاوز الحد الأقصى لمستويات التصنيف");
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.ParentAssetGroupId = request.ParentAssetGroupId;
        entity.AssetCategory = request.AssetCategory;
        entity.IsDepreciable = request.IsDepreciable;
        entity.DepreciationMethod = request.DepreciationMethod;
        entity.DepreciationRate = request.DepreciationRate;
        entity.DefaultUsefulLifeYears = request.DefaultUsefulLifeYears;
        entity.ResidualValuePercentage = request.ResidualValuePercentage;
        entity.AssetAccountId = request.AssetAccountId;
        entity.AccumulatedDepreciationAccountId = request.AccumulatedDepreciationAccountId;
        entity.DepreciationExpenseAccountId = request.DepreciationExpenseAccountId;
        entity.DisposalAccountId = request.DisposalAccountId;
        entity.LastModified = DateTimeOffset.UtcNow;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<int>.Failure(ErrorCodes.Request.ConcurrencyConflict, ErrorCategory.Conflict, "تم تعديل المجموعة من مستخدم آخر");
        }

        return Result<int>.Success(entity.Id);
    }
}

public class UpdateAssetGroupCommandValidator : AbstractValidator<UpdateAssetGroupCommand>
{
    public UpdateAssetGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب")
            .MaximumLength(200).WithMessage("اسم المجموعة يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.AssetCategory)
            .NotEmpty().WithMessage("فئة الأصول مطلوبة");

        RuleFor(x => x.DepreciationMethod)
            .NotEmpty().WithMessage("طريقة الإهلاك مطلوبة");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion مطلوب");
    }
}
