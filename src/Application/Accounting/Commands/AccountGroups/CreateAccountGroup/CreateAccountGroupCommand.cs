using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.Commands.AccountGroups.CreateAccountGroup;

[Authorize(Policy = PermissionCodes.ChartOfAccountsCreate)]
public class CreateAccountGroupCommand : IRequest<Result<int>>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public AccountGroupType Type { get; init; }
    public NormalBalanceType NormalBalance { get; init; }
    public string? Description { get; init; }
    public int? ParentId { get; init; }
}

public class CreateAccountGroupCommandHandler(
    IApplicationDbContext context) : IRequestHandler<CreateAccountGroupCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateAccountGroupCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var name = request.Name.Trim();
        var description = request.Description?.Trim();

        // Normalized permanent unique
        if (!await AccountGroupGuardHelper.IsCodeUniquePermanentAsync(context, code, null, cancellationToken))
            return Result<int>.Failure(["الكود موجود مسبقاً."]);

        // Matrix
        if (!AccountGroupHierarchyHelper.IsNormalBalanceCompatible(request.Type, request.NormalBalance))
            return Result<int>.Failure(["الرصيد الطبيعي غير متوافق مع النوع."]);

        AccountGroup? parent = null;
        byte level = 1;
        if (request.ParentId.HasValue)
        {
            parent = await context.AccountGroups.FindAsync(request.ParentId.Value, cancellationToken);
            if (parent == null)
                return Result<int>.Failure(["المجموعة الأب غير موجودة."]);
            if (!parent.IsActive)
                return Result<int>.Failure(["لا يمكن إنشاء مجموعة تحت أب معطل — يجب تفعيل الأب أولاً."]);
            if (!AccountGroupHierarchyHelper.IsTypeInheritanceValid(request.Type, parent.Type))
                return Result<int>.Failure(["نوع الابن يجب أن يساوي نوع الأب."]);
            level = AccountGroupHierarchyHelper.ComputeLevel(parent);
            if (AccountGroupHierarchyHelper.ExceedsMaxDepth(level))
                return Result<int>.Failure(["تجاوز الحد الأقصى للمستويات (5)."]);
        }

        var entity = new AccountGroup
        {
            Code = code,
            Name = name,
            Type = request.Type,
            NormalBalance = request.NormalBalance,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            ParentId = request.ParentId,
            Level = level,
            IsActive = true
        };

        context.AccountGroups.Add(entity);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_AccountGroups_Code", StringComparison.OrdinalIgnoreCase) == true || ex.Message.Contains("IX_AccountGroups_Code", StringComparison.OrdinalIgnoreCase))
        {
            return Result<int>.Failure(["الكود موجود مسبقاً."]);
        }

        return Result<int>.Success(entity.Id);
    }
}

public class CreateAccountGroupCommandValidator : AbstractValidator<CreateAccountGroupCommand>
{
    public CreateAccountGroupCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("الكود مطلوب.")
            .MaximumLength(20).WithMessage("الكود يجب ألا يتجاوز 20 حرفاً.")
            .Must(c => !string.IsNullOrWhiteSpace(c)).WithMessage("الكود مطلوب.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("الاسم مطلوب.")
            .MaximumLength(200).WithMessage("الاسم يجب ألا يتجاوز 200 حرفاً.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع المجموعة غير صالح.");

        RuleFor(x => x.NormalBalance)
            .IsInEnum().WithMessage("الرصيد الطبيعي غير صالح.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("الوصف يجب ألا يتجاوز 500 حرفاً.");

        RuleFor(x => x.ParentId)
            .GreaterThan(0).When(x => x.ParentId.HasValue).WithMessage("معرف الأب غير صالح.");
    }
}
