using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Commands.CreateAssetGroup;

[Authorize(Policy = PermissionCodes.AssetGroupsCreate)]
public record CreateAssetGroupCommand(
    string Code,
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
    int? DisposalAccountId) : IRequest<Result<int>>;

public class CreateAssetGroupCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateAssetGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAssetGroupCommand request, CancellationToken ct)
    {
        if (await context.AssetGroups.AnyAsync(g => g.Code == request.Code, ct))
            return Result<int>.Failure(ErrorCodes.Assets.DuplicateAssetGroupCode, ErrorCategory.Validation, "كود المجموعة مستخدم بالفعل");

        if (request.ParentAssetGroupId.HasValue)
        {
            if (request.ParentAssetGroupId.Value == 0)
                return Result<int>.Failure(ErrorCodes.Assets.SelfParent, ErrorCategory.Validation, "لا يمكن أن تكون المجموعة والدتها");

            var parent = await context.AssetGroups.FindAsync([request.ParentAssetGroupId.Value], ct);
            if (parent is null)
                return Result<int>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة الأب غير موجودة");

            if (await AssetGroupValidationHelper.WouldCreateCycleAsync(context, request.ParentAssetGroupId.Value, 0, ct))
                return Result<int>.Failure(ErrorCodes.Assets.CycleDetected, ErrorCategory.Validation, "لا يمكن إنشاء حلقة في التسلسل الهرمي");

            if (await AssetGroupValidationHelper.GetDepthAsync(context, request.ParentAssetGroupId.Value, ct) >= 3)
                return Result<int>.Failure(ErrorCodes.Assets.MaxDepthExceeded, ErrorCategory.Validation, "تم تجاوز الحد الأقصى لمستويات التصنيف");
        }

        var entity = new AssetGroup
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            ParentAssetGroupId = request.ParentAssetGroupId,
            AssetCategory = request.AssetCategory,
            IsDepreciable = request.IsDepreciable,
            DepreciationMethod = request.DepreciationMethod,
            DepreciationRate = request.DepreciationRate,
            DefaultUsefulLifeYears = request.DefaultUsefulLifeYears,
            ResidualValuePercentage = request.ResidualValuePercentage,
            AssetAccountId = request.AssetAccountId,
            AccumulatedDepreciationAccountId = request.AccumulatedDepreciationAccountId,
            DepreciationExpenseAccountId = request.DepreciationExpenseAccountId,
            DisposalAccountId = request.DisposalAccountId,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.AssetGroups.Add(entity);
        await context.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}

public class CreateAssetGroupCommandValidator : AbstractValidator<CreateAssetGroupCommand>
{
    public CreateAssetGroupCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("كود المجموعة مطلوب")
            .MaximumLength(50).WithMessage("كود المجموعة يجب أن لا يتجاوز 50 حرف");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب")
            .MaximumLength(200).WithMessage("اسم المجموعة يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.AssetCategory)
            .NotEmpty().WithMessage("فئة الأصول مطلوبة");

        RuleFor(x => x.DepreciationMethod)
            .NotEmpty().WithMessage("طريقة الإهلاك مطلوبة");
    }
}
